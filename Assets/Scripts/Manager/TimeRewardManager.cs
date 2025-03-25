// 시간 보상 관리
using UnityEngine;
using TMPro;
using System;

public class TimeRewardManager : MonoBehaviour
{
    public static TimeRewardManager instance;

    [Header("Reward Settings")]
    public int rewardGold = 100;
    public int rewardGems = 5;
    public float rewardInterval = 4 * 60 * 60;  // 4시간(최대 오프라인 시간)
    public float maxOfflineTime = 86400f; // 24시간

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject rewardButton;

    private DateTime lastRewardTime;
    private DateTime lastSaveTime;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadLastSaveTime();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveLastSaveTime();
        }
        else
        {
            LoadLastSaveTime();
        }
    }

    private void SaveLastSaveTime()
    {
        lastSaveTime = DateTime.Now;
        PlayerPrefs.SetString("LastSaveTime", lastSaveTime.ToString());
    }

    private void LoadLastSaveTime()
    {
        string savedTime = PlayerPrefs.GetString("LastSaveTime", DateTime.Now.ToString());
        lastSaveTime = DateTime.Parse(savedTime);
        lastRewardTime = lastSaveTime;
    }

    private void Update()
    {
        UpdateRewardStatus();
        UpdateTimerUI();
    }

    private void UpdateRewardStatus()
    {
        // 최대 오프라인 시간 초과 여부 확인
        TimeSpan timeSinceLastReward = DateTime.Now - lastRewardTime;
        float remainingTime = rewardInterval - (float)timeSinceLastReward.TotalSeconds;

        // 최대 오프라인 시간 초과 시 보상 버튼 비활성화
        if (rewardButton != null)
            rewardButton.SetActive(remainingTime <= 0);
    }

    private void UpdateTimerUI()
    {
        if (timerText != null && GetRemainingTime() > 0)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(GetRemainingTime());
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        else if (timerText != null)
        {
            timerText.text = "오프라인 시간을 초과했습니다!";
        }
    }

    public float GetRemainingTime()
    {
        TimeSpan timeSinceLastReward = DateTime.Now - lastRewardTime;
        float remainingTime = rewardInterval - (float)timeSinceLastReward.TotalSeconds;
        return Mathf.Max(0f, remainingTime);
    }

    public void ClaimReward()
    {
        if (CanClaimReward())
        {
            // 보상 지급
            CurrencyManager.instance?.AddGold(rewardGold);
            CurrencyManager.instance?.AddGems(rewardGems);

            // 보상 지급 후 보상 지급 시간 갱신
            lastRewardTime = DateTime.Now;
            SaveLastSaveTime();

            // 로그 출력
            Debug.Log("보상 지급: 골드 " + rewardGold + ", 젬스 " + rewardGems);
        }
    }

    public bool CanClaimReward()
    {
        return GetRemainingTime() <= 0f;
    }
}