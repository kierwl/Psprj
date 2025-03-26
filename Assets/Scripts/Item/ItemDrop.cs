using UnityEngine;
using System.Collections;

public class ItemDrop : MonoBehaviour
{
    [Header("������ ����")]
    [SerializeField] private ItemSO item;
    [SerializeField] private int amount = 1;

    [Header("��� ����")]
    [SerializeField] private float pickupRadius = 1.5f;  // �÷��̾ �������� �ֿ� �� �ִ� �Ÿ�
    [SerializeField] private float despawnTime = 30f;   // �������� �ڵ����� ������� �ð�
    [SerializeField] private LayerMask playerLayer;     // �÷��̾� ���̾�

    [Header("�ð� ȿ��")]
    [SerializeField] private GameObject pickupEffect;   // ȹ�� �� ȿ��
    [SerializeField] private float bounceHeight = 0.5f; // �ٿ ����
    [SerializeField] private float bounceSpeed = 2f;    // �ٿ �ӵ�
    [SerializeField] private float rotationSpeed = 50f; // ȸ�� �ӵ�

    // ������Ʈ ĳ��
    private SpriteRenderer spriteRenderer;
    private Rigidbody rb;
    private Collider itemCollider;

    // ���� ����
    private bool canBePickedUp = false;
    private float despawnTimer;
    private Vector3 startPosition;
    private bool isAnimating = true;

    private void Awake()
    {
        // ������Ʈ ���� ����
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (rb == null && GetComponent<Rigidbody>() == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (itemCollider == null && GetComponent<Collider>() == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = pickupRadius;
            itemCollider = sphereCollider;
        }

        // �ʱ� ���� ����
        startPosition = transform.position;
        despawnTimer = despawnTime;
    }

    private void Start()
    {
        // �������� �����Ǿ� ���� �ʴٸ� ��� ǥ��
        if (item == null)
        {
            Debug.LogWarning("ItemDrop�� �������� �������� �ʾҽ��ϴ�.", this);
        }
        else
        {
            // ������ ������ �ð��� ��� ����
            if (spriteRenderer != null && item.icon != null)
            {
                spriteRenderer.sprite = item.icon;
            }
        }

        // ������ ��� ȿ�� (�ٿ)
        StartCoroutine(DropEffect());
    }

    private void Update()
    {
        // �������� �ڵ����� ������� Ÿ�̸�
        if (despawnTime > 0)
        {
            despawnTimer -= Time.deltaTime;
            if (despawnTimer <= 0)
            {
                Destroy(gameObject);
                return;
            }
        }

        // ������ ȸ��
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // �÷��̾ ������ �Դ��� Ȯ��
        if (canBePickedUp)
        {
            return; // �̹� �ֿ� �� �ִ� ���¸� �˻� ��ŵ
        }

        // �ֺ��� �÷��̾ �ִ��� Ȯ��
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius, playerLayer);
        if (colliders.Length > 0)
        {
            // �÷��̾ �߰����� ��
            OnPlayerDetected(colliders[0].gameObject);
        }
    }

    // ������ ���� �ʱ�ȭ
    public void Initialize(ItemSO newItem, int newAmount = 1)
    {
        item = newItem;
        amount = newAmount;

        // �������� �����Ǹ� �ð��� ��� ������Ʈ
        if (spriteRenderer != null && item != null && item.icon != null)
        {
            spriteRenderer.sprite = item.icon;
        }

        // ������ �̸� ����
        gameObject.name = item != null ? $"Item_{item.itemName}" : "Item_Unknown";
    }

    // ������ �ٿ ȿ��
    private IEnumerator DropEffect()
    {
        // �ʱ� ��� �ִϸ��̼� ������
        yield return new WaitForSeconds(0.2f);

        float time = 0;
        isAnimating = true;

        while (isAnimating)
        {
            time += Time.deltaTime * bounceSpeed;

            // �����ĸ� ����� �ٿ ȿ��
            float height = Mathf.Abs(Mathf.Sin(time)) * bounceHeight;
            transform.position = startPosition + new Vector3(0, height, 0);

            yield return null;
        }
    }

    // �÷��̾ �����Ǿ��� �� ȣ��
    private void OnPlayerDetected(GameObject player)
    {
        canBePickedUp = true;

        // �÷��̾��� �κ��丮�� ������ �߰�
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory != null)
        {
            // �κ��丮�� ������ �߰� �õ�
            if (inventory.AddItem(item, amount))
            {
                // ȹ�� ���� �� ȿ�� ���
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // ������ ȹ�� �޽��� ǥ�� (������)
                Debug.Log($"������ ȹ��: {item.itemName} x{amount}");

                // ������ �ı�
                Destroy(gameObject);
            }
            else
            {
                // �κ��丮 ������ ������ ���
                Debug.Log("�κ��丮 ������ �����Ͽ� �������� �ֿ� �� �����ϴ�.");
                canBePickedUp = false;
            }
        }
        else
        {
            Debug.LogWarning("�÷��̾ Inventory ������Ʈ�� �����ϴ�.");
            canBePickedUp = false;
        }
    }

    // �÷��̾ ���� Trigger�� ������ �� (������)
    private void OnTriggerEnter(Collider other)
    {
        if (!canBePickedUp && other.CompareTag("Player"))
        {
            OnPlayerDetected(other.gameObject);
        }
    }

    // ������ ������ ���ڿ��� ��ȯ (������)
    public override string ToString()
    {
        if (item != null)
        {
            return $"{item.itemName} x{amount}";
        }
        return "�� ������";
    }
}