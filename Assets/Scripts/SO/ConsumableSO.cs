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
        // 소비 아이템 사용 로직
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

            // 이펙트 재생
            if (useEffect != null)
                Instantiate(useEffect, character.transform.position, Quaternion.identity);

            // 사운드 재생
            if (useSound != null)
                AudioSource.PlayClipAtPoint(useSound, character.transform.position);

            Debug.Log($"{character.name}이(가) {itemName} 아이템을 사용했습니다.");
        }
    }
}