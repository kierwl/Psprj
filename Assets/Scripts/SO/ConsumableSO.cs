using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Game/Items/Consumable")]
public class ConsumableSO : ItemSO
{
    [Header("Consumable Effects")]
    public float healthRestore;
    public float manaRestore;
    public float temporaryAttackBoost;
    public float temporaryDefenseBoost;
    public float effectDuration;
    public bool isTemporary;

    [Header("Consumable Properties")]
    public GameObject useEffect;
    public AudioClip useSound;

    public override void Use(CharacterController character)
    {
        // �Һ� ������ ��� ����
        if (character != null)
        {
           /* if (healthRestore > 0)
                character.RestoreHealth(healthRestore);

            if (manaRestore > 0)
                character.RestoreMana(manaRestore);

            if (temporaryAttackBoost > 0)
                character.ApplyTemporaryBuff("attack", temporaryAttackBoost, effectDuration);

            if (temporaryDefenseBoost > 0)
                character.ApplyTemporaryBuff("defense", temporaryDefenseBoost, effectDuration);*/

            // ����Ʈ ���
            if (useEffect != null)
                Instantiate(useEffect, character.transform.position, Quaternion.identity);

            // ���� ���
            if (useSound != null)
                AudioSource.PlayClipAtPoint(useSound, character.transform.position);

            Debug.Log($"{character.name}��(��) {itemName} �������� ����߽��ϴ�.");
        }
    }
}