using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;

    [Header("������ ������")]
    [SerializeField] private List<ItemSO> allItems = new List<ItemSO>();
    [SerializeField] private Transform itemsParent; // ���� ������ �������� �θ� ��ü

    // ������ ID�� ������ ������ �����ϴ� ����
    private Dictionary<string, ItemSO> itemsDict = new Dictionary<string, ItemSO>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ������ ���� �ʱ�ȭ
        InitializeItemsDictionary();
    }

    private void InitializeItemsDictionary()
    {
        itemsDict.Clear();

        // ���� ������ �����۵��� ������ �߰�
        foreach (ItemSO item in allItems)
        {
            if (item != null && !itemsDict.ContainsKey(item.name))
            {
                itemsDict.Add(item.name, item);
            }
        }

        // Resources �������� ��� ������ �ε� (������)
        ItemSO[] resourceItems = Resources.LoadAll<ItemSO>("Items");
        foreach (ItemSO item in resourceItems)
        {
            if (item != null && !itemsDict.ContainsKey(item.name))
            {
                itemsDict.Add(item.name, item);
                allItems.Add(item);
            }
        }

        Debug.Log($"{itemsDict.Count}���� �������� ������ �ε�Ǿ����ϴ�.");
    }

    // ID�� ������ ã��
    public ItemSO GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("������ ID�� null �Ǵ� �� ���ڿ��Դϴ�.");
            return null;
        }

        // �������� ���� ã��
        if (itemsDict.TryGetValue(itemId, out ItemSO item))
        {
            return item;
        }

        // ������ ���� ���, Resources���� ���� �ε� �õ�
        item = Resources.Load<ItemSO>("Items/" + itemId);
        if (item != null)
        {
            // ã�� �������� ������ �߰�
            itemsDict[itemId] = item;
            if (!allItems.Contains(item))
            {
                allItems.Add(item);
            }
            return item;
        }

        Debug.LogWarning($"ID '{itemId}'�� �ش��ϴ� �������� ã�� �� �����ϴ�.");
        return null;
    }

    // ������ �̸����� ������ ã��
    public ItemSO GetItemByName(string itemName)
    {
        foreach (ItemSO item in allItems)
        {
            if (item != null && item.itemName == itemName)
            {
                return item;
            }
        }
        Debug.LogWarning($"�̸� '{itemName}'�� �ش��ϴ� �������� ã�� �� �����ϴ�.");
        return null;
    }

    // ������ �������� ������ ã��
    public List<ItemSO> GetItemsByType(ItemSO.ItemType type)
    {
        List<ItemSO> items = new List<ItemSO>();
        foreach (ItemSO item in allItems)
        {
            if (item != null && item.itemType == type)
            {
                items.Add(item);
            }
        }
        return items;
    }

    // ��͵����� ������ ã��
    public List<ItemSO> GetItemsByRarity(ItemSO.ItemRarity rarity)
    {
        List<ItemSO> items = new List<ItemSO>();
        foreach (ItemSO item in allItems)
        {
            if (item != null && item.rarity == rarity)
            {
                items.Add(item);
            }
        }
        return items;
    }

    // ���忡 ������ ��� ����
    // ItemManager.cs�� DropItem �޼��带 ����

    public GameObject DropItem(ItemSO item, Vector3 position, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogError("����Ϸ��� �������� null�Դϴ�.");
            return null;
        }

        // ������ �������� ���� ��� �⺻ ������ ���
        GameObject itemPrefab = null;

        // �����ۿ� dropPrefab�� �����Ǿ� �ִ��� Ȯ��
        if (item.dropPrefab != null)
        {
            itemPrefab = item.dropPrefab;
        }
        else
        {
            // �⺻ ������ �������� ���ҽ����� �ε�
            itemPrefab = Resources.Load<GameObject>("Prefabs/DefaultItemDrop");

            if (itemPrefab == null)
            {
                // �⺻ �����յ� ������ ������ ť�� ����
                Debug.LogWarning($"������ {item.itemName}�� dropPrefab�� �������� �ʾ�����, �⺻ �����յ� ã�� �� �����ϴ�. ������ ť��� ��ü�մϴ�.");

                // �⺻ ť�� ����
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                itemPrefab = cube;
            }
        }

        // ������ �ν��Ͻ� ����
        GameObject itemInstance = Instantiate(itemPrefab, position, Quaternion.identity);

        // �θ� ��ü�� �����Ǿ� ������ �ش� �θ��� �ڽ����� ����
        if (itemsParent != null)
        {
            itemInstance.transform.SetParent(itemsParent);
        }

        // ItemDrop ������Ʈ�� ������ ���� ����
        ItemDrop itemDrop = itemInstance.GetComponent<ItemDrop>();
        if (itemDrop != null)
        {
            itemDrop.Initialize(item, amount);
        }
        else
        {
            // ItemDrop ������Ʈ�� ���� ��� �߰�
            itemDrop = itemInstance.AddComponent<ItemDrop>();
            itemDrop.Initialize(item, amount);
        }

        return itemInstance;
    }

    // ���� ������ ���� ��� (��: ���� óġ ����)
    public List<GameObject> DropRandomItems(Vector3 position, int minCount = 1, int maxCount = 3, ItemSO.ItemRarity minRarity = ItemSO.ItemRarity.Common)
    {
        List<GameObject> droppedItems = new List<GameObject>();

        // ����� ������ �� ����
        int itemCount = Random.Range(minCount, maxCount + 1);

        // �ش� ��͵� �̻��� ������ ��� ��������
        List<ItemSO> eligibleItems = new List<ItemSO>();
        foreach (ItemSO item in allItems)
        {
            if (item != null && (int)item.rarity >= (int)minRarity)
            {
                eligibleItems.Add(item);
            }
        }

        if (eligibleItems.Count == 0)
        {
            Debug.LogWarning("��� ������ �������� �����ϴ�.");
            return droppedItems;
        }

        // ������ ���� ���� �� ���
        for (int i = 0; i < itemCount; i++)
        {
            // ���� ������ ����
            ItemSO selectedItem = eligibleItems[Random.Range(0, eligibleItems.Count)];

            // ������ �ֺ��� �ణ�� ���� ������ ����
            Vector3 dropPosition = position + new Vector3(
                Random.Range(-0.5f, 0.5f),
                0.5f,
                Random.Range(-0.5f, 0.5f)
            );

            // ������ ���
            GameObject droppedItem = DropItem(selectedItem, dropPosition, 1);
            if (droppedItem != null)
            {
                droppedItems.Add(droppedItem);
            }
        }

        return droppedItems;
    }
}