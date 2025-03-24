using UnityEngine;
using System.Collections;

public abstract class Skill : MonoBehaviour
{
    public string skillName;
    public float cooldown;
    public float manaCost;
    public float range;
    public GameObject effectPrefab;

    protected float nextUseTime = 0f;

    public bool CanUse()
    {
        return Time.time >= nextUseTime;
    }

    public abstract void Use(Transform user, Transform target);

    protected void StartCooldown()
    {
        nextUseTime = Time.time + cooldown;
    }
}

// 예시 스킬
public class FireballSkill : Skill
{
    public float damage = 20f;

    public override void Use(Transform user, Transform target)
    {
        if (!CanUse()) return;

        // 스킬 사용 로직
        Debug.Log("파이어볼 스킬 사용!");

        // 데미지 적용
        if (target != null)
        {
            target.GetComponent<EnemyController>()?.TakeDamage(damage);
        }

        // 이펙트 재생
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, target.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 쿨다운 시작
        StartCooldown();
    }
}