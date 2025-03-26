using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject healthBarPrefab;
    public DamageCalculator damageCalculator = new DamageCalculator();

    [Header("캐릭터 상태")]
    private bool autoBattleEnabled = true;
    private bool isDead = false;

    // 컴포넌트 참조
    private NavMeshAgent agent;
    private Animator animator;
    private HealthBar healthBar;
    private PlayerStats playerStats;

    // 전투 관련 변수
    private float nextAttackTime = 0f;
    private GameObject targetEnemy;

    // 애니메이션 파라미터 이름
    private const string PARAM_IS_ATTACKING = "IsAttacking";
    private const string PARAM_IS_DEAD = "IsDead";

    void Start()
    {
        // 필요한 컴포넌트 가져오기
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>() ?? gameObject.AddComponent<PlayerStats>();

        // 플레이어 스탯이 없을 경우 경고
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats 컴포넌트가 없습니다!");
            return;
        }

        // 이동 속도 동기화
        if (agent != null)
        {
            agent.speed = playerStats.GetStat(PlayerStats.StatType.MoveSpeed);
        }

        // 체력바 생성 및 설정
        if (healthBarPrefab != null)
        {
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarObj.GetComponent<HealthBar>();

            if (healthBar != null)
            {
                healthBar.target = transform;
                UpdateHealthBar();
            }
        }

        // 플레이어 스탯 이벤트 구독
        playerStats.OnHealthChanged += OnHealthChanged;

        // 장비 스탯 적용
        playerStats.ApplyEquipmentStats();

        // 인벤토리 장비 스탯 적용
        playerStats.ApplyInventoryEquipment();
    }

    void Update()
    {
        if (isDead) return;

        if (!autoBattleEnabled) return;

        // 가장 가까운 적 찾기
        FindClosestEnemy();

        // 적이 있고 공격 범위 내에 있는지 확인
        if (targetEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.transform.position);
            float attackRange = playerStats.GetAttackRange();

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

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= OnHealthChanged;
        }
    }

    // 플레이어 스탯에서 체력이 변경되었을 때 호출되는 메서드
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthBar();

        // 체력이 0 이하면 사망 처리
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    // 체력바 업데이트
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(playerStats.GetCurrentHealth(), playerStats.GetMaxHealth());
        }
    }

    public void SetAutoBattle(bool enabled)
    {
        autoBattleEnabled = enabled;

        // 자동 전투가 비활성화된 경우 이동 및 공격 중지
        if (!autoBattleEnabled && agent != null)
        {
            agent.isStopped = true;

            if (animator != null)
            {
                animator.SetBool(PARAM_IS_ATTACKING, false);
            }
        }
        else if (agent != null)
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
        float attackSpeed = playerStats.GetStat(PlayerStats.StatType.AttackSpeed);

        if (Time.time >= nextAttackTime && targetEnemy != null)
        {
            // 공격 애니메이션 재생
            if (animator != null)
            {
                animator.SetBool(PARAM_IS_ATTACKING, true);
            }

            // 크리티컬 확률과 공격력 가져오기
            float critChance = playerStats.GetStat(PlayerStats.StatType.CriticalChance);
            float attackPower = playerStats.GetStat(PlayerStats.StatType.Attack);

            // 데미지 계산 및 크리티컬 확인
            damageCalculator.criticalChance = critChance;
            bool isCritical = damageCalculator.IsCritical();
            float damage = damageCalculator.CalculateDamage(attackPower);

            // 데미지 적용
            targetEnemy.GetComponent<EnemyController>()?.TakeDamage(damage);

            // 공격 후 대기 시간 설정
            nextAttackTime = Time.time + 1f / attackSpeed;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 플레이어 스탯의 TakeDamage 호출 (방어력 계산도 여기서 처리)
        playerStats.TakeDamage(damage);

        // 피해 텍스트 표시 (10% 크리티컬 확률)
        bool isCritical = Random.Range(0f, 100f) <= 10f;

        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageText(damage, transform.position + Vector3.up, isCritical);
        }

        // 히트 이펙트 표시
        if (EffectsManager.instance != null)
        {
            EffectsManager.instance.PlayHitEffect(transform.position + Vector3.up * 0.5f);
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


    // 자동 전투 상태 가져오기
    public bool GetAutoBattleEnabled()
    {
        return autoBattleEnabled;
    }

    // 자동 전투 상태 설정 (기존 메서드 유지, 추가로 상태만 반환)
    public bool ToggleAutoBattle()
    {
        autoBattleEnabled = !autoBattleEnabled;

        // NavMeshAgent 상태 업데이트
        if (agent != null)
        {
            agent.isStopped = !autoBattleEnabled;
        }

        Debug.Log($"자동 전투 {(autoBattleEnabled ? "활성화" : "비활성화")}");
        return autoBattleEnabled;
    }

    // 플레이어 스탯 직접 접근 메서드들
    public float GetCurrentHealth() => playerStats?.GetCurrentHealth() ?? 0f;
    public float GetMaxHealth() => playerStats?.GetMaxHealth() ?? 0f;
    public float GetAttackPower() => playerStats?.GetStat(PlayerStats.StatType.Attack) ?? 0f;
    public float GetDefenseValue() => playerStats?.GetStat(PlayerStats.StatType.Defense) ?? 0f;
    public float GetMoveSpeed() => playerStats?.GetStat(PlayerStats.StatType.MoveSpeed) ?? 0f;
    public float GetCriticalChance() => playerStats?.GetStat(PlayerStats.StatType.CriticalChance) ?? 0f;
    public float GetAttackSpeed() => playerStats?.GetStat(PlayerStats.StatType.AttackSpeed) ?? 1f;
}