using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Configuration")]
    public EnemySO enemyData;
    public int level = 1;

    [Header("Runtime Stats")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    [SerializeField] private float attackDamage;
    [SerializeField] private float defense;

    [Header("References")]
    public GameObject healthBarPrefab;
    private HealthBar healthBar;
    public DamageCalculator damageCalculator = new DamageCalculator();

    // 컴포넌트 참조
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    // 전투 관련 변수
    private float nextAttackTime = 0f;
    private bool isDead = false;

    // 이벤트
    public event Action OnDeathEvent;

    void Start()
    {
        InitializeComponents();

        // 스탯 초기화
        InitializeStats();

        // 체력바 생성
        CreateHealthBar();
    }

    private void InitializeComponents()
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
        if (agent != null && enemyData != null)
        {
            agent.speed = enemyData.baseSpeed;
        }
    }

    private void InitializeStats()
    {
        if (enemyData == null)
        {
            Debug.LogWarning("Enemy Data가 설정되지 않았습니다. 기본값을 사용합니다.");
            maxHealth = 50f;
            health = 50f;
            attackDamage = 5f;
            defense = 0f;
            return;
        }

        // enemyData로부터 레벨에 따른 스탯 계산
        maxHealth = enemyData.GetScaledHealth(level);
        health = maxHealth;
        attackDamage = enemyData.GetScaledDamage(level);
        defense = enemyData.GetScaledDefense(level);

        // 보스인 경우 크기 조정
        if (enemyData.isBoss)
        {
            transform.localScale *= 1.5f;
        }

        // 엘리트인 경우 스탯 보너스
        if (enemyData.isElite)
        {
            maxHealth *= 1.5f;
            health = maxHealth;
            attackDamage *= 1.2f;
        }
    }

    private void CreateHealthBar()
    {
        if (healthBarPrefab != null)
        {
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarObj.GetComponent<HealthBar>();
            if (healthBar != null)
            {
                healthBar.target = transform;
                healthBar.UpdateHealthBar(health, maxHealth);
            }
        }
    }

    void Update()
    {
        if (isDead || player == null || agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 감지 범위 내에 있는지 확인
        if (distanceToPlayer <= enemyData.detectionRange)
        {
            // 공격 범위 내에 있는지 확인
            if (distanceToPlayer <= enemyData.attackRange)
            {
                // 공격 범위 내 - 공격
                StopMoving();
                Attack();
            }
            else
            {
                // 공격 범위 밖 - 이동
                MoveToPlayer();
            }
        }
        else
        {
            // 감지 범위 밖 - 대기
            StopMoving();
        }
    }

    private void MoveToPlayer()
    {
        if (agent.enabled)
        {
            agent.SetDestination(player.position);

            // 애니메이션 처리
            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }
    }

    private void StopMoving()
    {
        if (agent.enabled)
        {
            agent.ResetPath();

            // 애니메이션 처리
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    private void Attack()
    {
        // 공격 쿨다운 체크
        if (Time.time < nextAttackTime) return;

        // 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 근접/원거리 공격 처리
        if (enemyData.isRanged)
        {
            // 원거리 공격 로직
            StartCoroutine(RangedAttackCoroutine());
        }
        else
        {
            // 근접 공격 로직
            MeleeAttack();
        }

        // 다음 공격 시간 설정
        nextAttackTime = Time.time + (1f / enemyData.baseSpeed);
    }

    private void MeleeAttack()
    {
        // 플레이어에게 데미지 적용
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
                Debug.Log($"{enemyData.enemyName} 공격: {attackDamage} 데미지");
            }
        }
    }

    private IEnumerator RangedAttackCoroutine()
    {
        // 발사체 생성 지연 (애니메이션 싱크를 위해)
        yield return new WaitForSeconds(0.5f);

        // TODO: 발사체 생성 및 발사 로직
        Debug.Log($"{enemyData.enemyName}의 원거리 공격!");

        // 플레이어에게 데미지 적용 (직접 적용 또는 발사체에 위임)
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 방어력에 따른 데미지 감소
        float damageReduction = defense / (defense + 50f);
        float finalDamage = damage * (1f - damageReduction);

        // 크리티컬 확인
        bool isCritical = damageCalculator != null ? damageCalculator.IsCritical() : UnityEngine.Random.value < 0.1f;
        if (isCritical)
        {
            finalDamage *= 1.5f;
        }

        // 체력 감소
        health -= finalDamage;

        // 체력바 업데이트
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }

        // 피해 텍스트 표시
        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageText(finalDamage, transform.position + Vector3.up, isCritical);
        }

        // 효과 표시
        if (EffectsManager.instance != null)
        {
            EffectsManager.instance.PlayHitEffect(transform.position);
        }

        // 피격 애니메이션
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // 체력이 0 이하면 사망
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{enemyData?.enemyName ?? "적"} 사망");

        // 사망 애니메이션
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // NavMeshAgent 비활성화
        if (agent != null)
        {
            agent.enabled = false;
        }

        // 콜라이더 비활성화
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 체력바 제거
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        // 보상 지급
        if (enemyData != null)
        {
            int expReward = enemyData.GetScaledExpReward(level);
            int goldReward = enemyData.GetScaledGoldReward(level);

            // 게임 매니저를 통해 보상 지급
            GameManager.instance?.EnemyDefeated(expReward, goldReward);

            // 아이템 드롭 처리
            DropItems();
        }
        else
        {
            // 기본 보상
            GameManager.instance?.EnemyDefeated(level * 5, level * 10);
        }

        // 사망 이벤트 호출
        OnDeathEvent?.Invoke();

        // 오브젝트 제거 (애니메이션 재생 시간 확보)
        Destroy(gameObject, 2f);
    }

    private void DropItems()
    {
        // 아이템 드롭 확률 체크
        if (enemyData.possibleDrops == null || enemyData.possibleDrops.Length == 0) return;

        if (UnityEngine.Random.value <= enemyData.itemDropChance)
        {
            // 랜덤 아이템 선택
            ItemSO item = enemyData.possibleDrops[UnityEngine.Random.Range(0, enemyData.possibleDrops.Length)];

            if (item != null)
            {
                // 인벤토리에 아이템 추가 또는 필드에 드롭
                // TODO: 인벤토리 시스템 연동
                Debug.Log($"{item.itemName} 아이템 드롭!");
            }
        }
    }

    // 외부에서 호출하여 적 초기화
    public void Initialize(EnemySO data, int enemyLevel)
    {
        enemyData = data;
        level = enemyLevel;

        // 이미 Start가 호출된 후라면 값을 재설정
        if (gameObject.activeInHierarchy)
        {
            InitializeStats();

            // 체력바 업데이트
            if (healthBar != null)
            {
                healthBar.UpdateHealthBar(health, maxHealth);
            }
        }
    }
}