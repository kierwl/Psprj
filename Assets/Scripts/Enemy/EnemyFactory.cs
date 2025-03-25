using UnityEngine;
using System.Collections.Generic;

public class EnemyFactory : MonoBehaviour
{
    public static EnemyFactory instance;

    [Header("Enemy Prefabs")]
    public GameObject defaultEnemyPrefab;
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    // �� ������ ĳ�� (ID -> ������)
    private Dictionary<string, GameObject> enemyPrefabCache = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // ������ ĳ�� �ʱ�ȭ
        InitializePrefabCache();
    }

    private void InitializePrefabCache()
    {
        foreach (var prefab in enemyPrefabs)
        {
            EnemyController controller = prefab.GetComponent<EnemyController>();
            if (controller != null && controller.enemyData != null)
            {
                string enemyId = controller.enemyData.name;
                enemyPrefabCache[enemyId] = prefab;
            }
        }
    }

    // �� ���� �޼��� - �����Ϳ� ������ ����
    public GameObject SpawnEnemy(EnemySO enemyData, int level, Vector3 position)
    {
        if (enemyData == null)
        {
            Debug.LogError("Enemy data is null!");
            return null;
        }

        // ������ ������ ã��
        GameObject prefab = null;
        string enemyId = enemyData.name;

        if (enemyPrefabCache.ContainsKey(enemyId))
        {
            prefab = enemyPrefabCache[enemyId];
        }
        else
        {
            // ĳ�ÿ� ������ �⺻ ������ ���
            prefab = defaultEnemyPrefab;
        }

        // ������ �ν��Ͻ�ȭ
        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

        // �� ��Ʈ�ѷ� �ʱ�ȭ
        EnemyController controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.Initialize(enemyData, level);
        }

        return enemy;
    }

    // �� ID�� ���� (�������� ������ ��� ���)
    public GameObject SpawnEnemyById(string enemyId, int level, Vector3 position)
    {
        // Resources �������� EnemyDataSO �ε�
        EnemySO enemyData = Resources.Load<EnemySO>($"EnemyData/{enemyId}");

        if (enemyData == null)
        {
            Debug.LogError($"Enemy data with ID {enemyId} not found!");
            return null;
        }

        return SpawnEnemy(enemyData, level, position);
    }
}