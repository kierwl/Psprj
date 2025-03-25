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
    public LayerMask obstacleLayer;

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
        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("플레이어를 찾을 수 없습니다.");
        }

        //   
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
        // 스폰 위치 계산
        Vector3 spawnPosition = CalculateSpawnPosition();

        // 적 오브젝트 인스턴스화
        GameObject enemy = Instantiate(enemyTypes[0].enemyData.prefab, spawnPosition, Quaternion.identity);

        // 적 컨트롤러 초기화
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.Initialize(enemyTypes[0].enemyData, enemyTypes[0].level);
            enemyController.OnDeathEvent += () => OnEnemyDeath(enemy);
        }

        // 스폰된 적 목록에 추가
        spawnedEnemies.Add(enemy);
        currentEnemies++;
    }

    // 스폰 위치 계산
    private Vector3 CalculateSpawnPosition()
    {
        if (player == null) return transform.position;

        // 플레이어 주변의 랜덤한 위치 계산
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(spawnRadius * 0.5f, spawnRadius);
        
        Vector3 randomOffset = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward * randomDistance;
        Vector3 spawnPosition = player.position + randomOffset;

        // NavMesh 위의 유효한 위치 찾기
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, spawnRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            spawnPosition = hit.position;
        }
        else
        {
            Debug.LogWarning("스폰 위치를 찾을 수 없습니다.");
            return transform.position;
        }

        return spawnPosition;
    }

    private EnemySpawnData SelectEnemyToSpawn()
    {
        // ġ հ� ���
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
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
            currentEnemies--;
            Debug.Log($"적 사망: {enemy.name}");
        }
    }

    private void CleanupDestroyedEnemies()
    {
        // null  
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
                currentEnemies--;
            }
        }
    }

    // 모든 적 제거
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

    // 현재 스폰된 적 수 반환
    public int GetCurrentEnemyCount()
    {
        return currentEnemies;
    }

    // 스폰 가능 여부 확인
    public bool CanSpawn()
    {
        return currentEnemies < maxEnemies;
    }

    //    ȯ
    private void OnDrawGizmosSelected()
    {
        //  ðȭ
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

    // StageManager에서 호출하는 스폰 메서드
    public void SpawnEnemy(EnemySO enemyData, int level)
    {
        if (enemyData == null || enemyData.prefab == null)
        {
            Debug.LogError($"적 프리팹이 없습니다: {enemyData?.name ?? "null"}");
            return;
        }

        // 스폰 위치 계산
        Vector3 spawnPosition = CalculateSpawnPosition();

        // 적 오브젝트 인스턴스화
        GameObject enemy = Instantiate(enemyData.prefab, spawnPosition, Quaternion.identity);

        // 적 컨트롤러 초기화
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.Initialize(enemyData, level);
            enemyController.OnDeathEvent += () => OnEnemyDeath(enemy);
        }
        else
        {
            Debug.LogError($"EnemyController가 없습니다: {enemy.name}");
            return;
        }

        // 스폰된 적 목록에 추가
        spawnedEnemies.Add(enemy);
        currentEnemies++;

        Debug.Log($"적 스폰: {enemyData.name} (레벨 {level})");
    }
}