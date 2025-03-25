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
        // ���� �������� �ִٸ� ����
        foreach (Transform child in itemDisplayPosition)
        {
            Destroy(child.gameObject);
        }

        // �� ������ �ν��Ͻ�ȭ
        GameObject item = Instantiate(itemPrefab, itemDisplayPosition);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // ���� ī�޶� �������� �� �� �ֵ��� ����
        virtualCamera.LookAt = item.transform;

        // UI�� �����ؽ�ó ����
        displayImage.texture = renderTexture;
    }

    // ������ ȸ�� ���� �߰� ����� ������ �� �ֽ��ϴ�
    public void RotateItem(float angle)
    {
        itemDisplayPosition.Rotate(Vector3.up, angle);
    }
}