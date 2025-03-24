using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class HierarchySorter : EditorWindow
{
    [MenuItem("Tools/하이라키 정렬")]
    static void SortHierarchy()
    {
        // 모든 최상위 오브젝트 가져오기
        var rootObjects = new List<GameObject>();
        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.transform.parent == null)
            {
                rootObjects.Add(obj);
            }
        }

        // 이름순으로 정렬
        rootObjects.Sort((a, b) => a.name.CompareTo(b.name));

        // 정렬된 순서대로 Undo 기록 생성
        Undo.RecordObjects(rootObjects.ToArray(), "Sort Hierarchy");

        // 정렬된 순서대로 재배치
        for (int i = 0; i < rootObjects.Count; i++)
        {
            rootObjects[i].transform.SetSiblingIndex(i);
        }
    }
} 