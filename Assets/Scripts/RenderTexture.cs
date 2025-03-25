using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class InventoryItemViewer : MonoBehaviour
{
    [SerializeField] private RawImage displayImage;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Camera itemCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform itemDisplayPosition;

    public void DisplayItem(GameObject itemPrefab)
    {
        // 기존 아이템이 있다면 제거
        foreach (Transform child in itemDisplayPosition)
        {
            Destroy(child.gameObject);
        }

        // 새 아이템 인스턴스화
        GameObject item = Instantiate(itemPrefab, itemDisplayPosition);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // 가상 카메라가 아이템을 볼 수 있도록 설정
        virtualCamera.LookAt = item.transform;

        // UI에 렌더텍스처 적용
        displayImage.texture = renderTexture;
    }

    // 아이템 회전 등의 추가 기능을 구현할 수 있습니다
    public void RotateItem(float angle)
    {
        itemDisplayPosition.Rotate(Vector3.up, angle);
    }
}