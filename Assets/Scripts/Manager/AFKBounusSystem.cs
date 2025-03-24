using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class AFKBonusSystem : MonoBehaviour
{
    [Header("AFK Bonus Settings")]
    public float bonusMaxTime = 3600f; // 최대 보너스 누적 시간 (1시간)
    public float bonusInterval = 60f; // 보너스 획득 간격 (60초)
    public int goldBonus = 50; // 기본 골드 보너스
    public int expBonus = 20; // 기본 경험치 보너스

    [Header("UI References")]
    public Slider bonusProgressBar;
    public TextMeshProUGUI bonusTimeText;
    public Button collectButton;

    private float currentBonusTime = 0f;
    private bool canCollect = false;

    private void Start()
    {
        // 버튼 이벤트 등록
        if (collectButton != null)
            collectButton.onClick.AddListener(CollectBonus);

        // 초기 상태 설정
        UpdateBonusUI();
    }

    private void Update()
    {
        // 보너스 시간 증가
        if (currentBonusTime < bonusMaxTime)
        {
            currentBonusTime += Time.deltaTime;
            currentBonusTime = Mathf.Min(currentBonusTime, bonusMaxTime);

            // 보너스 획득 가능 여부 확인
            canCollect = currentBonusTime >= bonusInterval;

            // UI 업데이트
            UpdateBonusUI();

            // 수집 버튼 활성화/비활성화
            if (collectButton != null)
                collectButton.interactable = canCollect;
        }
    }

    private void UpdateBonusUI()
    {
        // 프로그레스 바 업데이트
        if (bonusProgressBar != null)
        {
            bonusProgressBar.value = currentBonusTime / bonusMaxTime;
        }

        // 시간 텍스트 업데이트
        if (bonusTimeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentBonusTime);
            bonusTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
    }

    public void CollectBonus()
    {
        if (!canCollect)
            return;

        // 보너스 횟수 계산 (몇 번의 간격이 지났는지)
        int bonusCount = Mathf.FloorToInt(currentBonusTime / bonusInterval);

        // 플레이어 레벨에 따른 보너스 증가
        int playerLevel = PlayerLevel.instance?.currentLevel ?? 1;
        int totalGoldBonus = goldBonus * bonusCount * playerLevel;
        int totalExpBonus = expBonus * bonusCount * playerLevel;

        // 보너스 지급
        CurrencyManager.instance?.AddGold(totalGoldBonus);
        PlayerLevel.instance?.AddExperience(totalExpBonus);

        // 보너스 시간 리셋 (완전히 리셋하거나 일부만 감소)
        currentBonusTime = 0f; // 또는 currentBonusTime % bonusInterval; (남은 시간 유지)
        canCollect = false;

        // UI 업데이트
        UpdateBonusUI();

        // 수집 버튼 비활성화
        if (collectButton != null)
            collectButton.interactable = false;

        // 효과 또는 알림
        Debug.Log("AFK 보너스 수집: 골드 " + totalGoldBonus + ", 경험치 " + totalExpBonus);
    }
}