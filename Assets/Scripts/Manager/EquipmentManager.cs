using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [Header("Equipment Settings")]
    public List<EquipmentData> playerEquipment = new List<EquipmentData>();
    public Dictionary<string, List<EquipmentData>> equipmentByCharacter = new Dictionary<string, List<EquipmentData>>();

    // ��� ���� ���
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
            Debug.Log("�� ��� ȹ��: " + equipment.name);
        }
    }

    public void EquipItem(string characterId, EquipmentData equipment)
    {
        if (string.IsNullOrEmpty(characterId) || equipment == null)
            return;

        // ĳ���� ��� ����� ������ ����
        if (!equipmentByCharacter.ContainsKey(characterId))
        {
            equipmentByCharacter[characterId] = new List<EquipmentData>();
        }

        // ���� Ÿ���� ��� ����
        UnequipItemOfType(characterId, equipment.type);

        // ��� ����
        equipmentByCharacter[characterId].Add(equipment);

        // �κ��丮���� ����
        inventory.Remove(equipment);

        // ĳ���� ���� ������Ʈ
        UpdateCharacterStats(characterId);
    }

    public void UnequipItem(string characterId, EquipmentData equipment)
    {
        if (string.IsNullOrEmpty(characterId) || equipment == null)
            return;

        if (equipmentByCharacter.ContainsKey(characterId))
        {
            equipmentByCharacter[characterId].Remove(equipment);

            // �κ��丮�� �߰�
            inventory.Add(equipment);

            // ĳ���� ���� ������Ʈ
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

        // ���� ���� ���� ���ʽ� ���
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

        // ĳ���� ���� ������Ʈ
        // ĳ���� �Ŵ����� ���� �ʿ�
        CharacterManager.instance.UpdateCharacterBonusStats(characterId, healthBonus, attackBonus, defenseBonus, speedBonus);
    }

    public void UpgradeEquipment(EquipmentData equipment)
    {
        if (equipment == null)
            return;

        int cost = equipment.GetUpgradeCost();

        if (CurrencyManager.instance.SpendGold(cost))
        {
            // ���� ����
            equipment.level++;

            Debug.Log(equipment.name + " ��� ���� " + equipment.level + "�� ��ȭ�Ǿ����ϴ�!");

            // ���� ���� ��� ���� ������Ʈ
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
            Debug.Log("��尡 �����մϴ�!");
        }
    }
}