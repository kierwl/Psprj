using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class AFKBonusSystem : MonoBehaviour
{
    [Header("AFK Bonus Settings")]
    public float bonusMaxTime = 3600f; // �ִ� ���ʽ� ���� �ð� (1�ð�)
    public float bonusInterval = 60f; // ���ʽ� ȹ�� ���� (60��)
    public int goldBonus = 50; // �⺻ ��� ���ʽ�
    public int expBonus = 20; // �⺻ ����ġ ���ʽ�

    [Header("UI References")]
    public Slider bonusProgressBar;
    public TextMeshProUGUI bonusTimeText;
    public Button collectButton;

    private float currentBonusTime = 0f;
    private bool canCollect = false;

    private void Start()
    {
        // ��ư �̺�Ʈ ���
        if (collectButton != null)
            collectButton.onClick.AddListener(CollectBonus);

        // �ʱ� ���� ����
        UpdateBonusUI();
    }

    private void Update()
    {
        // ���ʽ� �ð� ����
        if (currentBonusTime < bonusMaxTime)
        {
            currentBonusTime += Time.deltaTime;
            currentBonusTime = Mathf.Min(currentBonusTime, bonusMaxTime);

            // ���ʽ� ȹ�� ���� ���� Ȯ��
            canCollect = currentBonusTime >= bonusInterval;

            // UI ������Ʈ
            UpdateBonusUI();

            // ���� ��ư Ȱ��ȭ/��Ȱ��ȭ
            if (collectButton != null)
                collectButton.interactable = canCollect;
        }
    }

    private void UpdateBonusUI()
    {
        // ���α׷��� �� ������Ʈ
        if (bonusProgressBar != null)
        {
            bonusProgressBar.value = currentBonusTime / bonusMaxTime;
        }

        // �ð� �ؽ�Ʈ ������Ʈ
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

        // ���ʽ� Ƚ�� ��� (�� ���� ������ ��������)
        int bonusCount = Mathf.FloorToInt(currentBonusTime / bonusInterval);

        // �÷��̾� ������ ���� ���ʽ� ����
        int playerLevel = PlayerLevel.instance?.currentLevel ?? 1;
        int totalGoldBonus = goldBonus * bonusCount * playerLevel;
        int totalExpBonus = expBonus * bonusCount * playerLevel;

        // ���ʽ� ����
        CurrencyManager.instance?.AddGold(totalGoldBonus);
        PlayerLevel.instance?.AddExperience(totalExpBonus);

        // ���ʽ� �ð� ���� (������ �����ϰų� �Ϻθ� ����)
        currentBonusTime = 0f; // �Ǵ� currentBonusTime % bonusInterval; (���� �ð� ����)
        canCollect = false;

        // UI ������Ʈ
        UpdateBonusUI();

        // ���� ��ư ��Ȱ��ȭ
        if (collectButton != null)
            collectButton.interactable = false;

        // ȿ�� �Ǵ� �˸�
        Debug.Log("AFK ���ʽ� ����: ��� " + totalGoldBonus + ", ����ġ " + totalExpBonus);
    }
}