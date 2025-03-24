using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health = 100f;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackSpeed = 1f;

    public GameObject healthBarPrefab;
    private HealthBar healthBar;
    public DamageCalculator damageCalculator = new DamageCalculator();


    private NavMeshAgent agent;
    private Animator animator;
    private float nextAttackTime = 0f;

    // ���� ����� ���� ���� ����
    private GameObject targetEnemy;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();  // ���߿� �߰��� �ִϸ�����

        // ü�¹� ����
        if (healthBarPrefab != null)
        {
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarObj.GetComponent<HealthBar>();
            healthBar.target = transform;
            healthBar.UpdateHealthBar(health, maxHealth);
        }
    }

    void Update()
    {
        // ���� ����� �� ã��
        FindClosestEnemy();

        // ���� �ְ� ���� ���� ���� ������ ����
        if (targetEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.transform.position);

            if (distanceToEnemy <= attackRange)
            {
                // ���� ���� �� - ����
                Attack();
            }
            else
            {
                // ���� ���� �� - �̵�
                MoveToTarget();
            }
        }
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        targetEnemy = closestEnemy;
    }

    void MoveToTarget()
    {
        if (targetEnemy != null && agent != null)
        {
            agent.SetDestination(targetEnemy.transform.position);
        }
    }

    void Attack()
    {
        if (Time.time >= nextAttackTime && targetEnemy != null)
        {
            // ���� �ִϸ��̼� (���߿� �߰�)

            // ������ ���
            bool isCritical = damageCalculator.IsCritical();
            float damage = damageCalculator.CalculateDamage(attackDamage);

            // ������ ����
            targetEnemy.GetComponent<EnemyController>()?.TakeDamage(damage);

            // ���� ���� �ð� ����
            nextAttackTime = Time.time + 1f / attackSpeed;
        }
    }

    public void TakeDamage(float damage)
    {
        bool isCritical = Random.Range(0f, 100f) <= 10f; // 10% ũ��Ƽ�� Ȯ��

        // ������ ����
        health -= damage;

        // ü�¹� ������Ʈ
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }

        // ������ �ؽ�Ʈ ǥ��
        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageText(damage, transform.position + Vector3.up, isCritical);
        }

        // �ǰ� ����Ʈ ���
        if (EffectsManager.instance != null)
        {
            EffectsManager.instance.PlayHitEffect(transform.position + Vector3.up * 0.5f);
        }

        if (health <= 0)
        {
            Die();
        }
    }



    void Die()
    {
        // ��� ����
        Debug.Log("�÷��̾ ����߽��ϴ�.");
        // ���߿� ���� ���� ó�� �Ǵ� ��Ȱ ���� �߰�
    }
}