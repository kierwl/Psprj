using UnityEngine;
using System;
using System.Collections.Generic;

public class OfflineProgressManager : MonoBehaviour
{
    public static OfflineProgressManager instance;

    [Header("Offline Progress Settings")]
    public float offlineEfficiency = 0.7f; // 오프라인 진행 효율 (100%보다 낮게 설정해 온라인 플레이 유도)
    public int maxOfflineTimeInHours = 12; // 최대 오프라인 시간 (12시간)

    // 마지막 접속 시간
    private DateTime lastLoginTime;
    private bool hasProcessedOfflineProgress = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 마지막 접속 시간 로드
        long lastLoginTicks = PlayerPrefs.GetInt("LastLoginTimeBinary", 0);
        if (lastLoginTicks == 0)
        {
            // 첫 접속인 경우 현재 시간 저장
            lastLoginTime = DateTime.Now;
            SaveLastLoginTime();
        }
        else
        {
            // 마지막 접속 시간 복원
            lastLoginTime = DateTime.FromBinary(lastLoginTicks);

            // 오프라인 진행 계산
            if (!hasProcessedOfflineProgress)
            {
                CalculateOfflineProgress();
                hasProcessedOfflineProgress = true;
            }
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 앱이 백그라운드로 갈 때 현재 시간 저장
            SaveLastLoginTime();
        }
        else
        {
            // 앱이 포그라운드로 돌아올 때 오프라인 진행 계산
            // 마지막 접속 시간 복원
            long lastLoginTicks = PlayerPrefs.GetInt("LastLoginTimeBinary", 0);
            if (lastLoginTicks != 0)
            {
                lastLoginTime = DateTime.FromBinary(lastLoginTicks);
                CalculateOfflineProgress();
            }

            // 현재 시간 다시 저장
            SaveLastLoginTime();
        }
    }

    private void OnApplicationQuit()
    {
        // 앱 종료 시 현재 시간 저장
        SaveLastLoginTime();
    }

    private void SaveLastLoginTime()
    {
        // 현재 시간을 이진 형식으로 저장
        DateTime now = DateTime.Now;
        PlayerPrefs.SetInt("LastLoginTimeBinary", now.ToBinary());
        PlayerPrefs.Save();
    }

    private void CalculateOfflineProgress()
    {
        // 현재 시간과 마지막 접속 시간의 차이 계산
        TimeSpan offlineTime = DateTime.Now - lastLoginTime;

        // 최대 오프라인 시간으로 제한
        double hoursOffline = Math.Min(offlineTime.TotalHours, maxOfflineTimeInHours);

        if (hoursOffline <= 0)
            return;

        Debug.Log("오프라인 시간: " + hoursOffline + "시간");

        // 오프라인 동안 획득한 자원 계산
        CalculateOfflineResources(hoursOffline);

        // 오프라인 동안 처치한 몬스터 계산
        CalculateOfflineMonsters(hoursOffline);

        // 오프라인 결과 표시
        ShowOfflineResultsUI(hoursOffline);
    }

    private void CalculateOfflineResources(double hoursOffline)
    {
        // 시간당 자원 획득량 (레벨, 업그레이드 등에 따라 조정)
        float goldPerHour = 100 * GameManager.instance.playerLevel.currentLevel;
        float expPerHour = 50 * GameManager.instance.playerLevel.currentLevel;

        // 오프라인 시간 동안 획득한 자원 계산
        int totalGold = Mathf.RoundToInt((float)(goldPerHour * hoursOffline * offlineEfficiency));
        int totalExp = Mathf.RoundToInt((float)(expPerHour * hoursOffline * offlineEfficiency));

        // 자원 추가
        CurrencyManager.instance?.AddGold(totalGold);
        PlayerLevel.instance?.AddExperience(totalExp);
    }

    private void CalculateOfflineMonsters(double hoursOffline)
    {
        // 시간당 처치 몬스터 수 (플레이어 공격력, 속도 등에 따라 조정)
        float monstersPerHour = 10 * GameManager.instance.playerLevel.currentLevel;

        // 오프라인 시간 동안 처치한 몬스터 수 계산
        int totalMonsters = Mathf.RoundToInt((float)(monstersPerHour * hoursOffline * offlineEfficiency));

        // 몬스터 처치 보상은 이미 CalculateOfflineResources에서 계산되었으므로 여기서는 통계용으로만 사용
        Debug.Log("오프라인 동안 처치한 몬스터: " + totalMonsters + "마리");
    }

    private void ShowOfflineResultsUI(double hoursOffline)
    {
        // 오프라인 결과 UI를 표시하는 코드
        // GameUIManager를 통해 구현

        // 시간당 자원 획득량 (레벨, 업그레이드 등에 따라 조정)
        float goldPerHour = 100 * GameManager.instance.playerLevel.currentLevel;
        float expPerHour = 50 * GameManager.instance.playerLevel.currentLevel;

        // 오프라인 시간 동안 획득한 자원 계산
        int totalGold = Mathf.RoundToInt((float)(goldPerHour * hoursOffline * offlineEfficiency));
        int totalExp = Mathf.RoundToInt((float)(expPerHour * hoursOffline * offlineEfficiency));

        // UI 매니저를 통해 결과 표시
        GameUIManager.instance?.ShowOfflineProgressResults(
            hoursOffline,
            totalGold,
            totalExp,
            Mathf.RoundToInt((float)(10 * GameManager.instance.playerLevel.currentLevel * hoursOffline * offlineEfficiency))
        );
    }
}