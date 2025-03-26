using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Button closeButton;

    [Header("���� ǥ�� ���")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI criticalChanceText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;


    // �÷��̾� ���� ����
    private PlayerStats playerStats;
    // ǥ�� ���� ���� ������ ���
    private List<GameObject> buffItems = new List<GameObject>();

    private void Awake()
    {
        // �ݱ� ��ư �̺�Ʈ ���
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseStatsPanel);

    }

    private void Start()
    {
        // �÷��̾� ���� ã��
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();

            // PlayerStats �̺�Ʈ ����
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += OnHealthChanged;
                playerStats.OnBuffApplied += OnBuffChanged;
                playerStats.OnBuffRemoved += OnBuffChanged;

                // �ʱ� ���� ������Ʈ
                UpdateAllStats();
            }
        }

    }

    private void OnDestroy()
    {
        // �̺�Ʈ ��� ����
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= OnHealthChanged;
            playerStats.OnBuffApplied -= OnBuffChanged;
            playerStats.OnBuffRemoved -= OnBuffChanged;
        }


        // �ݱ� ��ư �̺�Ʈ ����
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseStatsPanel);
    }

    // ��� ���� ������Ʈ
    public void UpdateAllStats()
    {
        if (playerStats == null) return;

        // �⺻ ���� ������Ʈ
        if (playerNameText != null)
            playerNameText.text = "�÷��̾�"; // �̸��� �ִٸ� ����

        if (playerLevelText != null && PlayerLevel.instance != null)
            playerLevelText.text = $"Lv. {PlayerLevel.instance.currentLevel}";

        // ü�� ���� ������Ʈ
        UpdateHealthInfo(playerStats.GetCurrentHealth(), playerStats.GetMaxHealth());

        // ���� ���� ������Ʈ
        UpdateStatTexts();

    }

    // ü�� ���� ������Ʈ
    private void UpdateHealthInfo(float currentHealth, float maxHealth)
    {
        if (healthText != null)
            healthText.text = $"ü��: {currentHealth:F0} / {maxHealth:F0}";
    }

    // ���� �ؽ�Ʈ ������Ʈ
    private void UpdateStatTexts()
    {
        if (playerStats == null) return;

        // ���ݷ�
        if (attackText != null)
        {
            float baseAttack = playerStats.GetBaseStatValue(PlayerStats.StatType.Attack);
            float totalAttack = playerStats.GetStat(PlayerStats.StatType.Attack);
            float bonus = totalAttack - baseAttack;

            if (bonus > 0)
                attackText.text = $"���ݷ�: {baseAttack:F1} <color=#00FF00>(+{bonus:F1})</color>";
            else
                attackText.text = $"���ݷ�: {totalAttack:F1}";
        }

        // ����
        if (defenseText != null)
        {
            float baseDefense = playerStats.GetBaseStatValue(PlayerStats.StatType.Defense);
            float totalDefense = playerStats.GetStat(PlayerStats.StatType.Defense);
            float bonus = totalDefense - baseDefense;

            if (bonus > 0)
                defenseText.text = $"����: {baseDefense:F1} <color=#00FF00>(+{bonus:F1})</color>";
            else
                defenseText.text = $"����: {totalDefense:F1}";
        }

        // ũ��Ƽ�� Ȯ��
        if (criticalChanceText != null)
        {
            float baseCrit = playerStats.GetBaseStatValue(PlayerStats.StatType.CriticalChance);
            float totalCrit = playerStats.GetStat(PlayerStats.StatType.CriticalChance);
            float bonus = totalCrit - baseCrit;

            if (bonus > 0)
                criticalChanceText.text = $"ġ��Ÿ Ȯ��: {baseCrit * 100:F1}% <color=#00FF00>(+{bonus * 100:F1}%)</color>";
            else
                criticalChanceText.text = $"ġ��Ÿ Ȯ��: {totalCrit * 100:F1}%";
        }

        // ���� �ӵ�
        if (attackSpeedText != null)
        {
            float baseSpeed = playerStats.GetBaseStatValue(PlayerStats.StatType.AttackSpeed);
            float totalSpeed = playerStats.GetStat(PlayerStats.StatType.AttackSpeed);
            float bonus = totalSpeed - baseSpeed;

            if (bonus > 0)
                attackSpeedText.text = $"���� �ӵ�: {baseSpeed:F2} <color=#00FF00>(+{bonus:F2})</color>";
            else
                attackSpeedText.text = $"���� �ӵ�: {totalSpeed:F2}";
        }

        // �̵� �ӵ�
        if (moveSpeedText != null)
        {
            float baseSpeed = playerStats.GetBaseStatValue(PlayerStats.StatType.MoveSpeed);
            float totalSpeed = playerStats.GetStat(PlayerStats.StatType.MoveSpeed);
            float bonus = totalSpeed - baseSpeed;

            if (bonus > 0)
                moveSpeedText.text = $"�̵� �ӵ�: {baseSpeed:F1} <color=#00FF00>(+{bonus:F1})</color>";
            else
                moveSpeedText.text = $"�̵� �ӵ�: {totalSpeed:F1}";
        }
    }


    // ü�� ���� �̺�Ʈ
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthInfo(currentHealth, maxHealth);
    }

    // ���� ���� �̺�Ʈ
    private void OnBuffChanged(PlayerStats.BuffData buff)
    {
        // ���� �ؽ�Ʈ ������Ʈ
        UpdateStatTexts();

    }

    // ���� ���� �̺�Ʈ
    private void OnPlayerLevelChanged(int newLevel)
    {
        if (playerLevelText != null)
            playerLevelText.text = $"Lv. {newLevel}";
    }

    // UI ǥ��/�����
    public void ToggleStatsPanel()
    {
        if (statsPanel != null)
        {
            bool isActive = !statsPanel.activeSelf;
            statsPanel.SetActive(isActive);

            if (isActive)
                UpdateAllStats();
        }
    }

    // ���� �г� �ݱ�
    public void CloseStatsPanel()
    {
        if (statsPanel != null)
            statsPanel.SetActive(false);
    }

    // ���� �г� ����
    public void OpenStatsPanel()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
            UpdateAllStats();
        }
    }
}