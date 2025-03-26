using UnityEngine;
using System.Collections;

public class ItemDrop : MonoBehaviour
{
    [Header("아이템 정보")]
    [SerializeField] private ItemSO item;
    [SerializeField] private int amount = 1;

    [Header("드롭 설정")]
    [SerializeField] private float pickupRadius = 1.5f;  // 플레이어가 아이템을 주울 수 있는 거리
    [SerializeField] private float despawnTime = 30f;   // 아이템이 자동으로 사라지는 시간
    [SerializeField] private LayerMask playerLayer;     // 플레이어 레이어

    [Header("시각 효과")]
    [SerializeField] private GameObject pickupEffect;   // 획득 시 효과
    [SerializeField] private float bounceHeight = 0.5f; // 바운스 높이
    [SerializeField] private float bounceSpeed = 2f;    // 바운스 속도
    [SerializeField] private float rotationSpeed = 50f; // 회전 속도

    // 컴포넌트 캐싱
    private SpriteRenderer spriteRenderer;
    private Rigidbody rb;
    private Collider itemCollider;

    // 내부 상태
    private bool canBePickedUp = false;
    private float despawnTimer;
    private Vector3 startPosition;
    private bool isAnimating = true;

    private void Awake()
    {
        // 컴포넌트 참조 설정
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

        // 초기 상태 설정
        startPosition = transform.position;
        despawnTimer = despawnTime;
    }

    private void Start()
    {
        // 아이템이 설정되어 있지 않다면 경고 표시
        if (item == null)
        {
            Debug.LogWarning("ItemDrop에 아이템이 설정되지 않았습니다.", this);
        }
        else
        {
            // 아이템 정보로 시각적 요소 설정
            if (spriteRenderer != null && item.icon != null)
            {
                spriteRenderer.sprite = item.icon;
            }
        }

        // 아이템 드롭 효과 (바운스)
        StartCoroutine(DropEffect());
    }

    private void Update()
    {
        // 아이템이 자동으로 사라지는 타이머
        if (despawnTime > 0)
        {
            despawnTimer -= Time.deltaTime;
            if (despawnTimer <= 0)
            {
                Destroy(gameObject);
                return;
            }
        }

        // 아이템 회전
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 플레이어가 가까이 왔는지 확인
        if (canBePickedUp)
        {
            return; // 이미 주울 수 있는 상태면 검사 스킵
        }

        // 주변에 플레이어가 있는지 확인
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius, playerLayer);
        if (colliders.Length > 0)
        {
            // 플레이어를 발견했을 때
            OnPlayerDetected(colliders[0].gameObject);
        }
    }

    // 아이템 정보 초기화
    public void Initialize(ItemSO newItem, int newAmount = 1)
    {
        item = newItem;
        amount = newAmount;

        // 아이템이 설정되면 시각적 요소 업데이트
        if (spriteRenderer != null && item != null && item.icon != null)
        {
            spriteRenderer.sprite = item.icon;
        }

        // 아이템 이름 설정
        gameObject.name = item != null ? $"Item_{item.itemName}" : "Item_Unknown";
    }

    // 아이템 바운스 효과
    private IEnumerator DropEffect()
    {
        // 초기 드롭 애니메이션 딜레이
        yield return new WaitForSeconds(0.2f);

        float time = 0;
        isAnimating = true;

        while (isAnimating)
        {
            time += Time.deltaTime * bounceSpeed;

            // 사인파를 사용한 바운스 효과
            float height = Mathf.Abs(Mathf.Sin(time)) * bounceHeight;
            transform.position = startPosition + new Vector3(0, height, 0);

            yield return null;
        }
    }

    // 플레이어가 감지되었을 때 호출
    private void OnPlayerDetected(GameObject player)
    {
        canBePickedUp = true;

        // 플레이어의 인벤토리에 아이템 추가
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory != null)
        {
            // 인벤토리에 아이템 추가 시도
            if (inventory.AddItem(item, amount))
            {
                // 획득 성공 시 효과 재생
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // 아이템 획득 메시지 표시 (선택적)
                Debug.Log($"아이템 획득: {item.itemName} x{amount}");

                // 아이템 파괴
                Destroy(gameObject);
            }
            else
            {
                // 인벤토리 공간이 부족한 경우
                Debug.Log("인벤토리 공간이 부족하여 아이템을 주울 수 없습니다.");
                canBePickedUp = false;
            }
        }
        else
        {
            Debug.LogWarning("플레이어에 Inventory 컴포넌트가 없습니다.");
            canBePickedUp = false;
        }
    }

    // 플레이어가 직접 Trigger에 들어왔을 때 (선택적)
    private void OnTriggerEnter(Collider other)
    {
        if (!canBePickedUp && other.CompareTag("Player"))
        {
            OnPlayerDetected(other.gameObject);
        }
    }

    // 아이템 정보를 문자열로 반환 (디버깅용)
    public override string ToString()
    {
        if (item != null)
        {
            return $"{item.itemName} x{amount}";
        }
        return "빈 아이템";
    }
}