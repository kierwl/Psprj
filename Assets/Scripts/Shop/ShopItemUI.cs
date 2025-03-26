using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Button itemButton;

    private ItemSO currentItem;
    private int currentPrice;

    // 아이템 클릭 이벤트
    public event Action<ItemSO, int> OnItemClicked;

    private void Awake()
    {
        // 버튼 이벤트 등록
        if (itemButton != null)
            itemButton.onClick.AddListener(HandleItemClick);
        else
            Debug.LogError("itemButton이 할당되지 않았습니다!", this);
    }

    public void SetItem(ItemSO item, int price)
    {
        if (item == null)
        {
            Debug.LogError("SetItem에 null 아이템이 전달되었습니다!", this);
            return;
        }

        currentItem = item;
        currentPrice = price;

        // UI 요소 업데이트
        if (itemIcon != null)
            itemIcon.sprite = item.icon;
        else
            Debug.LogWarning("itemIcon이 할당되지 않았습니다!", this);

        if (itemName != null)
            itemName.text = item.itemName;
        else
            Debug.LogWarning("itemName이 할당되지 않았습니다!", this);

        if (itemPrice != null)
            itemPrice.text = price.ToString() + " 골드";
        else
            Debug.LogWarning("itemPrice가 할당되지 않았습니다!", this);

        // 디버그 로그
        Debug.Log($"아이템 설정됨: {item.itemName}, 가격: {price}");
    }

    private void HandleItemClick()
    {
        if (currentItem != null)
        {
            OnItemClicked?.Invoke(currentItem, currentPrice);
            Debug.Log($"아이템 클릭됨: {currentItem.itemName}");
        }
        else
        {
            Debug.LogWarning("클릭된 아이템이 null입니다!", this);
        }
    }

    private void OnDestroy()
    {
        // 버튼 이벤트 해제
        if (itemButton != null)
            itemButton.onClick.RemoveListener(HandleItemClick);
    }
}