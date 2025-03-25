#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class EnemyDataCreator : EditorWindow
{
    private string enemyName = "New Enemy";
    private float baseHealth = 100f;
    private float baseDamage = 10f;
    private float baseDefense = 5f;
    private float baseSpeed = 3.5f;
    private EnemySO.EnemyType enemyType = EnemySO.EnemyType.Normal;
    private bool isBoss = false;
    private GameObject prefab;

    [MenuItem("Game/Create Enemy Data")]
    public static void ShowWindow()
    {
        GetWindow<EnemyDataCreator>("Enemy Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create New Enemy Data", EditorStyles.boldLabel);

        enemyName = EditorGUILayout.TextField("Enemy Name", enemyName);
        baseHealth = EditorGUILayout.FloatField("Base Health", baseHealth);
        baseDamage = EditorGUILayout.FloatField("Base Damage", baseDamage);
        baseDefense = EditorGUILayout.FloatField("Base Defense", baseDefense);
        baseSpeed = EditorGUILayout.FloatField("Base Speed", baseSpeed);
        enemyType = (EnemySO.EnemyType)EditorGUILayout.EnumPopup("Enemy Type", enemyType);
        isBoss = EditorGUILayout.Toggle("Is Boss", isBoss);
        prefab = (GameObject)EditorGUILayout.ObjectField("Enemy Prefab", prefab, typeof(GameObject), false);

        if (GUILayout.Button("Create Enemy Data"))
        {
            CreateEnemyData();
        }
    }

    private void CreateEnemyData()
    {
        EnemySO enemyData = ScriptableObject.CreateInstance<EnemySO>();

        enemyData.enemyID = System.Guid.NewGuid().ToString();
        enemyData.enemyName = enemyName;
        enemyData.baseHealth = baseHealth;
        enemyData.baseDamage = baseDamage;
        enemyData.baseDefense = baseDefense;
        enemyData.baseSpeed = baseSpeed;
        enemyData.enemyType = enemyType;
        enemyData.isBoss = isBoss;
        enemyData.prefab = prefab;

        // 추가 속성 설정 (보상 등)
        enemyData.expReward = Mathf.RoundToInt(baseHealth * 0.1f);
        enemyData.goldReward = Mathf.RoundToInt(baseHealth * 0.05f);

        // 에셋 저장
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Enemy Data",
            enemyName + ".asset",
            "asset",
            "Please enter a file name to save the enemy data to"
        );

        if (path.Length > 0)
        {
            AssetDatabase.CreateAsset(enemyData, path);
            AssetDatabase.SaveAssets();

            // 에디터에서 선택
            Selection.activeObject = enemyData;
        }
    }
}
#endif