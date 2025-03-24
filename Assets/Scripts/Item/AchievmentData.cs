using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AchievementData
{
    public string id;
    public string title;
    public string description;
    public AchievementType type;
    public int targetValue;
    public int currentValue;
    public bool isCompleted;
    public Sprite icon;

    // ����
    public int goldReward;
    public int gemReward;

    // ����� ���
    public float GetProgress() => Mathf.Clamp01((float)currentValue / targetValue);
}

public enum AchievementType
{
    KillMonsters,
    ClearStages,
    LevelUpCharacters,
    CollectEquipment,
    SpendCurrency,
    LoginDays
}