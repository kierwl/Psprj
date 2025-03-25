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

    // ������ ���� �� ���� ���
    public float GetScaledDefense(int armorLevel)
    {
        // �⺻ �� * (1 + 0.08 * (���� - 1))
        return defense * (1f + 0.08f * (armorLevel - 1));
    }

    public override void Use(CharacterController character)
    {
        // �� ���� ����
        if (character != null)
        {
            Debug.Log($"{character.name}��(��) {itemName} ���� �����߽��ϴ�.");
        }
    }

    public override ItemSO Clone()
    {
        ArmorSO clone = (ArmorSO)base.Clone();
        // �� Ư�� �Ӽ� ����...
        return clone;
    }
}