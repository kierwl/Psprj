using UnityEngine;
using System.Collections.Generic;

// 게임 진행 상황을 추적합니다 (클리어한 스테이지, 완료한 퀘스트 등)
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

    [Header("진행 데이터")]
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

    // 스테이지가 클리어되었는지 확인
    public bool IsStageCleared(string stageID)
    {
        return clearedStages.Exists(s => s.stageID == stageID && s.cleared);
    }

    // 스테이지 진행 데이터를 가져옴
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

    // 스테이지를 클리어로 표시
    public void MarkStageCleared(string stageID, int stars, float clearTime)
    {
        StageProgress progress = GetStageProgress(stageID);
        progress.cleared = true;
        progress.timesPlayed++;

        // 별과 최고 기록을 업데이트 (이전 기록보다 나을 경우)
        if (stars > progress.starsEarned)
            progress.starsEarned = stars;

        if (progress.bestTime <= 0 || clearTime < progress.bestTime)
            progress.bestTime = clearTime;

        SaveProgress();

        // 스테이지 선택 매니저에게 잠금 해제 상태를 새로고침하도록 알림
        StageSelectionManager.instance?.CheckStageUnlocks();
    }

    // 퀘스트가 완료되었는지 확인
    public bool IsQuestCompleted(string questID)
    {
        return quests.Exists(q => q.questID == questID && q.completed);
    }

    // 퀘스트를 완료로 표시
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

        // 스테이지 선택 매니저에게 잠금 해제 상태를 새로고침하도록 알림
        StageSelectionManager.instance?.CheckStageUnlocks();
    }

    // 기능이 잠금 해제되었는지 확인
    public bool IsFeatureUnlocked(string featureID)
    {
        return unlockedFeatures.Contains(featureID);
    }

    // 기능을 잠금 해제
    public void UnlockFeature(string featureID)
    {
        if (!unlockedFeatures.Contains(featureID))
        {
            unlockedFeatures.Add(featureID);
            SaveProgress();
        }
    }

    // 진행 상황을 PlayerPrefs에 저장
    private void SaveProgress()
    {
        // JSON으로 변환하여 저장
        string stagesJson = JsonUtility.ToJson(new Wrapper<StageProgress>(clearedStages));
        string questsJson = JsonUtility.ToJson(new Wrapper<QuestProgress>(quests));
        string featuresJson = JsonUtility.ToJson(new Wrapper<string>(unlockedFeatures));

        PlayerPrefs.SetString("ClearedStages", stagesJson);
        PlayerPrefs.SetString("CompletedQuests", questsJson);
        PlayerPrefs.SetString("UnlockedFeatures", featuresJson);
        PlayerPrefs.Save();
    }

    // PlayerPrefs에서 진행 상황을 로드
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

    // 모든 진행 상황을 초기화 (테스트용)
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

    // 리스트를 직렬화하기 위한 헬퍼 클래스
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
