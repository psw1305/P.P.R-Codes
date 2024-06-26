using PSW.Core.Enums;
using PSW.Core.Probability;
using System.Collections.Generic;
using UnityEngine;

public partial class GameBoard : BehaviourSingleton<GameBoard>
{
    [Header("Board Settings")]
    [SerializeField] private int rowCount = 6;
    [SerializeField] private int columnCount = 6;
    [SerializeField] private int skillStack = 5;
    [SerializeField] private float rescaling = 0.25f;

    [Header("Card Settings")]
    [SerializeField] private List<ItemBlueprintCard> cards = new();
    [SerializeField] private List<ItemBlueprintCard> skillCards = new();
    [SerializeField] private List<ItemBlueprintCard> usedSkillCards = new();
    [SerializeField] private GameBoardCardList<ItemBlueprintCard> gameBoardCards = new();

    [Header("Additional")]
    [SerializeField] private GameBoardCountingText countingText;
    [SerializeField] private GameBoardSelectionLine selectionLine;
    [SerializeField] private GameObject battleCardPrefab;

    private GameBoardInput boardInput;
    private GameBoardMovement boardMovement;
    private GameBoardSpawning boardSpawning;

    public List<GameBoardCard> Cards { get; } = new List<GameBoardCard>();
    public GameBoardCardList<ItemBlueprintCard> GameBoardCards => this.gameBoardCards;
    public Vector2 StartCellPosition => this.BoardCenter - this.Rescaling * new Vector2(this.ColumnCount - 1, this.RowCount - 1) / 2.0f;
    public int RowCount => this.rowCount;
    public int ColumnCount => this.columnCount;
    public float Rescaling => this.rescaling;
    public Vector2 BoardCenter => this.transform.position;
    public Transform BoardContainer => this.transform;

    protected override void Awake()
    {
        base.Awake();

        this.boardInput = new GameBoardInput(this, this.countingText, this.selectionLine);
        this.boardMovement = new GameBoardMovement(this);
        this.boardSpawning = new GameBoardSpawning(this, this.skillStack);

        // 가중치 값 list 생성
        ProbabilityList();
    }

    /// <summary>
    /// 선택된 카드 리스트
    /// </summary>
    /// <returns></returns>
    public List<GameBoardCard> GetSelectCards()
    {
        return this.boardInput.SelectedCards;
    }

    /// <summary>
    /// 주어진 카드 가중치 값으로 카드 리스트 생성 
    /// </summary>
    private void ProbabilityList()
    {
        foreach (var card in this.cards)
        {
            this.gameBoardCards.Add(card, card.CardWeighted);
        }
    }

    /// <summary>
    /// GameBoard 카드 생성
    /// </summary>
    public void SetBoard()
    {
        if (Player.Instance != null)
        {
            foreach (var card in Player.Instance.GetCardDeck())
            {
                this.skillCards.Add(card.GetCardData());
            }
        }

        for (int y = 0; y < this.ColumnCount; y++)
        {
            for (int x = 0; x < this.RowCount; x++)
            {
                var card = Instantiate(this.battleCardPrefab).GetComponent<GameBoardCard>();
                card.transform.localScale = Vector2.one;
                card.transform.position = this.BoardToWorldPosition(x, y);
                card.transform.SetParent(this.BoardContainer, true);
                card.Set(this, this.GameBoardCards.Get());

                this.Cards.Add(card);
            }
        }

        // 플레이어가 장착한 소모품 생성
        //BattlePlayer.Instance.SetPotions();
    }

    /// <summary>
    /// 일반 카드 중에 랜덤으로 하나 픽업
    /// </summary>
    /// <returns></returns>
    public GameBoardCard RandomCardFromNormal()
    {
        if (AllCardsIsNotNormal()) return null;

        while (true)
        {
            var random = Random.Range(0, this.Cards.Count);
            var randomCard = this.Cards[random];
            
            if (randomCard.CardDetailType == CardDetailType.Normal)
            {
                return randomCard;
            }

            // 무한 루프 검사
            InfiniteLoopDetector.Run();
        }
    }

    /// <summary>
    /// 변환되는 카드 중에 랜덤 픽업
    /// </summary>
    /// <returns></returns>
    public ItemBlueprintCard SkillCardRandomPickUp()
    {
        var random = Random.Range(0, this.skillCards.Count);
        var randomSkillCard = this.skillCards[random];
        
        // 스킬 카드 사용 후 List 변환
        this.skillCards.Remove(randomSkillCard);
        this.usedSkillCards.Add(randomSkillCard);

        return randomSkillCard;
    }

    /// <summary>
    /// 사용된 스킬 카드 초기화
    /// </summary>
    /// <param name="data"></param>
    public void SkillCardReset(ItemBlueprintCard data)
    {
        this.usedSkillCards.Remove(data);
        this.skillCards.Add(data);
    }

    /// <summary>
    /// 특정 장애물 카드 갯수 홧인
    /// </summary>
    /// <returns></returns>
    public int ObstacleCardsCount(CardDetailType cardDetailType)
    {
        int count = 0;

        foreach (var card in this.Cards)
        {
            if (card.CardDetailType == cardDetailType)
            {
                count++;
            }
        }

        return count;
    }

    #region Game Board Play Coroutine
    /// <summary>
    /// GameBoard 안에 있는 카드들이 움직일때 까지 대기
    /// </summary>
    public Coroutine WaitForMovement()
    {
        return StartCoroutine(this.boardMovement.WaitForMovement());
    }

    /// <summary>
    /// 카드 끼리 selection 라인이 이어지기 까지 대기
    /// </summary>
    public Coroutine WaitForSelection()
    {
        return StartCoroutine(this.boardInput.WaitForSelection());
    }

    /// <summary>
    /// 선택된 카드들이 소멸될 때 까지 대기
    /// </summary>
    public Coroutine RespawnCards()
    {
        return StartCoroutine(this.boardSpawning.Respawn());
    }

    /// <summary>
    /// 카드들이 다 선택될 때 까지 대기
    /// </summary>
    public Coroutine DespawnSelection()
    {
        return StartCoroutine(this.boardSpawning.Despawn(GetSelectCards()));
    }

    /// <summary>
    /// 스킬 카드 생성
    /// </summary>
    public Coroutine SkillCardSpawn()
    {
        return StartCoroutine(this.boardSpawning.SkillCardSpawn());
    }

    /// <summary>
    /// 장애물 카드 생성
    /// </summary>
    public Coroutine ObstacleCardSpawn(ItemBlueprintCard obstacleCard)
    {
        return StartCoroutine(this.boardSpawning.ObstacleCardSpawn(obstacleCard));
    }
    #endregion

    #region Game Board Check Function
    /// <summary>
    /// 스킬 카드가 비어있을 경우
    /// </summary>
    /// <returns></returns>
    public bool IsSkillCardEmpty()
    {
        if (this.skillCards.Count == 0) return true;

        return false;
    }

    /// <summary>
    /// 게임 보드상에 일반 카드 하나라도 없을 경우
    /// </summary>
    /// <returns></returns>
    private bool AllCardsIsNotNormal()
    {
        foreach (var card in this.Cards)
        {
            if (card.CardDetailType == CardDetailType.Normal)
            {
                return false;
            }
        }

        return true;
    }
    #endregion
}
