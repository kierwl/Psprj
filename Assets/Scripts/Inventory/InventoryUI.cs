using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("인벤토리 참조")]
    [SerializeField] private Inventory inventory;
    
    [Header("UI 요소")]
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private TextMeshProUGUI inventoryTitle;
    [SerializeField] private Button sortButton;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Button Close;
    [SerializeField] private GameObject statsPanel;

    [Header("아이템 정보 패널")]
    [SerializeField] private GameObject itemInfoPanel;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private TextMeshProUGUI itemStats;
    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button upgradeButton;
    
    private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();
    private Inventory.InventoryItem selectedItem;
    private int selectedSlotIndex = -1;
    
    private void Awake()
    {
        if (inventory == null)
            inventory = Inventory.instance;
            
        if (sortButton != null)
            sortButton.onClick.AddListener(OnSortButtonClicked);
            
        if (useButton != null)
            useButton.onClick.AddListener(OnUseButtonClicked);
            
        if (dropButton != null)
            dropButton.onClick.AddListener(OnDropButtonClicked);
            
        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipButtonClicked);
            
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        if (Close != null)
            Close.onClick.AddListener(ToggleInventory);

        // CloseButton 이벤트 연결
        Button closeButton = inventoryPanel.GetComponentInChildren<Button>(true);
        if (closeButton != null && closeButton.name == "CloseButton")
            closeButton.onClick.AddListener(ToggleInventory);
    }
    
    private void Start()
    {
        // 초기화
        CreateItemSlots();
        
        // 이벤트 등록
        inventory.OnInventoryChanged += UpdateInventoryUI;
        
        // UI 초기 상태 설정
        //HideItemInfo();
        UpdateInventoryUI();
    }
    
    private void CreateItemSlots()
    {
        // 기존 슬롯 제거
        foreach (Transform child in itemSlotContainer)
        {
            Destroy(child.gameObject);
        }
        itemSlots.Clear();
        
        // 새 슬롯 생성
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, itemSlotContainer);
            ItemSlotUI slot = slotGO.GetComponent<ItemSlotUI>();
            if (slot != null)
            {
                slot.SetSlotIndex(i);
                slot.OnSlotClicked += OnItemSlotClicked;
                itemSlots.Add(slot);
            }
        }
    }
    
    public void UpdateInventoryUI()
    {
        // 모든 슬롯 초기화
        foreach (var slot in itemSlots)
        {
            slot.ClearSlot();
        }
        
        // 아이템으로 슬롯 채우기
        foreach (var item in inventory.items)
        {
            if (item.slotIndex >= 0 && item.slotIndex < itemSlots.Count)
            {
                itemSlots[item.slotIndex].SetItem(item.item, item.amount, item.isEquipped);
            }
        }
        
        // 선택된 아이템 정보 업데이트
        UpdateSelectedItemInfo();
    }
    
    private void OnItemSlotClicked(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        selectedItem = inventory.GetItemAtSlot(slotIndex);
        
        if (selectedItem != null)
        {
            ShowItemInfo();
        }
        else
        {
            HideItemInfo();
        }
    }
    
    private void ShowItemInfo()
    {
        if (selectedItem == null || itemInfoPanel == null) return;
        
        itemInfoPanel.SetActive(true);
        
        // 기본 정보 설정
        itemIcon.sprite = selectedItem.item.icon;
        itemName.text = selectedItem.item.itemName;
        itemDescription.text = selectedItem.item.description;
        
        // 아이템 유형에 따른 스탯 정보 출력
        string statsText = $"유형: {GetItemTypeText(selectedItem.item.itemType)}\n" +
                         $"희귀도: {GetRarityText(selectedItem.item.rarity)}\n";
        
        // 업그레이드 매니저에서 아이템 레벨 가져오기
        UpgradeManager upgradeManager = UpgradeManager.instance;
        int itemLevel = 1;
        float itemStat = 0;
        
        if (upgradeManager != null)
        {
            itemLevel = upgradeManager.GetItemLevel(selectedItem.item);
            itemStat = upgradeManager.GetItemStat(selectedItem.item);
        }
        
        if (selectedItem.item is WeaponSO weapon)
        {
            statsText += $"공격력: {(itemStat > 0 ? itemStat : weapon.attackDamage):F1}\n";
            statsText += $"공격 속도: {weapon.attackSpeed}\n";
            
            if (itemLevel > 1)
                statsText += $"아이템 레벨: {itemLevel}\n";
        }
        else if (selectedItem.item is ArmorSO armor)
        {
            statsText += $"방어력: {(itemStat > 0 ? itemStat : armor.defense):F1}\n";
            
            if (itemLevel > 1)
                statsText += $"아이템 레벨: {itemLevel}\n";
        }
        else if (selectedItem.item is ConsumableSO consumable)
        {
            statsText += $"회복량: {consumable.healthRestore}\n";
        }
        
        // 장착 상태 표시
        if (selectedItem.isEquipped)
        {
            statsText += "\n<color=#00FF00>[ 장착 중 ]</color>";
        }
        
        itemStats.text = statsText;
        
        // 아이템 타입에 따라 사용 버튼 활성화/비활성화
        useButton.interactable = (selectedItem.item.itemType == ItemSO.ItemType.Consumable);
        
        // 장착 버튼 업데이트
        UpdateEquipButtonText();
        
        // 업그레이드 버튼 활성화 여부 설정
        if (upgradeButton != null)
        {
            bool isUpgradeable = (selectedItem.item is WeaponSO || selectedItem.item is ArmorSO);
            upgradeButton.gameObject.SetActive(isUpgradeable);
        }
    }
    
    private void HideItemInfo()
    {
        if (itemInfoPanel != null)
            itemInfoPanel.SetActive(false);
            
        selectedItem = null;
        selectedSlotIndex = -1;
    }
    
    private void UpdateSelectedItemInfo()
    {
        if (selectedSlotIndex >= 0)
        {
            selectedItem = inventory.GetItemAtSlot(selectedSlotIndex);
            if (selectedItem != null)
            {
                ShowItemInfo();
            }
            else
            {
                HideItemInfo();
            }
        }
    }
    
    private void OnSortButtonClicked()
    {
        inventory.SortInventory();
    }
    
    private void OnUseButtonClicked()
    {
        if (selectedItem != null)
        {
            // 플레이어 캐릭터 참조 가져오기
            CharacterController player = FindObjectOfType<CharacterController>();
            
            if (player != null)
            {
                selectedItem.item.Use(player);
                inventory.RemoveItem(selectedItem.item, 1);
            }
        }
    }
    
    private void OnDropButtonClicked()
    {
        if (selectedItem != null)
        {
            inventory.RemoveItem(selectedItem.item, 1);
            // 여기에 아이템 드롭 로직 추가 (맵에 아이템 생성 등)
        }
    }
    
    private void OnEquipButtonClicked()
    {
        if (selectedItem == null || selectedSlotIndex < 0) return;
        
        // 이미 장착된 아이템이면 해제, 아니면 장착
        if (selectedItem.isEquipped)
        {
            inventory.UnequipItem(selectedSlotIndex);
        }
        else
        {
            inventory.EquipItem(selectedSlotIndex);
        }
        
        // UI 업데이트
        UpdateInventoryUI();
        UpdateEquipButtonText();
    }
    
    private void UpdateEquipButtonText()
    {
        if (equipButton != null && selectedItem != null)
        {
            TextMeshProUGUI buttonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = selectedItem.isEquipped ? "해제" : "장착";
            }
            
            // 장착 가능한 아이템인지 확인 (무기, 방어구, 악세서리)
            bool isEquippable = selectedItem.item.itemType == ItemSO.ItemType.Weapon ||
                               selectedItem.item.itemType == ItemSO.ItemType.Armor ||
                               selectedItem.item.itemType == ItemSO.ItemType.Accessory;
                               
            equipButton.gameObject.SetActive(isEquippable);
        }
        else if (equipButton != null)
        {
            equipButton.gameObject.SetActive(false);
        }
    }
    
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);
            statsPanel.SetActive(isActive);

            if (isActive)
                UpdateInventoryUI();
            else
                HideItemInfo();
        }
    }
    
    // 도우미 메서드들
    private string GetItemTypeText(ItemSO.ItemType type)
    {
        switch (type)
        {
            case ItemSO.ItemType.Weapon: return "무기";
            case ItemSO.ItemType.Armor: return "방어구";
            case ItemSO.ItemType.Accessory: return "악세서리";
            case ItemSO.ItemType.Consumable: return "소모품";
            case ItemSO.ItemType.Material: return "재료";
            default: return "기타";
        }
    }
    
    private string GetRarityText(ItemSO.ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemSO.ItemRarity.Common: return "<color=#CCCCCC>일반</color>";
            case ItemSO.ItemRarity.Uncommon: return "<color=#00CC00>고급</color>";
            case ItemSO.ItemRarity.Rare: return "<color=#0000FF>희귀</color>";
            case ItemSO.ItemRarity.Epic: return "<color=#CC00CC>영웅</color>";
            case ItemSO.ItemRarity.Legendary: return "<color=#FFCC00>전설</color>";
            default: return "알 수 없음";
        }
    }

    private void OnUpgradeButtonClicked()
    {
        try
        {
            Debug.Log("[InventoryUI] OnUpgradeButtonClicked 호출됨");

            // 선택된 아이템이 없는 경우
            if (selectedItem == null)
            {
                Debug.LogError("[InventoryUI] 업그레이드 버튼 클릭: 선택된 아이템이 없습니다!");
                return;
            }

            // 선택된 슬롯 인덱스가 유효하지 않은 경우
            if (selectedSlotIndex < 0)
            {
                Debug.LogError("[InventoryUI] 업그레이드 버튼 클릭: 선택된 슬롯 인덱스가 유효하지 않습니다!");
                return;
            }

            // selectedItem.item이 null인 경우
            if (selectedItem.item == null)
            {
                Debug.LogError("[InventoryUI] 업그레이드 버튼 클릭: 선택된 아이템의 item 속성이 null입니다!");
                return;
            }

            Debug.Log($"[InventoryUI] 아이템 {selectedItem.item.itemName} 업그레이드 시도");

            // 먼저 씬에 UpgradeManager가 있는지 직접 확인 (instance 사용하지 않음)
            UpgradeManager upgradeManager = FindObjectOfType<UpgradeManager>();

            if (upgradeManager == null)
            {
                Debug.LogError("[InventoryUI] UpgradeManager를 찾을 수 없습니다!");

                // 우회 방법: 씬에 GameObject 생성 및 UpgradeManager 추가
                GameObject managerObj = new GameObject("UpgradeManager");
                upgradeManager = managerObj.AddComponent<UpgradeManager>();

                if (upgradeManager == null)
                {
                    Debug.LogError("[InventoryUI] UpgradeManager를 생성할 수 없습니다!");
                    return;
                }

                // 패널 활성화 시도 (초기화 및 Awake 호출을 위해)
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(managerObj, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            // 인벤토리 패널 닫기
            try
            {
                // 인벤토리 패널 상태 저장
                bool wasActive = inventoryPanel.activeSelf;

                // 패널이 활성화된 경우에만 닫기
                if (wasActive)
                {
                    Debug.Log("[InventoryUI] 인벤토리 패널 닫기 시도");
                    inventoryPanel.SetActive(false);
                    statsPanel.SetActive(false);
                }

                // 업그레이드 패널 열기 시도
                Debug.Log("[InventoryUI] 업그레이드 패널 열기 시도");
                upgradeManager.OpenUpgradePanel(selectedItem.item);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InventoryUI] 예외 발생: {e.Message}\n{e.StackTrace}");

                // 인벤토리 패널 유지 (오류 발생 시)
                if (inventoryPanel != null && !inventoryPanel.activeSelf)
                {
                    inventoryPanel.SetActive(true);
                    statsPanel.SetActive(true);
                }
            }
        }
        catch (System.Exception e)
        {
            // 모든 예외 처리
            Debug.LogError($"[InventoryUI] OnUpgradeButtonClicked 메서드에서 예외 발생: {e.Message}\n{e.StackTrace}");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 등록 해제
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateInventoryUI;
            
        // 슬롯 이벤트 등록 해제
        foreach (var slot in itemSlots)
        {
            if (slot != null)
                slot.OnSlotClicked -= OnItemSlotClicked;
        }
    }

} 