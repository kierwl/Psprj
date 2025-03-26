using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Button closeButton;

    [Header("스탯 표시 요소")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI criticalChanceText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;


    // 플레이어 스탯 참조
    private PlayerStats playerStats;
    // 표시 중인 버프 아이템 목록
    private List<GameObject> buffItems = new List<GameObject>();

    private void Awake()
    {
        // 닫기 버튼 이벤트 등록
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseStatsPanel);

    }

    private void Start()
    {
        // 플레이어 스탯 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();

            // PlayerStats 이벤트 구독
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += OnHealthChanged;
                playerStats.OnBuffApplied += OnBuffChanged;
                playerStats.OnBuffRemoved += OnBuffChanged;

                // 초기 스탯 업데이트
                UpdateAllStats();
            }
        }

    }

    private void OnDestroy()
    {
        // 이벤트 등록 해제
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= OnHealthChanged;
            playerStats.OnBuffApplied -= OnBuffChanged;
            playerStats.OnBuffRemoved -= OnBuffChanged;
        }


        // 닫기 버튼 이벤트 해제
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseStatsPanel);
    }

    // 모든 스탯 업데이트
    public void UpdateAllStats()
    {
        if (playerStats == null) return;

        // 기본 정보 업데이트
        if (playerNameText != null)
            playerNameText.text = "플레이어"; // 이름이 있다면 변경

        if (playerLevelText != null && PlayerLevel.instance != null)
            playerLevelText.text = $"Lv. {PlayerLevel.instance.currentLevel}";

        // 체력 정보 업데이트
        UpdateHealthInfo(playerStats.GetCurrentHealth(), playerStats.GetMaxHealth());

        // 스탯 정보 업데이트
        UpdateStatTexts();

    }

    // 체력 정보 업데이트
    private void UpdateHealthInfo(float currentHealth, float maxHealth)
    {
        if (healthText != null)
            healthText.text = $"체력: {currentHealth:F0} / {maxHealth:F0}";
    }

    // 스탯 텍스트 업데이트
    private void UpdateStatTexts()
    {
        if (playerStats == null) return;

        // 공격력
        if (attackText != null)
        {
            float baseAttack = playerStats.GetBaseStatValue(PlayerStats.StatType.Attack);
            float totalAttack = playerStats.GetStat(PlayerStats.StatType.Attack);
            float bonus = totalAttack - baseAttack;

            if (bonus > 0)
                attackText.text = $"공격력: {baseAttack:F1} <color=#00FF00>(+{bonus:F1})</color>";
            else
                attackText.text = $"공격력: {totalAttack:F1}";
        }

        // 방어력
        if (defenseText != null)
        {
            float baseDefense = playerStats.GetBaseStatValue(PlayerStats.StatType.Defense);
            float totalDefense = playerStats.GetStat(PlayerStats.StatType.Defense);
            float bonus = totalDefense - baseDefense;

            if (bonus > 0)
                defenseText.text = $"방어력: {baseDefense:F1} <color=#00FF00>(+{bonus:F1})</color>";
            else
                defenseText.text = $"방어력: {totalDefense:F1}";
        }

        // 크리티컬 확률
        if (criticalChanceText != null)
        {
            float baseCrit = playerStats.GetBaseStatValue(PlayerStats.StatType.CriticalChance);
            float totalCrit = playerStats.GetStat(PlayerStats.StatType.CriticalChance);
            float bonus = totalCrit - baseCrit;

            if (bonus > 0)
                criticalChanceText.text = $"치명타 확률: {baseCrit * 100:F1}% <color=#00FF00>(+{bonus * 100:F1}%)</color>";
            else
                criticalChanceText.text = $"치명타 확률: {totalCrit * 100:F1}%";
        }

        // 공격 속도
        if (attackSpeedText != null)
        {
            float baseSpeed = playerStats.GetBaseStatValue(PlayerStats.StatType.AttackSpeed);
            float totalSpeed = playerStats.GetStat(PlayerStats.StatType.AttackSpeed);
            float bonus = totalSpeed - baseSpeed;

            if (bonus > 0)
                attackSpeedText.text = $"공격 속도: {baseSpeed:F2} <color=#00FF00>(+{bonus:F2})</color>";
            else
                attackSpeedText.text = $"공격 속도: {totalSpeed:F2}";
        }

        // 이동 속도
        if (moveSpeedText != null)
        {
            float baseSpeed = playerStats.GetBaseStatValue(PlayerStats.StatType.MoveSpeed);
            float totalSpeed = playerStats.GetStat(PlayerStats.StatType.MoveSpeed);
            float bonus = totalSpeed - baseSpeed;

            if (bonus > 0)
                moveSpeedText.text = $"이동 속도: {baseSpeed:F1} <color=#00FF00>(+{bonus:F1})</color>";
            else
                moveSpeedText.text = $"이동 속도: {totalSpeed:F1}";
        }
    }


    // 체력 변경 이벤트
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthInfo(currentHealth, maxHealth);
    }

    // 버프 변경 이벤트
    private void OnBuffChanged(PlayerStats.BuffData buff)
    {
        // 스탯 텍스트 업데이트
        UpdateStatTexts();

    }

    // 레벨 변경 이벤트
    private void OnPlayerLevelChanged(int newLevel)
    {
        if (playerLevelText != null)
            playerLevelText.text = $"Lv. {newLevel}";
    }

    // UI 표시/숨기기
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

    // 스탯 패널 닫기
    public void CloseStatsPanel()
    {
        if (statsPanel != null)
            statsPanel.SetActive(false);
    }

    // 스탯 패널 열기
    public void OpenStatsPanel()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
            UpdateAllStats();
        }
    }
}