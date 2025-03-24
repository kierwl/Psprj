using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public float maxHealth = 50f;
    public float health = 50f;
    public float attackDamage = 5f;
    public float attackRange = 1.5f;
    public float attackSpeed = 0.8f;
    public float moveSpeed = 3.5f;
    public int level = 1;

    public GameObject healthBarPrefab;
    private HealthBar healthBar;
    public DamageCalculator damageCalculator = new DamageCalculator();

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();  // ���߿� �߰��� �ִϸ�����

        // �÷��̾� ã��
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // �̵� �ӵ� ����
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
        // ü�� �ʱ�ȭ
        health = maxHealth;

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
        if (player != null && agent != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                // ���� ���� �� - ����
                Attack();
            }
            else
            {
                // ���� ���� �� - �̵�
                agent.SetDestination(player.position);
            }
        }
    }

    void Attack()
    {
        // ���� ��ٿ� üũ
        if (Time.time >= nextAttackTime)
        {
            // ���� ����
            // �ִϸ��̼� ��� (���߿� �߰�)
            // if (animator != null)
            //     animator.SetTrigger("Attack");

            // ������ ����
            player.GetComponent<PlayerController>()?.TakeDamage(attackDamage);

            // ���� ���� �ð� ����
            nextAttackTime = Time.time + 1f / attackSpeed;

            Debug.Log("���� �����߽��ϴ�: " + attackDamage + " ������");
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
            EffectsManager.instance.PlayHitEffect(transform.position);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // ��� ����
        Debug.Log("���� ����߽��ϴ�.");

        // ����ġ �� ��� ���� (���߿� ����)

        // ��� ����Ʈ ���
        if (EffectsManager.instance != null)
        {
            EffectsManager.instance.PlayHitEffect(transform.position);
        }

        // ü�¹� ����
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        // ������Ʈ ����
        Destroy(gameObject, 0.5f);
    }
}