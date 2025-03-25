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
    private bool autoBattleEnabled = true;
    private bool isDead = false;

    // 애니메이션 파라미터 이름
    private const string PARAM_IS_ATTACKING = "IsAttacking";
    private const string PARAM_IS_DEAD = "IsDead";

    // 애니메이션   
    private GameObject targetEnemy;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

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
        if (isDead) return;

        if (!autoBattleEnabled)
            return;
        //    ã
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

    public void SetAutoBattle(bool enabled)
    {
        autoBattleEnabled = enabled;

        // �ڵ� ������ ��Ȱ��ȭ�� ��� �̵� �� ���� ����
        if (!autoBattleEnabled)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
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
            animator.SetBool(PARAM_IS_ATTACKING, false);
            agent.SetDestination(targetEnemy.transform.position);
        }
    }

    void Attack()
    {
        if (Time.time >= nextAttackTime && targetEnemy != null)
        {
            // 공격 애니메이션 재생
            if (animator != null)
            {
                animator.SetBool(PARAM_IS_ATTACKING,true);
            }

            // 데미지 계산
            bool isCritical = damageCalculator.IsCritical();
            float damage = damageCalculator.CalculateDamage(attackDamage);

            // 데미지 적용
            targetEnemy.GetComponent<EnemyController>()?.TakeDamage(damage);

            // 공격 후 대기 시간 설정
            nextAttackTime = Time.time + 1f / attackSpeed;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        bool isCritical = Random.Range(0f, 100f) <= 10f; // 10% 크리티컬 확률

        // 체력 감소
        health -= damage;

        // ü¹ Ʈ
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }

        // 피해 텍스트 표시
        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageText(damage, transform.position + Vector3.up, isCritical);
        }

        // 히트 이펙트 표시
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
        isDead = true;
        
        // 사망 애니메이션 재생
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_DEAD, true);
        }

        // NavMeshAgent 비활성화
        if (agent != null)
        {
            agent.enabled = false;
        }

        Debug.Log("플레이어가 사망했습니다.");
    }
}