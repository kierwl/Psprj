using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class BuffTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    private string buffName;
    private string description;
    
    private void Awake()
    {
        // 툴팁 초기 상태 설정
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    public void SetTooltip(string name, string desc)
    {
        buffName = name;
        description = desc;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            // 툴팁 내용 설정
            if (titleText != null)
                titleText.text = buffName;
                
            if (descriptionText != null)
                descriptionText.text = description;
                
            // 툴팁 표시
            tooltipPanel.SetActive(true);
            
            // 마우스 위치에 툴팁 배치
            PositionTooltip(eventData.position);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    private void PositionTooltip(Vector2 mousePosition)
    {
        // Canvas 찾기
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        // 툴팁 위치 계산
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        Vector2 position = mousePosition;
        
        // 화면 밖으로 나가지 않도록 조정
        float pivotX = 0;
        float pivotY = 0;
        
        // 화면 오른쪽에 가까우면 툴팁을 왼쪽으로 표시
        if (position.x + tooltipRect.sizeDelta.x > Screen.width)
        {
            pivotX = 1;
        }
        
        // 화면 위쪽에 가까우면 툴팁을 아래쪽으로 표시
        if (position.y + tooltipRect.sizeDelta.y > Screen.height)
        {
            pivotY = 1;
        }
        
        tooltipRect.pivot = new Vector2(pivotX, pivotY);
        tooltipPanel.transform.position = position;
    }
} 