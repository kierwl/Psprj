using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class StageSelectionManager : MonoBehaviour
{
    public static StageSelectionManager instance;

    [Header("스테이지 데이터")]
    public List<StageSO> availableStages = new List<StageSO>();

    [Header("UI 요소")]
    public GameObject stageButtonPrefab;
    public Transform stageButtonContainer;
    public GameObject stageDetailsPanel;
    public Button closeDetailsButton;
    public Button enterStageButton;

    [Header("스테이지 상세 정보")]
    public Image stageImage;
    public TextMeshProUGUI stageNameText;
    public TextMeshProUGUI stageDescriptionText;
    public TextMeshProUGUI stageLevelText;
    public TextMeshProUGUI stageEnemiesText;
    public TextMeshProUGUI stageRewardsText;
    public TextMeshProUGUI staminaCostText;

    private StageSO selectedStage;
    private PlayerStats playerStats;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void Start()
    {
        // 스테이지 상세 패널 초기 숨김
        if (stageDetailsPanel != null)
            stageDetailsPanel.SetActive(false);

        // 버튼 이벤트 설정
        if (closeDetailsButton != null)
            closeDetailsButton.onClick.AddListener(CloseStageDetails);

        if (enterStageButton != null)
            enterStageButton.onClick.AddListener(EnterSelectedStage);

        // 스테이지 로드
        LoadStages();

        // 스테이지 버튼 생성
        CreateStageButtons();
    }

    // 스테이지 데이터 로드
    private void LoadStages()
    {
        // Resources 폴더에서 스테이지 로드
        StageSO[] stages = Resources.LoadAll<StageSO>("Stages");
        if (stages != null && stages.Length > 0)
        {
            availableStages.AddRange(stages);
            Debug.Log($"{stages.Length}개의 스테이지를 로드했습니다.");
        }

        // 인스펙터에 추가된 스테이지도 유지
        if (availableStages.Count == 0)
        {
            Debug.LogWarning("스테이지가 없습니다. Resources/Stages 폴더에 스테이지를 추가하거나 인스펙터에서 직접 설정하세요.");
        }
    }

    // 스테이지 버튼 생성
    private void CreateStageButtons()
    {
        // 기존 버튼 제거
        foreach (Transform child in stageButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 레벨순으로 정렬
        var sortedStages = availableStages.OrderBy(s => s.recommendedLevel).ToList();

        // 스테이지 버튼 생성
        foreach (var stage in sortedStages)
        {
            GameObject buttonObj = Instantiate(stageButtonPrefab, stageButtonContainer);

            // 버튼 텍스트 설정
            TextMeshProUGUI nameText = buttonObj.transform.Find("StageName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = stage.stageName;

            TextMeshProUGUI levelText = buttonObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
                levelText.text = $"Lv.{stage.recommendedLevel}";

            // 버튼 아이콘 설정
            Image iconImage = buttonObj.transform.Find("StageIcon")?.GetComponent<Image>();
            if (iconImage != null && stage.stageIcon != null)
                iconImage.sprite = stage.stageIcon;

            // 이벤트 설정
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                StageSO stageRef = stage;
                button.onClick.AddListener(() => ShowStageDetails(stageRef));
            }

            // 잠금 여부 설정
            bool isLocked = !stage.isUnlocked || (PlayerLevel.instance != null && PlayerLevel.instance.currentLevel < stage.recommendedLevel);

            // 잠금 아이콘 설정
            GameObject lockIcon = buttonObj.transform.Find("LockIcon")?.gameObject;
            if (lockIcon != null)
                lockIcon.SetActive(isLocked);

            // 특별 스테이지 아이콘 설정
            GameObject specialIcon = buttonObj.transform.Find("SpecialIcon")?.gameObject;
            if (specialIcon != null)
                specialIcon.SetActive(stage.isSpecialStage);

            // 이벤트 스테이지 아이콘 설정
            GameObject eventIcon = buttonObj.transform.Find("EventIcon")?.gameObject;
            if (eventIcon != null)
                eventIcon.SetActive(stage.isEventStage);
        }
    }

    // 스테이지 상세 정보 표시
    public void ShowStageDetails(StageSO stage)
    {
        selectedStage = stage;

        if (stageDetailsPanel == null) return;

        // 상세 정보 패널 활성화
        stageDetailsPanel.SetActive(true);

        // 스테이지 정보 업데이트
        if (stageNameText != null)
            stageNameText.text = stage.stageName;

        if (stageDescriptionText != null)
            stageDescriptionText.text = stage.description;

        if (stageLevelText != null)
            stageLevelText.text = $"권장 레벨: {stage.recommendedLevel}";

        if (stageEnemiesText != null)
        {
            int totalEnemies = 0;
            foreach (var spawn in stage.enemySpawns)
            {
                totalEnemies += spawn.count;
            }
            string bossText = stage.hasBoss ? " + 보스" : "";
            stageEnemiesText.text = $"적: {totalEnemies}{bossText}";
        }

        if (stageRewardsText != null)
        {
            int playerLevel = PlayerLevel.instance != null ? PlayerLevel.instance.currentLevel : 1;
            int expReward = stage.GetScaledExpReward(playerLevel);
            int goldReward = stage.GetScaledGoldReward(playerLevel);
            stageRewardsText.text = $"보상: {expReward} 경험치, {goldReward} 골드";
        }

        if (staminaCostText != null)
            staminaCostText.text = $"스태미나: {stage.staminaCost}";

        if (stageImage != null && stage.stageIcon != null)
            stageImage.sprite = stage.stageIcon;

        // 입장 버튼 상태 업데이트
        UpdateEnterButtonState();
    }

    // 입장 버튼 상태 업데이트
    private void UpdateEnterButtonState()
    {
        if (enterStageButton == null || selectedStage == null) return;

        bool hasEnoughStamina = true;
        bool meetsLevelReq = true;
        bool isUnlocked = selectedStage.isUnlocked;

        // 스태미나 체크
        if (CurrencyManager.instance != null)
        {
            hasEnoughStamina = CurrencyManager.instance.energy >= selectedStage.staminaCost;
        }

        // 레벨 체크
        if (PlayerLevel.instance != null)
        {
            meetsLevelReq = PlayerLevel.instance.currentLevel >= selectedStage.recommendedLevel;
        }

        // 버튼 활성화 상태 설정
        enterStageButton.interactable = hasEnoughStamina && meetsLevelReq && isUnlocked;

        // 버튼 텍스트 업데이트
        TextMeshProUGUI buttonText = enterStageButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (!isUnlocked)
                buttonText.text = "잠김";
            else if (!meetsLevelReq)
                buttonText.text = "레벨 부족";
            else if (!hasEnoughStamina)
                buttonText.text = "스태미나 부족";
            else
                buttonText.text = "입장";
        }
    }

    // 스테이지 상세 패널 닫기
    public void CloseStageDetails()
    {
        if (stageDetailsPanel != null)
            stageDetailsPanel.SetActive(false);

        selectedStage = null;
    }

    // 선택한 스테이지 입장
    public void EnterSelectedStage()
    {
        if (selectedStage == null) return;

        // 플레이어 레벨 체크
        if (PlayerLevel.instance != null && PlayerLevel.instance.currentLevel < selectedStage.recommendedLevel)
        {
            GameUIManager.instance?.ShowMessage($"이 스테이지는 레벨 {selectedStage.recommendedLevel} 이상이 필요합니다.", 2f);
            return;
        }

        // 스테이지 잠금 체크
        if (!selectedStage.isUnlocked)
        {
            GameUIManager.instance?.ShowMessage(selectedStage.unlockCondition, 2f);
            return;
        }

        // 스태미나 체크
        if (CurrencyManager.instance != null)
        {
            int currentEnergy = CurrencyManager.instance.energy;
            if (currentEnergy < selectedStage.staminaCost)
            {
                GameUIManager.instance?.ShowMessage($"스태미나가 부족합니다! {selectedStage.staminaCost} 스태미나가 필요합니다.", 2f);
                return;
            }

            // 스태미나 소모
            CurrencyManager.instance.SpendEnergy(selectedStage.staminaCost);
        }

        // 스테이지 선택 UI 숨기기
        gameObject.SetActive(false);

        // 스테이지 시작
        StageManager.instance?.StartStage(selectedStage);

        // 스테이지 UI 표시
        GameUIManager.instance?.SetStageProgressVisible(true);

        Debug.Log($"스테이지 입장: {selectedStage.stageName}");
    }

    // 스테이지 선택으로 돌아오기
    public void ReturnToStageSelection()
    {
        // 현재 스테이지 정리
        StageManager.instance?.ClearStage();

        // 스테이지 진행 UI 숨기기
        GameUIManager.instance?.SetStageProgressVisible(false);

        // 스테이지 선택 UI 표시
        gameObject.SetActive(true);

        // 스테이지 버튼 새로고침
        CreateStageButtons();
    }

    // 스테이지 잠금 해제
    public void UnlockStage(StageSO stage)
    {
        if (stage == null) return;
        stage.isUnlocked = true;
        // 스테이지 버튼 새로고침
        CreateStageButtons();
        // 상세 정보 업데이트
        if (selectedStage == stage)
            ShowStageDetails(stage);
    }

    // 스테이지 잠금 해제확인
    public void CheckStageUnlocks()
    {
        foreach (var stage in availableStages)
        {
            if (!stage.isUnlocked && PlayerProgress.instance.IsStageCleared(stage.stageID))
            {
                UnlockStage(stage);
            }
        }
    }
}