using UnityEngine;

[CreateAssetMenu(fileName = "New Accessory", menuName = "Items/Accessory")]
public class AccessorySO : ItemSO
{
    [Header("악세서리 스탯")]
    public float healthBonus = 0f;        // 체력 보너스
    public float criticalBonus = 0f;      // 크리티컬 확률 보너스
    public float moveSpeedBonus = 0f;     // 이동 속도 보너스
    public bool isPercentage = false;     // 퍼센트 증가 여부

    [Header("특수 효과")]
    [Tooltip("아이템의 특수 효과 설명")]
    [TextArea(3, 5)]
    public string specialEffectDescription;

    public override void Use(CharacterController character)
    {
        // 악세서리는 사용 시 장착/해제
        Debug.Log($"{itemName} 악세서리를 장착/해제합니다.");

        // 인벤토리 시스템이 있다면 해당 아이템을 장착/해제
        Inventory inventory = Inventory.instance;
        if (inventory != null)
        {
            foreach (var item in inventory.items)
            {
                if (item.item == this)
                {
                    if (item.isEquipped)
                    {
                        // 이미 장착 중이면 해제
                        inventory.UnequipItem(item.slotIndex);
                    }
                    else
                    {
                        // 장착되지 않았으면 장착
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

        // 악세서리 고유 스탯 추가
        if (healthBonus > 0)
        {
            tooltip += $"\n체력 {(isPercentage ? "+" + (healthBonus * 100) + "%" : "+" + healthBonus)}";
        }

        if (criticalBonus > 0)
        {
            tooltip += $"\n크리티컬 확률 +{criticalBonus * 100}%";
        }

        if (moveSpeedBonus > 0)
        {
            tooltip += $"\n이동 속도 {(isPercentage ? "+" + (moveSpeedBonus * 100) + "%" : "+" + moveSpeedBonus)}";
        }

        // 특수 효과가 있으면 추가
        if (!string.IsNullOrEmpty(specialEffectDescription))
        {
            tooltip += $"\n\n<color=#FFD700>특수 효과:</color> {specialEffectDescription}";
        }

        return tooltip;
    }
}