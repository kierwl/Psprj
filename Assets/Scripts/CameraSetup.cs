using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [SerializeField] private float fieldOfView = 60f;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.3f); // 어두운 파란색
    [SerializeField] private Vector3 cameraPosition = new Vector3(0f, 20f, -20f);
    [SerializeField] private Vector3 cameraRotation = new Vector3(45f, 0f, 0f);

    void Start()
    {
        Camera mainCamera = GetComponent<Camera>();
        
        // 카메라 기본 설정
        mainCamera.fieldOfView = fieldOfView;
        mainCamera.backgroundColor = backgroundColor;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // 카메라 위치 및 회전 설정
        transform.position = cameraPosition;
        transform.rotation = Quaternion.Euler(cameraRotation);
    }
} 