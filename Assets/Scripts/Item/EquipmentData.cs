using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EquipmentData
{
    public string id;
    public string name;
    public string description;
    public EquipmentType type;
    public EquipmentRarity rarity;
    public int level = 1;
    public Sprite icon;

    // 기본 스탯 보너스
    public float healthBonus;
    public float attackBonus;
    public float defenseBonus;
    public float speedBonus;

    // 추가 속성 (선택적)
    public List<EquipmentAttribute> attributes = new List<EquipmentAttribute>();

    // 레벨당 스탯 증가량
    public float healthBonusPerLevel;
    public float attackBonusPerLevel;
    public float defenseBonusPerLevel;

    // 현재 레벨의 스탯 보너스 계산
    public float GetCurrentHealthBonus() => healthBonus + (level - 1) * healthBonusPerLevel;
    public float GetCurrentAttackBonus() => attackBonus + (level - 1) * attackBonusPerLevel;
    public float GetCurrentDefenseBonus() => defenseBonus + (level - 1) * defenseBonusPerLevel;

    // 강화 비용 계산
    public int GetUpgradeCost() => 100 * level * (int)rarity;
}

public enum EquipmentType
{
    Weapon,
    Armor,
    Accessory,
    Helmet,
    Boots
}

public enum EquipmentRarity
{
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Epic = 4,
    Legendary = 5
}

[System.Serializable]
public class EquipmentAttribute
{
    public string name;
    public float value;
}