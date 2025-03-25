using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    public float spawnInterval = 3f;
    public Transform[] customSpawnPoints; // 사용자 지정 스폰 포인트 (옵션)

    [Header("Enemy Configuration")]
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>(); // 생성할 적 유형 목록

    [Header("Dynamic Settings")]
    public bool usePlayerAsCenter = true; // 플레이어 주변 스폰 여부
    public bool spawnOnStart = true;      // 시작 시 스폰 여부
    public bool limitEnemies = true;      // 최대 적 수 제한 여부

    [Header("Runtime References")]
    [SerializeField] private int currentEnemies = 0;
    private Transform player;
    private Coroutine spawnCoroutine;
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // 생성된 적 추적

    // 적 스폰 데이터 클래스
    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemySO enemyData; // 적 ScriptableObject
        public int weight = 1;    // 스폰 가중치 (높을수록 더 자주 등장)
        public int level = 1;     // 적 레벨
        [Range(0, 100)]
        public int spawnChance = 100; // 스폰 확률 (%)
    }

    void Start()
    {
        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null && usePlayerAsCenter)
        {
            Debug.LogWarning("Player를 찾을 수 없습니다. 스폰 시스템이 제대로 작동하지 않을 수 있습니다.");
        }

        // 시작 시 스폰
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        // 이미 실행 중인 경우 중복 실행 방지
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
        // 적 생성 유형이 없는 경우 경고
        if (enemyTypes.Count == 0)
        {
            Debug.LogWarning("생성할 적 유형이 설정되지 않았습니다.");
            yield break;
        }

        while (true)
        {
            // 최대 적 수 제한 확인
            if (!limitEnemies || currentEnemies < maxEnemies)
            {
                // 클린업: 제거된 적 참조 정리
                CleanupDestroyedEnemies();

                // 적 생성
                SpawnEnemy();
            }

            // 다음 스폰까지 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        // 스폰 위치 결정
        Vector3 spawnPosition;
        if (customSpawnPoints != null && customSpawnPoints.Length > 0)
        {
            // 사용자 지정 스폰 포인트 사용
            Transform spawnPoint = customSpawnPoints[Random.Range(0, customSpawnPoints.Length)];
            spawnPosition = spawnPoint.position;
        }
        else if (usePlayerAsCenter && player != null)
        {
            // 플레이어 주변 랜덤 위치
            Vector3 randomPos = player.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0; // 바닥 높이로 조정

            // NavMesh 위에 위치 지정
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                Debug.LogWarning("적합한 NavMesh 위치를 찾을 수 없습니다.");
                return;
            }
        }
        else
        {
            // 스포너 주변 랜덤 위치
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0;

            // NavMesh 위에 위치 지정
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                Debug.LogWarning("적합한 NavMesh 위치를 찾을 수 없습니다.");
                return;
            }
        }

        // 가중치 기반 적 유형 선택
        EnemySpawnData selectedEnemyData = SelectEnemyToSpawn();
        if (selectedEnemyData == null || selectedEnemyData.enemyData == null)
        {
            Debug.LogWarning("선택할 적 유형이 없습니다.");
            return;
        }

        // 스폰 확률 체크
        if (Random.Range(0, 100) >= selectedEnemyData.spawnChance)
        {
            return; // 스폰 확률 실패
        }

        // 적 생성 - EnemyFactory 사용 (가능한 경우)
        GameObject enemy;

        if (EnemyFactory.instance != null)
        {
            // EnemyFactory를 통해 생성
            enemy = EnemyFactory.instance.SpawnEnemy(
                selectedEnemyData.enemyData,
                selectedEnemyData.level,
                spawnPosition
            );
        }
        else
        {
            // 직접 생성 (EnemyFactory가 없는 경우)
            enemy = CreateEnemyDirectly(selectedEnemyData, spawnPosition);
        }

        if (enemy != null)
        {
            // 생성된 적 추적
            spawnedEnemies.Add(enemy);
            currentEnemies++;

            // 사망 이벤트 연결
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.OnDeathEvent += () => OnEnemyDeath(enemy);
            }
        }
    }

    private GameObject CreateEnemyDirectly(EnemySpawnData spawnData, Vector3 position)
    {
        // 프리팹 결정
        GameObject prefab = spawnData.enemyData.prefab;
        if (prefab == null)
        {
            Debug.LogError($"적 데이터 '{spawnData.enemyData.name}'에 프리팹이 설정되지 않았습니다.");
            return null;
        }

        // 적 인스턴스 생성
        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

        // 적 컨트롤러 초기화
        EnemyController controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.Initialize(spawnData.enemyData, spawnData.level);
        }
        else
        {
            Debug.LogError($"생성된 적 '{enemy.name}'에 EnemyController 컴포넌트가 없습니다.");
        }

        return enemy;
    }

    private EnemySpawnData SelectEnemyToSpawn()
    {
        // 가중치 합계 계산
        int totalWeight = 0;
        foreach (var enemyType in enemyTypes)
        {
            totalWeight += enemyType.weight;
        }

        // 가중치 기반 랜덤 선택
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

        // 기본값으로 첫 번째 적 반환
        return enemyTypes.Count > 0 ? enemyTypes[0] : null;
    }

    private void OnEnemyDeath(GameObject enemy)
    {
        // 적 카운터 감소
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
            currentEnemies--;
        }
    }

    private void CleanupDestroyedEnemies()
    {
        // null 참조 제거
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

    // 현재 적 수 반환
    public int GetCurrentEnemyCount()
    {
        return currentEnemies;
    }

    // 에디터에서 시각화
    private void OnDrawGizmosSelected()
    {
        // 스폰 범위 시각화
        Gizmos.color = new Color(1f, 0.5f, 0, 0.2f);

        if (usePlayerAsCenter && Application.isPlaying && player != null)
        {
            Gizmos.DrawSphere(player.position, spawnRadius);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, spawnRadius);
        }

        // 스폰 포인트 시각화
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