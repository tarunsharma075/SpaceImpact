using UnityEngine;

public sealed class FighterAttack : MonoBehaviour
{
    private readonly RectTransform[] bulletSpawnPoints = new RectTransform[2];
    private EnemyDefinition definition;
    private RectTransform playArea;
    private BulletPool bulletPool;
    private float nextFireTime;

    private void Awake()
    {
        bulletSpawnPoints[0] = transform.Find("BulletPointOne") as RectTransform;
        bulletSpawnPoints[1] = transform.Find("BulletPointtwo") as RectTransform;
    }

    public void Initialize(EnemyDefinition enemyDefinition, RectTransform area, BulletPool pool)
    {
        definition = enemyDefinition;
        playArea = area;
        bulletPool = pool;
        // Fire the opening volley immediately; the definition controls subsequent intervals.
        nextFireTime = Time.time;
    }

    private void Update()
    {
        if (definition == null || bulletPool == null || definition.ProjectilePrefab == null || Time.time < nextFireTime)
            return;

        FireFrom(bulletSpawnPoints[0]);
        FireFrom(bulletSpawnPoints[1]);
        nextFireTime = Time.time + definition.FireInterval;
    }

    private void FireFrom(RectTransform spawnPoint)
    {
        if (spawnPoint == null || playArea == null) return;
        GameObject bulletObject = bulletPool.GetBullet(definition.ProjectilePrefab);
        if (bulletObject == null) return;

        EnemyBulletMovement bullet = bulletObject.GetComponent<EnemyBulletMovement>();
        if (bullet == null)
        {
            bulletPool.ReturnBullet(bulletObject);
            return;
        }

        RectTransform bulletRect = bullet.transform as RectTransform;
        Vector3 spawnLocal = playArea.InverseTransformPoint(spawnPoint.position);
        Vector2 anchorReference = playArea.rect.min + Vector2.Scale(playArea.rect.size, bulletRect.anchorMin);
        Vector2 startPosition = (Vector2)spawnLocal - anchorReference;
        bullet.Launch(playArea, startPosition, bulletPool, definition.ProjectileSpeed);
    }
}
