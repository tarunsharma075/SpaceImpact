using UnityEngine;

public sealed class FighterMovement : EnemyMovement
{
    private RectTransform fighterRect;
    private float movementSpeed;

    public override void Initialize(EnemyDefinition definition, RectTransform playArea)
    {
        fighterRect = transform as RectTransform;
        movementSpeed = definition.MovementSpeed;
    }

    public override void Move()
    {
        if (fighterRect == null) return;
        fighterRect.anchoredPosition += Vector2.left * movementSpeed * Time.deltaTime;
    }
}
