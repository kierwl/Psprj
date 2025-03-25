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
        // 캐릭터 슬롯 초기 생성
        InitializeCharacterSlots();

        // 디테일 패널 초기 숨김
        if (characterDetailPanel != null)
            characterDetailPanel.SetActive(false);

        // 업그레이드 버튼 이벤트 등록
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedCharacter);
    }

    private void InitializeCharacterSlots()
    {
        // 기존 슬롯 제거
        foreach (Transform child in characterSlotsContainer)
        {
            Destroy(child.gameObject);
        }
        characterSlots.Clear();

        // 새 슬롯 생성
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

            // 슬롯 기본 상태 설정
            slot.lockIcon.SetActive(true);
            slot.activeIndicator.SetActive(false);

            // 클릭 이벤트 등록
            int slotIndex = i;
            Button button = slotObj.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnCharacterSlotClicked(slotIndex));

            characterSlots.Add(slot);
        }
    }

    // 캐릭터 데이터로 UI 업데이트
    public void UpdateCharacterSlots(List<CharacterManager.CharacterData> characters)
    {
        for (int i = 0; i < characterSlots.Count; i++)
        {
            if (i < characters.Count)
            {
                CharacterManager.CharacterData character = characters[i];
                CharacterSlot slot = characterSlots[i];

                // 슬롯 데이터 설정
                slot.characterId = character.id;
                slot.characterIcon.sprite = character.icon;
                slot.levelText.text = "Lv." + character.level;
                slot.lockIcon.SetActive(!character.isUnlocked);
                slot.activeIndicator.SetActive(character.isActive);

                // 잠금 해제 상태에 따라 이미지 조정
                Color iconColor = character.isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                slot.characterIcon.color = iconColor;

                // 활성 상태에 따라 프레임 색상 변경
                slot.frameImage.color = character.isActive ? new Color(1f, 0.8f, 0.2f) : Color.white;
            }
            else
            {
                // 빈 슬롯 설정
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

        // 캐릭터 매니저에서 데이터 가져오기
        selectedCharacter = CharacterManager.instance.GetCharacterById(slot.characterId);

        if (selectedCharacter != null)
        {
            // 캐릭터가 잠금 해제된 경우에만 상세 정보 표시
            if (selectedCharacter.isUnlocked)
            {
                ShowCharacterDetail(selectedCharacter);
            }
            else
            {
                HideCharacterDetail();
            }

            // 활성 상태 토글
            CharacterManager.instance.ToggleCharacter(slot.characterId);

            // UI 업데이트
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

            // 스탯 정보 표시
            characterDetailStats.text =
                $"체력: {character.GetCurrentHealth():F0}\n" +
                $"공격력: {character.GetCurrentAttack():F1}\n" +
                $"방어력: {character.GetCurrentDefense():F1}\n" +
                $"속도: {character.GetCurrentSpeed():F2}";

            // 업그레이드 버튼 비용 표시
            int upgradeCost = character.GetUpgradeCost();
            upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = $"강화\n{upgradeCost}";

            // 비용 충족 여부에 따라 버튼 활성화
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

        // 선택된 캐릭터 정보 업데이트
        selectedCharacter = CharacterManager.instance.GetCharacterById(selectedCharacter.id);

        // UI 업데이트
        ShowCharacterDetail(selectedCharacter);
        UpdateCharacterSlots(CharacterManager.instance.characters);
    }
}