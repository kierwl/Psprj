using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    [Header("기본 스탯")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float attackPower = 10f;
    [SerializeField] private float defenseValue = 5f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float criticalChance = 0.05f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int gold;

    [Header("장비 관련")]
    [SerializeField] private string characterId = "player";  // 해당 플레이어의 캐릭터 ID


    public event Action<float, float> OnHealthChanged; // (현재체력, 최대체력)
    public event Action<int> OnGoldChanged; // (현재 골드)
    public event Action<BuffData> OnBuffApplied; // (적용된 버프)
    public event Action<BuffData> OnBuffRemoved; // (제거된 버프)

    // 임시 버프 관리
    private List<BuffData> activeBuffs = new List<BuffData>();

    [System.Serializable]
    public class BuffData
    {
        public string buffId;
        public string buffName;
        public StatType statType;
        public float value;
        public float duration;
        public float remainingTime;
        public Sprite icon;
        public bool isPercentage; // true: 퍼센트 증가, false: 고정값 증가
    }

    public enum StatType
    {
        Health,
        Attack,
        Defense,
        MoveSpeed,
        CriticalChance,
        AttackSpeed,
        AttackRange
    }

    public float GetBaseStatValue(PlayerStats.StatType statType)
    {
        switch (statType)
        {
            case PlayerStats.StatType.Health: return maxHealth;
            case PlayerStats.StatType.Attack: return attackPower;
            case PlayerStats.StatType.Defense: return defenseValue;
            case PlayerStats.StatType.MoveSpeed: return moveSpeed;
            case PlayerStats.StatType.CriticalChance: return criticalChance;
            case PlayerStats.StatType.AttackSpeed: return attackSpeed;
            case PlayerStats.StatType.AttackRange: return attackRange;
            default: return 0;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        currentHealth = maxHealth;
    }

    private void Update()
    {
        UpdateBuffs();
    }

    private void UpdateBuffs()
    {
        // 만료된 버프 목록
        List<BuffData> expiredBuffs = new List<BuffData>();

        // 각 버프의 남은 시간 갱신
        foreach (var buff in activeBuffs)
        {
            buff.remainingTime -= Time.deltaTime;
            if (buff.remainingTime <= 0)
            {
                expiredBuffs.Add(buff);
            }
        }

        // 만료된 버프 제거
        foreach (var buff in expiredBuffs)
        {
            RemoveBuff(buff);
        }
    }

    // 체력 변경 메서드
    public void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(1, damage - GetStat(StatType.Defense));
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            // Die() 호출은 이제 PlayerController에서 OnHealthChanged 이벤트로 처리
            Debug.Log("PlayerStats: 체력이 0이 되었습니다.");
        }
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"체력 회복: +{amount}, 현재 체력: {currentHealth}/{maxHealth}");
    }

    // 버프 적용 메서드
    public void ApplyBuff(BuffData newBuff)
    {
        // 이미 같은 ID의 버프가 있는지 확인
        BuffData existingBuff = activeBuffs.Find(b => b.buffId == newBuff.buffId);
        if (existingBuff != null)
        {
            // 기존 버프 갱신
            existingBuff.remainingTime = newBuff.duration;
            existingBuff.value = newBuff.value;
        }
        else
        {
            // 새 버프 추가
            activeBuffs.Add(newBuff);
            OnBuffApplied?.Invoke(newBuff);
        }

        Debug.Log($"{newBuff.buffName} 버프가 {newBuff.duration}초 동안 적용되었습니다.");
    }

    // 버프 제거 메서드
    public void RemoveBuff(BuffData buff)
    {
        if (activeBuffs.Remove(buff))
        {
            OnBuffRemoved?.Invoke(buff);
            Debug.Log($"{buff.buffName} 버프가 만료되었습니다.");
        }
    }

    // 특정 스탯 값 가져오기 (버프 적용)
    public float GetStat(StatType statType)
    {
        float baseValue = GetBaseStatValue(statType);
        float additiveBonus = 0;
        float percentageBonus = 0;

        // 적용 중인 버프에서 해당 스탯 증가량 계산
        foreach (var buff in activeBuffs)
        {
            if (buff.statType == statType)
            {
                if (buff.isPercentage)
                    percentageBonus += buff.value;
                else
                    additiveBonus += buff.value;
            }
        }

        // 최종 스탯 값 계산: 기본값 + 고정 증가량 + (기본값 * 퍼센트 증가량)
        return baseValue + additiveBonus + (baseValue * percentageBonus);
    }

    

    // 골드 관련 메서드
    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
        Debug.Log($"골드 획득: +{amount}, 현재 골드: {gold}");
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            Debug.Log($"골드 사용: -{amount}, 남은 골드: {gold}");
            return true;
        }
        Debug.LogWarning($"골드가 부족합니다. 필요 골드: {amount}, 보유 골드: {gold}");
        return false;
    }

    public int GetGold()
    {
        return gold;
    }

    // 직접 스탯 값을 수정하는 메서드들
    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetAttackPower(float value) { attackPower = value; }
    public void SetDefenseValue(float value) { defenseValue = value; }
    public void SetMoveSpeed(float value) { moveSpeed = value; }
    public void SetAttackSpeed(float value) { attackSpeed = value; }
    public void SetCriticalChance(float value) { criticalChance = Mathf.Clamp01(value); }
    public void SetAttackRange(float value) { attackRange = value; }

    // 편의성 메서드들
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return GetStat(StatType.Health); }
    public float GetAttackPower() { return GetStat(StatType.Attack); }
    public float GetDefenseValue() { return GetStat(StatType.Defense); }
    public float GetAttackRange() { return GetStat(StatType.AttackRange); }

    // 임시 버프 생성 헬퍼 메서드
    public static BuffData CreateBuff(string id, string name, StatType statType, float value, float duration, Sprite icon, bool isPercentage = false)
    {
        return new BuffData
        {
            buffId = id,
            buffName = name,
            statType = statType,
            value = value,
            duration = duration,
            remainingTime = duration,
            icon = icon,
            isPercentage = isPercentage
        };
    }

    // 장비 스탯 적용
    public void ApplyEquipmentStats()
    {
        if (EquipmentManager.instance == null)
        {
            Debug.LogWarning("EquipmentManager가 존재하지 않습니다.");
            return;
        }

        ClearEquipmentBuffs();

        // 캐릭터의 모든 장착 장비 가져오기
        List<EquipmentData> equippedItems = EquipmentManager.instance.GetAllEquippedItems(characterId);

        foreach (EquipmentData equipment in equippedItems)
        {
            if (equipment == null) continue;

            // 체력 증가 버프
            if (equipment.GetCurrentHealthBonus() > 0)
            {
                BuffData healthBuff = CreateBuff(
                    $"equip_{equipment.name}_health",
                    $"{equipment.name} 체력 보너스",
                    StatType.Health,
                    equipment.GetCurrentHealthBonus(),
                    float.MaxValue, // 영구 지속
                    equipment.icon,
                    false // 고정값
                );
                ApplyBuff(healthBuff);
            }

            // 공격력 증가 버프
            if (equipment.GetCurrentAttackBonus() > 0)
            {
                BuffData attackBuff = CreateBuff(
                    $"equip_{equipment.name}_attack",
                    $"{equipment.name} 공격력 보너스",
                    StatType.Attack,
                    equipment.GetCurrentAttackBonus(),
                    float.MaxValue, // 영구 지속
                    equipment.icon,
                    false // 고정값
                );
                ApplyBuff(attackBuff);
            }

            // 방어력 증가 버프
            if (equipment.GetCurrentDefenseBonus() > 0)
            {
                BuffData defenseBuff = CreateBuff(
                    $"equip_{equipment.name}_defense",
                    $"{equipment.name} 방어력 보너스",
                    StatType.Defense,
                    equipment.GetCurrentDefenseBonus(),
                    float.MaxValue, // 영구 지속
                    equipment.icon,
                    false // 고정값
                );
                ApplyBuff(defenseBuff);
            }

            // 이동 속도 증가 버프
            if (equipment.speedBonus > 0)
            {
                BuffData speedBuff = CreateBuff(
                    $"equip_{equipment.name}_speed",
                    $"{equipment.name} 이동속도 보너스",
                    StatType.MoveSpeed,
                    equipment.speedBonus,
                    float.MaxValue, // 영구 지속
                    equipment.icon,
                    false // 고정값
                );
                ApplyBuff(speedBuff);
            }
        }

        Debug.Log($"플레이어에 장비 스탯 적용 완료: 장비 {equippedItems.Count}개");
    }
    // 인벤토리 시스템과 장비 효과 연동
    public void ApplyInventoryEquipment()
    {
        // 인벤토리 시스템 참조 가져오기
        Inventory inventory = Inventory.instance;
        if (inventory == null)
        {
            Debug.LogWarning("인벤토리 시스템을 찾을 수 없습니다.");
            return;
        }

        // 장착 장비 관련 버프 초기화
        ClearInventoryEquipmentBuffs();

        // 장착된 모든 아이템 가져오기
        foreach (var item in inventory.items)
        {
            if (item.isEquipped && item.item != null)
            {
                // 무기 장착 효과
                if (item.item is WeaponSO weapon)
                {
                    // 업그레이드 매니저에서 강화된 공격력 가져오기
                    float attackValue = weapon.attackDamage;

                    if (UpgradeManager.instance != null)
                    {
                        attackValue = UpgradeManager.instance.GetItemStat(weapon);
                    }

                    // 공격력 증가 버프 적용
                    BuffData weaponBuff = CreateBuff(
                        $"inv_equip_{weapon.name}_attack",
                        $"{weapon.itemName} 공격력",
                        StatType.Attack,
                        attackValue,
                        float.MaxValue, // 영구 지속
                        weapon.icon,
                        false // 고정값
                    );
                    ApplyBuff(weaponBuff);

                    // 공격 속도 증가 버프 적용
                    if (weapon.attackSpeed > 0)
                    {
                        BuffData attackSpeedBuff = CreateBuff(
                            $"inv_equip_{weapon.name}_speed",
                            $"{weapon.itemName} 공격 속도",
                            StatType.AttackSpeed,
                            weapon.attackSpeed,
                            float.MaxValue, // 영구 지속
                            weapon.icon,
                            false // 고정값
                        );
                        ApplyBuff(attackSpeedBuff);
                    }
                }
                // 방어구 장착 효과
                else if (item.item is ArmorSO armor)
                {
                    // 업그레이드 매니저에서 강화된 방어력 가져오기
                    float defenseValue = armor.defense;

                    if (UpgradeManager.instance != null)
                    {
                        defenseValue = UpgradeManager.instance.GetItemStat(armor);
                    }

                    // 방어력 증가 버프 적용
                    BuffData armorBuff = CreateBuff(
                        $"inv_equip_{armor.name}_defense",
                        $"{armor.itemName} 방어력",
                        StatType.Defense,
                        defenseValue,
                        float.MaxValue, // 영구 지속
                        armor.icon,
                        false // 고정값
                    );
                    ApplyBuff(armorBuff);
                }
                // 악세서리 장착 효과 (예시)
                else if (item.item is AccessorySO accessory)
                {
                    // 악세서리에 특수 효과가 있을 경우 적용
                    if (accessory.healthBonus > 0)
                    {
                        BuffData healthBuff = CreateBuff(
                            $"inv_equip_{accessory.name}_health",
                            $"{accessory.itemName} 체력 보너스",
                            StatType.Health,
                            accessory.healthBonus,
                            float.MaxValue, // 영구 지속
                            accessory.icon,
                            accessory.isPercentage // 퍼센트 또는 고정값
                        );
                        ApplyBuff(healthBuff);
                    }

                    if (accessory.criticalBonus > 0)
                    {
                        BuffData critBuff = CreateBuff(
                            $"inv_equip_{accessory.name}_crit",
                            $"{accessory.itemName} 크리티컬 보너스",
                            StatType.CriticalChance,
                            accessory.criticalBonus,
                            float.MaxValue, // 영구 지속
                            accessory.icon,
                            true // 퍼센트 증가
                        );
                        ApplyBuff(critBuff);
                    }
                }
            }
        }

        Debug.Log("인벤토리 장비 스탯 적용 완료");
    }

    // 인벤토리 장비 버프 초기화
    private void ClearInventoryEquipmentBuffs()
    {
        List<BuffData> inventoryBuffs = new List<BuffData>();

        // 인벤토리 장비 관련 버프 찾기
        foreach (var buff in activeBuffs)
        {
            if (buff.buffId.StartsWith("inv_equip_"))
            {
                inventoryBuffs.Add(buff);
            }
        }

        // 버프 제거
        foreach (var buff in inventoryBuffs)
        {
            RemoveBuff(buff);
        }
    }
    // 장비 관련 버프 초기화
    private void ClearEquipmentBuffs()
    {
        List<BuffData> equipmentBuffs = new List<BuffData>();

        // 장비 관련 버프 찾기
        foreach (var buff in activeBuffs)
        {
            if (buff.buffId.StartsWith("equip_"))
            {
                equipmentBuffs.Add(buff);
            }
        }

        // 장비 버프 제거
        foreach (var buff in equipmentBuffs)
        {
            RemoveBuff(buff);
        }
    }

    // 캐릭터 ID 설정
    public void SetCharacterId(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            characterId = id;
            // ID가 변경되면 장비 스탯 다시 적용
            ApplyEquipmentStats();
        }
    }

    // 캐릭터 ID 가져오기
    public string GetCharacterId()
    {
        return characterId;
    }

    // 장비 장착 이벤트 처리 - EquipmentManager에서 직접 호출할 수 있는 메서드
    public void OnEquipmentChanged()
    {
        ApplyEquipmentStats();
    }
}