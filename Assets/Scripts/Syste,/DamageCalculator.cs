using UnityEngine;

[System.Serializable]
public class DamageCalculator
{
    [Range(0, 100)]
    public float criticalChance = 10f;  // 기본 10% 크리티컬 확률

    [Range(1f, 3f)]
    public float criticalMultiplier = 1.5f;  // 크리티컬 데미지 배율

    // 데미지 계산 (기본 데미지, 공격자 레벨, 방어자 레벨)
    public float CalculateDamage(float baseDamage, int attackerLevel = 1, int defenderLevel = 1)
    {
        // 레벨 차이에 따른 데미지 보정
        float levelFactor = 1f + (attackerLevel - defenderLevel) * 0.05f;
        levelFactor = Mathf.Clamp(levelFactor, 0.5f, 2f);  // 최소 50%, 최대 200%

        // 기본 데미지 계산
        float damage = baseDamage * levelFactor;

        // 크리티컬 확인
        bool isCritical = Random.Range(0f, 100f) <= criticalChance;

        // 크리티컬이면 데미지 증가
        if (isCritical)
        {
            damage *= criticalMultiplier;
        }

        // 무작위 변동 (90% ~ 110%)
        float randomFactor = Random.Range(0.9f, 1.1f);
        damage *= randomFactor;

        return Mathf.Round(damage);
    }

    // 크리티컬 여부 반환
    public bool IsCritical()
    {
        return Random.Range(0f, 100f) <= criticalChance;
    }
}