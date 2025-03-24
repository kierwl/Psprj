// ���� ȿ�� �ý��� (����)
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class StatusEffectSystem : MonoBehaviour
{
    [System.Serializable]
    public class StatusEffect
    {
        public string name;
        public Sprite icon;
        public float duration;
        public float remainingTime;
        public bool isActive;
    }

    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    public GameObject statusIconPrefab;
    public Transform statusIconsParent;

    private Dictionary<string, GameObject> activeIcons = new Dictionary<string, GameObject>();

    private void Start()
    {
        // ���� ������ �θ� ������ ����
        if (statusIconsParent == null)
        {
            GameObject iconsParent = new GameObject("StatusIcons");
            iconsParent.transform.SetParent(transform);
            iconsParent.transform.localPosition = new Vector3(0, 2, 0);  // ĳ���� ���� ��ġ
            statusIconsParent = iconsParent.transform;
        }
    }

    private void Update()
    {
        UpdateStatusEffects();
    }

    public void ApplyStatusEffect(string effectName, float duration)
    {
        StatusEffect effect = statusEffects.Find(e => e.name == effectName);

        if (effect != null)
        {
            effect.isActive = true;
            effect.remainingTime = duration;

            // ������ ǥ��
            ShowStatusIcon(effect);
        }
    }

    private void UpdateStatusEffects()
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.isActive)
            {
                effect.remainingTime -= Time.deltaTime;

                if (effect.remainingTime <= 0)
                {
                    effect.isActive = false;
                    HideStatusIcon(effect);
                }
            }
        }
    }

    private void ShowStatusIcon(StatusEffect effect)
    {
        if (!activeIcons.ContainsKey(effect.name) && statusIconPrefab != null)
        {
            GameObject icon = Instantiate(statusIconPrefab, statusIconsParent);
            Image iconImage = icon.GetComponent<Image>();

            if (iconImage != null && effect.icon != null)
            {
                iconImage.sprite = effect.icon;
            }

            activeIcons.Add(effect.name, icon);
        }
    }

    private void HideStatusIcon(StatusEffect effect)
    {
        if (activeIcons.ContainsKey(effect.name))
        {
            Destroy(activeIcons[effect.name]);
            activeIcons.Remove(effect.name);
        }
    }
}