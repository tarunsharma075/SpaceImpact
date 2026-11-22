using UnityEngine;

/// <summary>Moves one pooled laser shot to the right and reports when it leaves the screen.</summary>
public sealed class PlayerBullet : MonoBehaviour
{
    private RectTransform bulletRect;
    private RectTransform movementArea;
    private BulletPool bulletPool;
    [SerializeField, Min(0f)] private float speed = 1800f;
    private bool isFlying;

    private void Awake()
    {
        bulletRect = GetComponent<RectTransform>();
    }

    public void Launch(RectTransform area, Vector2 startPosition, BulletPool pool, float bulletSpeed)
    {
        movementArea = area;
        bulletPool = pool;
        speed = Mathf.Max(0f, bulletSpeed);
        isFlying = true;

        transform.SetParent(area, false);
        bulletRect.localScale = Vector3.one;
        bulletRect.localRotation = Quaternion.identity;
        bulletRect.anchoredPosition = startPosition;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isFlying || movementArea == null) return;

        bulletRect.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        if (GameplayController.Instance != null && GameplayController.Instance.TryHitEnemy(bulletRect))
        {
            isFlying = false;
            bulletPool?.ReturnBullet(gameObject);
            return;
        }

        float anchorX = movementArea.rect.xMin + movementArea.rect.width * bulletRect.anchorMin.x;
        float bulletRightEdge = anchorX + bulletRect.anchoredPosition.x + bulletRect.rect.xMax;
        if (bulletRightEdge >= movementArea.rect.xMax)
        {
            isFlying = false;
            bulletPool?.ReturnBullet(gameObject);
        }
    }
}
