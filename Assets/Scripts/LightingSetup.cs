using UnityEngine;

public class LightingSetup : MonoBehaviour
{
    [SerializeField] private Color directionalLightColor = new Color(1f, 0.8f, 0.6f); // 따뜻한 주황색
    [SerializeField] private float directionalLightIntensity = 1.2f;
    [SerializeField] private Vector3 directionalLightRotation = new Vector3(50f, -30f, 0f);
    
    [SerializeField] private Color pointLightColor = new Color(1f, 0.6f, 0.3f); // 따뜻한 주황색
    [SerializeField] private float pointLightIntensity = 1.5f;
    [SerializeField] private float pointLightRange = 10f;

    void Start()
    {
        // Directional Light 설정
        GameObject directionalLight = new GameObject("Directional Light");
        Light dirLight = directionalLight.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = directionalLightColor;
        dirLight.intensity = directionalLightIntensity;
        dirLight.shadows = LightShadows.Soft;
        directionalLight.transform.rotation = Quaternion.Euler(directionalLightRotation);

        // Point Light 생성
        CreatePointLight(new Vector3(0f, 5f, 0f));
        CreatePointLight(new Vector3(10f, 3f, 10f));
        CreatePointLight(new Vector3(-10f, 3f, -10f));
    }

    private void CreatePointLight(Vector3 position)
    {
        GameObject pointLight = new GameObject("Point Light");
        pointLight.transform.position = position;
        Light light = pointLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = pointLightColor;
        light.intensity = pointLightIntensity;
        light.range = pointLightRange;
        light.shadows = LightShadows.Soft;
    }
} 