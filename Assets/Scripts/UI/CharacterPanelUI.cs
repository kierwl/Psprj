using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterPanelUI : MonoBehaviour
{
    [Header("Panel Settings")]
    public Transform characterSlotsContainer;
    public GameObject characterSlotPrefab;
    public int maxSlots = 8;

    [Header("Character Detail")]
    public GameObject characterDetailPanel;
    public Image characterDetailIcon;
    public TextMeshProUGUI characterDetailName;
    public TextMeshProUGUI characterDetailLevel;
    public TextMeshProUGUI characterDetailStats;
    public Button upgradeButton;

    private List<CharacterSlot> characterSlots = new List<CharacterSlot>();
    private CharacterManager.CharacterData selectedCharacter;

    [System.Serializable]
    public class CharacterSlot
    {
        public GameObject slotObject;
        public Image characterIcon;
        public TextMeshProUGUI levelText;
        public Image frameImage;
        public GameObject lockIcon;
        public GameObject activeIndicator;
        public string characterId;
    }

    private void Start()
    {
        // ĳ���� ���� �ʱ� ����
        InitializeCharacterSlots();

        // ������ �г� �ʱ� ����
        if (characterDetailPanel != null)
            characterDetailPanel.SetActive(false);

        // ���׷��̵� ��ư �̺�Ʈ ���
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedCharacter);
    }

    private void InitializeCharacterSlots()
    {
        // ���� ���� ����
        foreach (Transform child in characterSlotsContainer)
        {
            Destroy(child.gameObject);
        }
        characterSlots.Clear();

        // �� ���� ����
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(characterSlotPrefab, characterSlotsContainer);

            CharacterSlot slot = new CharacterSlot
            {
                slotObject = slotObj,
                characterIcon = slotObj.transform.Find("Icon").GetComponent<Image>(),
                levelText = slotObj.transform.Find("LevelText").GetComponent<TextMeshProUGUI>(),
                frameImage = slotObj.GetComponent<Image>(),
                lockIcon = slotObj.transform.Find("LockIcon").gameObject,
                activeIndicator = slotObj.transform.Find("ActiveIndicator").gameObject
            };

            // ���� �⺻ ���� ����
            slot.lockIcon.SetActive(true);
            slot.activeIndicator.SetActive(false);

            // Ŭ�� �̺�Ʈ ���
            int slotIndex = i;
            Button button = slotObj.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnCharacterSlotClicked(slotIndex));

            characterSlots.Add(slot);
        }
    }

    // ĳ���� �����ͷ� UI ������Ʈ
    public void UpdateCharacterSlots(List<CharacterManager.CharacterData> characters)
    {
        for (int i = 0; i < characterSlots.Count; i++)
        {
            if (i < characters.Count)
            {
                CharacterManager.CharacterData character = characters[i];
                CharacterSlot slot = characterSlots[i];

                // ���� ������ ����
                slot.characterId = character.id;
                slot.characterIcon.sprite = character.icon;
                slot.levelText.text = "Lv." + character.level;
                slot.lockIcon.SetActive(!character.isUnlocked);
                slot.activeIndicator.SetActive(character.isActive);

                // ��� ���� ���¿� ���� �̹��� ����
                Color iconColor = character.isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                slot.characterIcon.color = iconColor;

                // Ȱ�� ���¿� ���� ������ ���� ����
                slot.frameImage.color = character.isActive ? new Color(1f, 0.8f, 0.2f) : Color.white;
            }
            else
            {
                // �� ���� ����
                CharacterSlot slot = characterSlots[i];
                slot.characterId = "";
                slot.lockIcon.SetActive(true);
                slot.activeIndicator.SetActive(false);
                slot.characterIcon.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                slot.levelText.text = "";
            }
        }
    }

    private void OnCharacterSlotClicked(int slotIndex)
    {
        if (slotIndex >= characterSlots.Count)
            return;

        CharacterSlot slot = characterSlots[slotIndex];

        // ĳ���� �Ŵ������� ������ ��������
        selectedCharacter = CharacterManager.instance.GetCharacterById(slot.characterId);

        if (selectedCharacter != null)
        {
            // ĳ���Ͱ� ��� ������ ��쿡�� �� ���� ǥ��
            if (selectedCharacter.isUnlocked)
            {
                ShowCharacterDetail(selectedCharacter);
            }
            else
            {
                HideCharacterDetail();
            }

            // Ȱ�� ���� ���
            CharacterManager.instance.ToggleCharacter(slot.characterId);

            // UI ������Ʈ
            UpdateCharacterSlots(CharacterManager.instance.characters);
        }
    }

    private void ShowCharacterDetail(CharacterManager.CharacterData character)
    {
        if (characterDetailPanel != null)
        {
            characterDetailPanel.SetActive(true);

            characterDetailIcon.sprite = character.icon;
            characterDetailName.text = character.name;
            characterDetailLevel.text = "Lv. " + character.level;

            // ���� ���� ǥ��
            characterDetailStats.text =
                $"ü��: {character.GetCurrentHealth():F0}\n" +
                $"���ݷ�: {character.GetCurrentAttack():F1}\n" +
                $"����: {character.GetCurrentDefense():F1}\n" +
                $"�ӵ�: {character.GetCurrentSpeed():F2}";

            // ���׷��̵� ��ư ��� ǥ��
            int upgradeCost = character.GetUpgradeCost();
            upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = $"��ȭ\n{upgradeCost}";

            // ��� ���� ���ο� ���� ��ư Ȱ��ȭ
            upgradeButton.interactable = CurrencyManager.instance.gold >= upgradeCost;
        }
    }

    private void HideCharacterDetail()
    {
        if (characterDetailPanel != null)
            characterDetailPanel.SetActive(false);

        selectedCharacter = null;
    }

    private void UpgradeSelectedCharacter()
    {
        if (selectedCharacter == null)
            return;

        CharacterManager.instance.UpgradeCharacter(selectedCharacter.id);

        // ���õ� ĳ���� ���� ������Ʈ
        selectedCharacter = CharacterManager.instance.GetCharacterById(selectedCharacter.id);

        // UI ������Ʈ
        ShowCharacterDetail(selectedCharacter);
        UpdateCharacterSlots(CharacterManager.instance.characters);
    }
}