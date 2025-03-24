using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [Header("Equipment Settings")]
    public List<EquipmentData> playerEquipment = new List<EquipmentData>();
    public Dictionary<string, List<EquipmentData>> equipmentByCharacter = new Dictionary<string, List<EquipmentData>>();

    // 모든 보유 장비
    public List<EquipmentData> inventory = new List<EquipmentData>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AddEquipment(EquipmentData equipment)
    {
        if (equipment != null)
        {
            inventory.Add(equipment);
            Debug.Log("새 장비 획득: " + equipment.name);
        }
    }

    public void EquipItem(string characterId, EquipmentData equipment)
    {
        if (string.IsNullOrEmpty(characterId) || equipment == null)
            return;

        // 캐릭터 장비 목록이 없으면 생성
        if (!equipmentByCharacter.ContainsKey(characterId))
        {
            equipmentByCharacter[characterId] = new List<EquipmentData>();
        }

        // 같은 타입의 장비 해제
        UnequipItemOfType(characterId, equipment.type);

        // 장비 장착
        equipmentByCharacter[characterId].Add(equipment);

        // 인벤토리에서 제거
        inventory.Remove(equipment);

        // 캐릭터 스탯 업데이트
        UpdateCharacterStats(characterId);
    }

    public void UnequipItem(string characterId, EquipmentData equipment)
    {
        if (string.IsNullOrEmpty(characterId) || equipment == null)
            return;

        if (equipmentByCharacter.ContainsKey(characterId))
        {
            equipmentByCharacter[characterId].Remove(equipment);

            // 인벤토리에 추가
            inventory.Add(equipment);

            // 캐릭터 스탯 업데이트
            UpdateCharacterStats(characterId);
        }
    }

    private void UnequipItemOfType(string characterId, EquipmentType type)
    {
        if (string.IsNullOrEmpty(characterId))
            return;

        if (equipmentByCharacter.ContainsKey(characterId))
        {
            EquipmentData existingEquipment = null;

            foreach (var equip in equipmentByCharacter[characterId])
            {
                if (equip.type == type)
                {
                    existingEquipment = equip;
                    break;
                }
            }

            if (existingEquipment != null)
            {
                equipmentByCharacter[characterId].Remove(existingEquipment);
                inventory.Add(existingEquipment);
            }
        }
    }

    public EquipmentData GetEquippedItemOfType(string characterId, EquipmentType type)
    {
        if (string.IsNullOrEmpty(characterId))
            return null;

        if (equipmentByCharacter.ContainsKey(characterId))
        {
            foreach (var equip in equipmentByCharacter[characterId])
            {
                if (equip.type == type)
                    return equip;
            }
        }

        return null;
    }

    public List<EquipmentData> GetAllEquippedItems(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return new List<EquipmentData>();

        if (equipmentByCharacter.ContainsKey(characterId))
            return new List<EquipmentData>(equipmentByCharacter[characterId]);

        return new List<EquipmentData>();
    }

    private void UpdateCharacterStats(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return;

        // 장비로 인한 스탯 보너스 계산
        float healthBonus = 0;
        float attackBonus = 0;
        float defenseBonus = 0;
        float speedBonus = 0;

        if (equipmentByCharacter.ContainsKey(characterId))
        {
            foreach (var equipment in equipmentByCharacter[characterId])
            {
                healthBonus += equipment.GetCurrentHealthBonus();
                attackBonus += equipment.GetCurrentAttackBonus();
                defenseBonus += equipment.GetCurrentDefenseBonus();
                speedBonus += equipment.speedBonus;
            }
        }

        // 캐릭터 스탯 업데이트
        // 캐릭터 매니저와 연동 필요
        CharacterManager.instance.UpdateCharacterBonusStats(characterId, healthBonus, attackBonus, defenseBonus, speedBonus);
    }

    public void UpgradeEquipment(EquipmentData equipment)
    {
        if (equipment == null)
            return;

        int cost = equipment.GetUpgradeCost();

        if (CurrencyManager.instance.SpendGold(cost))
        {
            // 레벨 증가
            equipment.level++;

            Debug.Log(equipment.name + " 장비가 레벨 " + equipment.level + "로 강화되었습니다!");

            // 장착 중인 경우 스탯 업데이트
            foreach (var characterId in equipmentByCharacter.Keys)
            {
                if (equipmentByCharacter[characterId].Contains(equipment))
                {
                    UpdateCharacterStats(characterId);
                    break;
                }
            }
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
        }
    }
}