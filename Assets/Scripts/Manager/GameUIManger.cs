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
    public InventoryUI inventoryPanel;
    public ShopManager shopPanel;
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

    [Header("Stage UI")]
    public GameObject stageProgressPanel;
    public Slider progressBar;
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI stageTimerText;
    public TextMeshProUGUI stageTitleText;

    // Add this field to fix the error
    [Header("Message UI")]
    public TextMeshProUGUI messageText;

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
        // 리소스 업데이트 - PlayerStats 시스템 우선 활용
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerStats playerStats = player?.GetComponent<PlayerStats>();

        if (topPanel != null)
        {
            if (playerStats != null)
            {
                // PlayerStats에서 골드 가져오기
                int gold = playerStats.GetGold();

                // 나머지 리소스는 CurrencyManager에서 가져오기
                int gems = CurrencyManager.instance != null ? CurrencyManager.instance.gems : 0;
                int energy = CurrencyManager.instance != null ? CurrencyManager.instance.energy : 0;
                int maxEnergy = CurrencyManager.instance != null ? CurrencyManager.instance.maxEnergy : 100;

                topPanel.UpdateResources(gold, gems, energy, maxEnergy);
            }
            else if (CurrencyManager.instance != null)
            {
                // 기존 방식 유지
                topPanel.UpdateResources(
                    CurrencyManager.instance.gold,
                    CurrencyManager.instance.gems,
                    CurrencyManager.instance.energy,
                    CurrencyManager.instance.maxEnergy
                );
            }
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

    // 스테이지 진행 상황 업데이트
    public void UpdateStageProgress(int enemiesKilled, int totalEnemies, float stageTimer)
    {
        // 스테이지 진행 패널이 없으면 리턴
        if (stageProgressPanel == null) return;

        // 스테이지 진행 패널 활성화
        if (!stageProgressPanel.activeSelf)
            stageProgressPanel.SetActive(true);

        // 프로그래스 바 업데이트
        if (progressBar != null)
        {
            float progress = totalEnemies > 0 ? (float)enemiesKilled / totalEnemies : 0f;
            progressBar.value = progress;
        }

        // 적 카운트 텍스트 업데이트
        if (enemyCountText != null)
        {
            enemyCountText.text = $"{enemiesKilled} / {totalEnemies}";
        }

        // 타이머 텍스트 업데이트
        if (stageTimerText != null)
        {
            int minutes = Mathf.FloorToInt(stageTimer / 60);
            int seconds = Mathf.FloorToInt(stageTimer % 60);
            stageTimerText.text = $"{minutes:00}:{seconds:00}";
        }

        // 스테이지 제목 업데이트 (스테이지 이름이 있는 경우)
        if (stageTitleText != null && StageManager.instance?.currentStage != null)
        {
            stageTitleText.text = StageManager.instance.currentStage.stageName;
        }
    }

    // 스테이지 UI 표시/숨기기
    public void SetStageProgressVisible(bool visible)
    {
        if (stageProgressPanel != null)
            stageProgressPanel.SetActive(visible);
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
            // 인벤토리 알림
            bool hasNewItems = false;

            // EquipmentManager 체크
            if (EquipmentManager.instance != null)
            {
                hasNewItems = EquipmentManager.instance.HasNewItems();
            }

            // Inventory 체크 (나중에 인벤토리에 새 아이템 알림 기능이 추가된다면)
            // if (Inventory.instance != null)
            // {
            //     hasNewItems = hasNewItems || Inventory.instance.HasNewItems();
            // }

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

            // 스테이지 진행 패널 숨기기
            SetStageProgressVisible(false);
        }
    }

    // 스테이지 실패 표시
    public void ShowStageFail()
    {
        if (stageFailPanel != null)
        {
            // 스테이지 실패 패널 데이터 설정 (필요시)

            // 패널 표시
            stageFailPanel.SetActive(true);

            // 스테이지 진행 패널 숨기기
            SetStageProgressVisible(false);
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

    // 메시지 표시 메서드
    public void ShowMessage(string message, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
            CancelInvoke("ClearMessage");
            Invoke("ClearMessage", duration);
        }
    }

    // 메시지 지우기 메서드
    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }
    }

    private void CloseLevelUpPanel()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }
}
