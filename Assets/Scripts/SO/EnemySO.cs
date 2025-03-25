using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemies/Enemy Data")]
public class EnemySO : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyID;
    public string enemyName;
    [TextArea(2, 5)]
    public string description;
    public Sprite icon;
    public GameObject prefab;

    [Header("Stats")]
    public float baseHealth;
    public float baseDamage;
    public float baseDefense;
    public float baseSpeed;
    public float attackRange = 1.5f;
    public float detectionRange = 10f;

    [Header("Level Scaling")]
    public float healthPerLevel = 20f;
    public float damagePerLevel = 5f;
    public float defensePerLevel = 2f;

    [Header("Rewards")]
    public int expReward;
    public int goldReward;
    public float itemDropChance = 0.3f;
    public ItemSO[] possibleDrops;

    [Header("Properties")]
    public EnemyType enemyType;
    public bool isBoss = false;
    public bool isRanged = false;
    public bool isElite = false;

    // �� Ÿ�� enum
    public enum EnemyType
    {
        Normal,
        Flying,
        Armored,
        Magic,
        Undead,
        Mechanical
    }

    // ������ ���� �� ü�� ���
    public float GetScaledHealth(int level)
    {
        return baseHealth + (level - 1) * healthPerLevel;
    }

    // ������ ���� �� ������ ���
    public float GetScaledDamage(int level)
    {
        return baseDamage + (level - 1) * damagePerLevel;
    }

    // ������ ���� �� ���� ���
    public float GetScaledDefense(int level)
    {
        return baseDefense + (level - 1) * defensePerLevel;
    }

    // ������ ���� ���� ���
    public int GetScaledExpReward(int level)
    {
        float multiplier = 1f + (level - 1) * 0.2f;
        return Mathf.RoundToInt(expReward * multiplier);
    }

    public int GetScaledGoldReward(int level)
    {
        float multiplier = 1f + (level - 1) * 0.15f;
        return Mathf.RoundToInt(goldReward * multiplier);
    }
}