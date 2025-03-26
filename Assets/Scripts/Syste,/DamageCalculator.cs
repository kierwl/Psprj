using UnityEngine;

[System.Serializable]
public class DamageCalculator
{
    [Header("데미지 설정")]
    public float damageVariance = 0.2f; // 데미지 변동 범위 (±20%)
    public float criticalChance = 0.05f; // 기본 크리티컬 확률 (5%)
    public float criticalMultiplier = 1.5f; // 크리티컬 데미지 배율 (150%)

    // 데미지 계산 (기본 버전)
    public float CalculateDamage(float baseDamage)
    {
        // 기본 데미지에 변동치 적용
        float variance = Random.Range(-damageVariance, damageVariance);
        float damage = baseDamage * (1 + variance);

        // 크리티컬 적용
        if (IsCritical())
        {
            damage *= criticalMultiplier;
        }

        return Mathf.Max(1f, Mathf.Round(damage)); // 최소 1의 데미지 보장
    }

    // 크리티컬 여부 확인
    public bool IsCritical()
    {
        return Random.value <= criticalChance;
    }

    // 데미지 계산 (크리티컬 여부 반환 버전)
    public (float damage, bool isCritical) CalculateDamageWithCritical(float baseDamage)
    {
        // 기본 데미지에 변동치 적용
        float variance = Random.Range(-damageVariance, damageVariance);
        float damage = baseDamage * (1 + variance);

        // 크리티컬 적용
        bool isCritical = IsCritical();
        if (isCritical)
        {
            damage *= criticalMultiplier;
        }

        return (Mathf.Max(1f, Mathf.Round(damage)), isCritical); // 최소 1의 데미지 보장
    }

    // 방어력을 고려한 데미지 계산
    public float CalculateDamageWithDefense(float baseDamage, float defense)
    {
        // 방어력 적용 공식 (방어력이 높을수록 데미지 감소)
        float damageReduction = defense / (defense + 100f); // 방어력 100에서 50% 감소
        float reducedDamage = baseDamage * (1f - damageReduction);

        // 기본 데미지에 변동치 적용
        float variance = Random.Range(-damageVariance, damageVariance);
        float damage = reducedDamage * (1 + variance);

        // 크리티컬 적용
        if (IsCritical())
        {
            damage *= criticalMultiplier;
        }

        return Mathf.Max(1f, Mathf.Round(damage)); // 최소 1의 데미지 보장
    }
}