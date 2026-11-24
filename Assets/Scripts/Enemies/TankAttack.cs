using UnityEngine;

public sealed class TankAttack : MonoBehaviour
{
    private EnemyDefinition definition;
    private RectTransform playArea;
    private RectTransform shellSpawnPoint;
    private BulletPool bulletPool;
    private float nextFireTime;

    private void Awake()
    {
        shellSpawnPoint = transform.Find("SpawnPoint") as RectTransform;
    }

    public void Initialize(EnemyDefinition enemyDefinition, RectTransform area, BulletPool pool)
    {
        definition = enemyDefinition;
        playArea = area;
        bulletPool = pool;
        nextFireTime = Time.time;
    }

    private void Update()
    {
        if (definition == null || bulletPool == null || definition.ProjectilePrefab == null ||
            shellSpawnPoint == null || Time.time < nextFireTime)
            return;

        GameObject shellObject = bulletPool.GetBullet(definition.ProjectilePrefab);
        if (shellObject == null) return;

        EnemyBulletMovement shell = shellObject.GetComponent<EnemyBulletMovement>();
        if (shell == null)
        {
            bulletPool.ReturnBullet(shellObject);
            return;
        }

        RectTransform shellRect = shell.transform as RectTransform;
        Vector3 spawnLocal = playArea.InverseTransformPoint(shellSpawnPoint.position);
        Vector2 anchorReference = playArea.rect.min + Vector2.Scale(playArea.rect.size, shellRect.anchorMin);
        Vector2 startPosition = (Vector2)spawnLocal - anchorReference;
        shell.Launch(playArea, startPosition, bulletPool, definition.ProjectileSpeed);
        nextFireTime = Time.time + definition.FireInterval;
    }
}
