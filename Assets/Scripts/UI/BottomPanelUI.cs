using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomPanelUI : MonoBehaviour
{
    [Header("Main Buttons")]
    public Button inventoryButton;
    public Button shopButton;
    public Button stageButton;
    public Button upgradeButton;
    public Button questButton;
    public Button settingsButton;

    [Header("Button Indicators")]
    public GameObject inventoryNotification;
    public GameObject shopNotification;
    public GameObject questNotification;

    // 버튼 클릭 이벤트 초기화
    private void Start()
    {
        // 인벤토리 버튼
        if (inventoryButton != null)
        {
            Debug.Log("인벤토리 버튼 이벤트 등록 시작");
            inventoryButton.onClick.RemoveAllListeners(); // 기존 이벤트 제거
            inventoryButton.onClick.AddListener(() => {
                Debug.Log("인벤토리 버튼 클릭됨");
                if (GameUIManager.instance != null)
                {
                    Debug.Log("GameUIManager 인스턴스 존재");
                    if (GameUIManager.instance.inventoryPanel != null)
                    {
                        Debug.Log("인벤토리 패널 참조 존재");
                        GameUIManager.instance.OpenInventory();
                    }
                    else
                    {
                        Debug.LogError("인벤토리 패널 참조가 없습니다!");
                    }
                }
                else
                {
                    Debug.LogError("GameUIManager 인스턴스가 없습니다!");
                }
            });
            Debug.Log("인벤토리 버튼 이벤트 등록 완료");
        }
        else
        {
            Debug.LogError("인벤토리 버튼이 null입니다!");
        }

        // 상점 버튼
        if (shopButton != null)
            shopButton.onClick.AddListener(() => GameUIManager.instance.OpenShop());

        // 스테이지 버튼
        if (stageButton != null)
            stageButton.onClick.AddListener(() => GameUIManager.instance.OpenStageSelection());

        // 강화 버튼
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(() => GameUIManager.instance.OpenUpgrades());

        // 퀘스트 버튼
        if (questButton != null)
            questButton.onClick.AddListener(() => GameUIManager.instance.OpenQuests());

        // 설정 버튼
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => GameUIManager.instance.OpenSettings());

        // 초기 알림 숨기기
        HideAllNotifications();
    }

    // 알림 표시 메서드
    public void ShowInventoryNotification(bool show)
    {
        if (inventoryNotification != null)
            inventoryNotification.SetActive(show);
    }

    public void ShowShopNotification(bool show)
    {
        if (shopNotification != null)
            shopNotification.SetActive(show);
    }

    public void ShowQuestNotification(bool show)
    {
        if (questNotification != null)
            questNotification.SetActive(show);
    }

    private void HideAllNotifications()
    {
        ShowInventoryNotification(false);
        ShowShopNotification(false);
        ShowQuestNotification(false);
    }


}