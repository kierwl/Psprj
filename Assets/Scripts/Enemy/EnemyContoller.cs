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
        animator = GetComponent<Animator>();  // 나중에 추가할 애니메이터

        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 이동 속도 설정
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
        // 체력 초기화
        health = maxHealth;

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
        if (player != null && agent != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                // 공격 범위 내 - 공격
                Attack();
            }
            else
            {
                // 공격 범위 밖 - 이동
                agent.SetDestination(player.position);
            }
        }
    }

    void Attack()
    {
        // 공격 쿨다운 체크
        if (Time.time >= nextAttackTime)
        {
            // 공격 로직
            // 애니메이션 재생 (나중에 추가)
            // if (animator != null)
            //     animator.SetTrigger("Attack");

            // 데미지 적용
            player.GetComponent<PlayerController>()?.TakeDamage(attackDamage);

            // 다음 공격 시간 설정
            nextAttackTime = Time.time + 1f / attackSpeed;

            Debug.Log("적이 공격했습니다: " + attackDamage + " 데미지");
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
            EffectsManager.instance.PlayHitEffect(transform.position);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 사망 로직
        Debug.Log("적이 사망했습니다.");

        // 경험치 및 골드 보상 (나중에 구현)

        // 사망 이펙트 재생
        if (EffectsManager.instance != null)
        {
            EffectsManager.instance.PlayHitEffect(transform.position);
        }

        // 체력바 제거
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        // 오브젝트 제거
        Destroy(gameObject, 0.5f);
    }
}