using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [Header("Stage References")]
    public StageSO currentStage;
    public EnemySpawner enemySpawner;
    public Transform bossSpawnPoint;

    [Header("Stage Runtime")]
    public bool isStageActive = false;
    public int enemiesKilled = 0;
    public int totalEnemies = 0;
    public bool bossDefeated = false;
    public float stageTimer = 0f;

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
                        enemySpawner.SpawnEnemy(spawnData.enemyData, spawnData.level);
                        yield return new WaitForSeconds(0.2f);
                    }
                    break;

                case StageSO.EnemySpawnData.SpawnMethod.Sequential:
                    // 순차적으로 스폰
                    for (int i = 0; i < spawnData.count; i++)
                    {
                        enemySpawner.SpawnEnemy(spawnData.enemyData, spawnData.level);
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
                            enemySpawner.SpawnEnemy(spawnData.enemyData, spawnData.level);
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

    // 보스 스폰
    private void SpawnBoss(EnemySO bossData, int level)
    {
        if (bossData == null || bossData.prefab == null) return;

        // 보스 스폰 위치
        Transform spawnPoint = bossSpawnPoint != null ? bossSpawnPoint : enemySpawner.transform;

        // 보스 오브젝트 인스턴스화
        spawnedBoss = Instantiate(bossData.prefab, spawnPoint.position, spawnPoint.rotation);

        // 보스 컨트롤러 초기화
        EnemyController bossController = spawnedBoss.GetComponent<EnemyController>();
        if (bossController != null)
        {
            bossController.Initialize(bossData, level);
            bossController.OnDeathEvent += OnBossKilled;
        }
    }

    // 적 처치 이벤트
    private void OnEnemyKilled()
    {
        enemiesKilled++;
        UpdateStageUI();
        CheckStageClear();
    }

    // 보스 처치 이벤트
    private void OnBossKilled()
    {
        bossDefeated = true;
        UpdateStageUI();
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

        // PlayerStats를 통해 골드 지급 (CurrencyManager 대신)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerStats playerStats = player?.GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            playerStats.AddGold(goldReward);
            Debug.Log($"플레이어 스탯 시스템을 통해 {goldReward} 골드를 지급했습니다.");
        }
        else if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.AddGold(goldReward);
        }

        if (gems > 0)
        {
            if (CurrencyManager.instance != null)
                CurrencyManager.instance.AddGems(gems);
        }

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
        enemySpawner.ClearAllEnemies();

        // 보스 제거
        if (spawnedBoss != null)
            Destroy(spawnedBoss);

        spawnedBoss = null;
    }

    // 아이템 보상 지급
    private void GiveItemRewards()
    {
        // 새로운 ItemManager를 사용한 아이템 처리
        ItemManager itemManager = ItemManager.instance;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Inventory inventory = Inventory.instance;

        // 확정 아이템 지급
        foreach (var dropData in currentStage.guaranteedDrops)
        {
            if (dropData.item != null)
            {
                int amount = Random.Range(dropData.minAmount, dropData.maxAmount + 1);

                // 인벤토리 시스템 사용 (우선)
                if (inventory != null)
                {
                    inventory.AddItem(dropData.item, amount);
                    Debug.Log($"인벤토리에 {dropData.item.itemName} x{amount}을(를) 추가했습니다.");

                    // 플레이어 스탯 업데이트 (아이템으로 인한 스탯 변화 반영)
                    if (player != null)
                    {
                        PlayerStats playerStats = player.GetComponent<PlayerStats>();
                        if (playerStats != null)
                        {
                            playerStats.ApplyInventoryEquipment();
                        }
                    }
                }
                // 기존 EquipmentManager 사용 (대체)
                else if (EquipmentManager.instance != null)
                {
                    EquipmentData equipment = ConvertToEquipmentData(dropData.item);
                    if (equipment != null)
                    {
                        EquipmentManager.instance.AddEquipment(equipment);
                        Debug.Log($"EquipmentManager에 {equipment.name}을(를) 추가했습니다.");
                    }
                }

                // 아이템 필드 드롭 효과 (선택적)
                if (itemManager != null && player != null)
                {
                    Vector3 dropPosition = player.transform.position + new Vector3(Random.Range(-2f, 2f), 0.5f, Random.Range(-2f, 2f));
                    itemManager.DropItem(dropData.item, dropPosition, amount);
                }
            }
        }

        // 확률 아이템 지급
        foreach (var dropData in currentStage.possibleDrops)
        {
            if (dropData.item != null && Random.value <= dropData.dropChance)
            {
                int amount = Random.Range(dropData.minAmount, dropData.maxAmount + 1);

                // 인벤토리 시스템 사용 (우선)
                if (inventory != null)
                {
                    inventory.AddItem(dropData.item, amount);
                    Debug.Log($"인벤토리에 {dropData.item.itemName} x{amount}을(를) 추가했습니다.");

                    // 플레이어 스탯 업데이트 (아이템으로 인한 스탯 변화 반영)
                    if (player != null)
                    {
                        PlayerStats playerStats = player.GetComponent<PlayerStats>();
                        if (playerStats != null)
                        {
                            playerStats.ApplyInventoryEquipment();
                        }
                    }
                }
                // 기존 EquipmentManager 사용 (대체)
                else if (EquipmentManager.instance != null)
                {
                    EquipmentData equipment = ConvertToEquipmentData(dropData.item);
                    if (equipment != null)
                    {
                        EquipmentManager.instance.AddEquipment(equipment);
                        Debug.Log($"EquipmentManager에 {equipment.name}을(를) 추가했습니다.");
                    }
                }

                // 아이템 필드 드롭 효과 (선택적)
                if (itemManager != null && player != null)
                {
                    Vector3 dropPosition = player.transform.position + new Vector3(Random.Range(-2f, 2f), 0.5f, Random.Range(-2f, 2f));
                    itemManager.DropItem(dropData.item, dropPosition, amount);
                }
            }
        }

        // 장비 효과 적용 (EquipmentManager 사용 시)
        if (EquipmentManager.instance != null && player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.ApplyEquipmentStats();
            }
        }
    }

    // ItemSO를 EquipmentData로 변환 (기존 EquipmentManager 지원용)
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
            equipment.baseAttackBonus = weapon.attackDamage;
            equipment.statGrowthRate = 0.1f; // 레벨당 10% 증가
        }
        else if (item is ArmorSO armor)
        {
            equipment.baseDefenseBonus = armor.defense;
            equipment.baseHealthBonus = armor.healthBonus;
            equipment.statGrowthRate = 0.08f; // 레벨당 8% 증가
        }
        else if (item is AccessorySO accessory)
        {
            // 액세서리 특수 스탯
            equipment.baseHealthBonus = accessory.healthBonus;
            equipment.speedBonus = accessory.moveSpeedBonus;
            // 추가 특성은 필요에 따라 확장
        }

        return equipment;
    }

    // ItemSO에서 EquipmentType 추출
    private EquipmentType GetEquipmentType(ItemSO item)
    {
        if (item is WeaponSO)
        {
            return EquipmentType.Weapon;
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
                    return EquipmentType.Gloves;
                case ArmorSO.ArmorType.Boots:
                    return EquipmentType.Boots;
                default:
                    return EquipmentType.Armor;
            }
        }
        else if (item is AccessorySO)
        {
            return EquipmentType.Accessory;
        }

        return EquipmentType.Accessory; // 기본값
    }

    // ItemSO에서 EquipmentRarity 추출
    private EquipmentRarity GetEquipmentRarity(ItemSO item)
    {
        switch (item.rarity)
        {
            case ItemSO.ItemRarity.Common:
                return EquipmentRarity.Common;
            case ItemSO.ItemRarity.Uncommon:
                return EquipmentRarity.Uncommon;
            case ItemSO.ItemRarity.Rare:
                return EquipmentRarity.Rare;
            case ItemSO.ItemRarity.Epic:
                return EquipmentRarity.Epic;
            case ItemSO.ItemRarity.Legendary:
                return EquipmentRarity.Legendary;
            default:
                return EquipmentRarity.Common;
        }
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
        if (GameUIManager.instance != null)
        {
            GameUIManager.instance.UpdateStageProgress(enemiesKilled, totalEnemies, stageTimer);
        }
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

            // 주기적 UI 업데이트 (필요시)
            if (Time.frameCount % 30 == 0) // 약 0.5초마다 업데이트
            {
                UpdateStageUI();
            }
        }
    }
}