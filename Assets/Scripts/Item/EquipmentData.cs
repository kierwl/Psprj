using UnityEngine;

[System.Serializable]
public class EquipmentData
{
    public string id;                 // ��� ���� ID
    public string name;               // ��� �̸�
    public string description;        // ��� ����
    public Sprite icon;               // ��� ������
    public EquipmentType type;        // ��� ����
    public EquipmentRarity rarity;    // ��� ��͵�

    [Header("�⺻ ���� ���ʽ�")]
    public float baseHealthBonus;    // �⺻ ü�� ���ʽ�
    public float baseAttackBonus;    // �⺻ ���ݷ� ���ʽ�
    public float baseDefenseBonus;   // �⺻ ���� ���ʽ�
    public float speedBonus;         // �̵� �ӵ� ���ʽ� (��ȭ �Ұ�)

    [Header("��ȭ ����")]
    public int level = 1;            // ���� ����
    public int maxLevel = 10;        // �ִ� ����
    public int baseCost = 100;       // �⺻ ��ȭ ���
    public float statGrowthRate = 0.1f; // ���� ����� (������ ���� ����)

    [Header("�߰� ����")]
    public bool isNew = true;        // ���ο� ������ ����
    public GameObject equipPrefab;   // ���� �� ���� ������ (���� �����)

    // �ɷ�ġ ��� �޼����
    public float GetCurrentHealthBonus()
    {
        return baseHealthBonus * (1 + (level - 1) * statGrowthRate);
    }

    public float GetCurrentAttackBonus()
    {
        return baseAttackBonus * (1 + (level - 1) * statGrowthRate);
    }

    public float GetCurrentDefenseBonus()
    {
        return baseDefenseBonus * (1 + (level - 1) * statGrowthRate);
    }

    // ���� ���� ���� ��� �޼����
    public float GetNextLevelHealthBonus()
    {
        if (level >= maxLevel) return GetCurrentHealthBonus();
        return baseHealthBonus * (1 + level * statGrowthRate);
    }

    public float GetNextLevelAttackBonus()
    {
        if (level >= maxLevel) return GetCurrentAttackBonus();
        return baseAttackBonus * (1 + level * statGrowthRate);
    }

    public float GetNextLevelDefenseBonus()
    {
        if (level >= maxLevel) return GetCurrentDefenseBonus();
        return baseDefenseBonus * (1 + level * statGrowthRate);
    }

    // ��ȭ ��� ���
    public int GetUpgradeCost()
    {
        if (level >= maxLevel) return 0;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, level - 1));
    }

    // ��� �Ǹ� ���� ���
    public int GetSellPrice()
    {
        return Mathf.RoundToInt(GetUpgradeCost() / 2);
    }

    // ���纻 ���� �޼���
    public EquipmentData Clone()
    {
        return new EquipmentData
        {
            id = this.id,
            name = this.name,
            description = this.description,
            icon = this.icon,
            type = this.type,
            rarity = this.rarity,
            baseHealthBonus = this.baseHealthBonus,
            baseAttackBonus = this.baseAttackBonus,
            baseDefenseBonus = this.baseDefenseBonus,
            speedBonus = this.speedBonus,
            level = this.level,
            maxLevel = this.maxLevel,
            baseCost = this.baseCost,
            statGrowthRate = this.statGrowthRate,
            isNew = this.isNew,
            equipPrefab = this.equipPrefab
        };
    }
}

// ��� Ÿ�� ������
public enum EquipmentType
{
    Weapon,     // ����
    Armor,      // ��
    Helmet,     // ����
    Gloves,     // �尩
    Boots,      // �Ź�
    Accessory    // �Ǽ�����
}

// ��� ��͵� ������
public enum EquipmentRarity
{
    Common,     // �Ϲ�
    Uncommon,   // ���
    Rare,       // ���
    Epic,       // ����
    Legendary   // ����
}