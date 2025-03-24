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

    [System.Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public string description;
        public int level = 1;
        public int stars = 1; // ��� (1-5��)
        public CharacterType type; // ĳ���� Ÿ�� (��Ŀ, ����, ������ ��)
        public Sprite icon;
        public GameObject prefab;
        public bool isUnlocked;
        public bool isActive;

        // �⺻ ����
        public float baseHealth = 100f;
        public float baseAttack = 10f;
        public float baseDefense = 5f;
        public float baseSpeed = 1f;

        // ������ ���� ������
        public float healthPerLevel = 10f;
        public float attackPerLevel = 1f;
        public float defensePerLevel = 0.5f;
        public float speedPerLevel = 0.02f;

        // ���� ������ ���� ���
        public float GetCurrentHealth() => baseHealth + (level - 1) * healthPerLevel;
        public float GetCurrentAttack() => baseAttack + (level - 1) * attackPerLevel;
        public float GetCurrentDefense() => baseDefense + (level - 1) * defensePerLevel;
        public float GetCurrentSpeed() => baseSpeed + (level - 1) * speedPerLevel;

        // ��ȭ ��� ���
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
                button.onClick.AddListener(() => ToggleCharacter(characterIndex));
            }
        }
    }

    public void ToggleCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
            return;

        // ��� ������ ĳ���͸� ��� ����
        if (!characters[index].isUnlocked)
            return;

        // Ȱ�� ���� ���
        if (characters[index].isActive)
        {
            characters[index].isActive = false;
        }
        else
        {
            // Ȱ�� ĳ���� �� Ȯ��
            int activeCount = 0;
            foreach (CharacterData character in characters)
            {
                if (character.isActive)
                    activeCount++;
            }

            // �ִ� Ȱ�� ĳ���� ���� �ʰ����� �ʴ� ��쿡�� Ȱ��ȭ
            if (activeCount < maxActiveCharacters)
                characters[index].isActive = true;
            else
                Debug.Log("Ȱ�� ĳ���� ���� �ִ�ġ�� �����߽��ϴ�.");
        }

        // UI ������Ʈ
        InitializeCharacterUI();

        // Ȱ�� ĳ���� �ٽ� ����
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
}