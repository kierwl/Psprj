using UnityEngine;
using System.Collections.Generic;

// ���� ���� ��Ȳ�� �����մϴ� (Ŭ������ ��������, �Ϸ��� ����Ʈ ��)
public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress instance;

    [System.Serializable]
    public class StageProgress
    {
        public string stageID;
        public bool cleared;
        public int starsEarned;
        public float bestTime;
        public int timesPlayed;
    }

    [System.Serializable]
    public class QuestProgress
    {
        public string questID;
        public bool completed;
        public float completionTime;
    }

    [Header("���� ������")]
    [SerializeField] private List<StageProgress> clearedStages = new List<StageProgress>();
    [SerializeField] private List<QuestProgress> quests = new List<QuestProgress>();
    [SerializeField] private List<string> unlockedFeatures = new List<string>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadProgress();
    }

    // ���������� Ŭ����Ǿ����� Ȯ��
    public bool IsStageCleared(string stageID)
    {
        return clearedStages.Exists(s => s.stageID == stageID && s.cleared);
    }

    // �������� ���� �����͸� ������
    public StageProgress GetStageProgress(string stageID)
    {
        StageProgress progress = clearedStages.Find(s => s.stageID == stageID);
        if (progress == null)
        {
            progress = new StageProgress { stageID = stageID };
            clearedStages.Add(progress);
        }
        return progress;
    }

    // ���������� Ŭ����� ǥ��
    public void MarkStageCleared(string stageID, int stars, float clearTime)
    {
        StageProgress progress = GetStageProgress(stageID);
        progress.cleared = true;
        progress.timesPlayed++;

        // ���� �ְ� ����� ������Ʈ (���� ��Ϻ��� ���� ���)
        if (stars > progress.starsEarned)
            progress.starsEarned = stars;

        if (progress.bestTime <= 0 || clearTime < progress.bestTime)
            progress.bestTime = clearTime;

        SaveProgress();

        // �������� ���� �Ŵ������� ��� ���� ���¸� ���ΰ�ħ�ϵ��� �˸�
        StageSelectionManager.instance?.CheckStageUnlocks();
    }

    // ����Ʈ�� �Ϸ�Ǿ����� Ȯ��
    public bool IsQuestCompleted(string questID)
    {
        return quests.Exists(q => q.questID == questID && q.completed);
    }

    // ����Ʈ�� �Ϸ�� ǥ��
    public void MarkQuestCompleted(string questID)
    {
        QuestProgress progress = quests.Find(q => q.questID == questID);
        if (progress == null)
        {
            progress = new QuestProgress { questID = questID };
            quests.Add(progress);
        }

        progress.completed = true;
        progress.completionTime = Time.time;

        SaveProgress();

        // �������� ���� �Ŵ������� ��� ���� ���¸� ���ΰ�ħ�ϵ��� �˸�
        StageSelectionManager.instance?.CheckStageUnlocks();
    }

    // ����� ��� �����Ǿ����� Ȯ��
    public bool IsFeatureUnlocked(string featureID)
    {
        return unlockedFeatures.Contains(featureID);
    }

    // ����� ��� ����
    public void UnlockFeature(string featureID)
    {
        if (!unlockedFeatures.Contains(featureID))
        {
            unlockedFeatures.Add(featureID);
            SaveProgress();
        }
    }

    // ���� ��Ȳ�� PlayerPrefs�� ����
    private void SaveProgress()
    {
        // JSON���� ��ȯ�Ͽ� ����
        string stagesJson = JsonUtility.ToJson(new Wrapper<StageProgress>(clearedStages));
        string questsJson = JsonUtility.ToJson(new Wrapper<QuestProgress>(quests));
        string featuresJson = JsonUtility.ToJson(new Wrapper<string>(unlockedFeatures));

        PlayerPrefs.SetString("ClearedStages", stagesJson);
        PlayerPrefs.SetString("CompletedQuests", questsJson);
        PlayerPrefs.SetString("UnlockedFeatures", featuresJson);
        PlayerPrefs.Save();
    }

    // PlayerPrefs���� ���� ��Ȳ�� �ε�
    private void LoadProgress()
    {
        if (PlayerPrefs.HasKey("ClearedStages"))
        {
            string stagesJson = PlayerPrefs.GetString("ClearedStages");
            Wrapper<StageProgress> stagesWrapper = JsonUtility.FromJson<Wrapper<StageProgress>>(stagesJson);
            clearedStages = stagesWrapper.items;
        }

        if (PlayerPrefs.HasKey("CompletedQuests"))
        {
            string questsJson = PlayerPrefs.GetString("CompletedQuests");
            Wrapper<QuestProgress> questsWrapper = JsonUtility.FromJson<Wrapper<QuestProgress>>(questsJson);
            quests = questsWrapper.items;
        }

        if (PlayerPrefs.HasKey("UnlockedFeatures"))
        {
            string featuresJson = PlayerPrefs.GetString("UnlockedFeatures");
            Wrapper<string> featuresWrapper = JsonUtility.FromJson<Wrapper<string>>(featuresJson);
            unlockedFeatures = featuresWrapper.items;
        }
    }

    // ��� ���� ��Ȳ�� �ʱ�ȭ (�׽�Ʈ��)
    public void ResetAllProgress()
    {
        clearedStages.Clear();
        quests.Clear();
        unlockedFeatures.Clear();

        PlayerPrefs.DeleteKey("ClearedStages");
        PlayerPrefs.DeleteKey("CompletedQuests");
        PlayerPrefs.DeleteKey("UnlockedFeatures");
        PlayerPrefs.Save();
    }

    // ����Ʈ�� ����ȭ�ϱ� ���� ���� Ŭ����
    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> items;

        public Wrapper(List<T> items)
        {
            this.items = items;
        }
    }
}
