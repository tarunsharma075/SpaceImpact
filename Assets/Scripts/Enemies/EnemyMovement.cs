using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    public abstract void Initialize(EnemyDefinition definition, RectTransform playArea);
    public abstract void Move();
}
