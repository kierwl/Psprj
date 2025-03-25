using UnityEngine;
using System.Collections.Generic;

public class EnemyFactory : MonoBehaviour
{
    public static EnemyFactory instance;

    [Header("Enemy Prefabs")]
    public GameObject defaultEnemyPrefab;
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    // 적 프리팹 캐시 (ID -> 프리팹)
    private Dictionary<string, GameObject> enemyPrefabCache = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // 프리팹 캐시 초기화
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

    // 적 생성 메서드 - 데이터와 레벨로 생성
    public GameObject SpawnEnemy(EnemySO enemyData, int level, Vector3 position)
    {
        if (enemyData == null)
        {
            Debug.LogError("Enemy data is null!");
            return null;
        }

        // 적절한 프리팹 찾기
        GameObject prefab = null;
        string enemyId = enemyData.name;

        if (enemyPrefabCache.ContainsKey(enemyId))
        {
            prefab = enemyPrefabCache[enemyId];
        }
        else
        {
            // 캐시에 없으면 기본 프리팹 사용
            prefab = defaultEnemyPrefab;
        }

        // 프리팹 인스턴스화
        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

        // 적 컨트롤러 초기화
        EnemyController controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.Initialize(enemyData, level);
        }

        return enemy;
    }

    // 적 ID로 생성 (스테이지 데이터 등에서 사용)
    public GameObject SpawnEnemyById(string enemyId, int level, Vector3 position)
    {
        // Resources 폴더에서 EnemyDataSO 로드
        EnemySO enemyData = Resources.Load<EnemySO>($"EnemyData/{enemyId}");

        if (enemyData == null)
        {
            Debug.LogError($"Enemy data with ID {enemyId} not found!");
            return null;
        }

        return SpawnEnemy(enemyData, level, position);
    }
}