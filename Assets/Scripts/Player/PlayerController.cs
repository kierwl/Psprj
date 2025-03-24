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

    // 가장 가까운 적을 담을 변수
    private GameObject targetEnemy;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();  // 나중에 추가할 애니메이터

        // 체력바 생성
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
        // 가장 가까운 적 찾기
        FindClosestEnemy();

        // 적이 있고 공격 범위 내에 있으면 공격
        if (targetEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.transform.position);

            if (distanceToEnemy <= attackRange)
            {
                // 공격 범위 내 - 공격
                Attack();
            }
            else
            {
                // 공격 범위 밖 - 이동
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
            // 공격 애니메이션 (나중에 추가)

            // 데미지 계산
            bool isCritical = damageCalculator.IsCritical();
            float damage = damageCalculator.CalculateDamage(attackDamage);

            // 데미지 적용
            targetEnemy.GetComponent<EnemyController>()?.TakeDamage(damage);

            // 다음 공격 시간 설정
            nextAttackTime = Time.time + 1f / attackSpeed;
        }
    }

    public void TakeDamage(float damage)
    {
        bool isCritical = Random.Range(0f, 100f) <= 10f; // 10% 크리티컬 확률

        // 데미지 적용
        health -= damage;

        // 체력바 업데이트
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }

        // 데미지 텍스트 표시
        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageText(damage, transform.position + Vector3.up, isCritical);
        }

        // 피격 이펙트 재생
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
        // 사망 로직
        Debug.Log("플레이어가 사망했습니다.");
        // 나중에 게임 오버 처리 또는 부활 로직 추가
    }
}