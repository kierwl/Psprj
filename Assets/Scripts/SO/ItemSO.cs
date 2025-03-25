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

    // ������ ���� �޼��� (�ν��Ͻ���)
    public virtual ItemSO Clone()
    {
        ItemSO clone = Instantiate(this);
        return clone;
    }

    // ������ ��� ���� �޼��� (��� Ŭ�������� ����)
    public virtual void Use(CharacterController character)
    {
        // �⺻ ��� ����
        Debug.Log($"{itemName} �������� ����߽��ϴ�.");
    }
}