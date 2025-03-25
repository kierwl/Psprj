using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopPanelUI : MonoBehaviour
{
    [Header("Resource References")]
    public Image goldIcon;
    public TextMeshProUGUI goldText;
    public Image gemIcon;
    public TextMeshProUGUI gemText;
    public Image energyIcon;
    public TextMeshProUGUI energyText;

    [Header("Level Info")]
    public TextMeshProUGUI levelText;
    public Image expBar;
    public TextMeshProUGUI expText;

    [Header("Timer")]
    public Image timerIcon;
    public TextMeshProUGUI timerText;
    public Button timerButton;

    // ���ҽ� ������Ʈ �޼���
    public void UpdateResources(int gold, int gems, int energy, int maxEnergy)
    {
        goldText.text = gold.ToString("N0");
        gemText.text = gems.ToString("N0");
        energyText.text = $"{energy}/{maxEnergy}";

    }

    // ���� ���� ������Ʈ �޼���
    public void UpdateLevelInfo(int level, int exp, int expToNextLevel)
    {
        levelText.text = level.ToString();
        expText.text = $"{exp}/{expToNextLevel}";
    }

    // Ÿ�̸� ������Ʈ �޼���
    public void UpdateTimer(string timeRemaining, bool isRewardReady)
    {
        timerText.text = timeRemaining;
        timerButton.interactable = isRewardReady;
        timerButton.gameObject.SetActive(isRewardReady);
    }
}