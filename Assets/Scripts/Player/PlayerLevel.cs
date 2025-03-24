// 플레이어 레벨 관리 스크립트
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
        // 싱글톤 설정
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

        // 레벨업 확인
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
        // 다음 레벨까지 필요한 경험치 증가
        expToNextLevel = (int)(expToNextLevel * 1.2f);

        // 레벨업 이벤트 및 효과
        Debug.Log("레벨 업! 현재 레벨: " + currentLevel);

        // 레벨업 시 플레이어 스탯 증가 등 구현
    }

    public void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = currentLevel.ToString();

        if (expFillImage != null)
            expFillImage.fillAmount = (float)currentExp / expToNextLevel;
    }
}