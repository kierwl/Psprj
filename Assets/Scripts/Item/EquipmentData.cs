using UnityEngine;

[System.Serializable]
public class EquipmentData
{
    public string id;                 // 장비 고유 ID
    public string name;               // 장비 이름
    public string description;        // 장비 설명
    public Sprite icon;               // 장비 아이콘
    public EquipmentType type;        // 장비 유형
    public EquipmentRarity rarity;    // 장비 희귀도

    [Header("기본 스탯 보너스")]
    public float baseHealthBonus;    // 기본 체력 보너스
    public float baseAttackBonus;    // 기본 공격력 보너스
    public float baseDefenseBonus;   // 기본 방어력 보너스
    public float speedBonus;         // 이동 속도 보너스 (강화 불가)

    [Header("강화 관련")]
    public int level = 1;            // 현재 레벨
    public int maxLevel = 10;        // 최대 레벨
    public int baseCost = 100;       // 기본 강화 비용
    public float statGrowthRate = 0.1f; // 스탯 성장률 (레벨당 증가 비율)

    [Header("추가 정보")]
    public bool isNew = true;        // 새로운 아이템 여부
    public GameObject equipPrefab;   // 장착 시 사용될 프리팹 (외형 변경용)

    // 능력치 계산 메서드들
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

    // 다음 레벨 스탯 계산 메서드들
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

    // 강화 비용 계산
    public int GetUpgradeCost()
    {
        if (level >= maxLevel) return 0;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, level - 1));
    }

    // 장비 판매 가격 계산
    public int GetSellPrice()
    {
        return Mathf.RoundToInt(GetUpgradeCost() / 2);
    }

    // 복사본 생성 메서드
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

// 장비 타입 열거형
public enum EquipmentType
{
    Weapon,     // 무기
    Armor,      // 방어구
    Helmet,     // 투구
    Gloves,     // 장갑
    Boots,      // 신발
    Accessory    // 악세서리
}

// 장비 희귀도 열거형
public enum EquipmentRarity
{
    Common,     // 일반
    Uncommon,   // 고급
    Rare,       // 희귀
    Epic,       // 영웅
    Legendary   // 전설
}