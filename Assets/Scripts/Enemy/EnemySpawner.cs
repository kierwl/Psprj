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
            // �� �ִ� �� Ȯ��
            if (currentEnemies < maxEnemies)
            {
                SpawnEnemy();
                currentEnemies++;
            }

            // ���� ���
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null && player != null)
        {
            // ���� ��ġ ���
            Vector3 randomPos = player.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0;  // �ٴ� ���̷� ����

            // NavMesh ���� ��ġ ����
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // �� ����
                GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);

                // ��� �� ī��Ʈ �����ϴ� �̺�Ʈ �߰� (������ ����)
                //Destroy(enemy, 30f);  // 30�� �� �ڵ� ���� (�׽�Ʈ��)
            }
        }
    }

    // ���� ������Ʈ �浹 �˻� (������)
    bool IsPositionClear(Vector3 targetPos, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, radius);
        return hitColliders.Length == 0;
    }
}