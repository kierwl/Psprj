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
        public float playerHealth;

        // ��ȭ ����
        public int gold;
        public int gems;
        public int energy;

        // ĳ���� ����
        public List<CharacterSaveData> characters = new List<CharacterSaveData>();

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

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
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

            // �÷��̾� ������ ����
            data.playerLevel = PlayerLevel.instance?.currentLevel ?? 1;
            data.playerExp = PlayerLevel.instance?.currentExp ?? 0;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            PlayerController playerController = player?.GetComponent<PlayerController>();
            if (playerController != null)
            {
                data.playerHealth = playerController.health;
            }

            // ��ȭ ������ ����
            data.gold = CurrencyManager.instance?.gold ?? 0;
            data.gems = CurrencyManager.instance?.gems ?? 0;
            data.energy = CurrencyManager.instance?.energy ?? 100;

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

            // ���� ���� ����
            data.autoBattleEnabled = AutoBattleManager.instance?.autoBattleEnabled ?? true;
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

                // �÷��̾� ������ �ε�
                if (PlayerLevel.instance != null)
                {
                    PlayerLevel.instance.currentLevel = data.playerLevel;
                    PlayerLevel.instance.currentExp = data.playerExp;
                    PlayerLevel.instance.UpdateLevelUI();
                }

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                PlayerController playerController = player?.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.health = data.playerHealth;
                }

                // ��ȭ ������ �ε�
                if (CurrencyManager.instance != null)
                {
                    CurrencyManager.instance.gold = data.gold;
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

                // ���� ���� �ε�
                if (AutoBattleManager.instance != null)
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