// ĳ���� ���� �ý���
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    [Header("Character Settings")]
    public string currentCharacterId; // 현재 선택된 캐릭터 ID
    public List<CharacterData> characters = new List<CharacterData>(); // 캐릭터 목록

    [System.Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public string description;
        public int level = 1;
        public int stars = 1; // 등급 (1-5등)
        public CharacterType type; // 캐릭터 타입 (탱크, 딜러, 서포터 등)
        public Sprite icon;
        public GameObject prefab;
        public bool isUnlocked;
        public bool isActive;

        // 기본 스탯
        public float baseHealth = 100f;
        public float baseAttack = 10f;
        public float baseDefense = 5f;
        public float baseSpeed = 1f;

        // 레벨당 증가 스탯
        public float healthPerLevel = 10f;
        public float attackPerLevel = 1f;
        public float defensePerLevel = 0.5f;
        public float speedPerLevel = 0.02f;

        // 현재 레벨의 스탯
        public float GetCurrentHealth() => baseHealth + (level - 1) * healthPerLevel;
        public float GetCurrentAttack() => baseAttack + (level - 1) * attackPerLevel;
        public float GetCurrentDefense() => baseDefense + (level - 1) * defensePerLevel;
        public float GetCurrentSpeed() => baseSpeed + (level - 1) * speedPerLevel;

        // 레벨업 비용
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

        // 기본 캐릭터 설정
        if (characters.Count > 0 && string.IsNullOrEmpty(currentCharacterId))
        {
            currentCharacterId = characters[0].id;
        }
    }

    public void InitializeCharacterUI()
    {
        // ���� ���� ����
        foreach (Transform child in characterSlotsParent)
        {
            Destroy(child.gameObject);
        }

        // ĳ���� ���� ����
        for (int i = 0; i < characters.Count; i++)
        {
            GameObject slot = Instantiate(characterSlotPrefab, characterSlotsParent);

            // ĳ���� ������ ����
            Image iconImage = slot.transform.Find("Icon").GetComponent<Image>();
            iconImage.sprite = characters[i].icon;

            // ĳ���� ���� ����
            TextMeshProUGUI levelText = slot.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
            levelText.text = "Lv." + characters[i].level;

            // ��� ������ ����
            Transform lockIcon = slot.transform.Find("LockIcon");
            if (lockIcon != null)
                lockIcon.gameObject.SetActive(!characters[i].isUnlocked);

            // Ȱ�� ���� ǥ��
            if (characters[i].isActive)
                slot.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            else
                slot.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1);

            // Ŭ�� �̺�Ʈ ����
            int characterIndex = i;  // Ŭ���� ������ ���ϱ� ���� ���� ����
            Button button = slot.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ToggleCharacter(characters[i].id));
            }
        }
    }

    public void ToggleCharacter(string characterId)
    {
        CharacterData character = GetCharacterById(characterId);
        if (character == null)
            return;

        // 잠금된 캐릭터는 토글 불가
        if (!character.isUnlocked)
            return;

        // 활성 상태 토글
        if (character.isActive)
        {
            character.isActive = false;
        }
        else
        {
            // 활성 캐릭터 수 확인
            int activeCount = 0;
            foreach (CharacterData c in characters)
            {
                if (c.isActive)
                    activeCount++;
            }

            // 최대 활성 캐릭터 수를 초과하지 않으면 활성화
            if (activeCount < maxActiveCharacters)
                character.isActive = true;
            else
                Debug.Log("활성 캐릭터 수가 최대치를 초과합니다.");
        }

        // UI 업데이트
        InitializeCharacterUI();

        // 활성 캐릭터 다시 생성
        DespawnAllCharacters();
        SpawnActiveCharacters();
    }

    public void SpawnActiveCharacters()
    {
        // Ȱ�� ĳ���� ����
        foreach (CharacterData character in characters)
        {
            if (character.isActive && character.prefab != null)
            {
                // �÷��̾� �ֺ��� ����
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

    public void LevelUpCharacter(string characterId)
    {
        CharacterData character = GetCharacterById(characterId);
        if (character != null)
        {
            character.level++;
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

    // 현재 캐릭터 변경
    public void SetCurrentCharacter(string characterId)
    {
        if (characters.Exists(c => c.id == characterId))
        {
            currentCharacterId = characterId;
            Debug.Log($"현재 캐릭터가 {GetCurrentCharacter().name}로 변경되었습니다.");
        }
    }

    // 현재 선택된 캐릭터 데이터 반환
    public CharacterData GetCurrentCharacter()
    {
        return characters.Find(c => c.id == currentCharacterId);
    }

    public CharacterData GetCharacterById(string characterId)
    {
        return characters.Find(c => c.id == characterId);
    }

    public void UpgradeCharacter(string characterId)
    {
        // 캐릭터 업그레이드
    }
}