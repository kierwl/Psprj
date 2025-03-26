using UnityEngine;
using System.Collections.Generic;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    [Header("Save Settings")]
    public float autoSaveInterval = 60f; // �ڵ� ���� ���� (��)

    private float nextAutoSaveTime;

    [System.Serializable]
    public class GameData
    {
        // �÷��̾� �⺻ ����
        public int playerLevel;
        public int playerExp;

        // �÷��̾� ���� ����
        public float playerMaxHealth;
        public float playerCurrentHealth;
        public float playerAttackPower;
        public float playerDefenseValue;
        public float playerMoveSpeed;
        public float playerAttackSpeed;
        public float playerCriticalChance;
        public float playerAttackRange;

        // ��ȭ ����
        public int gold;
        public int gems;
        public int energy;

        // ĳ���� ����
        public List<CharacterSaveData> characters = new List<CharacterSaveData>();

        // �κ��丮 ����
        public List<ItemSaveData> inventoryItems = new List<ItemSaveData>();

        // ��� ��ȭ ����
        public List<UpgradeSaveData> upgradeItems = new List<UpgradeSaveData>();

        // �߰� ���� ����
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
        public string itemId; // ������ ���� ID
        public int amount;    // ������ ����
        public int slotIndex; // �κ��丮 ���� �ε���
        public bool isEquipped; // ���� ����
    }

    [System.Serializable]
    public class UpgradeSaveData
    {
        public string itemId; // ������ ���� ID
        public int level;     // ��ȭ ����
        public int baseCost;  // �⺻ ��ȭ ���
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
        // ���� ���� �� ������ �ε�
        LoadGame();

        // �ڵ� ���� �ð� ����
        nextAutoSaveTime = Time.time + autoSaveInterval;
    }

    private void Update()
    {
        // �ڵ� ����
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

            // �÷��̾� ���� ������ ����
            if (PlayerLevel.instance != null)
            {
                data.playerLevel = PlayerLevel.instance.currentLevel;
                data.playerExp = PlayerLevel.instance.currentExp;
            }

            // �÷��̾� ���� ������ ����
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

                // ��� ���� ����
                data.gold = playerStats.GetGold();
            }

            // ��ȭ ������ ���� (CurrencyManager�� �ִ� ���)
            if (CurrencyManager.instance != null)
            {
                data.gems = CurrencyManager.instance.gems;
                data.energy = CurrencyManager.instance.energy;
            }

            // ĳ���� ������ ����
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

            // �κ��丮 ������ ����
            Inventory inventory = Inventory.instance;
            if (inventory != null && inventory.items != null)
            {
                foreach (var item in inventory.items)
                {
                    if (item != null && item.item != null)
                    {
                        ItemSaveData itemData = new ItemSaveData
                        {
                            itemId = item.item.name, // �������� ���� �ĺ��� (������ �̸� ���)
                            amount = item.amount,
                            slotIndex = item.slotIndex,
                            isEquipped = item.isEquipped
                        };

                        data.inventoryItems.Add(itemData);
                    }
                }
            }

            // ��ȭ ������ ����
            UpgradeManager upgradeManager = UpgradeManager.instance;
            if (upgradeManager != null && upgradeManager.upgradeableItems != null)
            {
                foreach (var upgradeItem in upgradeManager.upgradeableItems)
                {
                    if (upgradeItem != null && upgradeItem.item != null)
                    {
                        UpgradeSaveData upgradeData = new UpgradeSaveData
                        {
                            itemId = upgradeItem.item.name, // �������� ���� �ĺ���
                            level = upgradeItem.level,
                            baseCost = upgradeItem.baseCost
                        };

                        data.upgradeItems.Add(upgradeData);
                    }
                }
            }

            // ���� ���� ����
            PlayerController playerController = player?.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // �ڵ� ���� ���� ����
                data.autoBattleEnabled = playerController.GetAutoBattleEnabled();
            }
            else if (AutoBattleManager.instance != null)
            {
                data.autoBattleEnabled = AutoBattleManager.instance.autoBattleEnabled;
            }

            data.lastSaveDate = DateTime.Now.ToString();

            // Json���� ��ȯ �� ����
            string jsonData = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString("GameSaveData", jsonData);
            PlayerPrefs.Save();

            Debug.Log("���� ���� �Ϸ�: " + DateTime.Now.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("���� ���� ����: " + e.Message);
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

                // �÷��̾� ���� ������ �ε�
                if (PlayerLevel.instance != null)
                {
                    PlayerLevel.instance.currentLevel = data.playerLevel;
                    PlayerLevel.instance.currentExp = data.playerExp;
                    PlayerLevel.instance.UpdateLevelUI();
                }

                // �÷��̾� ���� ������ �ε�
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                PlayerStats playerStats = player?.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    // ���� ����
                    playerStats.SetMaxHealth(data.playerMaxHealth);
                    playerStats.RestoreHealth(data.playerCurrentHealth);
                    playerStats.SetAttackPower(data.playerAttackPower);
                    playerStats.SetDefenseValue(data.playerDefenseValue);
                    playerStats.SetMoveSpeed(data.playerMoveSpeed);
                    playerStats.SetAttackSpeed(data.playerAttackSpeed);
                    playerStats.SetCriticalChance(data.playerCriticalChance);
                    playerStats.SetAttackRange(data.playerAttackRange);

                    // ��� ����
                    // PlayerStats�� ���� �����ϴ� �޼��尡 �ִٸ� ���
                    if (data.gold > 0)
                    {
                        playerStats.AddGold(data.gold);
                    }
                }

                // ��ȭ ������ �ε� (CurrencyManager�� �ִ� ���)
                if (CurrencyManager.instance != null)
                {
                    CurrencyManager.instance.gems = data.gems;
                    CurrencyManager.instance.energy = data.energy;
                    CurrencyManager.instance.UpdateCurrencyUI();
                }

                // ĳ���� ������ �ε�
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
                    // ĳ���� �ٽ� ����
                    charManager.DespawnAllCharacters();
                    charManager.SpawnActiveCharacters();
                }

                // �κ��丮 ������ �ε�
                LoadInventoryData(data);

                // ��ȭ ������ �ε�
                LoadUpgradeData(data);

                // ���� ���� �ε�
                PlayerController playerController = player?.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.SetAutoBattle(data.autoBattleEnabled);
                }
                else if (AutoBattleManager.instance != null)
                {
                    AutoBattleManager.instance.autoBattleEnabled = data.autoBattleEnabled;
                }

                Debug.Log("���� �ε� �Ϸ�: " + data.lastSaveDate);
            }
            else
            {
                Debug.Log("����� ���� �����Ͱ� �����ϴ�. �� ������ �����մϴ�.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("���� �ε� ����: " + e.Message);
        }
    }

    private void LoadInventoryData(GameData data)
    {
        Inventory inventory = Inventory.instance;
        if (inventory != null && data.inventoryItems.Count > 0)
        {
            // �κ��丮 �ʱ�ȭ
            inventory.ClearInventory();

            // ������ �Ŵ����� ���� ������ ���� ã��
            ItemManager itemManager = FindObjectOfType<ItemManager>();

            foreach (var itemData in data.inventoryItems)
            {
                // ������ �Ŵ����� ������ �ش� ������ ã��
                ItemSO item = null;

                if (itemManager != null)
                {
                    item = itemManager.GetItemById(itemData.itemId);
                }

                // �������� ã�� ���� ��� �ٸ� ��� �õ�
                if (item == null)
                {
                    // ���ҽ� �������� ã�� �õ�
                    item = Resources.Load<ItemSO>("Items/" + itemData.itemId);

                    if (item == null)
                    {
                        Debug.LogWarning($"�������� ã�� �� �����ϴ�: {itemData.itemId}");
                        continue;
                    }
                }

                // ������ �߰�
                if (inventory.AddItem(item, itemData.amount))
                {
                    // ���� �ε��� ���� (���� ���� �Ǵ� ���� �޼��� ���)
                    Inventory.InventoryItem inventoryItem = inventory.GetItemAtSlot(0);
                    if (inventoryItem != null && inventoryItem.item == item)
                    {
                        inventory.SwapItems(0, itemData.slotIndex);
                    }

                    // ���� ���� ����
                    if (itemData.isEquipped)
                    {
                        inventory.EquipItem(itemData.slotIndex);
                    }
                }
            }

            // �κ��丮 UI ������Ʈ
            inventory.NotifyInventoryChanged();
        }
    }

    private void LoadUpgradeData(GameData data)
    {
        UpgradeManager upgradeManager = UpgradeManager.instance;
        if (upgradeManager != null && data.upgradeItems.Count > 0)
        {
            // ������ �Ŵ����� ���� ������ ���� ã��
            ItemManager itemManager = FindObjectOfType<ItemManager>();

            foreach (var upgradeData in data.upgradeItems)
            {
                // ������ �Ŵ����� ������ �ش� ������ ã��
                ItemSO item = null;

                if (itemManager != null)
                {
                    item = itemManager.GetItemById(upgradeData.itemId);
                }

                // �������� ã�� ���� ��� �ٸ� ��� �õ�
                if (item == null)
                {
                    // ���ҽ� �������� ã�� �õ�
                    item = Resources.Load<ItemSO>("Items/" + upgradeData.itemId);

                    if (item == null)
                    {
                        Debug.LogWarning($"��ȭ �������� ã�� �� �����ϴ�: {upgradeData.itemId}");
                        continue;
                    }
                }

                // ��ȭ ������ ���� (���׷��̵� �Ŵ����� �� ����� �ִٰ� ����)
                upgradeManager.SetItemLevel(item, upgradeData.level, upgradeData.baseCost);
            }
        }
    }

    // ���� ������ �ʱ�ȭ (�׽�Ʈ��)
    public void ResetGameData()
    {
        PlayerPrefs.DeleteKey("GameSaveData");
        PlayerPrefs.DeleteKey("LastLoginTimeBinary");
        PlayerPrefs.Save();
        Debug.Log("���� �����Ͱ� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    // �� �Ͻ� ���� �Ǵ� ���� �� �ڵ� ����
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