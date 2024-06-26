using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PSW.Core.Map
{    
    public class MapNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] MapNodeType mapType;
        [SerializeField] private Image image;
        [SerializeField] private Image visitedImage;
        [SerializeField] private Image visitedCheckImage;

        private Button button;
        private const float HoverScaleFactor = 1.25f;
        private const float TweenAnimateDuration = 0.3f;

        public Node Node { get; private set; }
        public NodeBlueprint Blueprint { get; private set; }

        private void Awake()
        {
            this.button = GetComponent<Button>();
            this.button.onClick.AddListener(MapNodeClick);
        }

        public void Set(Node node, NodeBlueprint blueprint)
        {
            this.Node = node;
            this.Blueprint = blueprint;

            this.image.sprite = blueprint.GetNormalSprite();
            this.visitedImage.sprite = blueprint.GetVisitedSprite();
            this.mapType = blueprint.GetNodeType();

            this.name = blueprint.name;

            this.visitedImage.gameObject.SetActive(false);

            if (this.mapType == MapNodeType.Boss)
            {
                this.transform.localScale *= 1.5f;
            }
        }

        /// <summary>
        /// 맵 노트 상태에 따른 시각 효과
        /// </summary>
        /// <param name="state"></param>
        public void SetState(StageState state)
        {
            if (this.image == null) return;

            this.image.DOKill();

            switch (state) 
            {
                case StageState.Locked:
                    this.image.color = MapView.Instance.lockedColor;
                    break;

                case StageState.Visited:
                    this.visitedImage.gameObject.SetActive(true);
                    break;

                case StageState.Attainable:
                    this.image.color = MapView.Instance.lockedColor;
                    this.image
                        .DOColor(MapView.Instance.visitedColor, 0.5f)
                        .SetLoops(-1, LoopType.Yoyo);
                    break;
            }
        }

        /// <summary>
        /// 스테이지 맵 클릭시 이벤트 => 전투, 미스터리, 상점, 휴식, 보물
        /// </summary>
        private void MapNodeClick()
        {
            MapPlayerTracker.Instance.SelectNode(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (this.image == null) return;

            this.image.transform
                .DOScale(1.0f * HoverScaleFactor, TweenAnimateDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (this.image == null) return;

            this.image.transform
                .DOScale(1.0f, TweenAnimateDuration);
        }

        /// <summary>
        /// 클릭시 회오리 애니메이션 작동
        /// </summary>
        public void ShowSwirlAnimation()
        {
            if (this.visitedCheckImage == null) return;

            this.visitedCheckImage
                .DOFillAmount(1f, TweenAnimateDuration);
        }

        /// <summary>
        /// 로비 씬으로 갈 경우, Tween Null 방지
        /// </summary>
        private void OnDestroy()
        {
            this.image.DOKill();
        }
    }
}
