using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI 요소")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image highlightImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject equippedIndicator; // 장착 표시 오브젝트
    
    [Header("드래그 드롭 설정")]
    [SerializeField] private Canvas parentCanvas;
    [SerializeField] private GameObject dragItemPrefab;
    
    private int slotIndex = -1;
    private ItemSO currentItem;
    private int currentAmount;
    private bool isEquipped;
    private GameObject draggedItem;
    
    // 이벤트
    public event Action<int> OnSlotClicked;
    
    private void Awake()
    {
        // 부모 캔버스 자동 찾기
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
            
        // 초기 상태 설정
        if (highlightImage != null)
            highlightImage.enabled = false;
            
        // 장착 표시기 초기 상태
        if (equippedIndicator != null)
            equippedIndicator.SetActive(false);
    }
    
    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }
    
    public void SetItem(ItemSO item, int amount, bool equipped = false)
    {
        currentItem = item;
        currentAmount = amount;
        isEquipped = equipped;
        
        if (item != null)
        {
            // 아이템 이미지 표시
            itemImage.enabled = true;
            itemImage.sprite = item.icon;
            
            // 아이템 개수 표시
            if (amountText != null)
            {
                amountText.gameObject.SetActive(amount > 1);
                amountText.text = amount.ToString();
            }
            
            // 희귀도에 따른 배경색 변경
            if (backgroundImage != null)
            {
                Color rarityColor = GetRarityColor(item.rarity);
                backgroundImage.color = rarityColor;
            }
            
            // 장착 상태 표시
            if (equippedIndicator != null)
            {
                equippedIndicator.SetActive(equipped);
            }
        }
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        currentAmount = 0;
        isEquipped = false;
        
        // UI 요소 초기화
        if (itemImage != null)
            itemImage.enabled = false;
            
        if (amountText != null)
            amountText.gameObject.SetActive(false);
            
        if (highlightImage != null)
            highlightImage.enabled = false;
            
        if (backgroundImage != null)
            backgroundImage.color = Color.white;
            
        if (equippedIndicator != null)
            equippedIndicator.SetActive(false);
    }
    
    public void SetHighlight(bool isHighlighted)
    {
        if (highlightImage != null)
            highlightImage.enabled = isHighlighted;
    }
    
    // 버튼 클릭 처리 (아이템 선택)
    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(slotIndex);
    }
    
    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        
        // 드래그 중인 아이템 생성
        draggedItem = Instantiate(dragItemPrefab, parentCanvas.transform);
        
        // 드래그 아이템 설정
        Image dragImage = draggedItem.GetComponent<Image>();
        if (dragImage != null)
        {
            dragImage.sprite = currentItem.icon;
            dragImage.SetNativeSize();
            
            // 크기 조정
            RectTransform rt = dragImage.rectTransform;
            rt.sizeDelta = new Vector2(50, 50);
        }
        
        // 드래그 아이템 위치 설정
        draggedItem.transform.position = eventData.position;
    }
    
    // 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            draggedItem.transform.position = eventData.position;
        }
    }
    
    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            Destroy(draggedItem);
            draggedItem = null;
        }
    }
    
    // 드롭 처리
    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 중인 아이템 슬롯 가져오기
        ItemSlotUI fromSlot = eventData.pointerDrag.GetComponent<ItemSlotUI>();
        if (fromSlot != null && fromSlot != this)
        {
            // 인벤토리에 아이템 위치 교환 요청
            Inventory.instance.SwapItems(fromSlot.slotIndex, slotIndex);
        }
    }
    
    // 아이템 희귀도에 따른 색상 반환
    private Color GetRarityColor(ItemSO.ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemSO.ItemRarity.Common:
                return new Color(0.8f, 0.8f, 0.8f, 0.3f);
            case ItemSO.ItemRarity.Uncommon:
                return new Color(0.0f, 0.8f, 0.0f, 0.3f);
            case ItemSO.ItemRarity.Rare:
                return new Color(0.0f, 0.0f, 1.0f, 0.3f);
            case ItemSO.ItemRarity.Epic:
                return new Color(0.8f, 0.0f, 0.8f, 0.3f);
            case ItemSO.ItemRarity.Legendary:
                return new Color(1.0f, 0.8f, 0.0f, 0.3f);
            default:
                return new Color(1.0f, 1.0f, 1.0f, 0.3f);
        }
    }
} 