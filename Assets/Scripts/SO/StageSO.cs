using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Stage", menuName = "Game/Stages/Stage Data")]
public class StageSO : ScriptableObject
{
    [Header("Basic Info")]
    public string stageID;
    public string stageName;
    [TextArea(2, 5)]
    public string description;
    public Sprite stageIcon;
    public GameObject stageEnvironment;

    [Header("Stage Properties")]
    public int recommendedLevel;
    public int staminaCost = 10;
    public float timeLimitInSeconds = 180f;
    public bool hasTimerLimit = false;

    [Header("Spawns")]
    public List<EnemySpawnData> enemySpawns = new List<EnemySpawnData>();
    public BossSpawnData bossSpawn;
    public bool hasBoss = false;

    [Header("Rewards")]
    public int baseExpReward;
    public int baseGoldReward;
    public float gemDropChance = 0.1f;
    public int gemAmount = 1;
    public List<ItemDropData> guaranteedDrops = new List<ItemDropData>();
    public List<ItemDropData> possibleDrops = new List<ItemDropData>();

    [Header("Special Properties")]
    public bool isSpecialStage = false;
    public bool isEventStage = false;
    public bool isUnlocked = false;
    public string unlockCondition;

    // 적 스폰 데이터
    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemySO enemyData;
        public int count = 3;
        public int level = 1;
        public float spawnInterval = 2f;
        public SpawnMethod spawnMethod = SpawnMethod.AllAtOnce;

        public enum SpawnMethod
        {
            AllAtOnce,
            Sequential,
            Wave
        }
    }

    // 보스 스폰 데이터
    [System.Serializable]
    public class BossSpawnData
    {
        public EnemySO bossData;
        public int level = 1;
        public float spawnDelay = 5f;
        public string introDialogue;
    }

    // 아이템 드롭 데이터
    [System.Serializable]
    public class ItemDropData
    {
        public ItemSO item;
        public float dropChance = 1f;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    // 스테이지 클리어 조건 확인 메서드
    public bool CheckClearCondition(int killedEnemies, bool bossDefeated, float elapsedTime)
    {
        // 일반 적 처치 조건
        int totalEnemies = 0;
        foreach (var spawn in enemySpawns)
        {
            totalEnemies += spawn.count;
        }

        bool enemiesCleared = killedEnemies >= totalEnemies;

        // 보스 처치 조건
        bool bossCleared = !hasBoss || bossDefeated;

        // 시간 제한 조건
        bool timeCondition = !hasTimerLimit || elapsedTime <= timeLimitInSeconds;

        return enemiesCleared && bossCleared && timeCondition;
    }

    // 레벨 스케일링된 보상 계산
    public int GetScaledExpReward(int playerLevel)
    {
        float levelDiff = Mathf.Max(0, recommendedLevel - playerLevel);
        float multiplier = 1f + levelDiff * 0.1f;
        return Mathf.RoundToInt(baseExpReward * multiplier);
    }

    public int GetScaledGoldReward(int playerLevel)
    {
        float levelDiff = Mathf.Max(0, recommendedLevel - playerLevel);
        float multiplier = 1f + levelDiff * 0.05f;
        return Mathf.RoundToInt(baseGoldReward * multiplier);
    }
}