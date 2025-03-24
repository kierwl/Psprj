// 캐릭터 관리 시스템
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    [System.Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public string description;
        public int level = 1;
        public int stars = 1; // 등급 (1-5성)
        public CharacterType type; // 캐릭터 타입 (탱커, 딜러, 서포터 등)
        public Sprite icon;
        public GameObject prefab;
        public bool isUnlocked;
        public bool isActive;

        // 기본 스탯
        public float baseHealth = 100f;
        public float baseAttack = 10f;
        public float baseDefense = 5f;
        public float baseSpeed = 1f;

        // 레벨당 스탯 증가량
        public float healthPerLevel = 10f;
        public float attackPerLevel = 1f;
        public float defensePerLevel = 0.5f;
        public float speedPerLevel = 0.02f;

        // 현재 레벨의 스탯 계산
        public float GetCurrentHealth() => baseHealth + (level - 1) * healthPerLevel;
        public float GetCurrentAttack() => baseAttack + (level - 1) * attackPerLevel;
        public float GetCurrentDefense() => baseDefense + (level - 1) * defensePerLevel;
        public float GetCurrentSpeed() => baseSpeed + (level - 1) * speedPerLevel;

        // 강화 비용 계산
        public int GetUpgradeCost() => 100 * level * stars;
    }

    public enum CharacterType
    {
        Tank,
        Damage,
        Support,
        Mage,
        Healer
    }

    public List<CharacterData> characters = new List<CharacterData>();
    public int maxActiveCharacters = 4;

    [Header("UI References")]
    public Transform characterSlotsParent;
    public GameObject characterSlotPrefab;

    private List<GameObject> activeCharacters = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializeCharacterUI();
        SpawnActiveCharacters();
    }

    public void InitializeCharacterUI()
    {
        // 기존 슬롯 제거
        foreach (Transform child in characterSlotsParent)
        {
            Destroy(child.gameObject);
        }

        // 캐릭터 슬롯 생성
        for (int i = 0; i < characters.Count; i++)
        {
            GameObject slot = Instantiate(characterSlotPrefab, characterSlotsParent);

            // 캐릭터 아이콘 설정
            Image iconImage = slot.transform.Find("Icon").GetComponent<Image>();
            iconImage.sprite = characters[i].icon;

            // 캐릭터 레벨 설정
            TextMeshProUGUI levelText = slot.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
            levelText.text = "Lv." + characters[i].level;

            // 잠금 아이콘 설정
            Transform lockIcon = slot.transform.Find("LockIcon");
            if (lockIcon != null)
                lockIcon.gameObject.SetActive(!characters[i].isUnlocked);

            // 활성 상태 표시
            if (characters[i].isActive)
                slot.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            else
                slot.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1);

            // 클릭 이벤트 설정
            int characterIndex = i;  // 클로저 문제를 피하기 위한 로컬 변수
            Button button = slot.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ToggleCharacter(characterIndex));
            }
        }
    }

    public void ToggleCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
            return;

        // 잠금 해제된 캐릭터만 토글 가능
        if (!characters[index].isUnlocked)
            return;

        // 활성 상태 토글
        if (characters[index].isActive)
        {
            characters[index].isActive = false;
        }
        else
        {
            // 활성 캐릭터 수 확인
            int activeCount = 0;
            foreach (CharacterData character in characters)
            {
                if (character.isActive)
                    activeCount++;
            }

            // 최대 활성 캐릭터 수를 초과하지 않는 경우에만 활성화
            if (activeCount < maxActiveCharacters)
                characters[index].isActive = true;
            else
                Debug.Log("활성 캐릭터 수가 최대치에 도달했습니다.");
        }

        // UI 업데이트
        InitializeCharacterUI();

        // 활성 캐릭터 다시 스폰
        DespawnAllCharacters();
        SpawnActiveCharacters();
    }

    public void SpawnActiveCharacters()
    {
        // 활성 캐릭터 스폰
        foreach (CharacterData character in characters)
        {
            if (character.isActive && character.prefab != null)
            {
                // 플레이어 주변에 스폰
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector3 spawnPos = player.transform.position + Random.insideUnitSphere * 2f;
                    spawnPos.y = player.transform.position.y;
                    GameObject companion = Instantiate(character.prefab, spawnPos, Quaternion.identity);
                    activeCharacters.Add(companion);
                }
            }
        }
    }

    public void DespawnAllCharacters()
    {
        foreach (GameObject character in activeCharacters)
        {
            Destroy(character);
        }
        activeCharacters.Clear();
    }

    public void LevelUpCharacter(int index)
    {
        if (index >= 0 && index < characters.Count)
        {
            characters[index].level++;
            InitializeCharacterUI();
        }
    }

    public void UpdateCharacter(CharacterData character)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].id == character.id)
            {
                characters[i] = character;
                break;
            }
        }
    }
    public void UpdateCharacterBonusStats(string characterId, float healthBonus, float attackBonus, float defenseBonus, float speedBonus)
    {
        CharacterData character = characters.Find(c => c.id == characterId);
        if (character != null)
        {
            character.baseHealth += healthBonus;
            character.baseAttack += attackBonus;
            character.baseDefense += defenseBonus;
            character.baseSpeed += speedBonus;
            UpdateCharacter(character);
        }
    }

}