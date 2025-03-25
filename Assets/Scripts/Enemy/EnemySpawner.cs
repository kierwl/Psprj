using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    public float spawnInterval = 3f;

    private Transform player;
    private int currentEnemies = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // 적 최대 수 확인
            if (currentEnemies < maxEnemies)
            {
                SpawnEnemy();
                currentEnemies++;
            }

            // 간격 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null && player != null)
        {
            // 랜덤 위치 계산
            Vector3 randomPos = player.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0;  // 바닥 높이로 조정

            // NavMesh 위에 위치 지정
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // 적 생성
                GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);

                // 사망 시 카운트 감소하는 이벤트 추가 (간단한 버전)
                //Destroy(enemy, 30f);  // 30초 후 자동 제거 (테스트용)
            }
        }
    }

    // 게임 오브젝트 충돌 검사 (선택적)
    bool IsPositionClear(Vector3 targetPos, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, radius);
        return hitColliders.Length == 0;
    }
}