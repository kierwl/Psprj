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

    // 레벨에 따른 무기 데미지 계산
    public float GetScaledDamage(int weaponLevel)
    {
        // 기본 값 * (1 + 0.1 * (레벨 - 1))
        return attackDamage * (1f + 0.1f * (weaponLevel - 1));
    }

    public override void Use(CharacterController character)
    {
        // 무기 장착 로직
        if (character != null)
        {
            
            Debug.Log($"{character.name}이(가) {itemName} 무기를 장착했습니다.");
        }
    }

    public override ItemSO Clone()
    {
        WeaponSO clone = (WeaponSO)base.Clone();
        // 무기 특정 속성 복사...
        return clone;
    }
}