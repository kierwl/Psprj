using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffUI : MonoBehaviour
{
    [SerializeField] private GameObject buffIconPrefab;
    [SerializeField] private Transform buffContainer;
    [SerializeField] private PlayerStats playerStats;

    private Dictionary<string, GameObject> activeBuffIcons = new Dictionary<string, GameObject>();

    private void Start()
    {
        if (playerStats == null)
            playerStats = PlayerStats.instance;

        if (playerStats != null)
        {
            playerStats.OnBuffApplied += AddBuffIcon;
            playerStats.OnBuffRemoved += RemoveBuffIcon;
        }
        else
        {
            Debug.LogError("PlayerStats 참조를 찾을 수 없습니다!");
        }
    }

    private void AddBuffIcon(PlayerStats.BuffData buff)
    {
        // 이미 있는 버프 아이콘인 경우 갱신
        if (activeBuffIcons.TryGetValue(buff.buffId, out GameObject existingIcon))
        {
            // 남은 시간만 업데이트
            TextMeshProUGUI cooltimeText = existingIcon.GetComponentInChildren<TextMeshProUGUI>();
            if (cooltimeText != null)
            {
                cooltimeText.text = Mathf.CeilToInt(buff.remainingTime).ToString();
            }
            return;
        }

        // 새 버프 아이콘 생성
        GameObject buffIcon = Instantiate(buffIconPrefab, buffContainer);
        activeBuffIcons.Add(buff.buffId, buffIcon);

        // 아이콘 이미지 설정
        Image iconImage = buffIcon.GetComponent<Image>();
        if (iconImage != null && buff.icon != null)
        {
            iconImage.sprite = buff.icon;
        }

        // 툴팁 설정
        BuffTooltip tooltip = buffIcon.GetComponent<BuffTooltip>();
        if (tooltip != null)
        {
            string description = GetBuffDescription(buff);
            tooltip.SetTooltip(buff.buffName, description);
        }

        // 남은 시간 표시
        TextMeshProUGUI timeText = buffIcon.GetComponentInChildren<TextMeshProUGUI>();
        if (timeText != null)
        {
            timeText.text = Mathf.CeilToInt(buff.remainingTime).ToString();
        }
    }

    private void RemoveBuffIcon(PlayerStats.BuffData buff)
    {
        if (activeBuffIcons.TryGetValue(buff.buffId, out GameObject buffIcon))
        {
            Destroy(buffIcon);
            activeBuffIcons.Remove(buff.buffId);
        }
    }

    private string GetBuffDescription(PlayerStats.BuffData buff)
    {
        string statName = GetStatName(buff.statType);
        string valueText = buff.isPercentage ? 
            $"{buff.value * 100}%" : 
            buff.value.ToString();

        return $"{statName} {valueText} 증가\n지속시간: {buff.duration}초";
    }

    private string GetStatName(PlayerStats.StatType statType)
    {
        switch (statType)
        {
            case PlayerStats.StatType.Health: return "체력";
            case PlayerStats.StatType.Attack: return "공격력";
            case PlayerStats.StatType.Defense: return "방어력";
            case PlayerStats.StatType.MoveSpeed: return "이동속도";
            case PlayerStats.StatType.CriticalChance: return "치명타 확률";
            default: return "알 수 없음";
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnBuffApplied -= AddBuffIcon;
            playerStats.OnBuffRemoved -= RemoveBuffIcon;
        }
    }
} 