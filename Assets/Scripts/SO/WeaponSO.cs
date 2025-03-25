using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Items/Weapon")]
public class WeaponSO : ItemSO
{
    [Header("Weapon Stats")]
    public float attackDamage;
    public float attackSpeed;
    public float criticalChance;
    public float criticalMultiplier = 1.5f;

    [Header("Weapon Properties")]
    public WeaponType weaponType;
    public GameObject weaponPrefab;

    public enum WeaponType
    {
        Sword,
        Axe,
        Dagger,
        Bow,
        Staff,
        Wand
    }

    // ������ ���� ���� ������ ���
    public float GetScaledDamage(int weaponLevel)
    {
        // �⺻ �� * (1 + 0.1 * (���� - 1))
        return attackDamage * (1f + 0.1f * (weaponLevel - 1));
    }

    public override void Use(CharacterController character)
    {
        // ���� ���� ����
        if (character != null)
        {
            
            Debug.Log($"{character.name}��(��) {itemName} ���⸦ �����߽��ϴ�.");
        }
    }

    public override ItemSO Clone()
    {
        WeaponSO clone = (WeaponSO)base.Clone();
        // ���� Ư�� �Ӽ� ����...
        return clone;
    }
}