using PSW.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    protected ItemBlueprint blueprint;

    /// <summary>
    /// 아이템을 추가하여 장착하고 UI로 표시
    /// </summary>
    /// <param name="blueprint">아이템 데이터</param>
    public virtual void Set(ItemBlueprint blueprint)
    {
        this.blueprint = blueprint;
        this.name = blueprint.name;
        this.image.sprite = blueprint.ItemImage;

        this.button.onClick.AddListener(ItemTooltipShow);
    }

    public ItemBlueprint GetBlueprint()
    {
        return this.blueprint;
    }

    /// <summary>
    /// 아이템 클릭시 툴팁 표시
    /// </summary>
    protected virtual void ItemTooltipShow() { }
}
