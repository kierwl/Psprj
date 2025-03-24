// 재화 관리 스크립트
using UnityEngine;
using TMPro;

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

    private void Awake()
    {
        // 싱글톤 설정
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateCurrencyUI();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateCurrencyUI();
    }

    public void AddGems(int amount)
    {
        gems += amount;
        UpdateCurrencyUI();
    }

    public void AddEnergy(int amount)
    {
        energy = Mathf.Min(energy + amount, maxEnergy);
        UpdateCurrencyUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateCurrencyUI();
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
            return true;
        }
        return false;
    }

    public void UpdateCurrencyUI()
    {
        if (goldText != null)
            goldText.text = gold.ToString();

        if (gemsText != null)
            gemsText.text = gems.ToString();

        if (energyText != null)
            energyText.text = energy + "/" + maxEnergy;
    }
}