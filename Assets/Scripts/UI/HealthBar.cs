using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthFill;
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    public bool alwaysFaceCamera = true;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Ÿ�� ����
        if (target != null)
        {
            transform.position = target.position + offset;

            // ī�޶� ���ϵ���
            if (alwaysFaceCamera && mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.forward);
            }
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;

            // ü�¿� ���� ���� ���� (���� ����)
            if (currentHealth / maxHealth < 0.3f)
                healthFill.color = Color.red;
            else if (currentHealth / maxHealth < 0.6f)
                healthFill.color = Color.yellow;
            else
                healthFill.color = Color.green;
        }
    }
}