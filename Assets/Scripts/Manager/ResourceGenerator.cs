using UnityEngine;
using System.Collections;

public class ResourceGenerator : MonoBehaviour
{
    [System.Serializable]
    public class ResourceType
    {
        public string name;
        public float baseAmount;
        public float perLevelIncrease;
        public float generateInterval;
    }

    public ResourceType goldGenerator;
    public ResourceType gemsGenerator;

    private int playerLevel = 1;
    private Coroutine goldRoutine;
    private Coroutine gemsRoutine;

    private void Start()
    {
        // 플레이어 레벨 참조
        playerLevel = PlayerLevel.instance?.currentLevel ?? 1;

        // 자원 생성 코루틴 시작
        StartGenerators();
    }

    public void StartGenerators()
    {
        if (goldRoutine != null)
            StopCoroutine(goldRoutine);

        if (gemsRoutine != null)
            StopCoroutine(gemsRoutine);

        goldRoutine = StartCoroutine(GenerateResource(goldGenerator, AddGold));
        gemsRoutine = StartCoroutine(GenerateResource(gemsGenerator, AddGems));
    }

    private IEnumerator GenerateResource(ResourceType resource, System.Action<int> addResourceFunc)
    {
        while (true)
        {
            // 현재 레벨에 따른 생성량 계산
            float amount = resource.baseAmount + (playerLevel - 1) * resource.perLevelIncrease;
            int roundedAmount = Mathf.RoundToInt(amount);

            // 자원 추가
            addResourceFunc(roundedAmount);

            // 다음 생성까지 대기
            yield return new WaitForSeconds(resource.generateInterval);
        }
    }

    private void AddGold(int amount)
    {
        CurrencyManager.instance?.AddGold(amount);
    }

    private void AddGems(int amount)
    {
        CurrencyManager.instance?.AddGems(amount);
    }

    // 플레이어 레벨 변경 시 호출
    public void UpdatePlayerLevel(int newLevel)
    {
        playerLevel = newLevel;

        // 새 레벨로 생성기 재시작 (선택 사항)
        StartGenerators();
    }
}