using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;

    [Header("아이템 데이터")]
    [SerializeField] private List<ItemSO> allItems = new List<ItemSO>();
    [SerializeField] private Transform itemsParent; // 씬에 생성된 아이템의 부모 객체

    // 아이템 ID와 아이템 참조를 매핑하는 사전
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

        // 아이템 사전 초기화
        InitializeItemsDictionary();
    }

    private void InitializeItemsDictionary()
    {
        itemsDict.Clear();

        // 직접 설정된 아이템들을 사전에 추가
        foreach (ItemSO item in allItems)
        {
            if (item != null && !itemsDict.ContainsKey(item.name))
            {
                itemsDict.Add(item.name, item);
            }
        }

        // Resources 폴더에서 모든 아이템 로드 (선택적)
        ItemSO[] resourceItems = Resources.LoadAll<ItemSO>("Items");
        foreach (ItemSO item in resourceItems)
        {
            if (item != null && !itemsDict.ContainsKey(item.name))
            {
                itemsDict.Add(item.name, item);
                allItems.Add(item);
            }
        }

        Debug.Log($"{itemsDict.Count}개의 아이템이 사전에 로드되었습니다.");
    }

    // ID로 아이템 찾기
    public ItemSO GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("아이템 ID가 null 또는 빈 문자열입니다.");
            return null;
        }

        // 사전에서 직접 찾기
        if (itemsDict.TryGetValue(itemId, out ItemSO item))
        {
            return item;
        }

        // 사전에 없는 경우, Resources에서 직접 로드 시도
        item = Resources.Load<ItemSO>("Items/" + itemId);
        if (item != null)
        {
            // 찾은 아이템을 사전에 추가
            itemsDict[itemId] = item;
            if (!allItems.Contains(item))
            {
                allItems.Add(item);
            }
            return item;
        }

        Debug.LogWarning($"ID '{itemId}'에 해당하는 아이템을 찾을 수 없습니다.");
        return null;
    }

    // 아이템 이름으로 아이템 찾기
    public ItemSO GetItemByName(string itemName)
    {
        foreach (ItemSO item in allItems)
        {
            if (item != null && item.itemName == itemName)
            {
                return item;
            }
        }
        Debug.LogWarning($"이름 '{itemName}'에 해당하는 아이템을 찾을 수 없습니다.");
        return null;
    }

    // 아이템 유형별로 아이템 찾기
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

    // 희귀도별로 아이템 찾기
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

    // 월드에 아이템 드롭 생성
    // ItemManager.cs의 DropItem 메서드를 수정

    public GameObject DropItem(ItemSO item, Vector3 position, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogError("드롭하려는 아이템이 null입니다.");
            return null;
        }

        // 아이템 프리팹이 없는 경우 기본 프리팹 사용
        GameObject itemPrefab = null;

        // 아이템에 dropPrefab이 설정되어 있는지 확인
        if (item.dropPrefab != null)
        {
            itemPrefab = item.dropPrefab;
        }
        else
        {
            // 기본 아이템 프리팹을 리소스에서 로드
            itemPrefab = Resources.Load<GameObject>("Prefabs/DefaultItemDrop");

            if (itemPrefab == null)
            {
                // 기본 프리팹도 없으면 간단한 큐브 생성
                Debug.LogWarning($"아이템 {item.itemName}의 dropPrefab이 설정되지 않았으며, 기본 프리팹도 찾을 수 없습니다. 간단한 큐브로 대체합니다.");

                // 기본 큐브 생성
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                itemPrefab = cube;
            }
        }

        // 아이템 인스턴스 생성
        GameObject itemInstance = Instantiate(itemPrefab, position, Quaternion.identity);

        // 부모 객체가 설정되어 있으면 해당 부모의 자식으로 설정
        if (itemsParent != null)
        {
            itemInstance.transform.SetParent(itemsParent);
        }

        // ItemDrop 컴포넌트에 아이템 정보 설정
        ItemDrop itemDrop = itemInstance.GetComponent<ItemDrop>();
        if (itemDrop != null)
        {
            itemDrop.Initialize(item, amount);
        }
        else
        {
            // ItemDrop 컴포넌트가 없는 경우 추가
            itemDrop = itemInstance.AddComponent<ItemDrop>();
            itemDrop.Initialize(item, amount);
        }

        return itemInstance;
    }

    // 다중 아이템 랜덤 드롭 (예: 몬스터 처치 보상)
    public List<GameObject> DropRandomItems(Vector3 position, int minCount = 1, int maxCount = 3, ItemSO.ItemRarity minRarity = ItemSO.ItemRarity.Common)
    {
        List<GameObject> droppedItems = new List<GameObject>();

        // 드롭할 아이템 수 결정
        int itemCount = Random.Range(minCount, maxCount + 1);

        // 해당 희귀도 이상의 아이템 목록 가져오기
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
            Debug.LogWarning("드롭 가능한 아이템이 없습니다.");
            return droppedItems;
        }

        // 아이템 랜덤 선택 및 드롭
        for (int i = 0; i < itemCount; i++)
        {
            // 랜덤 아이템 선택
            ItemSO selectedItem = eligibleItems[Random.Range(0, eligibleItems.Count)];

            // 아이템 주변에 약간의 랜덤 오프셋 적용
            Vector3 dropPosition = position + new Vector3(
                Random.Range(-0.5f, 0.5f),
                0.5f,
                Random.Range(-0.5f, 0.5f)
            );

            // 아이템 드롭
            GameObject droppedItem = DropItem(selectedItem, dropPosition, 1);
            if (droppedItem != null)
            {
                droppedItems.Add(droppedItem);
            }
        }

        return droppedItems;
    }
}