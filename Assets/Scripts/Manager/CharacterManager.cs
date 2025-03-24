// ĳ���� ���� �ý���
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
        public string name;
        public int level;
        public Sprite icon;
        public GameObject prefab;
        public bool isUnlocked;
        public bool isActive;
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

    private void InitializeCharacterUI()
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

    private void SpawnActiveCharacters()
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

    private void DespawnAllCharacters()
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
}