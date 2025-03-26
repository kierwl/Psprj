using UnityEngine;

[CreateAssetMenu(fileName = "New Accessory", menuName = "Items/Accessory")]
public class AccessorySO : ItemSO
{
    [Header("�Ǽ����� ����")]
    public float healthBonus = 0f;        // ü�� ���ʽ�
    public float criticalBonus = 0f;      // ũ��Ƽ�� Ȯ�� ���ʽ�
    public float moveSpeedBonus = 0f;     // �̵� �ӵ� ���ʽ�
    public bool isPercentage = false;     // �ۼ�Ʈ ���� ����

    [Header("Ư�� ȿ��")]
    [Tooltip("�������� Ư�� ȿ�� ����")]
    [TextArea(3, 5)]
    public string specialEffectDescription;

    public override void Use(CharacterController character)
    {
        // �Ǽ������� ��� �� ����/����
        Debug.Log($"{itemName} �Ǽ������� ����/�����մϴ�.");

        // �κ��丮 �ý����� �ִٸ� �ش� �������� ����/����
        Inventory inventory = Inventory.instance;
        if (inventory != null)
        {
            foreach (var item in inventory.items)
            {
                if (item.item == this)
                {
                    if (item.isEquipped)
                    {
                        // �̹� ���� ���̸� ����
                        inventory.UnequipItem(item.slotIndex);
                    }
                    else
                    {
                        // �������� �ʾ����� ����
                        inventory.EquipItem(item.slotIndex);
                    }
                    break;
                }
            }
        }
    }

    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();

        // �Ǽ����� ���� ���� �߰�
        if (healthBonus > 0)
        {
            tooltip += $"\nü�� {(isPercentage ? "+" + (healthBonus * 100) + "%" : "+" + healthBonus)}";
        }

        if (criticalBonus > 0)
        {
            tooltip += $"\nũ��Ƽ�� Ȯ�� +{criticalBonus * 100}%";
        }

        if (moveSpeedBonus > 0)
        {
            tooltip += $"\n�̵� �ӵ� {(isPercentage ? "+" + (moveSpeedBonus * 100) + "%" : "+" + moveSpeedBonus)}";
        }

        // Ư�� ȿ���� ������ �߰�
        if (!string.IsNullOrEmpty(specialEffectDescription))
        {
            tooltip += $"\n\n<color=#FFD700>Ư�� ȿ��:</color> {specialEffectDescription}";
        }

        return tooltip;
    }
}