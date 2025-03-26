using UnityEngine;

[System.Serializable]
public class DamageCalculator
{
    [Header("������ ����")]
    public float damageVariance = 0.2f; // ������ ���� ���� (��20%)
    public float criticalChance = 0.05f; // �⺻ ũ��Ƽ�� Ȯ�� (5%)
    public float criticalMultiplier = 1.5f; // ũ��Ƽ�� ������ ���� (150%)

    // ������ ��� (�⺻ ����)
    public float CalculateDamage(float baseDamage)
    {
        // �⺻ �������� ����ġ ����
        float variance = Random.Range(-damageVariance, damageVariance);
        float damage = baseDamage * (1 + variance);

        // ũ��Ƽ�� ����
        if (IsCritical())
        {
            damage *= criticalMultiplier;
        }

        return Mathf.Max(1f, Mathf.Round(damage)); // �ּ� 1�� ������ ����
    }

    // ũ��Ƽ�� ���� Ȯ��
    public bool IsCritical()
    {
        return Random.value <= criticalChance;
    }

    // ������ ��� (ũ��Ƽ�� ���� ��ȯ ����)
    public (float damage, bool isCritical) CalculateDamageWithCritical(float baseDamage)
    {
        // �⺻ �������� ����ġ ����
        float variance = Random.Range(-damageVariance, damageVariance);
        float damage = baseDamage * (1 + variance);

        // ũ��Ƽ�� ����
        bool isCritical = IsCritical();
        if (isCritical)
        {
            damage *= criticalMultiplier;
        }

        return (Mathf.Max(1f, Mathf.Round(damage)), isCritical); // �ּ� 1�� ������ ����
    }

    // ������ ����� ������ ���
    public float CalculateDamageWithDefense(float baseDamage, float defense)
    {
        // ���� ���� ���� (������ �������� ������ ����)
        float damageReduction = defense / (defense + 100f); // ���� 100���� 50% ����
        float reducedDamage = baseDamage * (1f - damageReduction);

        // �⺻ �������� ����ġ ����
        float variance = Random.Range(-damageVariance, damageVariance);
        float damage = reducedDamage * (1 + variance);

        // ũ��Ƽ�� ����
        if (IsCritical())
        {
            damage *= criticalMultiplier;
        }

        return Mathf.Max(1f, Mathf.Round(damage)); // �ּ� 1�� ������ ����
    }
}