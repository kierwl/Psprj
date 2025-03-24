// �ð� �� ���� ������
using UnityEngine;
using TMPro;
using System;

public class TimeRewardManager : MonoBehaviour
{
    public static TimeRewardManager instance;

    [Header("Reward Settings")]
    public int rewardGold = 100;
    public int rewardGems = 5;
    public float rewardInterval = 4 * 60 * 60;  // 4�ð�(�� ����)

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

        // ������ ���� �ð� �ε� (�÷��̾������� ��� ����)
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
        // ������ ���� ���� ��� �ð� ���
        float currentTime = Time.time;
        float elapsedTime = currentTime - lastRewardTime;

        // ���� �ð� ���
        remainingTime = Mathf.Max(0, rewardInterval - elapsedTime);

        // ���� ��ư Ȱ��ȭ ���� ����
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
            timerText.text = "���� �غ� �Ϸ�!";
        }
    }

    public void ClaimReward()
    {
        if (remainingTime <= 0)
        {
            // ���� ����
            CurrencyManager.instance?.AddGold(rewardGold);
            CurrencyManager.instance?.AddGems(rewardGems);

            // ������ ���� �ð� ������Ʈ
            lastRewardTime = Time.time;
            PlayerPrefs.SetFloat("LastRewardTime", lastRewardTime);

            // ȿ�� �� �˸�
            Debug.Log("���� ����: ��� " + rewardGold + ", ���� " + rewardGems);
        }
    }
}