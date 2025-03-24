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
        // 컴포넌트 가져오기
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // 플레이어 찾기
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("Player를 찾을 수 없습니다. Player 오브젝트에 'Player' 태그가 설정되어 있는지 확인하세요.");
            return;
        }
        player = playerObject.transform;

        // 이동 속도 설정
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
        else
        {
            Debug.LogError("NavMeshAgent 컴포넌트가 없습니다.");
            return;
        }

        // 체력 초기화
        health = maxHealth;

        // 체력바 생성
        if (healthBarPrefab != null)
        {
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarObj.GetComponent<HealthBar>();
            if (healthBar != null)
            {
                healthBar.target = transform;
                healthBar.UpdateHealthBar(health, maxHealth);
            }
            else
            {
                Debug.LogError("HealthBar 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("healthBarPrefab이 설정되지 않았습니다.");
        }
    }

    void Update()
    {
        if (player != null && agent != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                // 공격 체크 - 공격
                Attack();
            }
            else
            {
                // 이동 체크 - 이동
                agent.SetDestination(player.position);
            }
        }
    }

    void Attack()
    {
        // 공격 체크 - 공격
        if (Time.time >= nextAttackTime)
        {
            // 공격 체크
            // 몬스터 애니메이션 설정 (애니메이션 컨트롤러에서 직접 설정)
            // if (animator != null)
            //     animator.SetTrigger("Attack");

            // 공격 후 대상 피해
            player.GetComponent<PlayerController>()?.TakeDamage(attackDamage);

            // 공격 후 대기 시간 설정
            nextAttackTime = Time.time + 1f / attackSpeed;

            Debug.Log("공격 성공했습니다: " + attackDamage + " 피해를 입혔습니다.");
        }
    }
    public void TakeDamage(float damage)
    {
        bool isCritical = Random.Range(0f, 100f) <= 10f; // 10% 확률로 크리티컬 히트

        // 체력 감소
        health -= damage;

        // 체력바 업데이트
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }

        // 피해 텍스트 표시
        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageText(damage, transform.position + Vector3.up, isCritical);
        }

        // 효과 표시
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

        // 경험치 및 골드 보상
        int expReward = level * 5;  // 적 레벨에 비례한 경험치
        int goldReward = level * 10;  // 적 레벨에 비례한 골드

        GameManager.instance?.EnemyDefeated(expReward, goldReward);

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