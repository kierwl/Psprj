using UnityEngine;
using System.Collections.Generic;
using System;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [System.Serializable]
    public class InventoryItem
    {
        public ItemSO item;
        public int amount;
        public int slotIndex = -1; // 인벤토리 슬롯 인덱스
        public bool isEquipped = false; // 장착 여부
    }

    [Header("인벤토리 설정")]
    public List<InventoryItem> items = new List<InventoryItem>();
    public int maxSlots = 20;
    public bool autoSort = false;


    public event Action<ItemSO, int> OnItemAdded;
    public event Action<ItemSO, int> OnItemRemoved;
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
            Destroy(gameObject);

        // 슬롯 인덱스 초기화
        for (int i = 0; i < items.Count; i++)
        {
            items[i].slotIndex = i;
        }

        Debug.Log("Inventory 초기화됨");
    }

    public bool AddItem(ItemSO item, int amount = 1)
    {
        if (item == null) return false;

        // 인벤토리가 가득 찼는지 확인
        if (items.Count >= maxSlots && !HasItem(item))
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다!");
            return false;
        }

        // 이미 있는 아이템인지 확인
        InventoryItem existingItem = items.Find(x => x.item == item);
        if (existingItem != null)
        {
            existingItem.amount += amount;
        }
        else
        {
            // 빈 슬롯 찾기
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1)
            {
                Debug.LogWarning("인벤토리에 빈 슬롯이 없습니다!");
                return false;
            }

            var newItem = new InventoryItem { item = item, amount = amount, slotIndex = emptySlot };
            items.Add(newItem);

            if (autoSort)
                SortInventory();
        }

        Debug.Log($"아이템 획득: {item.name} x{amount}");
        OnItemAdded?.Invoke(item, amount);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemSO item, int amount = 1)
    {
        if (item == null) return false;

        InventoryItem existingItem = items.Find(x => x.item == item);
        if (existingItem != null && existingItem.amount >= amount)
        {
            existingItem.amount -= amount;
            if (existingItem.amount <= 0)
            {
                items.Remove(existingItem);
            }

            OnItemRemoved?.Invoke(item, amount);
            OnInventoryChanged?.Invoke();
            Debug.Log($"아이템 제거: {item.name} x{amount}");
            return true;
        }

        return false;
    }

    public bool HasItem(ItemSO item)
    {
        return items.Exists(x => x.item == item);
    }

    public int GetItemAmount(ItemSO item)
    {
        InventoryItem existingItem = items.Find(x => x.item == item);
        return existingItem != null ? existingItem.amount : 0;
    }

    public void ClearInventory()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }

    public int FindEmptySlot()
    {
        // 현재 사용중인 슬롯 인덱스 목록 가져오기
        HashSet<int> usedSlots = new HashSet<int>();
        foreach (var item in items)
        {
            if (item.slotIndex >= 0)
                usedSlots.Add(item.slotIndex);
        }

        // 비어있는 첫 번째 슬롯 찾기
        for (int i = 0; i < maxSlots; i++)
        {
            if (!usedSlots.Contains(i))
                return i;
        }

        return -1; // 빈 슬롯이 없음
    }

    public void SortInventory()
    {
        // 아이템 유형 및 희귀도 기준으로 정렬
        items.Sort((a, b) => {
            // 먼저 아이템 유형별로 정렬
            if (a.item.itemType != b.item.itemType)
                return a.item.itemType.CompareTo(b.item.itemType);

            // 그 다음 희귀도로 정렬 (높은 희귀도가 먼저)
            if (a.item.rarity != b.item.rarity)
                return b.item.rarity.CompareTo(a.item.rarity);

            // 마지막으로 이름으로 정렬
            return a.item.itemName.CompareTo(b.item.itemName);
        });

        // 정렬 후 슬롯 인덱스 재할당
        for (int i = 0; i < items.Count; i++)
        {
            items[i].slotIndex = i;
        }

        OnInventoryChanged?.Invoke();
    }

    public InventoryItem GetItemAtSlot(int slotIndex)
    {
        return items.Find(item => item.slotIndex == slotIndex);
    }

    public bool SwapItems(int slotIndexA, int slotIndexB)
    {
        InventoryItem itemA = GetItemAtSlot(slotIndexA);
        InventoryItem itemB = GetItemAtSlot(slotIndexB);

        if (itemA == null && itemB == null)
            return false;

        if (itemA != null)
            itemA.slotIndex = slotIndexB;

        if (itemB != null)
            itemB.slotIndex = slotIndexA;

        OnInventoryChanged?.Invoke();
        return true;
    }

    public List<InventoryItem> GetItemsByType(ItemSO.ItemType type)
    {
        return items.FindAll(item => item.item.itemType == type);
    }

    // 아이템 장착 메서드
    public bool EquipItem(int slotIndex)
    {
        InventoryItem item = GetItemAtSlot(slotIndex);
        if (item == null || item.isEquipped) return false;

        // 같은 유형의 다른 장착 아이템이 있는지 확인하고 해제
        foreach (var otherItem in items)
        {
            if (otherItem.isEquipped && otherItem.item.itemType == item.item.itemType)
            {
                otherItem.isEquipped = false;
            }
        }

        item.isEquipped = true;
        OnInventoryChanged?.Invoke();
        return true;
    }

    // 아이템 장착 해제 메서드
    public bool UnequipItem(int slotIndex)
    {
        InventoryItem item = GetItemAtSlot(slotIndex);
        if (item == null || !item.isEquipped) return false;

        item.isEquipped = false;
        OnInventoryChanged?.Invoke();
        return true;
    }

    // 특정 아이템이 장착되어 있는지 확인
    public bool IsItemEquipped(ItemSO item)
    {
        return items.Exists(i => i.item == item && i.isEquipped);
    }

    // 특정 슬롯의 아이템이 장착되어 있는지 확인
    public bool IsSlotEquipped(int slotIndex)
    {
        InventoryItem item = GetItemAtSlot(slotIndex);
        return item != null && item.isEquipped;
    }

    // 인벤토리 변경 이벤트 수동 발생 (외부에서 호출)
    public void NotifyInventoryChanged()
    {
        Debug.Log("인벤토리 변경 알림 발생");
        OnInventoryChanged?.Invoke();
    }
}