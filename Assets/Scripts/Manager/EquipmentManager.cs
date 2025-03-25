using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [Header("Equipment Settings")]
    public List<EquipmentData> playerEquipment = new List<EquipmentData>();
    public Dictionary<string, List<EquipmentData>> equipmentByCharacter = new Dictionary<string, List<EquipmentData>>();

    // 전체 아이템 목록
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
            Debug.Log("새로운 아이템을 획득했습니다: " + equipment.name);
        }
    }

    public void EquipItem(string characterId, EquipmentData equipment)
    {
        if (string.IsNullOrEmpty(characterId) || equipment == null)
            return;

        // 해당 캐릭터가 해당 타입의 장비를 가지고 있는지 확인
        if (!equipmentByCharacter.ContainsKey(characterId))
        {
            equipmentByCharacter[characterId] = new List<EquipmentData>();
        }

        // 해당 캐릭터가 해당 타입의 장비를 장착하고 있는지 확인
        UnequipItemOfType(characterId, equipment.type);

        // 장착
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

        // 캐릭터의 기본 스탯 계산 방식
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
        // 캐릭터의 최대 스탯 계산 방식
        CharacterManager.instance.UpdateCharacterBonusStats(characterId, healthBonus, attackBonus, defenseBonus, speedBonus);
    }

    public void UpgradeEquipment(EquipmentData equipment)
    {
        if (equipment == null)
            return;

        int cost = equipment.GetUpgradeCost();

        if (CurrencyManager.instance.SpendGold(cost))
        {
            // 장비 레벨 증가
            equipment.level++;

            Debug.Log(equipment.name + " 업그레이드 완료! 현재 레벨: " + equipment.level);

            // 장비 스탯 업데이트
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

        // UI 업데이트 처리
    }

    public void SellEquipment(EquipmentData equipment)
    {
        if (equipment == null)
            return;
        int sellPrice = equipment.GetUpgradeCost() / 2;
        CurrencyManager.instance.AddGold(sellPrice);
        // 인벤토리에서 제거
        inventory.Remove(equipment);
        // 장비 제거
        foreach (var characterId in equipmentByCharacter.Keys)
        {
            if (equipmentByCharacter[characterId].Contains(equipment))
            {
                UnequipItem(characterId, equipment);
                break;
            }
        }
        Debug.Log(equipment.name + " 판매 완료! 판매 수익: " + sellPrice + " 골드를 획득했습니다!");
        // UI 업데이트 처리
    }

    public void RemoveEquipment(EquipmentData equipment)
    {
        if (equipment == null)
            return;
        // 인벤토리에서 제거
        inventory.Remove(equipment);
        // 장비 제거
        foreach (var characterId in equipmentByCharacter.Keys)
        {
            if (equipmentByCharacter[characterId].Contains(equipment))
            {
                UnequipItem(characterId, equipment);
                break;
            }
        }
        // UI 업데이트 처리
    }

    // 특정 타입의 아이템 목록 반환
    public List<EquipmentData> GetItemsByType(EquipmentType type)
    {
        List<EquipmentData> items = new List<EquipmentData>();
        foreach (var item in inventory)
        {
            if (item.type == type)
            {
                items.Add(item);
            }
        }
        return items;
    }

    // 아이템이 장착되어 있는지 확인
    public bool IsEquipped(EquipmentData item)
    {
        if (item == null)
            return false;

        foreach (var characterEquipment in equipmentByCharacter.Values)
        {
            if (characterEquipment.Contains(item))
                return true;
        }
        return false;
    }

    // 새로운 아이템이 있는지 확인
    public bool HasNewItems()
    {
        return false;
    }
}