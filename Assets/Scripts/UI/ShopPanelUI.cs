using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopPanelUI : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject shopPanel;
    public Button closeButton;
    public TabGroup tabGroup;

    [Header("Shop Items Grid")]
    public Transform itemsContainer;
    public GameObject shopItemPrefab;

    [Header("Ad Rewards")]
    public Button watchAdButton;
    public TextMeshProUGUI adRewardText;

    [System.Serializable]
    public class TabGroup
    {
        public Button gemsTab;
        public Button goldTab;
        public Button packagesTab;
        public Button specialTab;

        public void Initialize(ShopPanelUI panel)
        {
            gemsTab.onClick.AddListener(() => panel.ShowGemsShop());
            goldTab.onClick.AddListener(() => panel.ShowGoldShop());
            packagesTab.onClick.AddListener(() => panel.ShowPackagesShop());
            specialTab.onClick.AddListener(() => panel.ShowSpecialShop());
        }

        public void SetActiveTab(int tabIndex)
        {
            // 모든 탭 비활성화 시각 효과
            gemsTab.GetComponent<Image>().color = Color.gray;
            goldTab.GetComponent<Image>().color = Color.gray;
            packagesTab.GetComponent<Image>().color = Color.gray;
            specialTab.GetComponent<Image>().color = Color.gray;

            // 선택된 탭 활성화 시각 효과
            switch (tabIndex)
            {
                case 0:
                    gemsTab.GetComponent<Image>().color = Color.white;
                    break;
                case 1:
                    goldTab.GetComponent<Image>().color = Color.white;
                    break;
                case 2:
                    packagesTab.GetComponent<Image>().color = Color.white;
                    break;
                case 3:
                    specialTab.GetComponent<Image>().color = Color.white;
                    break;
            }
        }
    }

    [System.Serializable]
    public class ShopItem
    {
        public string id;
        public string title;
        public string description;
        public Sprite icon;
        public int price;
        public string currencyType; // "gold", "gems", "cash"
        public int amount;
        public bool isSpecial;
        public bool isLimited;
    }

    public List<ShopItem> gemsItems = new List<ShopItem>();
    public List<ShopItem> goldItems = new List<ShopItem>();
    public List<ShopItem> packageItems = new List<ShopItem>();
    public List<ShopItem> specialItems = new List<ShopItem>();

    private void Start()
    {
        // 이벤트 초기화
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);


        // 탭 그룹 초기화
        tabGroup.Initialize(this);

        // 초기 탭 표시
        ShowGemsShop();

        // 패널 숨기기
        gameObject.SetActive(false);
    }

    public void ShowGemsShop()
    {
        tabGroup.SetActiveTab(0);
        UpdateShopItems(gemsItems);
    }

    public void ShowGoldShop()
    {
        tabGroup.SetActiveTab(1);
        UpdateShopItems(goldItems);
    }

    public void ShowPackagesShop()
    {
        tabGroup.SetActiveTab(2);
        UpdateShopItems(packageItems);
    }

    public void ShowSpecialShop()
    {
        tabGroup.SetActiveTab(3);
        UpdateShopItems(specialItems);
    }

    private void UpdateShopItems(List<ShopItem> items)
    {
        // 기존 아이템 제거
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // 새 아이템 생성
        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(shopItemPrefab, itemsContainer);

            // 아이템 정보 설정
            Image itemIcon = itemObj.transform.Find("ItemIcon").GetComponent<Image>();
            TextMeshProUGUI titleText = itemObj.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = itemObj.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = itemObj.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
            Image currencyIcon = itemObj.transform.Find("CurrencyIcon").GetComponent<Image>();
            GameObject specialTag = itemObj.transform.Find("SpecialTag").gameObject;
            GameObject limitedTag = itemObj.transform.Find("LimitedTag").gameObject;

            itemIcon.sprite = item.icon;
            titleText.text = item.title;
            descText.text = item.description;
            priceText.text = item.price.ToString();

            // 통화 아이콘 설정
            if (item.currencyType == "gold")
                currencyIcon.sprite = Resources.Load<Sprite>("UI/GoldIcon");
            else if (item.currencyType == "gems")
                currencyIcon.sprite = Resources.Load<Sprite>("UI/GemIcon");
            else
                currencyIcon.sprite = Resources.Load<Sprite>("UI/CashIcon");

            // 특별/한정 태그 설정
            specialTag.SetActive(item.isSpecial);
            limitedTag.SetActive(item.isLimited);

            // 구매 버튼 이벤트
            Button buyButton = itemObj.transform.Find("BuyButton").GetComponent<Button>();
            string itemId = item.id; // 클로저 문제 방지
            buyButton.onClick.AddListener(() => PurchaseItem(itemId));
        }
    }

    private void PurchaseItem(string itemId)
    {
        // 모든 상품 목록에서 아이템 찾기
        ShopItem item = FindItemById(itemId);

        if (item == null)
            return;

        // 구매 처리
        bool purchaseSuccess = false;

        if (item.currencyType == "gold")
        {
            purchaseSuccess = CurrencyManager.instance.SpendGold(item.price);
        }
        else if (item.currencyType == "gems")
        {
            purchaseSuccess = CurrencyManager.instance.SpendGems(item.price);
        }

        if (purchaseSuccess)
        {
            // 아이템 구매 보상 지급
            GiveItemReward(item);
        }
        else
        {
            // 통화 부족 메시지
            Debug.Log(item.currencyType + " 부족으로 구매 실패");
            // 통화 부족 알림 UI 표시
        }
    }

    private ShopItem FindItemById(string itemId)
    {
        // 모든 상품 목록에서 검색
        foreach (var item in gemsItems)
            if (item.id == itemId) return item;

        foreach (var item in goldItems)
            if (item.id == itemId) return item;

        foreach (var item in packageItems)
            if (item.id == itemId) return item;

        foreach (var item in specialItems)
            if (item.id == itemId) return item;

        return null;
    }

    private void GiveItemReward(ShopItem item)
    {
        // 아이템 타입에 따라 보상 지급
        if (item.id.StartsWith("gems_"))
        {
            CurrencyManager.instance.AddGems(item.amount);
            Debug.Log(item.amount + " 보석 획득!");
        }
        else if (item.id.StartsWith("gold_"))
        {
            CurrencyManager.instance.AddGold(item.amount);
            Debug.Log(item.amount + " 골드 획득!");
        }
        else if (item.id.StartsWith("energy_"))
        {
            CurrencyManager.instance.AddEnergy(item.amount);
            Debug.Log(item.amount + " 에너지 획득!");
        }
        else if (item.id.StartsWith("package_"))
        {
            // 패키지 보상 (여러 종류의 아이템)
            CurrencyManager.instance.AddGems(item.amount / 10);
            CurrencyManager.instance.AddGold(item.amount);
            Debug.Log("패키지 획득: " + item.amount / 10 + " 보석, " + item.amount + " 골드");
        }
    }


    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}