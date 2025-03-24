// 게임 매니저 업데이트
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Managers")]
    public CurrencyManager currencyManager;
    public PlayerLevel playerLevel;
    public CharacterManager characterManager;
    public GameUIManager uiManager;
    public TimeRewardManager timeRewardManager;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 게임 초기화
        InitializeGame();
    }

    private void InitializeGame()
    {
        // 각 매니저 초기화
        if (currencyManager == null)
            currencyManager = FindObjectOfType<CurrencyManager>();

        if (playerLevel == null)
            playerLevel = FindObjectOfType<PlayerLevel>();

        if (characterManager == null)
            characterManager = FindObjectOfType<CharacterManager>();

        if (uiManager == null)
            uiManager = FindObjectOfType<GameUIManager>();

        if (timeRewardManager == null)
            timeRewardManager = FindObjectOfType<TimeRewardManager>();
    }

    // 적 사망 시 경험치 및 골드 보상
    public void EnemyDefeated(int expReward, int goldReward)
    {
        playerLevel?.AddExperience(expReward);
        currencyManager?.AddGold(goldReward);
    }
}