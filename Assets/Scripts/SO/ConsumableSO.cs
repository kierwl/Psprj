using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Game/Items/Consumable")]
public class ConsumableSO : ItemSO
{
    [Header("Consumable Effects")]
    public float healthRestore;
    public float manaRestore;
    
    [Header("임시 버프 효과")]
    public bool hasAttackBoost;
    public float attackBoostValue = 0.1f; // 10% 증가
    public float attackBoostDuration = 30f; // 30초
    
    public bool hasDefenseBoost;
    public float defenseBoostValue = 0.1f; // 10% 증가
    public float defenseBoostDuration = 30f; // 30초
    
    public bool hasSpeedBoost;
    public float speedBoostValue = 0.15f; // 15% 증가
    public float speedBoostDuration = 20f; // 20초

    [Header("Consumable Properties")]
    public GameObject useEffect;
    public AudioClip useSound;

    public override void Use(CharacterController character)
    {
        // 플레이어 스탯 컴포넌트 가져오기
        PlayerStats playerStats = PlayerStats.instance;
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // 체력 회복 적용
        if (healthRestore > 0)
        {
            playerStats.RestoreHealth(healthRestore);
            Debug.Log($"체력을 {healthRestore} 만큼 회복했습니다.");
        }

        // 버프 적용
        ApplyBuffs(playerStats);

        // 이펙트 재생
        if (useEffect != null)
            Instantiate(useEffect, character.transform.position, Quaternion.identity);

        // 효과음 재생
        if (useSound != null)
            AudioSource.PlayClipAtPoint(useSound, character.transform.position);

        Debug.Log($"{character.name}이(가) {itemName} 아이템을 사용했습니다.");
    }
    
    private void ApplyBuffs(PlayerStats playerStats)
    {
        // 공격력 버프
        if (hasAttackBoost && attackBoostValue > 0)
        {
            var buff = PlayerStats.CreateBuff(
                "attack_boost_" + itemID,
                "공격력 강화",
                PlayerStats.StatType.Attack,
                attackBoostValue,
                attackBoostDuration,
                icon,
                true // 퍼센트 증가
            );
            playerStats.ApplyBuff(buff);
        }
        
        // 방어력 버프
        if (hasDefenseBoost && defenseBoostValue > 0)
        {
            var buff = PlayerStats.CreateBuff(
                "defense_boost_" + itemID,
                "방어력 강화",
                PlayerStats.StatType.Defense,
                defenseBoostValue,
                defenseBoostDuration,
                icon,
                true // 퍼센트 증가
            );
            playerStats.ApplyBuff(buff);
        }
        
        // 이동속도 버프
        if (hasSpeedBoost && speedBoostValue > 0)
        {
            var buff = PlayerStats.CreateBuff(
                "speed_boost_" + itemID,
                "이동속도 증가",
                PlayerStats.StatType.MoveSpeed,
                speedBoostValue,
                speedBoostDuration,
                icon,
                true // 퍼센트 증가
            );
            playerStats.ApplyBuff(buff);
        }
    }
}