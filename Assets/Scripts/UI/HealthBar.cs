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
        // 타겟 추적
        if (target != null)
        {
            transform.position = target.position + offset;

            // 카메라를 향하도록
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

            // 체력에 따라 색상 변경 (선택 사항)
            if (currentHealth / maxHealth < 0.3f)
                healthFill.color = Color.red;
            else if (currentHealth / maxHealth < 0.6f)
                healthFill.color = Color.yellow;
            else
                healthFill.color = Color.green;
        }
    }
}