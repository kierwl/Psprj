using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Items/Base Item")]
public class ItemSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;
    public string itemName;
    [TextArea(2, 5)]
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public ItemRarity rarity;
    [Header("ItemDrop")]
    public GameObject dropPrefab;
    [Header("Shop Info")]
    public int buyPrice;
    public int sellPrice;
    public bool isSellable = true;

    public enum ItemType
    {
        Weapon,
        Armor,
        Accessory,
        Consumable,
        Material
    }

    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }

    // 아이템 복사 메서드 (인스턴스용)
    public virtual ItemSO Clone()
    {
        ItemSO clone = Instantiate(this);
        return clone;
    }

    // 아이템 사용 가상 메서드 (상속 클래스에서 구현)
    public virtual void Use(CharacterController character)
    {
        // 기본 사용 로직
        Debug.Log($"{itemName} 아이템을 사용했습니다.");
    }
    public virtual string GetTooltip()
    {
        string tooltipText = $"<b>{itemName}</b>\n";
        tooltipText += $"<color=#{GetRarityColorHex(rarity)}>{rarity}</color>\n";
        tooltipText += $"유형: {GetItemTypeText(itemType)}\n\n";
        tooltipText += description;

        return tooltipText;
    }

    protected string GetRarityColorHex(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "CCCCCC"; // 회색
            case ItemRarity.Uncommon: return "00CC00"; // 초록색
            case ItemRarity.Rare: return "0000FF"; // 파란색
            case ItemRarity.Epic: return "CC00CC"; // 보라색
            case ItemRarity.Legendary: return "FFCC00"; // 금색
            default: return "FFFFFF"; // 흰색
        }
    }

    protected string GetItemTypeText(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return "무기";
            case ItemType.Armor: return "방어구";
            case ItemType.Accessory: return "악세서리";
            case ItemType.Consumable: return "소모품";
            case ItemType.Material: return "재료";
            default: return "기타";
        }
    }

}