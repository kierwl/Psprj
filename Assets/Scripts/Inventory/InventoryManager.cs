using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    
    [Header("인벤토리 참조")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventoryUI inventoryUI;
    
    [Header("입력 설정")]
    [SerializeField] private KeyCode toggleInventoryKey = KeyCode.I;
    [SerializeField] private string toggleInventoryButton = "Inventory"; // 입력 시스템 사용시
    
    [Header("디버그 도구")]
    [SerializeField] private ItemSO[] debugItems;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
            
        // 인스턴스 참조 확인
        if (inventory == null)
            inventory = GetComponentInChildren<Inventory>();
            
        if (inventoryUI == null)
            inventoryUI = GetComponentInChildren<InventoryUI>();
    }
    
    private void Update()
    {
        // 키보드 입력으로 인벤토리 토글
        if (Input.GetKeyDown(toggleInventoryKey))
        {
            ToggleInventory();
        }
        
    }
    
    public void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.ToggleInventory();
        }
    }
    
    // 새 아이템 추가 메서드 (외부 스크립트에서 사용)
    public bool AddItem(ItemSO item, int amount = 1)
    {
        if (inventory != null && item != null)
        {
            return inventory.AddItem(item, amount);
        }
        return false;
    }
    
    // 아이템 제거 메서드 (외부 스크립트에서 사용)
    public bool RemoveItem(ItemSO item, int amount = 1)
    {
        if (inventory != null && item != null)
        {
            return inventory.RemoveItem(item, amount);
        }
        return false;
    }
    
    // 인벤토리에 아이템이 있는지 확인 (외부 스크립트에서 사용)
    public bool HasItem(ItemSO item)
    {
        if (inventory != null && item != null)
        {
            return inventory.HasItem(item);
        }
        return false;
    }
    
    // 디버그: 테스트 아이템 추가
    [ContextMenu("Add Debug Items")]
    public void AddDebugItems()
    {
        if (debugItems != null && debugItems.Length > 0)
        {
            foreach (var item in debugItems)
            {
                AddItem(item, Random.Range(1, 5));
            }
        }
    }
    
    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        if (inventory != null)
        {
            inventory.ClearInventory();
        }
    }
} 