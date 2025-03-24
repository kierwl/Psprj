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

// ���� ��ų
public class FireballSkill : Skill
{
    public float damage = 20f;

    public override void Use(Transform user, Transform target)
    {
        if (!CanUse()) return;

        // ��ų ��� ����
        Debug.Log("���̾ ��ų ���!");

        // ������ ����
        if (target != null)
        {
            target.GetComponent<EnemyController>()?.TakeDamage(damage);
        }

        // ����Ʈ ���
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, target.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // ��ٿ� ����
        StartCooldown();
    }
}