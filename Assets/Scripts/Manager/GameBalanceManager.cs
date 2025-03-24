using UnityEngine;

public class GameBalanceManager : MonoBehaviour
{
    public static GameBalanceManager instance;

    [Header("Player Balance")]
    public float playerBaseHealthGrowth = 1.1f;  // ������ ü�� ������
    public float playerBaseAttackGrowth = 1.08f; // ������ ���ݷ� ������

    [Header("Enemy Balance")]
    public float enemyHealthGrowth = 1.12f;      // ������ �� ü�� ������
    public float enemyAttackGrowth = 1.07f;      // ������ �� ���ݷ� ������
    public float bossHealthMultiplier = 5f;      // ���� ü�� ����
    public float bossAttackMultiplier = 2f;      // ���� ���ݷ� ����

    [Header("Economy Balance")]
    public int baseGoldDropMin = 10;             // �⺻ ��� ��� (�ּ�)
    public int baseGoldDropMax = 20;             // �⺻ ��� ��� (�ִ�)
    public float goldDropLevelMultiplier = 0.2f; // ������ ��� ������
    public float gemDropChance = 0.05f;          // ���� ��� Ȯ��
    public int gemDropAmount = 1;                // ���� ��ӷ�

    [Header("Idle Balance")]
    public float offlineEfficiencyMax = 0.7f;    // �ִ� �������� ȿ��
    public float vipOfflineBonus = 0.1f;         // VIP �������� ���ʽ�

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // �÷��̾� ������ ���� ���� �������� ���� ���
    public int CalculateRecommendedStageLevel(int playerLevel)
    {
        return Mathf.Max(1, playerLevel - 3);
    }

    // �� óġ �� ȹ�� ��� ���
    public int CalculateGoldDrop(int enemyLevel, bool isBoss = false)
    {
        float levelMultiplier = 1f + (enemyLevel - 1) * goldDropLevelMultiplier;
        float baseGold = Random.Range(baseGoldDropMin, baseGoldDropMax + 1);
        float goldAmount = baseGold * levelMultiplier;

        if (isBoss)
            goldAmount *= 3f;

        return Mathf.RoundToInt(goldAmount);
    }

    // �������� �ʿ��� ����ġ ���
    public int CalculateExpToNextLevel(int currentLevel)
    {
        return 100 * currentLevel * currentLevel;
    }

    // ĳ���� ������ ��� ���
    public int CalculateCharacterUpgradeCost(int currentLevel, int stars)
    {
        return 100 * currentLevel * stars;
    }

    // ��� ��ȭ ��� ���
    public int CalculateEquipmentUpgradeCost(int currentLevel, int rarity)
    {
        return 50 * currentLevel * rarity;
    }

    // �������� ����� ���
    public float CalculateOfflineEfficiency(bool isVip)
    {
        float efficiency = offlineEfficiencyMax;

        if (isVip)
            efficiency += vipOfflineBonus;

        return Mathf.Clamp01(efficiency);
    }

    // �������� Ŭ���� ���� ���
    public (int exp, int gold, int gems) CalculateStageRewards(int stageLevel, bool isBossStage)
    {
        int expReward = 50 * stageLevel;
        int goldReward = 100 * stageLevel;
        int gemReward = isBossStage ? Random.Range(1, 3) : 0;

        return (expReward, goldReward, gemReward);
    }
}