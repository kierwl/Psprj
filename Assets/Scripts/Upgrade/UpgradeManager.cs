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
    [SerializeField] public ItemUpgradeData[] upgradeableItems;

    [Header("플레이어 참조")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Inventory inventory;

    private ItemUpgradeData selectedItem;

    // UI 패널 캐싱용 변수
    private static GameObject _staticUpgradePanel;

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
        Debug.Log("UpgradeManager.Awake 호출됨");

        // 싱글톤 패턴 적용
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("UpgradeManager 초기화됨");

        // 업그레이드 패널을 정적 변수에 캐싱
        if (upgradePanel != null)
        {
            _staticUpgradePanel = upgradePanel;
        }

        // 필수 참조 초기화
        InitializeReferences();
    }

    private void InitializeReferences()
    {
        // 플레이어 스탯 초기화
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogWarning("PlayerStats를 찾을 수 없습니다.");
            }
        }

        // 인벤토리 초기화
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
            if (inventory == null)
            {
                Debug.LogWarning("Inventory를 찾을 수 없습니다.");
            }
        }

        // 업그레이드 패널 초기화
        if (upgradePanel == null)
        {
            // 정적 변수에서 복원 시도
            if (_staticUpgradePanel != null)
            {
                upgradePanel = _staticUpgradePanel;
                Debug.Log("정적 변수에서 upgradePanel 복원됨");
            }
            else
            {
                // 씬에서 찾기 시도
                GameObject panel = GameObject.Find("UpgradePanel");
                if (panel != null)
                {
                    upgradePanel = panel;
                    _staticUpgradePanel = panel;
                    Debug.Log("씬에서 UpgradePanel을 찾았습니다.");
                }
                else
                {
                    Debug.LogError("upgradePanel을 찾을 수 없습니다!");
                }
            }
        }

        // 버튼 이벤트 등록
        InitializeButtons();

        // UI 요소 초기화
        InitializeUIElements();

        // upgradeableItems 체크
        CheckUpgradeableItems();
    }

    private void InitializeButtons()
    {
        // 업그레이드 버튼 이벤트 연결
        if (upgradeButton != null)
        {
            // 기존 리스너 제거 (중복 방지)
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(UpgradeSelectedItem);
        }
        else
        {
            Debug.LogWarning("upgradeButton이 null입니다!");

            // 업그레이드 패널 내에서 버튼 찾기 시도
            if (upgradePanel != null)
            {
                Button[] buttons = upgradePanel.GetComponentsInChildren<Button>(true);
                foreach (Button button in buttons)
                {
                    if (button.name.Contains("Upgrade") || button.name.Contains("업그레이드"))
                    {
                        upgradeButton = button;
                        upgradeButton.onClick.RemoveAllListeners();
                        upgradeButton.onClick.AddListener(UpgradeSelectedItem);
                        Debug.Log("upgradeButton을 자동으로 찾았습니다: " + button.name);
                        break;
                    }
                }
            }
        }

        // 닫기 버튼 이벤트 연결
        if (closeButton != null)
        {
            // 기존 리스너 제거 (중복 방지)
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseUpgradePanel);
        }
        else
        {
            Debug.LogWarning("closeButton이 null입니다!");

            // 업그레이드 패널 내에서 닫기 버튼 찾기 시도
            if (upgradePanel != null)
            {
                Button[] buttons = upgradePanel.GetComponentsInChildren<Button>(true);
                foreach (Button button in buttons)
                {
                    if (button.name.Contains("Close") || button.name.Contains("닫기"))
                    {
                        closeButton = button;
                        closeButton.onClick.RemoveAllListeners();
                        closeButton.onClick.AddListener(CloseUpgradePanel);
                        Debug.Log("closeButton을 자동으로 찾았습니다: " + button.name);
                        break;
                    }
                }
            }
        }
    }

    private void InitializeUIElements()
    {
        // 업그레이드 패널이 없으면 나머지 UI 요소 초기화가 불가능
        if (upgradePanel == null) return;

        // UI 요소가 할당되지 않은 경우 자동으로 찾기
        if (itemIcon == null)
        {
            Image[] images = upgradePanel.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.name.Contains("Icon") || img.transform.parent.name.Contains("Icon"))
                {
                    itemIcon = img;
                    Debug.Log("itemIcon을 자동으로 찾았습니다: " + img.name);
                    break;
                }
            }
        }

        // TextMeshProUGUI 요소들 초기화
        InitializeTextElements();
    }

    private void InitializeTextElements()
    {
        if (upgradePanel == null) return;

        TextMeshProUGUI[] texts = upgradePanel.GetComponentsInChildren<TextMeshProUGUI>(true);

        if (itemName == null)
        {
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.Contains("Name") || text.name.Contains("이름"))
                {
                    itemName = text;
                    Debug.Log("itemName을 자동으로 찾았습니다: " + text.name);
                    break;
                }
            }
        }

        if (currentStats == null)
        {
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.Contains("Current") || text.name.Contains("현재"))
                {
                    currentStats = text;
                    Debug.Log("currentStats을 자동으로 찾았습니다: " + text.name);
                    break;
                }
            }
        }

        if (nextLevelStats == null)
        {
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.Contains("Next") || text.name.Contains("다음"))
                {
                    nextLevelStats = text;
                    Debug.Log("nextLevelStats을 자동으로 찾았습니다: " + text.name);
                    break;
                }
            }
        }

        if (upgradeCost == null)
        {
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.Contains("Cost") || text.name.Contains("비용"))
                {
                    upgradeCost = text;
                    Debug.Log("upgradeCost를 자동으로 찾았습니다: " + text.name);
                    break;
                }
            }
        }

        if (playerGold == null)
        {
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.Contains("Gold") || text.name.Contains("골드"))
                {
                    playerGold = text;
                    Debug.Log("playerGold를 자동으로 찾았습니다: " + text.name);
                    break;
                }
            }
        }
    }

    private void CheckUpgradeableItems()
    {
        if (upgradeableItems == null || upgradeableItems.Length == 0)
        {
            Debug.LogWarning("upgradeableItems 배열이 비어 있습니다. 기본 배열을 생성합니다.");
            upgradeableItems = new ItemUpgradeData[0];
        }
    }

    private void Start()
    {
        Debug.Log("UpgradeManager.Start 호출됨");

        // 골드 변경 이벤트 구독
        if (playerStats != null)
        {
            playerStats.OnGoldChanged += UpdateGoldText;
            Debug.Log("플레이어 골드 변경 이벤트 구독됨");
        }
        else
        {
            Debug.LogError("playerStats가 null입니다!");
        }

        // 초기 UI 설정
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Debug.Log("업그레이드 패널 비활성화됨");
        }
        else
        {
            Debug.LogError("upgradePanel이 null입니다!");
        }
    }

    public void OpenUpgradePanel(ItemSO item)
    {
        Debug.Log($"OpenUpgradePanel 호출됨: {(item != null ? item.itemName : "null")}");

        if (item == null)
        {
            Debug.LogError("업그레이드할 아이템이 null입니다!");
            return;
        }

        // 참조가 누락되었을 경우 다시 초기화 시도
        if (upgradePanel == null || upgradeButton == null)
        {
            Debug.LogWarning("UI 요소가 null입니다. 다시 초기화합니다.");
            InitializeReferences();
        }

        // upgradePanel이 여전히 null이면 치명적인 오류
        if (upgradePanel == null)
        {
            Debug.LogError("upgradePanel이 여전히 null입니다. 업그레이드 패널을 열 수 없습니다!");
            return;
        }

        // upgradeableItems가 null인지 확인
        if (upgradeableItems == null || upgradeableItems.Length == 0)
        {
            Debug.LogWarning("upgradeableItems 배열이 비어 있습니다. 기본 배열을 생성합니다.");
            upgradeableItems = new ItemUpgradeData[1];
            upgradeableItems[0] = new ItemUpgradeData { item = item, level = 1, baseCost = 100 };
            selectedItem = upgradeableItems[0];
        }
        else
        {
            // 선택된 아이템의 업그레이드 데이터 찾기
            selectedItem = System.Array.Find(upgradeableItems, data => data != null && data.item == item);

            // 없으면 임시로 생성
            if (selectedItem == null)
            {
                Debug.LogWarning($"아이템 {item.itemName}을(를) 위한 업그레이드 데이터가 없습니다. 임시 데이터를 생성합니다.");

                // 기존 배열 확장
                System.Collections.Generic.List<ItemUpgradeData> itemsList = new System.Collections.Generic.List<ItemUpgradeData>(upgradeableItems);
                itemsList.Add(new ItemUpgradeData { item = item, level = 1, baseCost = 100 });

                // 업데이트된 배열로 교체
                upgradeableItems = itemsList.ToArray();
                selectedItem = upgradeableItems[upgradeableItems.Length - 1];
            }
        }

        // UI 활성화 및 업데이트
        try
        {
            upgradePanel.SetActive(true);
            Debug.Log("업그레이드 패널 활성화됨");

            // UI 업데이트 (예외 처리 추가)
            UpdateUpgradeUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"업그레이드 패널 열기/업데이트 실패: {e.Message}\n{e.StackTrace}");
        }
    }

    public void CloseUpgradePanel()
    {
        Debug.Log("CloseUpgradePanel 호출됨");

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Debug.Log("업그레이드 패널 비활성화됨");
        }
        else
        {
            Debug.LogError("upgradePanel이 null입니다!");
        }
    }

    private void UpdateUpgradeUI()
    {
        if (selectedItem == null)
        {
            Debug.LogError("UpdateUpgradeUI: selectedItem이 null입니다!");
            return;
        }

        // null 체크 후 UI 업데이트
        UpdateItemIcon();
        UpdateItemName();
        UpdateItemStats();
        UpdateCostAndButton();
    }

    private void UpdateItemIcon()
    {
        if (itemIcon != null && selectedItem.item != null && selectedItem.item.icon != null)
        {
            itemIcon.sprite = selectedItem.item.icon;
        }
        else
        {
            if (itemIcon == null)
                Debug.LogWarning("itemIcon이 null입니다!");
            else if (selectedItem.item == null)
                Debug.LogWarning("selectedItem.item이 null입니다!");
            else if (selectedItem.item.icon == null)
                Debug.LogWarning($"{selectedItem.item.name}의 icon이 null입니다!");
        }
    }

    private void UpdateItemName()
    {
        if (itemName != null && selectedItem.item != null)
        {
            itemName.text = $"{selectedItem.item.itemName} (Lv. {selectedItem.level})";
        }
        else
        {
            Debug.LogWarning("itemName이 null이거나 selectedItem.item이 null입니다!");
        }
    }

    private void UpdateItemStats()
    {
        // 아이템 타입에 따른 스탯 계산
        try
        {
            CalculateItemStats();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"스탯 계산 실패: {e.Message}");

            // 기본값 설정
            selectedItem.currentStat = 0;
            selectedItem.nextLevelStat = 0;
        }

        // 현재 및 다음 레벨 스탯 표시
        if (currentStats != null)
        {
            currentStats.text = GetItemStatText(selectedItem.currentStat);
        }

        if (nextLevelStats != null)
        {
            if (selectedItem.level < maxUpgradeLevel)
                nextLevelStats.text = $"다음 레벨: {GetItemStatText(selectedItem.nextLevelStat)}";
            else
                nextLevelStats.text = "최대 레벨";
        }
    }

    private void UpdateCostAndButton()
    {
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
            bool canUpgrade = selectedItem.level < maxUpgradeLevel &&
                              (playerStats != null && playerStats.GetGold() >= cost);
            upgradeButton.interactable = canUpgrade;
        }

        // 골드 표시 갱신
        if (playerStats != null)
            UpdateGoldText(playerStats.GetGold());
    }

    private void CalculateItemStats()
    {
        if (selectedItem == null || selectedItem.item == null)
        {
            Debug.LogError("CalculateItemStats: selectedItem 또는 item이 null입니다!");
            return;
        }

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
        else
        {
            // 기본값 설정
            selectedItem.currentStat = 0;
            selectedItem.nextLevelStat = 0;
            Debug.LogWarning($"지원되지 않는 아이템 타입: {selectedItem.item.GetType().Name}");
        }
    }

    private string GetItemStatText(float statValue)
    {
        if (selectedItem.item is WeaponSO)
            return $"공격력: {statValue:F1}";
        else if (selectedItem.item is ArmorSO)
            return $"방어력: {statValue:F1}";
        else
            return $"스탯: {statValue:F1}";
    }

    private int CalculateUpgradeCost()
    {
        if (selectedItem == null)
        {
            Debug.LogError("CalculateUpgradeCost: selectedItem이 null입니다!");
            return 0;
        }

        // 기본 비용 * (승수 ^ (레벨 - 1))
        return Mathf.RoundToInt(selectedItem.baseCost * Mathf.Pow(costMultiplier, selectedItem.level - 1));
    }

    private void UpgradeSelectedItem()
    {
        // 예외 처리 추가
        try
        {
            if (selectedItem == null)
            {
                Debug.LogError("UpgradeSelectedItem: selectedItem이 null입니다!");
                return;
            }

            if (selectedItem.level >= maxUpgradeLevel)
            {
                Debug.LogWarning("이미 최대 레벨입니다!");
                return;
            }

            int cost = CalculateUpgradeCost();

            // PlayerStats null 체크
            if (playerStats == null)
            {
                Debug.LogError("playerStats가 null입니다!");
                return;
            }

            // 골드 확인 및 차감
            if (playerStats.SpendGold(cost))
            {
                // 레벨 증가
                selectedItem.level++;
                Debug.Log($"{selectedItem.item.itemName}이(가) 레벨 {selectedItem.level}로 업그레이드되었습니다.");

                // UI 갱신
                UpdateUpgradeUI();

                // 인벤토리 UI 갱신 (아이템 스탯 변경을 반영하기 위해)
                if (inventory != null)
                {
                    try
                    {
                        inventory.NotifyInventoryChanged();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"인벤토리 UI 갱신 실패: {e.Message}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("골드가 부족합니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"업그레이드 처리 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }

    public ItemUpgradeData[] GetUpgradeableItems()
    {
        return upgradeableItems;
    }


    private void UpdateGoldText(int amount)
    {
        if (playerGold != null)
        {
            playerGold.text = $"보유 골드: {amount}";
        }
    }

    public void SetItemLevel(ItemSO item, int level, int baseCost)
    {
        if (item == null)
        {
            Debug.LogError("SetItemLevel: 아이템이 null입니다!");
            return;
        }

        // 업그레이드 데이터 찾기
        ItemUpgradeData data = System.Array.Find(upgradeableItems, d => d != null && d.item == item);

        // 데이터가 없으면 새로 생성
        if (data == null)
        {
            Debug.Log($"아이템 {item.name}의 강화 데이터가 없어 새로 생성합니다.");

            // 기존 배열 확장
            System.Collections.Generic.List<ItemUpgradeData> itemsList = new System.Collections.Generic.List<ItemUpgradeData>(upgradeableItems);
            itemsList.Add(new ItemUpgradeData
            {
                item = item,
                level = level,
                baseCost = baseCost
            });

            // 업데이트된 배열로 교체
            upgradeableItems = itemsList.ToArray();
        }
        else
        {
            // 기존 데이터 업데이트
            data.level = level;
            data.baseCost = baseCost;
        }

        Debug.Log($"아이템 {item.name}의 강화 레벨이 {level}로 설정되었습니다.");
    }

    private void OnDestroy()
    {
        // 이벤트 등록 해제
        if (playerStats != null)
        {
            playerStats.OnGoldChanged -= UpdateGoldText;
        }

        // 싱글톤 인스턴스 정리
        if (instance == this)
        {
            instance = null;
        }
    }

    // 특정 아이템의 업그레이드 레벨 가져오기 (외부에서 사용)
    public int GetItemLevel(ItemSO item)
    {
        if (item == null)
        {
            Debug.LogWarning("GetItemLevel: 아이템이 null입니다!");
            return 1;
        }

        // 업그레이드 가능 아이템 배열 체크
        if (upgradeableItems == null || upgradeableItems.Length == 0)
        {
            Debug.LogWarning("GetItemLevel: upgradeableItems 배열이 비어 있습니다!");
            return 1;
        }

        ItemUpgradeData data = System.Array.Find(upgradeableItems, d => d != null && d.item == item);
        return data != null ? data.level : 1;
    }

    // 특정 아이템의 현재 스탯 가져오기 (외부에서 사용)
    public float GetItemStat(ItemSO item)
    {
        if (item == null)
        {
            Debug.LogWarning("GetItemStat: 아이템이 null입니다!");
            return 0;
        }

        // 업그레이드 가능 아이템 배열 체크
        if (upgradeableItems == null || upgradeableItems.Length == 0)
        {
            Debug.LogWarning("GetItemStat: upgradeableItems 배열이 비어 있습니다!");
            return 0;
        }

        ItemUpgradeData data = System.Array.Find(upgradeableItems, d => d != null && d.item == item);
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