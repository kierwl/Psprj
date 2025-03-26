using UnityEngine;
using System.Collections.Generic;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    [Header("Save Settings")]
    public float autoSaveInterval = 60f; // 자동 저장 간격 (초)

    private float nextAutoSaveTime;

    [System.Serializable]
    public class GameData
    {
        // 플레이어 기본 정보
        public int playerLevel;
        public int playerExp;

        // 플레이어 스탯 정보
        public float playerMaxHealth;
        public float playerCurrentHealth;
        public float playerAttackPower;
        public float playerDefenseValue;
        public float playerMoveSpeed;
        public float playerAttackSpeed;
        public float playerCriticalChance;
        public float playerAttackRange;

        // 통화 정보
        public int gold;
        public int gems;
        public int energy;

        // 캐릭터 정보
        public List<CharacterSaveData> characters = new List<CharacterSaveData>();

        // 인벤토리 정보
        public List<ItemSaveData> inventoryItems = new List<ItemSaveData>();

        // 장비 강화 정보
        public List<UpgradeSaveData> upgradeItems = new List<UpgradeSaveData>();

        // 추가 게임 설정
        public bool autoBattleEnabled;
        public string lastSaveDate;
    }

    [System.Serializable]
    public class CharacterSaveData
    {
        public string name;
        public int level;
        public bool isUnlocked;
        public bool isActive;
    }

    [System.Serializable]
    public class ItemSaveData
    {
        public string itemId; // 아이템 고유 ID
        public int amount;    // 아이템 수량
        public int slotIndex; // 인벤토리 슬롯 인덱스
        public bool isEquipped; // 장착 여부
    }

    [System.Serializable]
    public class UpgradeSaveData
    {
        public string itemId; // 아이템 고유 ID
        public int level;     // 강화 레벨
        public int baseCost;  // 기본 강화 비용
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 게임 시작 시 데이터 로드
        LoadGame();

        // 자동 저장 시간 설정
        nextAutoSaveTime = Time.time + autoSaveInterval;
    }

    private void Update()
    {
        // 자동 저장
        if (Time.time >= nextAutoSaveTime)
        {
            SaveGame();
            nextAutoSaveTime = Time.time + autoSaveInterval;
        }
    }

    public void SaveGame()
    {
        try
        {
            GameData data = new GameData();

            // 플레이어 레벨 데이터 저장
            if (PlayerLevel.instance != null)
            {
                data.playerLevel = PlayerLevel.instance.currentLevel;
                data.playerExp = PlayerLevel.instance.currentExp;
            }

            // 플레이어 스탯 데이터 저장
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            PlayerStats playerStats = player?.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                data.playerMaxHealth = playerStats.GetMaxHealth();
                data.playerCurrentHealth = playerStats.GetCurrentHealth();
                data.playerAttackPower = playerStats.GetAttackPower();
                data.playerDefenseValue = playerStats.GetDefenseValue();
                data.playerMoveSpeed = playerStats.GetStat(PlayerStats.StatType.MoveSpeed);
                data.playerAttackSpeed = playerStats.GetStat(PlayerStats.StatType.AttackSpeed);
                data.playerCriticalChance = playerStats.GetStat(PlayerStats.StatType.CriticalChance);
                data.playerAttackRange = playerStats.GetAttackRange();

                // 골드 정보 저장
                data.gold = playerStats.GetGold();
            }

            // 통화 데이터 저장 (CurrencyManager가 있는 경우)
            if (CurrencyManager.instance != null)
            {
                data.gems = CurrencyManager.instance.gems;
                data.energy = CurrencyManager.instance.energy;
            }

            // 캐릭터 데이터 저장
            CharacterManager charManager = CharacterManager.instance;
            if (charManager != null)
            {
                foreach (var character in charManager.characters)
                {
                    CharacterSaveData charData = new CharacterSaveData
                    {
                        name = character.name,
                        level = character.level,
                        isUnlocked = character.isUnlocked,
                        isActive = character.isActive
                    };

                    data.characters.Add(charData);
                }
            }

            // 인벤토리 데이터 저장
            Inventory inventory = Inventory.instance;
            if (inventory != null && inventory.items != null)
            {
                foreach (var item in inventory.items)
                {
                    if (item != null && item.item != null)
                    {
                        ItemSaveData itemData = new ItemSaveData
                        {
                            itemId = item.item.name, // 아이템의 고유 식별자 (프리팹 이름 사용)
                            amount = item.amount,
                            slotIndex = item.slotIndex,
                            isEquipped = item.isEquipped
                        };

                        data.inventoryItems.Add(itemData);
                    }
                }
            }

            // 강화 데이터 저장
            UpgradeManager upgradeManager = UpgradeManager.instance;
            if (upgradeManager != null && upgradeManager.upgradeableItems != null)
            {
                foreach (var upgradeItem in upgradeManager.upgradeableItems)
                {
                    if (upgradeItem != null && upgradeItem.item != null)
                    {
                        UpgradeSaveData upgradeData = new UpgradeSaveData
                        {
                            itemId = upgradeItem.item.name, // 아이템의 고유 식별자
                            level = upgradeItem.level,
                            baseCost = upgradeItem.baseCost
                        };

                        data.upgradeItems.Add(upgradeData);
                    }
                }
            }

            // 게임 설정 저장
            PlayerController playerController = player?.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // 자동 전투 설정 저장
                data.autoBattleEnabled = playerController.GetAutoBattleEnabled();
            }
            else if (AutoBattleManager.instance != null)
            {
                data.autoBattleEnabled = AutoBattleManager.instance.autoBattleEnabled;
            }

            data.lastSaveDate = DateTime.Now.ToString();

            // Json으로 변환 및 저장
            string jsonData = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString("GameSaveData", jsonData);
            PlayerPrefs.Save();

            Debug.Log("게임 저장 완료: " + DateTime.Now.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("게임 저장 실패: " + e.Message);
        }
    }

    public void LoadGame()
    {
        try
        {
            if (PlayerPrefs.HasKey("GameSaveData"))
            {
                string jsonData = PlayerPrefs.GetString("GameSaveData");
                GameData data = JsonUtility.FromJson<GameData>(jsonData);

                // 플레이어 레벨 데이터 로드
                if (PlayerLevel.instance != null)
                {
                    PlayerLevel.instance.currentLevel = data.playerLevel;
                    PlayerLevel.instance.currentExp = data.playerExp;
                    PlayerLevel.instance.UpdateLevelUI();
                }

                // 플레이어 스탯 데이터 로드
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                PlayerStats playerStats = player?.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    // 스탯 설정
                    playerStats.SetMaxHealth(data.playerMaxHealth);
                    playerStats.RestoreHealth(data.playerCurrentHealth);
                    playerStats.SetAttackPower(data.playerAttackPower);
                    playerStats.SetDefenseValue(data.playerDefenseValue);
                    playerStats.SetMoveSpeed(data.playerMoveSpeed);
                    playerStats.SetAttackSpeed(data.playerAttackSpeed);
                    playerStats.SetCriticalChance(data.playerCriticalChance);
                    playerStats.SetAttackRange(data.playerAttackRange);

                    // 골드 설정
                    // PlayerStats에 직접 설정하는 메서드가 있다면 사용
                    if (data.gold > 0)
                    {
                        playerStats.AddGold(data.gold);
                    }
                }

                // 통화 데이터 로드 (CurrencyManager가 있는 경우)
                if (CurrencyManager.instance != null)
                {
                    CurrencyManager.instance.gems = data.gems;
                    CurrencyManager.instance.energy = data.energy;
                    CurrencyManager.instance.UpdateCurrencyUI();
                }

                // 캐릭터 데이터 로드
                CharacterManager charManager = CharacterManager.instance;
                if (charManager != null && data.characters.Count > 0)
                {
                    for (int i = 0; i < Mathf.Min(charManager.characters.Count, data.characters.Count); i++)
                    {
                        charManager.characters[i].level = data.characters[i].level;
                        charManager.characters[i].isUnlocked = data.characters[i].isUnlocked;
                        charManager.characters[i].isActive = data.characters[i].isActive;
                    }

                    charManager.InitializeCharacterUI();
                    // 캐릭터 다시 스폰
                    charManager.DespawnAllCharacters();
                    charManager.SpawnActiveCharacters();
                }

                // 인벤토리 데이터 로드
                LoadInventoryData(data);

                // 강화 데이터 로드
                LoadUpgradeData(data);

                // 게임 설정 로드
                PlayerController playerController = player?.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.SetAutoBattle(data.autoBattleEnabled);
                }
                else if (AutoBattleManager.instance != null)
                {
                    AutoBattleManager.instance.autoBattleEnabled = data.autoBattleEnabled;
                }

                Debug.Log("게임 로드 완료: " + data.lastSaveDate);
            }
            else
            {
                Debug.Log("저장된 게임 데이터가 없습니다. 새 게임을 시작합니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("게임 로드 실패: " + e.Message);
        }
    }

    private void LoadInventoryData(GameData data)
    {
        Inventory inventory = Inventory.instance;
        if (inventory != null && data.inventoryItems.Count > 0)
        {
            // 인벤토리 초기화
            inventory.ClearInventory();

            // 아이템 매니저를 통해 아이템 참조 찾기
            ItemManager itemManager = FindObjectOfType<ItemManager>();

            foreach (var itemData in data.inventoryItems)
            {
                // 아이템 매니저가 있으면 해당 아이템 찾기
                ItemSO item = null;

                if (itemManager != null)
                {
                    item = itemManager.GetItemById(itemData.itemId);
                }

                // 아이템을 찾지 못한 경우 다른 방법 시도
                if (item == null)
                {
                    // 리소스 폴더에서 찾기 시도
                    item = Resources.Load<ItemSO>("Items/" + itemData.itemId);

                    if (item == null)
                    {
                        Debug.LogWarning($"아이템을 찾을 수 없습니다: {itemData.itemId}");
                        continue;
                    }
                }

                // 아이템 추가
                if (inventory.AddItem(item, itemData.amount))
                {
                    // 슬롯 인덱스 설정 (직접 접근 또는 스왑 메서드 사용)
                    Inventory.InventoryItem inventoryItem = inventory.GetItemAtSlot(0);
                    if (inventoryItem != null && inventoryItem.item == item)
                    {
                        inventory.SwapItems(0, itemData.slotIndex);
                    }

                    // 장착 상태 설정
                    if (itemData.isEquipped)
                    {
                        inventory.EquipItem(itemData.slotIndex);
                    }
                }
            }

            // 인벤토리 UI 업데이트
            inventory.NotifyInventoryChanged();
        }
    }

    private void LoadUpgradeData(GameData data)
    {
        UpgradeManager upgradeManager = UpgradeManager.instance;
        if (upgradeManager != null && data.upgradeItems.Count > 0)
        {
            // 아이템 매니저를 통해 아이템 참조 찾기
            ItemManager itemManager = FindObjectOfType<ItemManager>();

            foreach (var upgradeData in data.upgradeItems)
            {
                // 아이템 매니저가 있으면 해당 아이템 찾기
                ItemSO item = null;

                if (itemManager != null)
                {
                    item = itemManager.GetItemById(upgradeData.itemId);
                }

                // 아이템을 찾지 못한 경우 다른 방법 시도
                if (item == null)
                {
                    // 리소스 폴더에서 찾기 시도
                    item = Resources.Load<ItemSO>("Items/" + upgradeData.itemId);

                    if (item == null)
                    {
                        Debug.LogWarning($"강화 아이템을 찾을 수 없습니다: {upgradeData.itemId}");
                        continue;
                    }
                }

                // 강화 데이터 설정 (업그레이드 매니저에 이 기능이 있다고 가정)
                upgradeManager.SetItemLevel(item, upgradeData.level, upgradeData.baseCost);
            }
        }
    }

    // 게임 데이터 초기화 (테스트용)
    public void ResetGameData()
    {
        PlayerPrefs.DeleteKey("GameSaveData");
        PlayerPrefs.DeleteKey("LastLoginTimeBinary");
        PlayerPrefs.Save();
        Debug.Log("게임 데이터가 초기화되었습니다.");
    }

    // 앱 일시 정지 또는 종료 시 자동 저장
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}