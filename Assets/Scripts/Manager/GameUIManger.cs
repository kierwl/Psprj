using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

    [Header("Main UI Components")]
    public Canvas mainCanvas;
    public TopPanelUI topPanel;
    public CharacterPanelUI characterPanel;
    public BottomPanelUI bottomPanel;

    [Header("Game Panels")]
    public InventoryPanelUI inventoryPanel;
    public ShopPanelUI shopPanel;
    public GameObject upgradePanel;
    public GameObject stagePanel;
    public GameObject settingsPanel;
    public GameObject questPanel;
    
    [Header("Popup Panels")]
    public GameObject offlineProgressPanel;
    public GameObject stageClearPanel;
    public GameObject stageFailPanel;
    public GameObject levelUpPanel;
    public GameObject achievementPanel;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 시작 시 모든 패널 닫기
        CloseAllPanels();

        // 기본 UI 업데이트
        UpdateAllUI();
    }

    // 모든 UI 업데이트
    public void UpdateAllUI()
    {
        // 리소스 업데이트
        if (topPanel != null && CurrencyManager.instance != null)
        {
            topPanel.UpdateResources(
                CurrencyManager.instance.gold,
                CurrencyManager.instance.gems,
                CurrencyManager.instance.energy,
                CurrencyManager.instance.maxEnergy
            );
        }

        // 레벨 정보 업데이트
        if (topPanel != null && PlayerLevel.instance != null)
        {
            topPanel.UpdateLevelInfo(
                PlayerLevel.instance.currentLevel,
                PlayerLevel.instance.currentExp,
                PlayerLevel.instance.expToNextLevel
            );
        }

        // 캐릭터 패널 업데이트
        if (characterPanel != null && CharacterManager.instance != null)
        {
            characterPanel.UpdateCharacterSlots(CharacterManager.instance.characters);
        }

        // 타이머 업데이트
        UpdateTimerDisplay();

        // 알림 표시 업데이트
        UpdateNotifications();
    }

    // 타이머 표시 업데이트
    private void UpdateTimerDisplay()
    {
        if (topPanel != null && TimeRewardManager.instance != null)
        {
            float remainingTime = TimeRewardManager.instance.GetRemainingTime();
            bool isRewardReady = remainingTime <= 0;
            
            string timeText;
            if (isRewardReady)
            {
                timeText = "보상 수령 가능!";
            }
            else
            {
                int hours = Mathf.FloorToInt(remainingTime / 3600);
                int minutes = Mathf.FloorToInt((remainingTime % 3600) / 60);
                int seconds = Mathf.FloorToInt(remainingTime % 60);
                timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
            }
            
            topPanel.UpdateTimer(timeText, isRewardReady);
        }
    }

    // 알림 표시 업데이트
    private void UpdateNotifications()
    {
        if (bottomPanel != null)
        {
            // 인벤토리 알림 (새 아이템 획득 등)
            bool hasNewItems = EquipmentManager.instance != null && EquipmentManager.instance.HasNewItems();
            bottomPanel.ShowInventoryNotification(hasNewItems);
            
        }
    }

    // 패널 열기 메서드들
    public void OpenInventory()
    {
        // 다른 패널들만 닫기
        if (shopPanel != null)
            shopPanel.gameObject.SetActive(false);
            
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
            
        if (stagePanel != null)
            stagePanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (questPanel != null)
            questPanel.SetActive(false);

        // 인벤토리 패널 활성화
        if (inventoryPanel != null)
        {
            inventoryPanel.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            inventoryPanel.gameObject.SetActive(false);
            inventoryPanel.gameObject.SetActive(true);
        }
    }

    public void OpenShop()
    {
        CloseAllPanels();
        if (shopPanel != null)
            shopPanel.gameObject.SetActive(true);
    }

    public void OpenUpgrades()
    {
        CloseAllPanels();
        if (upgradePanel != null)
            upgradePanel.SetActive(true);
    }

    public void OpenStageSelection()
    {
        CloseAllPanels();
        if (stagePanel != null)
            stagePanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAllPanels();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OpenQuests()
    {
        CloseAllPanels();
        if (questPanel != null)
            questPanel.SetActive(true);
    }

    // 모든 패널 닫기
    public void CloseAllPanels()
    {
        if (inventoryPanel != null)
            inventoryPanel.gameObject.SetActive(false);
            
        if (shopPanel != null)
            shopPanel.gameObject.SetActive(false);
            
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
            
        if (stagePanel != null)
            stagePanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (questPanel != null)
            questPanel.SetActive(false);
    }

    // 오프라인 결과 표시
    public void ShowOfflineProgressResults(double hours, int gold, int exp, int monsters)
    {
        if (offlineProgressPanel != null)
        {
            // 오프라인 패널 데이터 설정
            Transform panel = offlineProgressPanel.transform;
            
            panel.Find("TimeText").GetComponent<TextMeshProUGUI>().text = 
                string.Format("{0:F1} 시간 동안 진행되었습니다", hours);
                
            panel.Find("GoldText").GetComponent<TextMeshProUGUI>().text = 
                gold.ToString("N0");
                
            panel.Find("ExpText").GetComponent<TextMeshProUGUI>().text = 
                exp.ToString("N0");
                
            panel.Find("MonstersText").GetComponent<TextMeshProUGUI>().text = 
                monsters.ToString("N0") + " 마리";
                
            // 패널 표시
            offlineProgressPanel.SetActive(true);
        }
    }

    // 스테이지 클리어 표시
    public void ShowStageClear(int stageLevel, int expReward, int goldReward, int gemReward)
    {
        if (stageClearPanel != null)
        {
            // 스테이지 클리어 패널 데이터 설정
            Transform panel = stageClearPanel.transform;
            
            panel.Find("StageTitleText").GetComponent<TextMeshProUGUI>().text = 
                string.Format("스테이지 {0} 클리어!", stageLevel);
                
            panel.Find("ExpText").GetComponent<TextMeshProUGUI>().text = 
                "+" + expReward.ToString("N0");
                
            panel.Find("GoldText").GetComponent<TextMeshProUGUI>().text = 
                "+" + goldReward.ToString("N0");
                
            panel.Find("GemText").GetComponent<TextMeshProUGUI>().text = 
                gemReward > 0 ? "+" + gemReward.ToString() : "";
                
            // 패널 표시
            stageClearPanel.SetActive(true);
        }
    }

    // 레벨업 표시
    public void ShowLevelUp(int newLevel)
    {
        if (levelUpPanel != null)
        {
            // 레벨업 패널 데이터 설정
            Transform panel = levelUpPanel.transform;
            
            panel.Find("LevelText").GetComponent<TextMeshProUGUI>().text = 
                string.Format("레벨 {0} 달성!", newLevel);
                
            // 패널 표시
            levelUpPanel.SetActive(true);
            
            // 자동 닫기
            Invoke("CloseLevelUpPanel", 3f);
        }
    }

    private void CloseLevelUpPanel()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }
}