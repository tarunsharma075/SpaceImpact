using System.Collections.Generic;
using UnityEngine;

/// <summary>Reusable pool for any projectile GameObject. Projectile behaviour is handled by its own component.</summary>
public sealed class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(1)] private int poolSize = 12;

    private readonly Queue<GameObject> availableBullets = new Queue<GameObject>();
    private RectTransform poolRoot;

    private void Awake() => InitializePool();

    private void InitializePool()
    {
        if (bulletPrefab == null) return;

        var poolObject = new GameObject("Bullet Pool", typeof(RectTransform));
        poolRoot = poolObject.GetComponent<RectTransform>();
        poolRoot.SetParent(transform.parent, false);
        poolRoot.anchorMin = poolRoot.anchorMax = new Vector2(0.5f, 0.5f);
        poolRoot.sizeDelta = Vector2.zero;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, poolRoot);
            bullet.SetActive(false);
            availableBullets.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        if (poolRoot == null || availableBullets.Count == 0) return null;
        GameObject bullet = availableBullets.Dequeue();
        bullet.SetActive(true);
        return bullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null || poolRoot == null) return;
        bullet.SetActive(false);
        bullet.transform.SetParent(poolRoot, false);
        availableBullets.Enqueue(bullet);
    }
}
