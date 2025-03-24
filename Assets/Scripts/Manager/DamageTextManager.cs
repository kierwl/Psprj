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
        // �̱��� ���� ����
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowDamageText(float damage, Vector3 position, bool isCritical = false)
    {
        // ������ �ؽ�Ʈ ����
        GameObject textObj = Instantiate(damageTextPrefab, position + Vector3.up, Quaternion.identity);
        TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();

        // �ؽ�Ʈ ���� ����
        textMesh.text = damage.ToString("0");

        // ũ��Ƽ���̸� ����� ũ�� ����
        if (isCritical)
        {
            textMesh.color = Color.red;
            textMesh.fontSize *= 1.5f;
            textObj.transform.localScale *= 1.2f;
        }

        // ���̵� �ƿ� �� ��� ȿ��
        StartCoroutine(AnimateDamageText(textObj, textMesh));
    }

    private IEnumerator AnimateDamageText(GameObject textObj, TextMeshProUGUI textMesh)
    {
        float startTime = Time.time;
        Color originalColor = textMesh.color;
        Vector3 startPos = textObj.transform.position;

        while (Time.time - startTime < lifetime)
        {
            // ��� �ð� ���
            float elapsedTime = Time.time - startTime;
            float alpha = 1 - (elapsedTime / lifetime);

            // ���� ���̵� �ƿ�
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // ��� ȿ��
            textObj.transform.position = startPos + Vector3.up * (elapsedTime * riseSpeed);

            yield return null;
        }

        Destroy(textObj);
    }
}