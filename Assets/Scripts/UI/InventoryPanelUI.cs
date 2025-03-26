using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

// CharacterManager의 CharacterData 클래스를 참조하기 위한 using 문
using static CharacterManager;

public class InventoryPanelUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TabGroup tabGroup;

    [Header("Item Grid")]
    [SerializeField] private Transform itemGridContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private int gridColumns = 4;
    [SerializeField] private int gridRows = 5;

    [Header("Item Detail")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private TextMeshProUGUI itemStats;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button upgradeButton;

    private List<ItemSlot> itemSlots = new List<ItemSlot>();
    private EquipmentData selectedItem;

    [System.Serializable]
    public class ItemSlot
    {
        public GameObject slotObject;
        public Image itemIcon;
        public Image frameImage;
        public TextMeshProUGUI levelText;
        public GameObject equippedIndicator;
    }

    [System.Serializable]
    public class TabGroup
    {
        public Button weaponTab;
        public Button armorTab;
        public Button accessoryTab;
        public Button consumableTab;

        public void Initialize(InventoryPanelUI panel)
        {
            weaponTab.onClick.AddListener(() => panel.ShowItemsOfType(EquipmentType.Weapon));
            armorTab.onClick.AddListener(() => panel.ShowItemsOfType(EquipmentType.Armor));
            accessoryTab.onClick.AddListener(() => panel.ShowItemsOfType(EquipmentType.Accessory));
            consumableTab.onClick.AddListener(() => panel.ShowConsumableItems());
        }

        public void SetActiveTab(EquipmentType type)
        {
            // 기존 탭 색상 초기화
            weaponTab.GetComponent<Image>().color = Color.gray;
            armorTab.GetComponent<Image>().color = Color.gray;
            accessoryTab.GetComponent<Image>().color = Color.gray;
            consumableTab.GetComponent<Image>().color = Color.gray;

            // 선택된 탭 색상 초기화
            switch (type)
            {
                case EquipmentType.Weapon:
                    weaponTab.GetComponent<Image>().color = Color.white;
                    break;
                case EquipmentType.Armor:
                    armorTab.GetComponent<Image>().color = Color.white;
                    break;
                case EquipmentType.Accessory:
                    accessoryTab.GetComponent<Image>().color = Color.white;
                    break;
                default:
                    consumableTab.GetComponent<Image>().color = Color.white;
                    break;
            }
        }
    }

    private void Awake()
    {
        // 매니저 인스턴스 확인
        if (EquipmentManager.instance == null)
        {
            Debug.LogError("EquipmentManager가 초기화되지 않았습니다!");
            return;
        }
        if (CurrencyManager.instance == null)
        {
            Debug.LogError("CurrencyManager가 초기화되지 않았습니다!");
            return;
        }
        if (CharacterManager.instance == null)
        {
            Debug.LogError("CharacterManager가 초기화되지 않았습니다!");
            return;
        }
    }

    private void Start()
    {
        // 이벤트 초기화
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (equipButton != null)
            equipButton.onClick.AddListener(EquipSelectedItem);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedItem);

        // 탭 그룹 초기화
        if (tabGroup != null)
            tabGroup.Initialize(this);

    }

    private void OnEnable()
    {
        if (itemGridContainer != null && itemSlotPrefab != null)
        {
            InitializeItemGrid();
        }
    }

    private void OnDisable()
    {
        Debug.Log("=== InventoryPanelUI OnDisable 시작 ===");
        Debug.Log($"현재 패널 활성화 상태: {gameObject.activeSelf}");
        Debug.Log($"스택 트레이스: {System.Environment.StackTrace}");
        Debug.Log("=== InventoryPanelUI OnDisable 종료 ===");
    }

    private void InitializeItemGrid()
    {
        // 기존 아이템 그리드 초기화
        foreach (Transform child in itemGridContainer)
        {
            Destroy(child.gameObject);
        }
        itemSlots.Clear();

        // 새로운 아이템 그리드 초기화
        int totalSlots = gridColumns * gridRows;
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemGridContainer);
            slotObj.SetActive(true);

            Transform iconTransform = slotObj.transform.Find("Icon");
            Transform levelTextTransform = slotObj.transform.Find("LevelText");
            Transform equippedIndicatorTransform = slotObj.transform.Find("EquippedIndicator");

            if (iconTransform == null || levelTextTransform == null || equippedIndicatorTransform == null)
            {
                continue;
            }

            // 각 컴포넌트 활성화
            iconTransform.gameObject.SetActive(true);
            levelTextTransform.gameObject.SetActive(true);
            equippedIndicatorTransform.gameObject.SetActive(false);

            ItemSlot slot = new ItemSlot
            {
                slotObject = slotObj,
                itemIcon = iconTransform.GetComponent<Image>(),
                frameImage = slotObj.GetComponent<Image>(),
                levelText = levelTextTransform.GetComponent<TextMeshProUGUI>(),
                equippedIndicator = equippedIndicatorTransform.gameObject
            };

            // 버튼 컴포넌트 설정
            Button button = slotObj.GetComponent<Button>();
            if (button != null)
            {
                int slotIndex = i;
                button.onClick.AddListener(() => OnItemSlotClicked(slotIndex));
            }

            itemSlots.Add(slot);
        }

        // 레이아웃 업데이트
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemGridContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void ShowItemsOfType(EquipmentType type)
    {
        // 선택된 탭 표시
        tabGroup.SetActiveTab(type);

        // 해당 타입의 아이템 가져오기
        List<EquipmentData> items = EquipmentManager.instance.GetItemsByType(type);

        // UI 업데이트
        UpdateItemGrid(items);
    }

    public void ShowConsumableItems()
    {
        // 선택된 탭 표시
        tabGroup.SetActiveTab(EquipmentType.Helmet); // 가상 아이템

        // 가상 아이템 가져오기 (더미)
        List<EquipmentData> items = new List<EquipmentData>();

        // UI 업데이트
        UpdateItemGrid(items);
    }

    private void UpdateItemGrid(List<EquipmentData> items)
    {
        // 기존 아이템 상태 초기화
        foreach (var slot in itemSlots)
        {
            slot.slotObject.SetActive(true);

            if (items.Count == 0)
            {
                // 아이템이 없는 경우에만 아이콘과 텍스트 비활성화
                slot.itemIcon.gameObject.SetActive(false);
                slot.levelText.gameObject.SetActive(false);
                slot.equippedIndicator.SetActive(false);
            }
        }

        // 아이템 데이터를 기반으로 그리드 업데이트
        for (int i = 0; i < Mathf.Min(items.Count, itemSlots.Count); i++)
        {
            EquipmentData item = items[i];
            ItemSlot slot = itemSlots[i];

            // 아이템이 있는 경우 활성화
            slot.itemIcon.gameObject.SetActive(true);
            slot.itemIcon.sprite = item.icon;

            slot.levelText.gameObject.SetActive(true);
            slot.levelText.text = "Lv." + item.level;

            // 장착 여부 확인
            bool isEquipped = EquipmentManager.instance.IsEquipped(item);
            slot.equippedIndicator.SetActive(isEquipped);

            // 등급에 따른 프레임 색상
            switch (item.rarity)
            {
                case EquipmentRarity.Common:
                    slot.frameImage.color = new Color(0.7f, 0.7f, 0.7f);
                    break;
                case EquipmentRarity.Uncommon:
                    slot.frameImage.color = new Color(0.0f, 0.8f, 0.0f);
                    break;
                case EquipmentRarity.Rare:
                    slot.frameImage.color = new Color(0.0f, 0.5f, 1.0f);
                    break;
                case EquipmentRarity.Epic:
                    slot.frameImage.color = new Color(0.6f, 0.0f, 1.0f);
                    break;
                case EquipmentRarity.Legendary:
                    slot.frameImage.color = new Color(1.0f, 0.5f, 0.0f);
                    break;
            }
        }
    }

    private void OnItemSlotClicked(int slotIndex)
    {
        Debug.Log($"슬롯 클릭됨: Index={slotIndex}");

        EquipmentType currentType = GetCurrentSelectedType();
        List<EquipmentData> items;

        if (currentType == EquipmentType.Helmet)
            items = new List<EquipmentData>();
        else
            items = EquipmentManager.instance.GetItemsByType(currentType);

        if (slotIndex >= items.Count)
        {
            Debug.LogError($"유효하지 않은 슬롯 인덱스: {slotIndex}");
            return;
        }

        selectedItem = items[slotIndex];
        ShowItemDetail(selectedItem);
    }

    private EquipmentType GetCurrentSelectedType()
    {
        // 선택된 탭에 따른 타입 반환
        if (tabGroup.weaponTab.GetComponent<Image>().color == Color.white)
            return EquipmentType.Weapon;
        else if (tabGroup.armorTab.GetComponent<Image>().color == Color.white)
            return EquipmentType.Armor;
        else if (tabGroup.accessoryTab.GetComponent<Image>().color == Color.white)
            return EquipmentType.Accessory;
        else
            return EquipmentType.Helmet; // 가상 아이템인 경우
    }

    private void ShowItemDetail(EquipmentData item)
    {
        if (itemDetailPanel == null || item == null)
            return;

        itemDetailPanel.SetActive(true);

        if (itemIcon != null)
            itemIcon.sprite = item.icon;

        if (itemName != null)
        {
            itemName.text = item.name;
            // 등급에 따른 이름 색상
            switch (item.rarity)
            {
                case EquipmentRarity.Common:
                    itemName.color = new Color(0.7f, 0.7f, 0.7f);
                    break;
                case EquipmentRarity.Uncommon:
                    itemName.color = new Color(0.0f, 0.8f, 0.0f);
                    break;
                case EquipmentRarity.Rare:
                    itemName.color = new Color(0.0f, 0.5f, 1.0f);
                    break;
                case EquipmentRarity.Epic:
                    itemName.color = new Color(0.6f, 0.0f, 1.0f);
                    break;
                case EquipmentRarity.Legendary:
                    itemName.color = new Color(1.0f, 0.5f, 0.0f);
                    break;
            }
        }

        if (itemDescription != null)
            itemDescription.text = item.description;

        if (itemStats != null)
        {
            itemStats.text =
                $"레벨: {item.level}\n" +
                $"체력 보너스: +{item.GetCurrentHealthBonus():F0}\n" +
                $"공격력 보너스: +{item.GetCurrentAttackBonus():F1}\n" +
                $"방어력 보너스: +{item.GetCurrentDefenseBonus():F1}\n";

            if (item.speedBonus > 0)
                itemStats.text += $"속도 보너스: +{item.speedBonus:F2}\n";

        }

        if (equipButton != null)
        {
            bool isEquipped = EquipmentManager.instance.IsEquipped(item);
            var buttonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = isEquipped ? "해제" : "장착";
        }

        if (upgradeButton != null)
        {
            int upgradeCost = item.GetUpgradeCost();
            var buttonText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = $"강화\n{upgradeCost}";

            // 강화 버튼 활성화 상태
            upgradeButton.interactable = CurrencyManager.instance.gold >= upgradeCost;
        }
    }

    private void EquipSelectedItem()
    {
        if (selectedItem == null)
            return;

        // 현재 선택된 캐릭터가 없으면 리턴
        CharacterData currentCharacter = CharacterManager.instance.GetCurrentCharacter();
        if (currentCharacter == null)
        {
            Debug.LogWarning("장비를 장착할 캐릭터가 선택되지 않았습니다.");
            return;
        }

        bool isEquipped = EquipmentManager.instance.IsEquipped(selectedItem);

        if (isEquipped)
        {
            // 아이템 해제
            EquipmentManager.instance.UnequipItem(currentCharacter.id, selectedItem);
        }
        else
        {
            // 아이템 장착
            EquipmentManager.instance.EquipItem(currentCharacter.id, selectedItem);
        }

        // UI 업데이트
        ShowItemsOfType(selectedItem.type);
        ShowItemDetail(selectedItem);
    }

    private void UpgradeSelectedItem()
    {
        if (selectedItem == null)
            return;

        EquipmentManager.instance.UpgradeEquipment(selectedItem);

        // UI 업데이트
        ShowItemDetail(selectedItem);
    }

    public void ClosePanel()
    {
        Debug.Log("ClosePanel 호출됨");
        gameObject.SetActive(false);
    }
}