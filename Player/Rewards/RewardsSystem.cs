using PSW.Core.Enums;
using PSW.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class RewardsSystem : MonoBehaviour
{
    private CanvasGroup rewardCanvas;
    private EnemyType enemyType;

    [SerializeField] private Transform rewardList;
    [SerializeField] private Button exitButton;

    [Header("Prefab")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject relicPrefab;
    [SerializeField] private GameObject potionPrefab;
    [SerializeField] private GameObject cashPrefab;

    private void Awake()
    {
        this.rewardCanvas = GetComponent<CanvasGroup>();
        this.rewardCanvas.CanvasInit();
        this.exitButton.onClick.AddListener(ExitToBattle);
    }

    /// <summary>
    /// 전투 종료시 보상 구분
    /// </summary>
    /// <param name="enemyType"></param>
    public void SetBattleRewards(EnemyType enemyType)
    {
        this.enemyType = enemyType;

        switch (this.enemyType)
        {
            case EnemyType.Minor:
                BattleRewardsMinor();
                break;
            case EnemyType.Elite:
                BattleRewardsElite();
                break;
            case EnemyType.Boss:
                BattleRewardsBoss();
                break;
        }
    }

    /// <summary>
    /// 일반 전투 보상 => 골드, 카드, 포션(일정 확률)
    /// </summary>
    private void BattleRewardsMinor()
    {
        CreateReward(this.cashPrefab);
        CreateReward(this.potionPrefab);
        CreateReward(this.cardPrefab);
    }

    /// <summary>
    /// 엘리트 전투 보상 => 유물, 골드, 카드, 포션(일정 확률)
    /// </summary>
    private void BattleRewardsElite()
    {
        CreateReward(this.relicPrefab);
        CreateReward(this.cashPrefab);
        CreateReward(this.potionPrefab);
        CreateReward(this.cardPrefab);
    }

    /// <summary>
    /// 보스 전투 보상 => 유물(Rare), 골드
    /// </summary>
    private void BattleRewardsBoss()
    {
        CreateReward(this.relicPrefab);
        CreateReward(this.cashPrefab);
    }

    private void CreateReward(GameObject rewardsPrefab)
    {
        var rewards = Instantiate(rewardsPrefab, this.rewardList).GetComponent<RewardsItem>();
        rewards.Set(this.enemyType);
    }

    /// <summary>
    /// 보상 창 보여주기
    /// </summary>
    public void Show()
    {
        BattleSFX.Instance.Play(BattleSFX.Instance.victory);

        this.rewardCanvas.CanvasFadeIn(DUR.CANVAS_FADE_TIME);
    }

    /// <summary>
    /// 떠나기 버튼 클릭 시 => Stage Scene 으로 복귀
    /// </summary>
    private void ExitToBattle()
    {
        UISFX.Instance.Play(UISFX.Instance.buttonClick);

        this.exitButton.interactable = false;
        SceneLoader.Instance.PlayerCheckSceneLoad(SceneNames.Battle);
    }
}
