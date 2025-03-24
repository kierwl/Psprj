using UnityEngine;
using System;
using System.Collections.Generic;

public class OfflineProgressManager : MonoBehaviour
{
    public static OfflineProgressManager instance;

    [Header("Offline Progress Settings")]
    public float offlineEfficiency = 0.7f; // �������� ���� ȿ�� (100%���� ���� ������ �¶��� �÷��� ����)
    public int maxOfflineTimeInHours = 12; // �ִ� �������� �ð� (12�ð�)

    // ������ ���� �ð�
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
        // ������ ���� �ð� �ε�
        long lastLoginTicks = PlayerPrefs.GetInt("LastLoginTimeBinary", 0);
        if (lastLoginTicks == 0)
        {
            // ù ������ ��� ���� �ð� ����
            lastLoginTime = DateTime.Now;
            SaveLastLoginTime();
        }
        else
        {
            // ������ ���� �ð� ����
            lastLoginTime = DateTime.FromBinary(lastLoginTicks);

            // �������� ���� ���
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
            // ���� ��׶���� �� �� ���� �ð� ����
            SaveLastLoginTime();
        }
        else
        {
            // ���� ���׶���� ���ƿ� �� �������� ���� ���
            // ������ ���� �ð� ����
            long lastLoginTicks = PlayerPrefs.GetInt("LastLoginTimeBinary", 0);
            if (lastLoginTicks != 0)
            {
                lastLoginTime = DateTime.FromBinary(lastLoginTicks);
                CalculateOfflineProgress();
            }

            // ���� �ð� �ٽ� ����
            SaveLastLoginTime();
        }
    }

    private void OnApplicationQuit()
    {
        // �� ���� �� ���� �ð� ����
        SaveLastLoginTime();
    }

    private void SaveLastLoginTime()
    {
        // ���� �ð��� ���� �������� ����
        DateTime now = DateTime.Now;
        PlayerPrefs.SetInt("LastLoginTimeBinary", now.ToBinary());
        PlayerPrefs.Save();
    }

    private void CalculateOfflineProgress()
    {
        // ���� �ð��� ������ ���� �ð��� ���� ���
        TimeSpan offlineTime = DateTime.Now - lastLoginTime;

        // �ִ� �������� �ð����� ����
        double hoursOffline = Math.Min(offlineTime.TotalHours, maxOfflineTimeInHours);

        if (hoursOffline <= 0)
            return;

        Debug.Log("�������� �ð�: " + hoursOffline + "�ð�");

        // �������� ���� ȹ���� �ڿ� ���
        CalculateOfflineResources(hoursOffline);

        // �������� ���� óġ�� ���� ���
        CalculateOfflineMonsters(hoursOffline);

        // �������� ��� ǥ��
        ShowOfflineResultsUI(hoursOffline);
    }

    private void CalculateOfflineResources(double hoursOffline)
    {
        // �ð��� �ڿ� ȹ�淮 (����, ���׷��̵� � ���� ����)
        float goldPerHour = 100 * GameManager.instance.playerLevel.currentLevel;
        float expPerHour = 50 * GameManager.instance.playerLevel.currentLevel;

        // �������� �ð� ���� ȹ���� �ڿ� ���
        int totalGold = Mathf.RoundToInt((float)(goldPerHour * hoursOffline * offlineEfficiency));
        int totalExp = Mathf.RoundToInt((float)(expPerHour * hoursOffline * offlineEfficiency));

        // �ڿ� �߰�
        CurrencyManager.instance?.AddGold(totalGold);
        PlayerLevel.instance?.AddExperience(totalExp);
    }

    private void CalculateOfflineMonsters(double hoursOffline)
    {
        // �ð��� óġ ���� �� (�÷��̾� ���ݷ�, �ӵ� � ���� ����)
        float monstersPerHour = 10 * GameManager.instance.playerLevel.currentLevel;

        // �������� �ð� ���� óġ�� ���� �� ���
        int totalMonsters = Mathf.RoundToInt((float)(monstersPerHour * hoursOffline * offlineEfficiency));

        // ���� óġ ������ �̹� CalculateOfflineResources���� ���Ǿ����Ƿ� ���⼭�� �������θ� ���
        Debug.Log("�������� ���� óġ�� ����: " + totalMonsters + "����");
    }

    private void ShowOfflineResultsUI(double hoursOffline)
    {
        // �������� ��� UI�� ǥ���ϴ� �ڵ�
        // GameUIManager�� ���� ����

        // �ð��� �ڿ� ȹ�淮 (����, ���׷��̵� � ���� ����)
        float goldPerHour = 100 * GameManager.instance.playerLevel.currentLevel;
        float expPerHour = 50 * GameManager.instance.playerLevel.currentLevel;

        // �������� �ð� ���� ȹ���� �ڿ� ���
        int totalGold = Mathf.RoundToInt((float)(goldPerHour * hoursOffline * offlineEfficiency));
        int totalExp = Mathf.RoundToInt((float)(expPerHour * hoursOffline * offlineEfficiency));

        // UI �Ŵ����� ���� ��� ǥ��
        GameUIManager.instance?.ShowOfflineProgressResults(
            hoursOffline,
            totalGold,
            totalExp,
            Mathf.RoundToInt((float)(10 * GameManager.instance.playerLevel.currentLevel * hoursOffline * offlineEfficiency))
        );
    }
}