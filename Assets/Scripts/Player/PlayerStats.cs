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
    [SerializeField] private float criticalChance = 0.05f;
    [SerializeField] private int gold = 0;

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
        CriticalChance
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

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
            Die();
        }
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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

    private float GetBaseStatValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return maxHealth;
            case StatType.Attack: return attackPower;
            case StatType.Defense: return defenseValue;
            case StatType.MoveSpeed: return moveSpeed;
            case StatType.CriticalChance: return criticalChance;
            default: return 0;
        }
    }

    // 골드 관련 메서드
    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            return true;
        }
        return false;
    }

    public int GetGold()
    {
        return gold;
    }

    // 사망 처리
    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다.");
        // 사망 처리 로직...
    }

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
} 