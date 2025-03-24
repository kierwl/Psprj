using UnityEngine;
using System.Collections.Generic;

public class AutoBattleManager : MonoBehaviour
{
    public static AutoBattleManager instance;

    [Header("Auto Battle Settings")]
    public bool autoBattleEnabled = true;
    public float updateInterval = 0.5f; // 최적화를 위한 업데이트 간격

    private float nextUpdateTime = 0f;
    private GameObject player;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (!autoBattleEnabled || player == null)
            return;

        // 최적화: 일정 간격으로만 업데이트
        if (Time.time >= nextUpdateTime)
        {
            UpdateEnemyList();
            OptimizeBattle();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateEnemyList()
    {
        // 활성 적 목록 갱신
        activeEnemies.Clear();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            activeEnemies.Add(enemy);
        }
    }

    private void OptimizeBattle()
    {
        // 너무 멀리 있는 적 비활성화 (최적화)
        foreach (GameObject enemy in activeEnemies)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, enemy.transform.position);

            // NavMeshAgent 및 AI 컴포넌트 최적화
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                if (distanceToPlayer > 20f) // 일정 거리 이상이면
                {
                    agent.enabled = false; // NavMeshAgent 비활성화
                }
                else
                {
                    agent.enabled = true;
                }
            }

            // 적 AI 스크립트 최적화
            EnemyController enemyAI = enemy.GetComponent<EnemyController>();
            if (enemyAI != null)
            {
                if (distanceToPlayer > 20f)
                {
                    enemyAI.enabled = false;
                }
                else
                {
                    enemyAI.enabled = true;
                }
            }
        }
    }

    // 자동 전투 토글
    public void ToggleAutoBattle()
    {
        autoBattleEnabled = !autoBattleEnabled;

        // 플레이어 컨트롤러 연결
        PlayerController playerController = player?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetAutoBattle(autoBattleEnabled);
        }

        Debug.Log("자동 전투: " + (autoBattleEnabled ? "활성화" : "비활성화"));
    }
}