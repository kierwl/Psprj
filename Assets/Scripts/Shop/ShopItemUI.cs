using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopItemUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Image background;
    
    private ItemSO item;
    private int price;
    
    // 아이템 클릭 이벤트
    public event Action<ItemSO, int> OnItemClicked;
    
    public void SetItem(ItemSO newItem, int priceValue)
    {
        item = newItem;
        price = priceValue;
        
        if (itemIcon != null && item.icon != null)
            itemIcon.sprite = item.icon;
            
        if (itemName != null)
            itemName.text = item.itemName;
            
        if (itemPrice != null)
            itemPrice.text = $"{price} 골드";
            
        // 아이템 희귀도에 따른 배경색 설정
        if (background != null)
            background.color = GetRarityColor(item.rarity);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnItemClicked?.Invoke(item, price);
    }
    
    private Color GetRarityColor(ItemSO.ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemSO.ItemRarity.Common:
                return new Color(0.8f, 0.8f, 0.8f, 0.5f);
            case ItemSO.ItemRarity.Uncommon:
                return new Color(0.0f, 0.8f, 0.0f, 0.5f);
            case ItemSO.ItemRarity.Rare:
                return new Color(0.0f, 0.0f, 1.0f, 0.5f);
            case ItemSO.ItemRarity.Epic:
                return new Color(0.8f, 0.0f, 0.8f, 0.5f);
            case ItemSO.ItemRarity.Legendary:
                return new Color(1.0f, 0.8f, 0.0f, 0.5f);
            default:
                return new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }
} 