using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager instance;

    public GameObject hitEffectPrefab;
    public GameObject healEffectPrefab;  // ���߿� ���

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayHitEffect(Vector3 position)
    {
        GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(effect, 2f);  // 2�� �� �ڵ� ����
    }

    public void PlayHealEffect(Vector3 position)
    {
        if (healEffectPrefab != null)
        {
            GameObject effect = Instantiate(healEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
}