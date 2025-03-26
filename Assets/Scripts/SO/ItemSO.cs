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
    public virtual string GetTooltip()
    {
        string tooltipText = $"<b>{itemName}</b>\n";
        tooltipText += $"<color=#{GetRarityColorHex(rarity)}>{rarity}</color>\n";
        tooltipText += $"����: {GetItemTypeText(itemType)}\n\n";
        tooltipText += description;

        return tooltipText;
    }

    protected string GetRarityColorHex(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "CCCCCC"; // ȸ��
            case ItemRarity.Uncommon: return "00CC00"; // �ʷϻ�
            case ItemRarity.Rare: return "0000FF"; // �Ķ���
            case ItemRarity.Epic: return "CC00CC"; // �����
            case ItemRarity.Legendary: return "FFCC00"; // �ݻ�
            default: return "FFFFFF"; // ���
        }
    }

    protected string GetItemTypeText(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return "����";
            case ItemType.Armor: return "��";
            case ItemType.Accessory: return "�Ǽ�����";
            case ItemType.Consumable: return "�Ҹ�ǰ";
            case ItemType.Material: return "���";
            default: return "��Ÿ";
        }
    }

}