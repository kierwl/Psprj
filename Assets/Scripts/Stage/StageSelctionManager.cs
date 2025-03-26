using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class StageSelectionManager : MonoBehaviour
{
    public static StageSelectionManager instance;

    [Header("�������� ������")]
    public List<StageSO> availableStages = new List<StageSO>();

    [Header("UI ���")]
    public GameObject stageButtonPrefab;
    public Transform stageButtonContainer;
    public GameObject stageDetailsPanel;
    public Button closeDetailsButton;
    public Button enterStageButton;

    [Header("�������� �� ����")]
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
        // �������� �� �г� �ʱ� ����
        if (stageDetailsPanel != null)
            stageDetailsPanel.SetActive(false);

        // ��ư �̺�Ʈ ����
        if (closeDetailsButton != null)
            closeDetailsButton.onClick.AddListener(CloseStageDetails);

        if (enterStageButton != null)
            enterStageButton.onClick.AddListener(EnterSelectedStage);

        // �������� �ε�
        LoadStages();

        // �������� ��ư ����
        CreateStageButtons();
    }

    // �������� ������ �ε�
    private void LoadStages()
    {
        // Resources �������� �������� �ε�
        StageSO[] stages = Resources.LoadAll<StageSO>("Stages");
        if (stages != null && stages.Length > 0)
        {
            availableStages.AddRange(stages);
            Debug.Log($"{stages.Length}���� ���������� �ε��߽��ϴ�.");
        }

        // �ν����Ϳ� �߰��� ���������� ����
        if (availableStages.Count == 0)
        {
            Debug.LogWarning("���������� �����ϴ�. Resources/Stages ������ ���������� �߰��ϰų� �ν����Ϳ��� ���� �����ϼ���.");
        }
    }

    // �������� ��ư ����
    private void CreateStageButtons()
    {
        // ���� ��ư ����
        foreach (Transform child in stageButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // ���������� ����
        var sortedStages = availableStages.OrderBy(s => s.recommendedLevel).ToList();

        // �������� ��ư ����
        foreach (var stage in sortedStages)
        {
            GameObject buttonObj = Instantiate(stageButtonPrefab, stageButtonContainer);

            // ��ư �ؽ�Ʈ ����
            TextMeshProUGUI nameText = buttonObj.transform.Find("StageName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = stage.stageName;

            TextMeshProUGUI levelText = buttonObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
                levelText.text = $"Lv.{stage.recommendedLevel}";

            // ��ư ������ ����
            Image iconImage = buttonObj.transform.Find("StageIcon")?.GetComponent<Image>();
            if (iconImage != null && stage.stageIcon != null)
                iconImage.sprite = stage.stageIcon;

            // �̺�Ʈ ����
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                StageSO stageRef = stage;
                button.onClick.AddListener(() => ShowStageDetails(stageRef));
            }

            // ��� ���� ����
            bool isLocked = !stage.isUnlocked || (PlayerLevel.instance != null && PlayerLevel.instance.currentLevel < stage.recommendedLevel);

            // ��� ������ ����
            GameObject lockIcon = buttonObj.transform.Find("LockIcon")?.gameObject;
            if (lockIcon != null)
                lockIcon.SetActive(isLocked);

            // Ư�� �������� ������ ����
            GameObject specialIcon = buttonObj.transform.Find("SpecialIcon")?.gameObject;
            if (specialIcon != null)
                specialIcon.SetActive(stage.isSpecialStage);

            // �̺�Ʈ �������� ������ ����
            GameObject eventIcon = buttonObj.transform.Find("EventIcon")?.gameObject;
            if (eventIcon != null)
                eventIcon.SetActive(stage.isEventStage);
        }
    }

    // �������� �� ���� ǥ��
    public void ShowStageDetails(StageSO stage)
    {
        selectedStage = stage;

        if (stageDetailsPanel == null) return;

        // �� ���� �г� Ȱ��ȭ
        stageDetailsPanel.SetActive(true);

        // �������� ���� ������Ʈ
        if (stageNameText != null)
            stageNameText.text = stage.stageName;

        if (stageDescriptionText != null)
            stageDescriptionText.text = stage.description;

        if (stageLevelText != null)
            stageLevelText.text = $"���� ����: {stage.recommendedLevel}";

        if (stageEnemiesText != null)
        {
            int totalEnemies = 0;
            foreach (var spawn in stage.enemySpawns)
            {
                totalEnemies += spawn.count;
            }
            string bossText = stage.hasBoss ? " + ����" : "";
            stageEnemiesText.text = $"��: {totalEnemies}{bossText}";
        }

        if (stageRewardsText != null)
        {
            int playerLevel = PlayerLevel.instance != null ? PlayerLevel.instance.currentLevel : 1;
            int expReward = stage.GetScaledExpReward(playerLevel);
            int goldReward = stage.GetScaledGoldReward(playerLevel);
            stageRewardsText.text = $"����: {expReward} ����ġ, {goldReward} ���";
        }

        if (staminaCostText != null)
            staminaCostText.text = $"���¹̳�: {stage.staminaCost}";

        if (stageImage != null && stage.stageIcon != null)
            stageImage.sprite = stage.stageIcon;

        // ���� ��ư ���� ������Ʈ
        UpdateEnterButtonState();
    }

    // ���� ��ư ���� ������Ʈ
    private void UpdateEnterButtonState()
    {
        if (enterStageButton == null || selectedStage == null) return;

        bool hasEnoughStamina = true;
        bool meetsLevelReq = true;
        bool isUnlocked = selectedStage.isUnlocked;

        // ���¹̳� üũ
        if (CurrencyManager.instance != null)
        {
            hasEnoughStamina = CurrencyManager.instance.energy >= selectedStage.staminaCost;
        }

        // ���� üũ
        if (PlayerLevel.instance != null)
        {
            meetsLevelReq = PlayerLevel.instance.currentLevel >= selectedStage.recommendedLevel;
        }

        // ��ư Ȱ��ȭ ���� ����
        enterStageButton.interactable = hasEnoughStamina && meetsLevelReq && isUnlocked;

        // ��ư �ؽ�Ʈ ������Ʈ
        TextMeshProUGUI buttonText = enterStageButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (!isUnlocked)
                buttonText.text = "���";
            else if (!meetsLevelReq)
                buttonText.text = "���� ����";
            else if (!hasEnoughStamina)
                buttonText.text = "���¹̳� ����";
            else
                buttonText.text = "����";
        }
    }

    // �������� �� �г� �ݱ�
    public void CloseStageDetails()
    {
        if (stageDetailsPanel != null)
            stageDetailsPanel.SetActive(false);

        selectedStage = null;
    }

    // ������ �������� ����
    public void EnterSelectedStage()
    {
        if (selectedStage == null) return;

        // �÷��̾� ���� üũ
        if (PlayerLevel.instance != null && PlayerLevel.instance.currentLevel < selectedStage.recommendedLevel)
        {
            GameUIManager.instance?.ShowMessage($"�� ���������� ���� {selectedStage.recommendedLevel} �̻��� �ʿ��մϴ�.", 2f);
            return;
        }

        // �������� ��� üũ
        if (!selectedStage.isUnlocked)
        {
            GameUIManager.instance?.ShowMessage(selectedStage.unlockCondition, 2f);
            return;
        }

        // ���¹̳� üũ
        if (CurrencyManager.instance != null)
        {
            int currentEnergy = CurrencyManager.instance.energy;
            if (currentEnergy < selectedStage.staminaCost)
            {
                GameUIManager.instance?.ShowMessage($"���¹̳��� �����մϴ�! {selectedStage.staminaCost} ���¹̳��� �ʿ��մϴ�.", 2f);
                return;
            }

            // ���¹̳� �Ҹ�
            CurrencyManager.instance.SpendEnergy(selectedStage.staminaCost);
        }

        // �������� ���� UI �����
        gameObject.SetActive(false);

        // �������� ����
        StageManager.instance?.StartStage(selectedStage);

        // �������� UI ǥ��
        GameUIManager.instance?.SetStageProgressVisible(true);

        Debug.Log($"�������� ����: {selectedStage.stageName}");
    }

    // �������� �������� ���ƿ���
    public void ReturnToStageSelection()
    {
        // ���� �������� ����
        StageManager.instance?.ClearStage();

        // �������� ���� UI �����
        GameUIManager.instance?.SetStageProgressVisible(false);

        // �������� ���� UI ǥ��
        gameObject.SetActive(true);

        // �������� ��ư ���ΰ�ħ
        CreateStageButtons();
    }

    // �������� ��� ����
    public void UnlockStage(StageSO stage)
    {
        if (stage == null) return;
        stage.isUnlocked = true;
        // �������� ��ư ���ΰ�ħ
        CreateStageButtons();
        // �� ���� ������Ʈ
        if (selectedStage == stage)
            ShowStageDetails(stage);
    }

    // �������� ��� ����Ȯ��
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