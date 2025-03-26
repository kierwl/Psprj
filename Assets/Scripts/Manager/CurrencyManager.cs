// ��ȭ ���� ��ũ��Ʈ
using UnityEngine;
using TMPro;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;

    [Header("Currency Amount")]
    public int gold = 0;
    public int gems = 0;
    public int energy = 100;
    public int maxEnergy = 100;

    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI energyText;

    // �̺�Ʈ ����
    public event Action<int> OnGoldChanged;
    public event Action<int> OnGemsChanged;
    public event Action<int, int> OnEnergyChanged;

    // �÷��̾� ���� ���� ����
    private PlayerStats playerStats;
    private bool syncWithPlayerStats = true;

    private void Awake()
    {
        // �̱��� ����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // �÷��̾� ���� ã��
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();

            // PlayerStats�� �̺�Ʈ ����
            if (playerStats != null)
            {
                // PlayerStats �ʱ� ��� �� ����ȭ
                gold = playerStats.GetGold();

                // ��� ���� �̺�Ʈ ����
                playerStats.OnGoldChanged += OnPlayerStatsGoldChanged;

                Debug.Log("CurrencyManager�� PlayerStats�� ����Ǿ����ϴ�.");
            }
        }

        UpdateCurrencyUI();
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (playerStats != null)
        {
            playerStats.OnGoldChanged -= OnPlayerStatsGoldChanged;
        }
    }

    // PlayerStats�� ��� ���� �̺�Ʈ �ڵ鷯
    private void OnPlayerStatsGoldChanged(int newGold)
    {
        // �̹� PlayerStats���� �߻��� �����̶�� ���� ������ �����ϱ� ���� ����ȭ �÷��� ����
        syncWithPlayerStats = false;

        // ��� ������Ʈ
        gold = newGold;

        // UI ������Ʈ
        UpdateCurrencyUI();

        // ����ȭ �÷��� ����
        syncWithPlayerStats = true;

        // �̺�Ʈ �߻�
        OnGoldChanged?.Invoke(gold);
    }

    public void AddGold(int amount)
    {
        gold += amount;

        // PlayerStats�� �ִٸ� PlayerStats���� ��� �߰�
        if (playerStats != null && syncWithPlayerStats)
        {
            playerStats.AddGold(amount);
        }
        else
        {
            // PlayerStats�� ���ų� ����ȭ ���� �ƴ� ��� ���� UI ������Ʈ
            UpdateCurrencyUI();

            // �̺�Ʈ �߻�
            OnGoldChanged?.Invoke(gold);
        }
    }

    public void AddGems(int amount)
    {
        gems += amount;
        UpdateCurrencyUI();

        // �̺�Ʈ �߻�
        OnGemsChanged?.Invoke(gems);
    }

    public void AddEnergy(int amount)
    {
        int oldEnergy = energy;
        energy = Mathf.Min(energy + amount, maxEnergy);
        UpdateCurrencyUI();

        // �̺�Ʈ �߻�
        OnEnergyChanged?.Invoke(energy, maxEnergy);
    }

    public bool SpendGold(int amount)
    {
        // PlayerStats�� �ִ� ��� PlayerStats�� ���� ó��
        if (playerStats != null && syncWithPlayerStats)
        {
            return playerStats.SpendGold(amount);
        }

        // PlayerStats�� ���ų� ����ȭ ���� �ƴ� ��� ���� ó��
        if (gold >= amount)
        {
            gold -= amount;
            UpdateCurrencyUI();

            // �̺�Ʈ �߻�
            OnGoldChanged?.Invoke(gold);
            return true;
        }
        return false;
    }

    public bool SpendGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            UpdateCurrencyUI();

            // �̺�Ʈ �߻�
            OnGemsChanged?.Invoke(gems);
            return true;
        }
        return false;
    }

    public bool SpendEnergy(int amount)
    {
        if (energy >= amount)
        {
            energy -= amount;
            UpdateCurrencyUI();

            // �̺�Ʈ �߻�
            OnEnergyChanged?.Invoke(energy, maxEnergy);
            return true;
        }
        return false;
    }

    public void UpdateCurrencyUI()
    {
        if (goldText != null)
            goldText.text = gold.ToString("N0"); // õ ���� ���� ���� ����

        if (gemsText != null)
            gemsText.text = gems.ToString();

        if (energyText != null)
            energyText.text = energy + "/" + maxEnergy;
    }

    // ��� �� ���� (�ʱ�ȭ �Ǵ� �ε� �� ���)
    public void SetGold(int amount)
    {
        gold = amount;

        // PlayerStats�� �ִٸ� PlayerStats���� ����
        if (playerStats != null && syncWithPlayerStats)
        {
            // PlayerStats ���� �޼��尡 �ִٰ� ����
            // playerStats.SetGold(amount);

            // �Ǵ� Add/Spend �޼��� ���
            int diff = amount - playerStats.GetGold();
            if (diff > 0)
                playerStats.AddGold(diff);
            else if (diff < 0)
                playerStats.SpendGold(-diff);
        }
        else
        {
            // PlayerStats�� ���ų� ����ȭ ���� �ƴ� ��� ���� UI ������Ʈ
            UpdateCurrencyUI();

            // �̺�Ʈ �߻�
            OnGoldChanged?.Invoke(gold);
        }
    }

    // ���� ���� �� �ʿ��� ���� ��������
    public int GetGold()
    {
        // PlayerStats�� �ִٸ� PlayerStats���� ��������
        if (playerStats != null)
        {
            return playerStats.GetGold();
        }

        return gold;
    }
}