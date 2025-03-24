using UnityEngine;
using System.Collections.Generic;

public class AutoBattleManager : MonoBehaviour
{
    public static AutoBattleManager instance;

    [Header("Auto Battle Settings")]
    public bool autoBattleEnabled = true;
    public float updateInterval = 0.5f; // ����ȭ�� ���� ������Ʈ ����

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

        // ����ȭ: ���� �������θ� ������Ʈ
        if (Time.time >= nextUpdateTime)
        {
            UpdateEnemyList();
            OptimizeBattle();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateEnemyList()
    {
        // Ȱ�� �� ��� ����
        activeEnemies.Clear();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            activeEnemies.Add(enemy);
        }
    }

    private void OptimizeBattle()
    {
        // �ʹ� �ָ� �ִ� �� ��Ȱ��ȭ (����ȭ)
        foreach (GameObject enemy in activeEnemies)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, enemy.transform.position);

            // NavMeshAgent �� AI ������Ʈ ����ȭ
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                if (distanceToPlayer > 20f) // ���� �Ÿ� �̻��̸�
                {
                    agent.enabled = false; // NavMeshAgent ��Ȱ��ȭ
                }
                else
                {
                    agent.enabled = true;
                }
            }

            // �� AI ��ũ��Ʈ ����ȭ
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

    // �ڵ� ���� ���
    public void ToggleAutoBattle()
    {
        autoBattleEnabled = !autoBattleEnabled;

        // �÷��̾� ��Ʈ�ѷ� ����
        PlayerController playerController = player?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetAutoBattle(autoBattleEnabled);
        }

        Debug.Log("�ڵ� ����: " + (autoBattleEnabled ? "Ȱ��ȭ" : "��Ȱ��ȭ"));
    }
}