using System.Collections.Generic;
using UnityEngine;

/// <summary>Reusable pool for any projectile GameObject. Projectile behaviour is handled by its own component.</summary>
public sealed class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(1)] private int poolSize = 12;
    [SerializeField] private RectTransform poolParent;

    private readonly Dictionary<GameObject, Queue<GameObject>> availableBullets = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> prefabByBullet = new Dictionary<GameObject, GameObject>();
    private RectTransform poolRoot;

    private void Awake() => InitializePool();

    private void InitializePool()
    {
        RectTransform parent = poolParent != null ? poolParent : transform.parent as RectTransform;
        if (parent == null) return;

        var poolObject = new GameObject("Bullet Pool", typeof(RectTransform));
        poolRoot = poolObject.GetComponent<RectTransform>();
        poolRoot.SetParent(parent, false);
        poolRoot.anchorMin = poolRoot.anchorMax = new Vector2(0.5f, 0.5f);
        poolRoot.sizeDelta = Vector2.zero;

        if (bulletPrefab != null) CreatePool(bulletPrefab);
    }

    public GameObject GetBullet() => GetBullet(bulletPrefab);

    public GameObject GetBullet(GameObject prefab)
    {
        if (poolRoot == null || prefab == null) return null;
        if (!availableBullets.TryGetValue(prefab, out Queue<GameObject> pool))
            pool = CreatePool(prefab);
        if (pool.Count == 0) return null;
        GameObject bullet = pool.Dequeue();
        bullet.SetActive(true);
        return bullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null || poolRoot == null) return;
        if (!prefabByBullet.TryGetValue(bullet, out GameObject prefab)) return;
        bullet.SetActive(false);
        bullet.transform.SetParent(poolRoot, false);
        availableBullets[prefab].Enqueue(bullet);
    }

    private Queue<GameObject> CreatePool(GameObject prefab)
    {
        var pool = new Queue<GameObject>();
        availableBullets.Add(prefab, pool);
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(prefab, poolRoot);
            bullet.SetActive(false);
            prefabByBullet.Add(bullet, prefab);
            pool.Enqueue(bullet);
        }
        return pool;
    }
}
