using UnityEngine;

public class GameBalanceManager : MonoBehaviour
{
    public static GameBalanceManager instance;

    [Header("Player Balance")]
    public float playerBaseHealthGrowth = 1.1f;  // 레벨당 체력 증가율
    public float playerBaseAttackGrowth = 1.08f; // 레벨당 공격력 증가율

    [Header("Enemy Balance")]
    public float enemyHealthGrowth = 1.12f;      // 레벨당 적 체력 증가율
    public float enemyAttackGrowth = 1.07f;      // 레벨당 적 공격력 증가율
    public float bossHealthMultiplier = 5f;      // 보스 체력 배율
    public float bossAttackMultiplier = 2f;      // 보스 공격력 배율

    [Header("Economy Balance")]
    public int baseGoldDropMin = 10;             // 기본 골드 드롭 (최소)
    public int baseGoldDropMax = 20;             // 기본 골드 드롭 (최대)
    public float goldDropLevelMultiplier = 0.2f; // 레벨당 골드 증가율
    public float gemDropChance = 0.05f;          // 보석 드롭 확률
    public int gemDropAmount = 1;                // 보석 드롭량

    [Header("Idle Balance")]
    public float offlineEfficiencyMax = 0.7f;    // 최대 오프라인 효율
    public float vipOfflineBonus = 0.1f;         // VIP 오프라인 보너스

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // 플레이어 레벨에 따른 적정 스테이지 레벨 계산
    public int CalculateRecommendedStageLevel(int playerLevel)
    {
        return Mathf.Max(1, playerLevel - 3);
    }

    // 적 처치 시 획득 골드 계산
    public int CalculateGoldDrop(int enemyLevel, bool isBoss = false)
    {
        float levelMultiplier = 1f + (enemyLevel - 1) * goldDropLevelMultiplier;
        float baseGold = Random.Range(baseGoldDropMin, baseGoldDropMax + 1);
        float goldAmount = baseGold * levelMultiplier;

        if (isBoss)
            goldAmount *= 3f;

        return Mathf.RoundToInt(goldAmount);
    }

    // 레벨업에 필요한 경험치 계산
    public int CalculateExpToNextLevel(int currentLevel)
    {
        return 100 * currentLevel * currentLevel;
    }

    // 캐릭터 레벨업 비용 계산
    public int CalculateCharacterUpgradeCost(int currentLevel, int stars)
    {
        return 100 * currentLevel * stars;
    }

    // 장비 강화 비용 계산
    public int CalculateEquipmentUpgradeCost(int currentLevel, int rarity)
    {
        return 50 * currentLevel * rarity;
    }

    // 오프라인 진행률 계산
    public float CalculateOfflineEfficiency(bool isVip)
    {
        float efficiency = offlineEfficiencyMax;

        if (isVip)
            efficiency += vipOfflineBonus;

        return Mathf.Clamp01(efficiency);
    }

    // 스테이지 클리어 보상 계산
    public (int exp, int gold, int gems) CalculateStageRewards(int stageLevel, bool isBossStage)
    {
        int expReward = 50 * stageLevel;
        int goldReward = 100 * stageLevel;
        int gemReward = isBossStage ? Random.Range(1, 3) : 0;

        return (expReward, goldReward, gemReward);
    }
}