using UnityEngine;
using UnityEngine.UI;

public sealed class EnemyController : MonoBehaviour
{
    private EnemyDefinition definition;
    private EnemyMovement movement;
    private FighterAttack fighterAttack;
    private TankAttack tankAttack;
    private RectTransform enemyRect;
    private RectTransform playArea;
    private int currentHealth;
    private bool defeated;
    public RectTransform Rect => enemyRect;

    private void Awake()
    {
        enemyRect = transform as RectTransform;
        movement = GetComponent<EnemyMovement>();
        fighterAttack = GetComponent<FighterAttack>();
        tankAttack = GetComponent<TankAttack>();
    }

    public void Initialize(EnemyDefinition enemyDefinition, RectTransform area)
    {
        definition = enemyDefinition;
        playArea = area;
        currentHealth = definition.MaximumHealth;
        Image image = GetComponent<Image>();
        if (image != null && definition.Sprite != null) image.sprite = definition.Sprite;
        if (movement != null) movement.Initialize(definition, playArea);
        if (fighterAttack != null)
            fighterAttack.Initialize(definition, playArea, GameplayController.Instance != null ? GameplayController.Instance.BulletPool : null);
        if (tankAttack != null)
            tankAttack.Initialize(definition, playArea, GameplayController.Instance != null ? GameplayController.Instance.BulletPool : null);
    }

    private void Update()
    {
        if (movement != null) movement.Move();
        if (enemyRect == null || playArea == null) return;

        Vector3[] corners = new Vector3[4];
        enemyRect.GetWorldCorners(corners);
        float enemyRightEdge = Mathf.Max(
            playArea.InverseTransformPoint(corners[2]).x,
            playArea.InverseTransformPoint(corners[3]).x);
        if (enemyRightEdge < playArea.rect.xMin) Destroy(gameObject);
    }

    public void TakeDamage(int amount)
    {
        if (defeated || amount <= 0) return;
        currentHealth -= amount;
        if (currentHealth > 0) return;

        defeated = true;
        PlayerService.Current?.AddScore(definition.ScoreValue);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameplayController.Instance?.RemoveEnemy(this);
    }
}
