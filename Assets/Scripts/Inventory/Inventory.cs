using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [System.Serializable]
    public class InventoryItem
    {
        public ItemSO item;
        public int amount;
    }

    public List<InventoryItem> items = new List<InventoryItem>();
    public int maxSlots = 20;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
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
            items.Add(new InventoryItem { item = item, amount = amount });
        }

        Debug.Log($"아이템 획득: {item.name} x{amount}");
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
    }
} 