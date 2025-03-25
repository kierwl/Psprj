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
            // ��� �� ��Ȱ��ȭ �ð� ȿ��
            gemsTab.GetComponent<Image>().color = Color.gray;
            goldTab.GetComponent<Image>().color = Color.gray;
            packagesTab.GetComponent<Image>().color = Color.gray;
            specialTab.GetComponent<Image>().color = Color.gray;

            // ���õ� �� Ȱ��ȭ �ð� ȿ��
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
        // �̺�Ʈ �ʱ�ȭ
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);


        // �� �׷� �ʱ�ȭ
        tabGroup.Initialize(this);

        // �ʱ� �� ǥ��
        ShowGemsShop();

        // �г� �����
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
        // ���� ������ ����
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // �� ������ ����
        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(shopItemPrefab, itemsContainer);

            // ������ ���� ����
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

            // ��ȭ ������ ����
            if (item.currencyType == "gold")
                currencyIcon.sprite = Resources.Load<Sprite>("UI/GoldIcon");
            else if (item.currencyType == "gems")
                currencyIcon.sprite = Resources.Load<Sprite>("UI/GemIcon");
            else
                currencyIcon.sprite = Resources.Load<Sprite>("UI/CashIcon");

            // Ư��/���� �±� ����
            specialTag.SetActive(item.isSpecial);
            limitedTag.SetActive(item.isLimited);

            // ���� ��ư �̺�Ʈ
            Button buyButton = itemObj.transform.Find("BuyButton").GetComponent<Button>();
            string itemId = item.id; // Ŭ���� ���� ����
            buyButton.onClick.AddListener(() => PurchaseItem(itemId));
        }
    }

    private void PurchaseItem(string itemId)
    {
        // ��� ��ǰ ��Ͽ��� ������ ã��
        ShopItem item = FindItemById(itemId);

        if (item == null)
            return;

        // ���� ó��
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
            // ������ ���� ���� ����
            GiveItemReward(item);
        }
        else
        {
            // ��ȭ ���� �޽���
            Debug.Log(item.currencyType + " �������� ���� ����");
            // ��ȭ ���� �˸� UI ǥ��
        }
    }

    private ShopItem FindItemById(string itemId)
    {
        // ��� ��ǰ ��Ͽ��� �˻�
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
        // ������ Ÿ�Կ� ���� ���� ����
        if (item.id.StartsWith("gems_"))
        {
            CurrencyManager.instance.AddGems(item.amount);
            Debug.Log(item.amount + " ���� ȹ��!");
        }
        else if (item.id.StartsWith("gold_"))
        {
            CurrencyManager.instance.AddGold(item.amount);
            Debug.Log(item.amount + " ��� ȹ��!");
        }
        else if (item.id.StartsWith("energy_"))
        {
            CurrencyManager.instance.AddEnergy(item.amount);
            Debug.Log(item.amount + " ������ ȹ��!");
        }
        else if (item.id.StartsWith("package_"))
        {
            // ��Ű�� ���� (���� ������ ������)
            CurrencyManager.instance.AddGems(item.amount / 10);
            CurrencyManager.instance.AddGold(item.amount);
            Debug.Log("��Ű�� ȹ��: " + item.amount / 10 + " ����, " + item.amount + " ���");
        }
    }


    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}