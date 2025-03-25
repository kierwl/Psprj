using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Game/Items/Armor")]
public class ArmorSO : ItemSO
{
    [Header("Armor Stats")]
    public float defense;
    public float healthBonus;
    public float elementalResistance;

    [Header("Armor Properties")]
    public ArmorType armorType;
    public GameObject armorPrefab;

    public enum ArmorType
    {
        Head,
        Chest,
        Gloves,
        Boots,
        Shield
    }

    // 레벨에 따른 방어구 방어력 계산
    public float GetScaledDefense(int armorLevel)
    {
        // 기본 값 * (1 + 0.08 * (레벨 - 1))
        return defense * (1f + 0.08f * (armorLevel - 1));
    }

    public override void Use(CharacterController character)
    {
        // 방어구 장착 로직
        if (character != null)
        {
            Debug.Log($"{character.name}이(가) {itemName} 방어구를 장착했습니다.");
        }
    }

    public override ItemSO Clone()
    {
        ArmorSO clone = (ArmorSO)base.Clone();
        // 방어구 특정 속성 복사...
        return clone;
    }
}