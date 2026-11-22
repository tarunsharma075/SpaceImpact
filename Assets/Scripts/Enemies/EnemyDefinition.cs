using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Space Impact/Enemy Definition")]
public sealed class EnemyDefinition : ScriptableObject
{
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Sprite sprite;
    [SerializeField, Min(1)] private int maximumHealth = 1;
    [SerializeField, Min(0f)] private float movementSpeed = 500f;
    [SerializeField, Min(0)] private int scoreValue = 3;
    [SerializeField, Min(0f)] private float zigzagAmplitude = 90f;
    [SerializeField, Min(0.01f)] private float zigzagFrequency = 2f;

    public EnemyType Type => enemyType;
    public GameObject Prefab => prefab;
    public Sprite Sprite => sprite;
    public int MaximumHealth => Mathf.Max(1, maximumHealth);
    public float MovementSpeed => Mathf.Max(0f, movementSpeed);
    public int ScoreValue => Mathf.Max(0, scoreValue);
    public float ZigzagAmplitude => Mathf.Max(0f, zigzagAmplitude);
    public float ZigzagFrequency => Mathf.Max(0.01f, zigzagFrequency);
}
