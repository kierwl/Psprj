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
        // �÷��̾� ���� ����
        playerLevel = PlayerLevel.instance?.currentLevel ?? 1;

        // �ڿ� ���� �ڷ�ƾ ����
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
            // ���� ������ ���� ������ ���
            float amount = resource.baseAmount + (playerLevel - 1) * resource.perLevelIncrease;
            int roundedAmount = Mathf.RoundToInt(amount);

            // �ڿ� �߰�
            addResourceFunc(roundedAmount);

            // ���� �������� ���
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

    // �÷��̾� ���� ���� �� ȣ��
    public void UpdatePlayerLevel(int newLevel)
    {
        playerLevel = newLevel;

        // �� ������ ������ ����� (���� ����)
        StartGenerators();
    }
}