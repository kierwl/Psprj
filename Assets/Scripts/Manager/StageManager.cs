using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [Header("Stage References")]
    public StageSO currentStage;
    public Transform[] spawnPoints;
    public Transform bossSpawnPoint;

    [Header("Stage Runtime")]
    public bool isStageActive = false;
    public int enemiesKilled = 0;
    public int totalEnemies = 0;
    public bool bossDefeated = false;
    public float stageTimer = 0f;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private GameObject spawnedBoss;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // 스테이지 시작
    public void StartStage(StageSO stageData)
    {
        if (stageData == null)
        {
            Debug.LogError("Stage data is null!");
            return;
        }

        // 이전 스테이지 정리
        ClearStage();

        // 새 스테이지 시작
        currentStage = stageData;
        isStageActive = true;
        enemiesKilled = 0;
        bossDefeated = false;
        stageTimer = 0f;

        // 총 적 수 계산
        totalEnemies = 0;
        foreach (var spawn in currentStage.enemySpawns)
        {
            totalEnemies += spawn.count;
        }

        // UI 업데이트
        UpdateStageUI();

        // 적 스폰 시작
        spawnCoroutine = StartCoroutine(SpawnEnemies());

        // 환경 로드
        LoadStageEnvironment();

        Debug.Log($"스테이지 {currentStage.stageName} 시작!");
    }

    // 적 스폰 코루틴
    private IEnumerator SpawnEnemies()
    {
        foreach (var spawnData in currentStage.enemySpawns)
        {
            switch (spawnData.spawnMethod)
            {
                case StageSO.EnemySpawnData.SpawnMethod.AllAtOnce:
                    // 한번에 모두 스폰
                    for (int i = 0; i < spawnData.count; i++)
                    {
                        SpawnEnemy(spawnData.enemyData, spawnData.level);
                        yield return new WaitForSeconds(0.2f); // 간격 조절
                    }
                    break;

                case StageSO.EnemySpawnData.SpawnMethod.Sequential:
                    // 순차적으로 스폰
                    for (int i = 0; i < spawnData.count; i++)
                    {
                        SpawnEnemy(spawnData.enemyData, spawnData.level);
                        yield return new WaitForSeconds(spawnData.spawnInterval);
                    }
                    break;

                case StageSO.EnemySpawnData.SpawnMethod.Wave:
                    // 웨이브 형태로 스폰
                    int waveCount = Mathf.CeilToInt(spawnData.count / 3f);
                    for (int wave = 0; wave < waveCount; wave++)
                    {
                        int enemiesInWave = Mathf.Min(3, spawnData.count - (wave * 3));
                        for (int i = 0; i < enemiesInWave; i++)
                        {
                            SpawnEnemy(spawnData.enemyData, spawnData.level);
                            yield return new WaitForSeconds(0.2f);
                        }

                        if (wave < waveCount - 1)
                            yield return new WaitForSeconds(spawnData.spawnInterval);
                    }
                    break;
            }

            // 스폰 그룹 사이 간격
            yield return new WaitForSeconds(2f);
        }

        // 모든 적 처치 후, 보스가 있는 경우
        if (currentStage.hasBoss && currentStage.bossSpawn != null && currentStage.bossSpawn.bossData != null)
        {
            // 보스 등장 전 대기
            yield return new WaitForSeconds(currentStage.bossSpawn.spawnDelay);

            // 보스 등장 시 연출(있는 경우)
            if (!string.IsNullOrEmpty(currentStage.bossSpawn.introDialogue))
            {
                // 연출 처리 추가 (있는 경우)
                // DialogueManager.instance?.ShowDialogue(currentStage.bossSpawn.introDialogue);
                yield return new WaitForSeconds(2f);
            }

            // 보스 스폰
            SpawnBoss(currentStage.bossSpawn.bossData, currentStage.bossSpawn.level);
        }
    }

    // 일반 적 스폰
    private void SpawnEnemy(EnemySO enemyData, int level)
    {
        if (enemyData == null || enemyData.prefab == null) return;

        // 스폰 위치 선택
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 적 오브젝트 인스턴스화
        GameObject enemy = Instantiate(enemyData.prefab, spawnPoint.position, spawnPoint.rotation);

        // 적 컨트롤러 초기화
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.Initialize(enemyData, level);

            // 죽음 이벤트 연결
            enemyController.OnDeathEvent += OnEnemyKilled;
        }

        // 스폰된 적 목록에 추가
        spawnedEnemies.Add(enemy);
    }

    // 보스 스폰
    private void SpawnBoss(EnemySO bossData, int level)
    {
        if (bossData == null || bossData.prefab == null) return;

        // 보스 스폰 위치
        Transform spawnPoint = bossSpawnPoint != null ? bossSpawnPoint : spawnPoints[0];

        // 보스 오브젝트 인스턴스화
        spawnedBoss = Instantiate(bossData.prefab, spawnPoint.position, spawnPoint.rotation);

        // 보스 컨트롤러 초기화
        EnemyController bossController = spawnedBoss.GetComponent<EnemyController>();
        if (bossController != null)
        {
            bossController.Initialize(bossData, level);

            // 죽음 이벤트 연결
            bossController.OnDeathEvent += OnBossKilled;
        }
    }

    // 적 처치 이벤트
    private void OnEnemyKilled()
    {
        enemiesKilled++;

        // UI 업데이트
        UpdateStageUI();

        // 스테이지 클리어 조건 확인
        CheckStageClear();
    }

    // 보스 처치 이벤트
    private void OnBossKilled()
    {
        bossDefeated = true;

        // UI 업데이트
        UpdateStageUI();

        // 스테이지 클리어 조건 확인
        CheckStageClear();
    }

    // 스테이지 클리어 조건 확인
    private void CheckStageClear()
    {
        if (!isStageActive) return;

        // 클리어 조건 확인
        if (currentStage.CheckClearCondition(enemiesKilled, bossDefeated, stageTimer))
        {
            // 스테이지 클리어
            StageClear();
        }
    }

    // 스테이지 클리어 시 호출
    private void StageClear()
    {
        isStageActive = false;

        // 보상 계산
        int playerLevel = PlayerLevel.instance?.currentLevel ?? 1;
        int expReward = currentStage.GetScaledExpReward(playerLevel);
        int goldReward = currentStage.GetScaledGoldReward(playerLevel);
        int gems = 0;

        // 보석 확률 체크
        if (Random.value < currentStage.gemDropChance)
        {
            gems = currentStage.gemAmount;
        }

        // 보상 지급
        PlayerLevel.instance?.AddExperience(expReward);
        CurrencyManager.instance?.AddGold(goldReward);

        if (gems > 0)
            CurrencyManager.instance?.AddGems(gems);

        // 아이템 보상 처리
        GiveItemRewards();

        // UI 표시
        GameUIManager.instance?.ShowStageClear(
            currentStage.recommendedLevel,
            expReward,
            goldReward,
            gems
        );

        Debug.Log($"스테이지 {currentStage.stageName} 클리어! 보상: {expReward} XP, {goldReward} 골드, {gems} 보석");
    }

    // 스테이지 실패 시 호출
    public void StageFail()
    {
        isStageActive = false;

        // 스폰 코루틴 중지
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // UI 표시
        // GameUIManager.instance?.ShowStageFail();

        Debug.Log($"스테이지 {currentStage.stageName} 실패!");
    }

    // 스테이지 정리 시 호출
    public void ClearStage()
    {
        isStageActive = false;

        // 스폰 코루틴 중지
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // 스폰된 적 제거
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        spawnedEnemies.Clear();

        // 보스 제거
        if (spawnedBoss != null)
            Destroy(spawnedBoss);

        spawnedBoss = null;
    }

    // 아이템 보상 지급
    private void GiveItemRewards()
    {
        // 확정 아이템 지급
        foreach (var dropData in currentStage.guaranteedDrops)
        {
            if (dropData.item != null)
            {
                int amount = Random.Range(dropData.minAmount, dropData.maxAmount + 1);
                EquipmentData equipment = ConvertToEquipmentData(dropData.item);
                if (equipment != null)
                {
                    EquipmentManager.instance?.AddEquipment(equipment);
                }
            }
        }

        // 확률 아이템 지급
        foreach (var dropData in currentStage.possibleDrops)
        {
            if (dropData.item != null && Random.value <= dropData.dropChance)
            {
                int amount = Random.Range(dropData.minAmount, dropData.maxAmount + 1);
                EquipmentData equipment = ConvertToEquipmentData(dropData.item);
                if (equipment != null)
                {
                    EquipmentManager.instance?.AddEquipment(equipment);
                }
            }
        }
    }

    // ItemSO를 EquipmentData로 변환
    private EquipmentData ConvertToEquipmentData(ItemSO item)
    {
        if (item == null) return null;

        EquipmentData equipment = new EquipmentData
        {
            id = item.itemID,
            name = item.itemName,
            description = item.description,
            icon = item.icon,
            type = GetEquipmentType(item),
            rarity = GetEquipmentRarity(item)
        };

        // 아이템 타입에 따른 스탯 설정
        if (item is WeaponSO weapon)
        {
            equipment.attackBonus = weapon.attackDamage;
            equipment.attackBonusPerLevel = weapon.attackDamage * 0.1f;
        }
        else if (item is ArmorSO armor)
        {
            equipment.defenseBonus = armor.defense;
            equipment.healthBonus = armor.healthBonus;
            equipment.defenseBonusPerLevel = armor.defense * 0.08f;
            equipment.healthBonusPerLevel = armor.healthBonus * 0.08f;
        }

        return equipment;
    }

    // ItemSO에서 EquipmentType 추출
    private EquipmentType GetEquipmentType(ItemSO item)
    {
        if (item is WeaponSO weapon)
        {
            switch (weapon.weaponType)
            {
                case WeaponSO.WeaponType.Sword:
                case WeaponSO.WeaponType.Axe:
                    return EquipmentType.Weapon;
                case WeaponSO.WeaponType.Bow:
                    return EquipmentType.Weapon;
                case WeaponSO.WeaponType.Staff:
                case WeaponSO.WeaponType.Wand:
                    return EquipmentType.Weapon;
                default:
                    return EquipmentType.Weapon;
            }
        }
        else if (item is ArmorSO armor)
        {
            switch (armor.armorType)
            {
                case ArmorSO.ArmorType.Head:
                    return EquipmentType.Helmet;
                case ArmorSO.ArmorType.Chest:
                    return EquipmentType.Armor;
                case ArmorSO.ArmorType.Gloves:
                case ArmorSO.ArmorType.Boots:
                    return EquipmentType.Accessory;
                case ArmorSO.ArmorType.Shield:
                    return EquipmentType.Accessory;
                default:
                    return EquipmentType.Accessory;
            }
        }
        return EquipmentType.Accessory;
    }

    // ItemSO에서 EquipmentRarity 추출
    private EquipmentRarity GetEquipmentRarity(ItemSO item)
    {
        // 기본값은 Common으로 설정
        return EquipmentRarity.Common;
    }

    // 스테이지 환경 로드
    private void LoadStageEnvironment()
    {
        if (currentStage.stageEnvironment != null)
        {
            // 이전 환경 제거 (있다면)
            GameObject currentEnv = GameObject.FindGameObjectWithTag("StageEnvironment");
            if (currentEnv != null)
                Destroy(currentEnv);

            // 새 환경 로드
            Instantiate(currentStage.stageEnvironment, Vector3.zero, Quaternion.identity);
        }
    }

    // UI 업데이트
    private void UpdateStageUI()
    {
        // 스테이지 UI 매니저에 UI 업데이트
        // 예: 남은 적 수, 진행 상태, 타이머 등
    }

    private void Update()
    {
        if (isStageActive)
        {
            // 스테이지 타이머 업데이트
            stageTimer += Time.deltaTime;

            // 시간 제한이 있는 경우 체크
            if (currentStage.hasTimerLimit && stageTimer >= currentStage.timeLimitInSeconds)
            {
                // 시간 초과로 실패
                StageFail();
            }
        }
    }
}