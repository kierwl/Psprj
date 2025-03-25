using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;
    
    [Header("업그레이드 설정")]
    [SerializeField] private float costMultiplier = 1.5f;
    [SerializeField] private int maxUpgradeLevel = 10;
    [SerializeField] private float weaponDamageIncreasePerLevel = 0.1f; // 레벨당 10% 증가
    [SerializeField] private float armorDefenseIncreasePerLevel = 0.1f; // 레벨당 10% 증가
    
    [Header("UI 요소")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI currentStats;
    [SerializeField] private TextMeshProUGUI nextLevelStats;
    [SerializeField] private TextMeshProUGUI upgradeCost;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private Button closeButton;
    
    [Header("아이템 참조")]
    [SerializeField] private ItemUpgradeData[] upgradeableItems;
    
    [Header("플레이어 참조")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Inventory inventory;
    
    private ItemUpgradeData selectedItem;
    
    [System.Serializable]
    public class ItemUpgradeData
    {
        public ItemSO item;
        public int level = 1;
        public int baseCost = 100;
        
        // 무기와 방어구에 따라 서로 다른 스탯 표시
        [HideInInspector] public float currentStat;
        [HideInInspector] public float nextLevelStat;
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
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedItem);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUpgradePanel);
    }
    
    private void Start()
    {
        // 골드 변경 이벤트 구독
        if (playerStats != null)
            playerStats.OnGoldChanged += UpdateGoldText;
            
        // 초기 UI 설정
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
    
    public void OpenUpgradePanel(ItemSO item)
    {
        // 선택된 아이템의 업그레이드 데이터 찾기
        selectedItem = System.Array.Find(upgradeableItems, data => data.item == item);
        
        if (selectedItem == null)
        {
            Debug.LogWarning("업그레이드할 수 없는 아이템입니다.");
            return;
        }
        
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            
            // UI 업데이트
            UpdateUpgradeUI();
        }
    }
    
    public void CloseUpgradePanel()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
    
    private void UpdateUpgradeUI()
    {
        if (selectedItem == null) return;
        
        // 아이템 정보 설정
        if (itemIcon != null)
            itemIcon.sprite = selectedItem.item.icon;
            
        if (itemName != null)
            itemName.text = $"{selectedItem.item.itemName} (Lv. {selectedItem.level})";
            
        // 아이템 타입에 따른 스탯 계산
        CalculateItemStats();
        
        // 현재 및 다음 레벨 스탯 표시
        if (currentStats != null)
            currentStats.text = GetItemStatText(selectedItem.currentStat);
            
        if (nextLevelStats != null)
        {
            if (selectedItem.level < maxUpgradeLevel)
                nextLevelStats.text = $"다음 레벨: {GetItemStatText(selectedItem.nextLevelStat)}";
            else
                nextLevelStats.text = "최대 레벨";
        }
        
        // 업그레이드 비용 계산 및 표시
        int cost = CalculateUpgradeCost();
        if (upgradeCost != null)
        {
            if (selectedItem.level < maxUpgradeLevel)
                upgradeCost.text = $"비용: {cost} 골드";
            else
                upgradeCost.text = "최대 레벨";
        }
        
        // 업그레이드 버튼 활성화 여부 설정
        if (upgradeButton != null)
        {
            bool canUpgrade = selectedItem.level < maxUpgradeLevel && playerStats.GetGold() >= cost;
            upgradeButton.interactable = canUpgrade;
        }
        
        // 골드 표시 갱신
        UpdateGoldText(playerStats.GetGold());
    }
    
    private void CalculateItemStats()
    {
        if (selectedItem.item is WeaponSO weapon)
        {
            // 현재 레벨 스탯
            selectedItem.currentStat = weapon.attackDamage * (1 + (selectedItem.level - 1) * weaponDamageIncreasePerLevel);
            
            // 다음 레벨 스탯
            selectedItem.nextLevelStat = weapon.attackDamage * (1 + selectedItem.level * weaponDamageIncreasePerLevel);
        }
        else if (selectedItem.item is ArmorSO armor)
        {
            // 현재 레벨 스탯
            selectedItem.currentStat = armor.defense * (1 + (selectedItem.level - 1) * armorDefenseIncreasePerLevel);
            
            // 다음 레벨 스탯
            selectedItem.nextLevelStat = armor.defense * (1 + selectedItem.level * armorDefenseIncreasePerLevel);
        }
    }
    
    private string GetItemStatText(float statValue)
    {
        if (selectedItem.item is WeaponSO)
            return $"공격력: {statValue:F1}";
        else if (selectedItem.item is ArmorSO)
            return $"방어력: {statValue:F1}";
        else
            return "스탯 없음";
    }
    
    private int CalculateUpgradeCost()
    {
        // 기본 비용 * (승수 ^ (레벨 - 1))
        return Mathf.RoundToInt(selectedItem.baseCost * Mathf.Pow(costMultiplier, selectedItem.level - 1));
    }
    
    private void UpgradeSelectedItem()
    {
        if (selectedItem == null || selectedItem.level >= maxUpgradeLevel) return;
        
        int cost = CalculateUpgradeCost();
        
        // 골드 확인 및 차감
        if (playerStats.SpendGold(cost))
        {
            // 레벨 증가
            selectedItem.level++;
            Debug.Log($"{selectedItem.item.itemName}이(가) 레벨 {selectedItem.level}로 업그레이드되었습니다.");
            
            // UI 갱신
            UpdateUpgradeUI();
        }
        else
        {
            Debug.LogWarning("골드가 부족합니다.");
        }
    }
    
    private void UpdateGoldText(int amount)
    {
        if (playerGold != null)
            playerGold.text = $"보유 골드: {amount}";
    }
    
    private void OnDestroy()
    {
        // 이벤트 등록 해제
        if (playerStats != null)
            playerStats.OnGoldChanged -= UpdateGoldText;
    }
    
    // 특정 아이템의 업그레이드 레벨 가져오기 (외부에서 사용)
    public int GetItemLevel(ItemSO item)
    {
        ItemUpgradeData data = System.Array.Find(upgradeableItems, d => d.item == item);
        return data != null ? data.level : 1;
    }
    
    // 특정 아이템의 현재 스탯 가져오기 (외부에서 사용)
    public float GetItemStat(ItemSO item)
    {
        ItemUpgradeData data = System.Array.Find(upgradeableItems, d => d.item == item);
        if (data != null)
        {
            // 스탯 계산
            if (item is WeaponSO weapon)
            {
                return weapon.attackDamage * (1 + (data.level - 1) * weaponDamageIncreasePerLevel);
            }
            else if (item is ArmorSO armor)
            {
                return armor.defense * (1 + (data.level - 1) * armorDefenseIncreasePerLevel);
            }
        }
        
        return 0;
    }
} 