using UnityEngine;

public sealed class TankMovement : EnemyMovement
{
    private RectTransform tankRect;
    private float movementSpeed;

    public override void Initialize(EnemyDefinition definition, RectTransform playArea)
    {
        tankRect = transform as RectTransform;
        movementSpeed = definition.MovementSpeed;
    }

    public override void Move()
    {
        if (tankRect == null) return;
        tankRect.anchoredPosition += Vector2.left * movementSpeed * Time.deltaTime;
    }
}
