// 시간 및 보상 관리자
using UnityEngine;
using TMPro;
using System;

public class TimeRewardManager : MonoBehaviour
{
    public static TimeRewardManager instance;

    [Header("Reward Settings")]
    public int rewardGold = 100;
    public int rewardGems = 5;
    public float rewardInterval = 4 * 60 * 60;  // 4시간(초 단위)

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject rewardButton;

    private float lastRewardTime;
    private float remainingTime;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // 마지막 보상 시간 로드 (플레이어프리팹 사용 예정)
        lastRewardTime = PlayerPrefs.GetFloat("LastRewardTime", 0);
    }

    private void Start()
    {
        UpdateRewardStatus();
    }

    private void Update()
    {
        UpdateRewardStatus();
        UpdateTimerUI();
    }

    private void UpdateRewardStatus()
    {
        // 마지막 보상 이후 경과 시간 계산
        float currentTime = Time.time;
        float elapsedTime = currentTime - lastRewardTime;

        // 남은 시간 계산
        remainingTime = Mathf.Max(0, rewardInterval - elapsedTime);

        // 보상 버튼 활성화 여부 결정
        if (rewardButton != null)
            rewardButton.SetActive(remainingTime <= 0);
    }

    private void UpdateTimerUI()
    {
        if (timerText != null && remainingTime > 0)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        else if (timerText != null)
        {
            timerText.text = "보상 준비 완료!";
        }
    }

    public void ClaimReward()
    {
        if (remainingTime <= 0)
        {
            // 보상 지급
            CurrencyManager.instance?.AddGold(rewardGold);
            CurrencyManager.instance?.AddGems(rewardGems);

            // 마지막 보상 시간 업데이트
            lastRewardTime = Time.time;
            PlayerPrefs.SetFloat("LastRewardTime", lastRewardTime);

            // 효과 및 알림
            Debug.Log("보상 수령: 골드 " + rewardGold + ", 보석 " + rewardGems);
        }
    }
}