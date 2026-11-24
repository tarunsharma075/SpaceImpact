using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns gameplay flow and enemy spawning for the active Game scene.</summary>
public sealed class GameplayController : MonoBehaviour
{
    public static GameplayController Instance { get; private set; }

    [SerializeField] private RectTransform playArea;
    [SerializeField] private Transform spawnPointsRoot;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private EnemyDefinition[] enemyDefinitions;
    [Header("Level pacing")]
    [SerializeField, Min(1)] private int currentLevel = 1;
    [SerializeField, Min(0.1f)] private float levelOneSpawnGapSeconds = 4f;
    [SerializeField, Min(0f)] private float spawnGapDecreasePerLevelSeconds = 0.2f;
    [SerializeField, Min(0.1f)] private float minimumSpawnGapSeconds = 1.2f;
    [Header("Enemy spawn ratio")]
    [SerializeField, Min(1)] private int scoutsPerFighter = 5;
    [SerializeField, Min(1)] private int scoutsRequiredForTank = 10;
    [SerializeField, Min(1)] private int fightersRequiredForTank = 3;

    private Transform[] spawnPoints;
    private EnemyDefinition scoutDefinition;
    private EnemyDefinition fighterDefinition;
    private EnemyDefinition tankDefinition;
    private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
    private readonly Vector3[] corners = new Vector3[4];
    private int previousSpawnPoint = -1;
    private int scoutsSinceFighter;
    private int scoutsSinceTank;
    private int fightersSinceTank;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        FindEnemyDefinitions();
        if (spawnPointsRoot != null)
        {
            spawnPoints = new Transform[spawnPointsRoot.childCount];
            for (int i = 0; i < spawnPoints.Length; i++)
                spawnPoints[i] = spawnPointsRoot.GetChild(i);
        }
    }

    private void Start()
    {
        if (playArea != null && spawnPoints != null && spawnPoints.Length > 0 && scoutDefinition != null)
            StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetSpawnGapSeconds());
            SpawnOneEnemy();
        }
    }

    private void SpawnOneEnemy()
    {
        EnemyDefinition definition = GetNextEnemyDefinition();
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

    public int CurrentLevel => currentLevel;

    public void SetLevel(int level) => currentLevel = Mathf.Max(1, level);

    public void AdvanceLevel() => SetLevel(currentLevel + 1);

    public BulletPool BulletPool => bulletPool;

    public bool TryHitPlayer(RectTransform projectile)
    {
        if (projectile == null || playerController == null || playArea == null) return false;
        if (!GetPlayAreaBounds(projectile).Overlaps(GetPlayAreaBounds(playerController.Rect))) return false;
        playerController.TakeDamage(1);
        return true;
    }

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

    private void FindEnemyDefinitions()
    {
        if (enemyDefinitions == null) return;
        foreach (EnemyDefinition definition in enemyDefinitions)
        {
            if (definition == null) continue;
            switch (definition.Type)
            {
                case EnemyType.Scout: scoutDefinition = definition; break;
                case EnemyType.Fighter: fighterDefinition = definition; break;
                case EnemyType.Tank: tankDefinition = definition; break;
            }
        }
    }

    private EnemyDefinition GetNextEnemyDefinition()
    {
        // Tank waits for both thresholds; the 5-Scout Fighter cadence means this occurs after 15 Scouts.
        if (tankDefinition != null && scoutsSinceTank >= scoutsRequiredForTank &&
            fightersSinceTank >= fightersRequiredForTank)
        {
            scoutsSinceTank = 0;
            fightersSinceTank = 0;
            return tankDefinition;
        }

        if (fighterDefinition != null && scoutsSinceFighter >= scoutsPerFighter)
        {
            scoutsSinceFighter = 0;
            fightersSinceTank++;
            return fighterDefinition;
        }

        scoutsSinceFighter++;
        scoutsSinceTank++;
        return scoutDefinition;
    }

    private float GetSpawnGapSeconds()
    {
        float levelGap = levelOneSpawnGapSeconds - (currentLevel - 1) * spawnGapDecreasePerLevelSeconds;
        return Mathf.Max(minimumSpawnGapSeconds, levelGap);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
