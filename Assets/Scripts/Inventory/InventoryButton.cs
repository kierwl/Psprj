using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리를 열기 위한 버튼에 연결하는 스크립트
/// </summary>
public class InventoryButton : MonoBehaviour
{
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OpenInventory);
        }
    }
    
    private void OpenInventory()
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.ToggleInventory();
        }
        else
        {
            Debug.LogWarning("인벤토리 매니저 인스턴스가 없습니다!");
        }
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OpenInventory);
        }
    }
} 