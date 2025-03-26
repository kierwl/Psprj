// 재화 관리 스크립트
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

    // 이벤트 선언
    public event Action<int> OnGoldChanged;
    public event Action<int> OnGemsChanged;
    public event Action<int, int> OnEnergyChanged;

    // 플레이어 스탯 관련 변수
    private PlayerStats playerStats;
    private bool syncWithPlayerStats = true;

    private void Awake()
    {
        // 싱글톤 설정
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
        // 플레이어 스탯 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();

            // PlayerStats의 이벤트 구독
            if (playerStats != null)
            {
                // PlayerStats 초기 골드 값 동기화
                gold = playerStats.GetGold();

                // 골드 변경 이벤트 구독
                playerStats.OnGoldChanged += OnPlayerStatsGoldChanged;

                Debug.Log("CurrencyManager가 PlayerStats와 연결되었습니다.");
            }
        }

        UpdateCurrencyUI();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerStats != null)
        {
            playerStats.OnGoldChanged -= OnPlayerStatsGoldChanged;
        }
    }

    // PlayerStats의 골드 변경 이벤트 핸들러
    private void OnPlayerStatsGoldChanged(int newGold)
    {
        // 이미 PlayerStats에서 발생한 변경이라면 무한 루프를 방지하기 위해 동기화 플래그 변경
        syncWithPlayerStats = false;

        // 골드 업데이트
        gold = newGold;

        // UI 업데이트
        UpdateCurrencyUI();

        // 동기화 플래그 복원
        syncWithPlayerStats = true;

        // 이벤트 발생
        OnGoldChanged?.Invoke(gold);
    }

    public void AddGold(int amount)
    {
        gold += amount;

        // PlayerStats가 있다면 PlayerStats에도 골드 추가
        if (playerStats != null && syncWithPlayerStats)
        {
            playerStats.AddGold(amount);
        }
        else
        {
            // PlayerStats가 없거나 동기화 중이 아닌 경우 직접 UI 업데이트
            UpdateCurrencyUI();

            // 이벤트 발생
            OnGoldChanged?.Invoke(gold);
        }
    }

    public void AddGems(int amount)
    {
        gems += amount;
        UpdateCurrencyUI();

        // 이벤트 발생
        OnGemsChanged?.Invoke(gems);
    }

    public void AddEnergy(int amount)
    {
        int oldEnergy = energy;
        energy = Mathf.Min(energy + amount, maxEnergy);
        UpdateCurrencyUI();

        // 이벤트 발생
        OnEnergyChanged?.Invoke(energy, maxEnergy);
    }

    public bool SpendGold(int amount)
    {
        // PlayerStats가 있는 경우 PlayerStats를 통해 처리
        if (playerStats != null && syncWithPlayerStats)
        {
            return playerStats.SpendGold(amount);
        }

        // PlayerStats가 없거나 동기화 중이 아닌 경우 직접 처리
        if (gold >= amount)
        {
            gold -= amount;
            UpdateCurrencyUI();

            // 이벤트 발생
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

            // 이벤트 발생
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

            // 이벤트 발생
            OnEnergyChanged?.Invoke(energy, maxEnergy);
            return true;
        }
        return false;
    }

    public void UpdateCurrencyUI()
    {
        if (goldText != null)
            goldText.text = gold.ToString("N0"); // 천 단위 구분 포맷 적용

        if (gemsText != null)
            gemsText.text = gems.ToString();

        if (energyText != null)
            energyText.text = energy + "/" + maxEnergy;
    }

    // 골드 값 설정 (초기화 또는 로드 시 사용)
    public void SetGold(int amount)
    {
        gold = amount;

        // PlayerStats가 있다면 PlayerStats에도 설정
        if (playerStats != null && syncWithPlayerStats)
        {
            // PlayerStats 설정 메서드가 있다고 가정
            // playerStats.SetGold(amount);

            // 또는 Add/Spend 메서드 사용
            int diff = amount - playerStats.GetGold();
            if (diff > 0)
                playerStats.AddGold(diff);
            else if (diff < 0)
                playerStats.SpendGold(-diff);
        }
        else
        {
            // PlayerStats가 없거나 동기화 중이 아닌 경우 직접 UI 업데이트
            UpdateCurrencyUI();

            // 이벤트 발생
            OnGoldChanged?.Invoke(gold);
        }
    }

    // 게임 저장 시 필요한 정보 가져오기
    public int GetGold()
    {
        // PlayerStats가 있다면 PlayerStats에서 가져오기
        if (playerStats != null)
        {
            return playerStats.GetGold();
        }

        return gold;
    }
}