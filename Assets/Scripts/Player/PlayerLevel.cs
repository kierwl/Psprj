// �÷��̾� ���� ���� ��ũ��Ʈ
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevel : MonoBehaviour
{
    public static PlayerLevel instance;

    [Header("Level Data")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public Image expFillImage;

    private void Awake()
    {
        // �̱��� ����
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateLevelUI();
    }

    public void AddExperience(int amount)
    {
        currentExp += amount;

        // ������ Ȯ��
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }

        UpdateLevelUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        // ���� �������� �ʿ��� ����ġ ����
        expToNextLevel = (int)(expToNextLevel * 1.2f);

        // ������ �̺�Ʈ �� ȿ��
        Debug.Log("���� ��! ���� ����: " + currentLevel);

        // ������ �� �÷��̾� ���� ���� �� ����
    }

    public void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = currentLevel.ToString();

        if (expFillImage != null)
            expFillImage.fillAmount = (float)currentExp / expToNextLevel;
    }
}