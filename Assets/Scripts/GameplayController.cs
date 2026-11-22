using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns gameplay flow and enemy spawning for the active Game scene.</summary>
public sealed class GameplayController : MonoBehaviour
{
    public static GameplayController Instance { get; private set; }

    [SerializeField] private RectTransform playArea;
    [SerializeField] private Transform spawnPointsRoot;
    [SerializeField] private EnemyDefinition[] enemyDefinitions;
    [SerializeField, Min(0.1f)] private float spawnGapSeconds = 2.5f;

    private Transform[] spawnPoints;
    private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
    private readonly Vector3[] corners = new Vector3[4];
    private int previousSpawnPoint = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (spawnPointsRoot != null)
        {
            spawnPoints = new Transform[spawnPointsRoot.childCount];
            for (int i = 0; i < spawnPoints.Length; i++)
                spawnPoints[i] = spawnPointsRoot.GetChild(i);
        }
    }

    private void Start()
    {
        if (playArea != null && spawnPoints != null && spawnPoints.Length > 0 && enemyDefinitions != null && enemyDefinitions.Length > 0)
            StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnGapSeconds);
            SpawnOneEnemy();
        }
    }

    private void SpawnOneEnemy()
    {
        EnemyDefinition definition = enemyDefinitions[Random.Range(0, enemyDefinitions.Length)];
        if (definition == null || definition.Prefab == null) return;

        int spawnIndex = GetNextSpawnIndex();
        if (spawnIndex < 0) return;

        GameObject enemy = Instantiate(definition.Prefab, playArea, false);
        RectTransform enemyRect = enemy.transform as RectTransform;
        if (enemyRect != null)
        {
            enemyRect.position = spawnPoints[spawnIndex].position;
            enemyRect.localScale = Vector3.one;
        }

        EnemyController controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.Initialize(definition, playArea);
            activeEnemies.Add(controller);
        }
    }

    public bool TryHitEnemy(RectTransform projectile)
    {
        if (projectile == null || playArea == null) return false;
        Rect projectileBounds = GetPlayAreaBounds(projectile);
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = activeEnemies[i];
            if (enemy == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }
            if (projectileBounds.Overlaps(GetPlayAreaBounds(enemy.Rect)))
            {
                enemy.TakeDamage(1);
                return true;
            }
        }
        return false;
    }

    private Rect GetPlayAreaBounds(RectTransform target)
    {
        target.GetWorldCorners(corners);
        Vector2 bottomLeft = playArea.InverseTransformPoint(corners[0]);
        Vector2 topRight = playArea.InverseTransformPoint(corners[2]);
        return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
    }

    public void RemoveEnemy(EnemyController enemy) => activeEnemies.Remove(enemy);

    private int GetNextSpawnIndex()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return -1;
        if (spawnPoints.Length == 1) return 0;
        if (previousSpawnPoint < 0)
        {
            previousSpawnPoint = Random.Range(0, spawnPoints.Length);
            return previousSpawnPoint;
        }

        int next = Random.Range(0, spawnPoints.Length - 1);
        if (next >= previousSpawnPoint) next++;
        previousSpawnPoint = next;
        return next;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
