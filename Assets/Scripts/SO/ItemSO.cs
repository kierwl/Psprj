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
}