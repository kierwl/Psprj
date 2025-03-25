using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    public float spawnInterval = 3f;
    public Transform[] customSpawnPoints; // ����� ���� ���� ����Ʈ (�ɼ�)

    [Header("Enemy Configuration")]
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>(); // ������ �� ���� ���

    [Header("Dynamic Settings")]
    public bool usePlayerAsCenter = true; // �÷��̾� �ֺ� ���� ����
    public bool spawnOnStart = true;      // ���� �� ���� ����
    public bool limitEnemies = true;      // �ִ� �� �� ���� ����

    [Header("Runtime References")]
    [SerializeField] private int currentEnemies = 0;
    private Transform player;
    private Coroutine spawnCoroutine;
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // ������ �� ����

    // �� ���� ������ Ŭ����
    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemySO enemyData; // �� ScriptableObject
        public int weight = 1;    // ���� ����ġ (�������� �� ���� ����)
        public int level = 1;     // �� ����
        [Range(0, 100)]
        public int spawnChance = 100; // ���� Ȯ�� (%)
    }

    void Start()
    {
        // �÷��̾� ã��
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null && usePlayerAsCenter)
        {
            Debug.LogWarning("Player�� ã�� �� �����ϴ�. ���� �ý����� ����� �۵����� ���� �� �ֽ��ϴ�.");
        }

        // ���� �� ����
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        // �̹� ���� ���� ��� �ߺ� ���� ����
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnEnemies());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnEnemies()
    {
        // �� ���� ������ ���� ��� ���
        if (enemyTypes.Count == 0)
        {
            Debug.LogWarning("������ �� ������ �������� �ʾҽ��ϴ�.");
            yield break;
        }

        while (true)
        {
            // �ִ� �� �� ���� Ȯ��
            if (!limitEnemies || currentEnemies < maxEnemies)
            {
                // Ŭ����: ���ŵ� �� ���� ����
                CleanupDestroyedEnemies();

                // �� ����
                SpawnEnemy();
            }

            // ���� �������� ���
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        // ���� ��ġ ����
        Vector3 spawnPosition;
        if (customSpawnPoints != null && customSpawnPoints.Length > 0)
        {
            // ����� ���� ���� ����Ʈ ���
            Transform spawnPoint = customSpawnPoints[Random.Range(0, customSpawnPoints.Length)];
            spawnPosition = spawnPoint.position;
        }
        else if (usePlayerAsCenter && player != null)
        {
            // �÷��̾� �ֺ� ���� ��ġ
            Vector3 randomPos = player.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0; // �ٴ� ���̷� ����

            // NavMesh ���� ��ġ ����
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                Debug.LogWarning("������ NavMesh ��ġ�� ã�� �� �����ϴ�.");
                return;
            }
        }
        else
        {
            // ������ �ֺ� ���� ��ġ
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0;

            // NavMesh ���� ��ġ ����
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                Debug.LogWarning("������ NavMesh ��ġ�� ã�� �� �����ϴ�.");
                return;
            }
        }

        // ����ġ ��� �� ���� ����
        EnemySpawnData selectedEnemyData = SelectEnemyToSpawn();
        if (selectedEnemyData == null || selectedEnemyData.enemyData == null)
        {
            Debug.LogWarning("������ �� ������ �����ϴ�.");
            return;
        }

        // ���� Ȯ�� üũ
        if (Random.Range(0, 100) >= selectedEnemyData.spawnChance)
        {
            return; // ���� Ȯ�� ����
        }

        // �� ���� - EnemyFactory ��� (������ ���)
        GameObject enemy;

        if (EnemyFactory.instance != null)
        {
            // EnemyFactory�� ���� ����
            enemy = EnemyFactory.instance.SpawnEnemy(
                selectedEnemyData.enemyData,
                selectedEnemyData.level,
                spawnPosition
            );
        }
        else
        {
            // ���� ���� (EnemyFactory�� ���� ���)
            enemy = CreateEnemyDirectly(selectedEnemyData, spawnPosition);
        }

        if (enemy != null)
        {
            // ������ �� ����
            spawnedEnemies.Add(enemy);
            currentEnemies++;

            // ��� �̺�Ʈ ����
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.OnDeathEvent += () => OnEnemyDeath(enemy);
            }
        }
    }

    private GameObject CreateEnemyDirectly(EnemySpawnData spawnData, Vector3 position)
    {
        // ������ ����
        GameObject prefab = spawnData.enemyData.prefab;
        if (prefab == null)
        {
            Debug.LogError($"�� ������ '{spawnData.enemyData.name}'�� �������� �������� �ʾҽ��ϴ�.");
            return null;
        }

        // �� �ν��Ͻ� ����
        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

        // �� ��Ʈ�ѷ� �ʱ�ȭ
        EnemyController controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.Initialize(spawnData.enemyData, spawnData.level);
        }
        else
        {
            Debug.LogError($"������ �� '{enemy.name}'�� EnemyController ������Ʈ�� �����ϴ�.");
        }

        return enemy;
    }

    private EnemySpawnData SelectEnemyToSpawn()
    {
        // ����ġ �հ� ���
        int totalWeight = 0;
        foreach (var enemyType in enemyTypes)
        {
            totalWeight += enemyType.weight;
        }

        // ����ġ ��� ���� ����
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var enemyType in enemyTypes)
        {
            currentWeight += enemyType.weight;
            if (randomValue < currentWeight)
            {
                return enemyType;
            }
        }

        // �⺻������ ù ��° �� ��ȯ
        return enemyTypes.Count > 0 ? enemyTypes[0] : null;
    }

    private void OnEnemyDeath(GameObject enemy)
    {
        // �� ī���� ����
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
            currentEnemies--;
        }
    }

    private void CleanupDestroyedEnemies()
    {
        // null ���� ����
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
                currentEnemies--;
            }
        }
    }

    // ��� �� ����
    public void ClearAllEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        spawnedEnemies.Clear();
        currentEnemies = 0;
    }

    // ���� �� �� ��ȯ
    public int GetCurrentEnemyCount()
    {
        return currentEnemies;
    }

    // �����Ϳ��� �ð�ȭ
    private void OnDrawGizmosSelected()
    {
        // ���� ���� �ð�ȭ
        Gizmos.color = new Color(1f, 0.5f, 0, 0.2f);

        if (usePlayerAsCenter && Application.isPlaying && player != null)
        {
            Gizmos.DrawSphere(player.position, spawnRadius);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, spawnRadius);
        }

        // ���� ����Ʈ �ð�ȭ
        if (customSpawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (var point in customSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.5f);
                }
            }
        }
    }
}