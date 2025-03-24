using UnityEngine;
using TMPro;
using System.Collections;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager instance;

    public GameObject damageTextPrefab;
    public float fadeSpeed = 2f;
    public float riseSpeed = 1f;
    public float lifetime = 1f;

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowDamageText(float damage, Vector3 position, bool isCritical = false)
    {
        // 데미지 텍스트 생성
        GameObject textObj = Instantiate(damageTextPrefab, position + Vector3.up, Quaternion.identity);
        TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();

        // 텍스트 내용 설정
        textMesh.text = damage.ToString("0");

        // 크리티컬이면 색상과 크기 변경
        if (isCritical)
        {
            textMesh.color = Color.red;
            textMesh.fontSize *= 1.5f;
            textObj.transform.localScale *= 1.2f;
        }

        // 페이드 아웃 및 상승 효과
        StartCoroutine(AnimateDamageText(textObj, textMesh));
    }

    private IEnumerator AnimateDamageText(GameObject textObj, TextMeshProUGUI textMesh)
    {
        float startTime = Time.time;
        Color originalColor = textMesh.color;
        Vector3 startPos = textObj.transform.position;

        while (Time.time - startTime < lifetime)
        {
            // 경과 시간 계산
            float elapsedTime = Time.time - startTime;
            float alpha = 1 - (elapsedTime / lifetime);

            // 색상 페이드 아웃
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // 상승 효과
            textObj.transform.position = startPos + Vector3.up * (elapsedTime * riseSpeed);

            yield return null;
        }

        Destroy(textObj);
    }
}