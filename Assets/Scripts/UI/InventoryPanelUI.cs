using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryPanelUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject inventoryPanel;
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

        // 아이템 그리드 초기화
        if (itemGridContainer != null && itemSlotPrefab != null)
            InitializeItemGrid();

        // 아이템 상세 패널 초기화
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);

        // 패널 초기화
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
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
        for (int i = 0; i < gridColumns * gridRows; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemGridContainer);

            ItemSlot slot = new ItemSlot
            {
                slotObject = slotObj,
                itemIcon = slotObj.transform.Find("Icon").GetComponent<Image>(),
                frameImage = slotObj.GetComponent<Image>(),
                levelText = slotObj.transform.Find("LevelText").GetComponent<TextMeshProUGUI>(),
                equippedIndicator = slotObj.transform.Find("EquippedIndicator").gameObject
            };

            // 기본 아이템 표시 초기화
            slot.itemIcon.gameObject.SetActive(false);
            slot.levelText.gameObject.SetActive(false);
            slot.equippedIndicator.SetActive(false);

            // 추가 이벤트 초기화
            int slotIndex = i;
            Button button = slotObj.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnItemSlotClicked(slotIndex));

            itemSlots.Add(slot);
        }
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
            slot.itemIcon.gameObject.SetActive(false);
            slot.levelText.gameObject.SetActive(false);
            slot.equippedIndicator.SetActive(false);
        }

        // 아이템 데이터를 기반으로 그리드 업데이트
        for (int i = 0; i < Mathf.Min(items.Count, itemSlots.Count); i++)
        {
            EquipmentData item = items[i];
            ItemSlot slot = itemSlots[i];

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
        // 선택된 아이템 타입에 따라 아이템 리스트 가져오기
        EquipmentType currentType = GetCurrentSelectedType();
        List<EquipmentData> items;

        if (currentType == EquipmentType.Helmet) // 가상 아이템인 경우
            items = new List<EquipmentData>(); // 가상 아이템 (더미)
        else
            items = EquipmentManager.instance.GetItemsByType(currentType);

        // 인덱스 유효성 확인
        if (slotIndex >= items.Count)
            return;

        // 선택된 아이템 가져오기
        selectedItem = items[slotIndex];

        // 아이템 상세 표시
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

            // 추가 속성 표시
            if (item.attributes.Count > 0)
            {
                itemStats.text += "\n추가 속성:";
                foreach (var attr in item.attributes)
                {
                    itemStats.text += $"\n- {attr.name}: {attr.value:F1}";
                }
            }
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

        bool isEquipped = EquipmentManager.instance.IsEquipped(selectedItem);

        if (isEquipped)
        {
            // 아이템 해제
            EquipmentManager.instance.UnequipItem(CharacterManager.instance.currentCharacterId, selectedItem);
        }
        else
        {
            // 아이템 장착
            EquipmentManager.instance.EquipItem(CharacterManager.instance.currentCharacterId, selectedItem);
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
        gameObject.SetActive(false);
    }
}