using UnityEngine;

[System.Serializable]
public class DamageCalculator
{
    [Range(0, 100)]
    public float criticalChance = 10f;  // �⺻ 10% ũ��Ƽ�� Ȯ��

    [Range(1f, 3f)]
    public float criticalMultiplier = 1.5f;  // ũ��Ƽ�� ������ ����

    // ������ ��� (�⺻ ������, ������ ����, ����� ����)
    public float CalculateDamage(float baseDamage, int attackerLevel = 1, int defenderLevel = 1)
    {
        // ���� ���̿� ���� ������ ����
        float levelFactor = 1f + (attackerLevel - defenderLevel) * 0.05f;
        levelFactor = Mathf.Clamp(levelFactor, 0.5f, 2f);  // �ּ� 50%, �ִ� 200%

        // �⺻ ������ ���
        float damage = baseDamage * levelFactor;

        // ũ��Ƽ�� Ȯ��
        bool isCritical = Random.Range(0f, 100f) <= criticalChance;

        // ũ��Ƽ���̸� ������ ����
        if (isCritical)
        {
            damage *= criticalMultiplier;
        }

        // ������ ���� (90% ~ 110%)
        float randomFactor = Random.Range(0.9f, 1.1f);
        damage *= randomFactor;

        return Mathf.Round(damage);
    }

    // ũ��Ƽ�� ���� ��ȯ
    public bool IsCritical()
    {
        return Random.Range(0f, 100f) <= criticalChance;
    }
}