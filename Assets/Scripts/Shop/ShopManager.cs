using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("상점 설정")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    
    [Header("상세 정보 패널")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailIcon;
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailDescription;
    [SerializeField] private TextMeshProUGUI detailPrice;
    [SerializeField] private Button buyButton;
    
    [Header("플레이어 참조")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Inventory inventory;
    
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button closeButton;
    
    // 현재 선택된 아이템
    private ShopItem selectedItem;
    
    [System.Serializable]
    public class ShopItem
    {
        public ItemSO item;
        public int price;
    }
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        
        // 초기화
        if (playerStats == null)
            playerStats = PlayerStats.instance;
            
        if (inventory == null)
            inventory = Inventory.instance;
            
        // 버튼 이벤트 등록
        if (buyButton != null)
            buyButton.onClick.AddListener(BuySelectedItem);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }
    
    private void Start()
    {
        // 골드 변경 이벤트 구독
        if (playerStats != null)
            playerStats.OnGoldChanged += UpdateGoldText;
            
        // 초기 UI 설정
        if (shopPanel != null)
            shopPanel.SetActive(false);
            
        if (detailPanel != null)
            detailPanel.SetActive(false);
    }
    
    public void OpenShop()
    {
        // 상점 UI 표시
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            
            // 상점 아이템 목록 생성
            CreateShopItems();
            
            // 골드 표시 갱신
            UpdateGoldText(playerStats.GetGold());
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
    
    private void CreateShopItems()
    {
        // 기존 아이템 제거
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 새 아이템 생성
        foreach (var shopItem in shopItems)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, itemContainer);
            ShopItemUI itemUI = itemGO.GetComponent<ShopItemUI>();
            
            if (itemUI != null)
            {
                itemUI.SetItem(shopItem.item, shopItem.price);
                itemUI.OnItemClicked += ShowItemDetails;
            }
        }
    }
    
    private void ShowItemDetails(ItemSO item, int price)
    {
        // 선택된 아이템 저장
        selectedItem = shopItems.Find(i => i.item == item);
        
        if (selectedItem != null && detailPanel != null)
        {
            // 상세 정보 패널 표시
            detailPanel.SetActive(true);
            
            // 정보 설정
            if (detailIcon != null)
                detailIcon.sprite = item.icon;
                
            if (detailName != null)
                detailName.text = item.itemName;
                
            if (detailDescription != null)
                detailDescription.text = item.description;
                
            if (detailPrice != null)
                detailPrice.text = $"가격: {price} 골드";
                
            // 구매 버튼 활성화 여부 설정
            if (buyButton != null)
                buyButton.interactable = playerStats.GetGold() >= price;
        }
    }
    
    private void BuySelectedItem()
    {
        if (selectedItem == null) return;
        
        // 골드 확인 및 차감
        if (playerStats.SpendGold(selectedItem.price))
        {
            // 인벤토리에 아이템 추가
            if (inventory.AddItem(selectedItem.item))
            {
                Debug.Log($"{selectedItem.item.itemName}을(를) 구매했습니다.");
                
                // 버튼 비활성화 (재구매 방지)
                buyButton.interactable = playerStats.GetGold() >= selectedItem.price;
            }
            else
            {
                // 인벤토리 공간 부족 시 골드 환불
                playerStats.AddGold(selectedItem.price);
                Debug.LogWarning("인벤토리 공간이 부족합니다.");
            }
        }
        else
        {
            Debug.LogWarning("골드가 부족합니다.");
        }
    }
    
    private void UpdateGoldText(int amount)
    {
        if (goldText != null)
            goldText.text = $"골드: {amount}";
    }
    
    private void OnDestroy()
    {
        // 이벤트 등록 해제
        if (playerStats != null)
            playerStats.OnGoldChanged -= UpdateGoldText;
            
        // 상점 아이템 이벤트 등록 해제
        foreach (Transform child in itemContainer)
        {
            ShopItemUI itemUI = child.GetComponent<ShopItemUI>();
            if (itemUI != null)
                itemUI.OnItemClicked -= ShowItemDetails;
        }
    }
} 