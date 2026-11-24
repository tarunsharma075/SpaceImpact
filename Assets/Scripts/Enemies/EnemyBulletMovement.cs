using UnityEngine;

public sealed class EnemyBulletMovement : MonoBehaviour
{
    private RectTransform bulletRect;
    private RectTransform movementArea;
    private BulletPool bulletPool;
    private float speed;
    private bool isFlying;
    private readonly Vector3[] corners = new Vector3[4];

    private void Awake() => bulletRect = transform as RectTransform;

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
        bulletRect.anchoredPosition += Vector2.left * speed * Time.deltaTime;

        GameplayController gameplay = GameplayController.Instance;
        if (gameplay != null && gameplay.TryHitPlayer(bulletRect))
        {
            ReturnToPool();
            return;
        }

        bulletRect.GetWorldCorners(corners);
        float bulletLeftEdge = Mathf.Min(
            movementArea.InverseTransformPoint(corners[0]).x,
            movementArea.InverseTransformPoint(corners[1]).x);
        if (bulletLeftEdge < movementArea.rect.xMin) ReturnToPool();
    }

    private void ReturnToPool()
    {
        isFlying = false;
        bulletPool?.ReturnBullet(gameObject);
    }
}
