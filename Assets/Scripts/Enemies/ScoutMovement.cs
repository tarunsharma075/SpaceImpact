using UnityEngine;

public sealed class ScoutMovement : EnemyMovement
{
    private RectTransform scoutRect;
    private float movementSpeed;
    private float zigzagAmplitude;
    private float zigzagFrequency;
    private float startY;
    private float elapsedTime;

    public override void Initialize(EnemyDefinition definition, RectTransform playArea)
    {
        scoutRect = transform as RectTransform;
        movementSpeed = definition.MovementSpeed;
        zigzagAmplitude = definition.ZigzagAmplitude;
        zigzagFrequency = definition.ZigzagFrequency;
        startY = scoutRect.anchoredPosition.y;
        elapsedTime = 0f;
    }

    public override void Move()
    {
        if (scoutRect == null) return;
        elapsedTime += Time.deltaTime;
        Vector2 position = scoutRect.anchoredPosition;
        position.x -= movementSpeed * Time.deltaTime;
        float zigzag = Mathf.PingPong(elapsedTime * zigzagFrequency, 2f) - 1f;
        position.y = startY + zigzag * zigzagAmplitude;
        scoutRect.anchoredPosition = position;
    }
}
