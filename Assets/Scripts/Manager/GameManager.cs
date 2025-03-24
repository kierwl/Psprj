// ���� �Ŵ��� ������Ʈ
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
        // ���� �ʱ�ȭ
        InitializeGame();
    }

    private void InitializeGame()
    {
        // �� �Ŵ��� �ʱ�ȭ
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

    // �� ��� �� ����ġ �� ��� ����
    public void EnemyDefeated(int expReward, int goldReward)
    {
        playerLevel?.AddExperience(expReward);
        currencyManager?.AddGold(goldReward);
    }
}