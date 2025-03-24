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
        public float playerHealth;

        // 통화 정보
        public int gold;
        public int gems;
        public int energy;

        // 캐릭터 정보
        public List<CharacterSaveData> characters = new List<CharacterSaveData>();

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

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
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

            // 플레이어 데이터 저장
            data.playerLevel = PlayerLevel.instance?.currentLevel ?? 1;
            data.playerExp = PlayerLevel.instance?.currentExp ?? 0;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            PlayerController playerController = player?.GetComponent<PlayerController>();
            if (playerController != null)
            {
                data.playerHealth = playerController.health;
            }

            // 통화 데이터 저장
            data.gold = CurrencyManager.instance?.gold ?? 0;
            data.gems = CurrencyManager.instance?.gems ?? 0;
            data.energy = CurrencyManager.instance?.energy ?? 100;

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

            // 게임 설정 저장
            data.autoBattleEnabled = AutoBattleManager.instance?.autoBattleEnabled ?? true;
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

                // 플레이어 데이터 로드
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

                // 통화 데이터 로드
                if (CurrencyManager.instance != null)
                {
                    CurrencyManager.instance.gold = data.gold;
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

                // 게임 설정 로드
                if (AutoBattleManager.instance != null)
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