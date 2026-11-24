using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Coordinates player input, player data, movement, and the public service.</summary>
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField, Min(1)] private int startingHealth = 3;
    [SerializeField, Min(0f)] private float upSpeed = 5f;
    [SerializeField, Min(0f)] private float downSpeed = 5f;
    [SerializeField, Min(0f)] private float forwardSpeed = 2.25f;
    [SerializeField, Min(0f)] private float backwardSpeed = 5f;
    [SerializeField, Min(0f)] private float screenPadding = 0f;
    [SerializeField] private RectTransform scorePanel;
    [Header("Firing")]
    [SerializeField, Min(0.01f)] private float fireInterval = 0.18f;
    [SerializeField, Min(0f)] private float bulletSpeed = 1800f;
    [SerializeField] private AudioClip pewSound;
    [SerializeField, Range(0f, 1f)] private float fireVolume = 0.7f;
    [SerializeField] private RectTransform bulletSpawnPoint;

    private PlayerModel model;
    private PlayerService service;
    private RectTransform movementArea;
    private Canvas playerCanvas;
    private BulletPool bulletPool;
    private Button fireButton;
    private AudioSource fireAudioSource;
    private float nextFireTime;

    public PlayerModel Model => model;
    public PlayerService Service => service;
    public RectTransform Rect => transform as RectTransform;

    public void TakeDamage(int amount) => service?.TakeDamage(amount);

    private void Awake()
    {
        float maximumSpeed = Mathf.Max(upSpeed, downSpeed, forwardSpeed, backwardSpeed);
        model = new PlayerModel(startingHealth, maximumSpeed);
        model.Initialize();
        service = new PlayerService(model, GetPosition);
        service.Register();

        var playerRect = transform as RectTransform;
        if (playerRect != null) movementArea = playerRect.parent as RectTransform;
        bulletPool = GetComponent<BulletPool>();
        if (bulletSpawnPoint == null)
        {
            Transform spawn = transform.Find("SpriteR/SingleBullet");
            if (spawn != null) bulletSpawnPoint = spawn as RectTransform;
        }
        GameObject fireObject = GameObject.Find("fire");
        if (fireObject != null) fireButton = fireObject.GetComponent<Button>();
        if (fireButton != null) fireButton.onClick.AddListener(FireSingleBullet);
        fireAudioSource = GetComponent<AudioSource>();
        if (fireAudioSource == null) fireAudioSource = gameObject.AddComponent<AudioSource>();
        fireAudioSource.playOnAwake = false;
        fireAudioSource.spatialBlend = 0f;
        playerCanvas = GetComponentInParent<Canvas>();
        if (scorePanel == null)
        {
            var panelObject = GameObject.Find("ScorePanel");
            if (panelObject != null) scorePanel = panelObject.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (!model.IsAlive) return;
        Vector2 direction = ReadMovement();
        if (direction.sqrMagnitude > 1f) direction.Normalize();
        var rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            float directionalSpeed = GetDirectionalSpeed(direction);
            Vector2 target = rectTransform.anchoredPosition + direction * directionalSpeed * 100f * Time.deltaTime;
            rectTransform.anchoredPosition = ClampToPlayArea(rectTransform, target);
        }
        else
            transform.position += (Vector3)(direction * GetDirectionalSpeed(direction) * Time.deltaTime);
    }

    private float GetDirectionalSpeed(Vector2 direction)
    {
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
            return direction.y >= 0f ? upSpeed : downSpeed;
        return direction.x >= 0f ? forwardSpeed : backwardSpeed;
    }

    private Vector2 ClampToPlayArea(RectTransform playerRect, Vector2 anchoredPosition)
    {
        if (movementArea == null) return anchoredPosition;

        Rect area = movementArea.rect;
        Vector2 minPivot = area.min - playerRect.rect.min + Vector2.one * screenPadding;
        Vector2 maxPivot = area.max - playerRect.rect.max - Vector2.one * screenPadding;
        Camera areaCamera = playerCanvas != null && playerCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? playerCanvas.worldCamera
            : null;

        Rect safeArea = Screen.safeArea;
        Vector2 safeBottomLeft = safeArea.min;
        Vector2 safeTopRight = safeArea.max;
        if (ScreenPointToMovementArea(safeBottomLeft, areaCamera, out var safeMin))
            minPivot = Vector2.Max(minPivot, safeMin - playerRect.rect.min + Vector2.one * screenPadding);
        if (ScreenPointToMovementArea(safeTopRight, areaCamera, out var safeMax))
            maxPivot = Vector2.Min(maxPivot, safeMax - playerRect.rect.max - Vector2.one * screenPadding);

        // Keep the ship's top edge below the HUD's score/lives panel.
        if (scorePanel != null)
        {
            var corners = new Vector3[4];
            scorePanel.GetWorldCorners(corners);
            Canvas panelCanvas = scorePanel.GetComponentInParent<Canvas>();
            Camera panelCamera = panelCanvas != null && panelCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? panelCanvas.worldCamera
                : null;
            Vector2 panelBottomLeft = RectTransformUtility.WorldToScreenPoint(panelCamera, corners[0]);
            Vector2 panelBottomRight = RectTransformUtility.WorldToScreenPoint(panelCamera, corners[3]);
            Vector2 panelBottomCenter = (panelBottomLeft + panelBottomRight) * 0.5f;
            if (ScreenPointToMovementArea(panelBottomCenter, areaCamera, out var panelBottom))
                maxPivot.y = Mathf.Min(maxPivot.y, panelBottom.y - playerRect.rect.yMax - screenPadding);
        }

        if (maxPivot.x < minPivot.x) maxPivot.x = minPivot.x;
        if (maxPivot.y < minPivot.y) maxPivot.y = minPivot.y;

        // Convert the clamped pivot point back to anchoredPosition for this player's anchors.
        Vector2 anchorReference = area.min + Vector2.Scale(area.size, playerRect.anchorMin);
        Vector2 localPivot = anchorReference + anchoredPosition;
        localPivot.x = Mathf.Clamp(localPivot.x, minPivot.x, maxPivot.x);
        localPivot.y = Mathf.Clamp(localPivot.y, minPivot.y, maxPivot.y);
        return localPivot - anchorReference;
    }

    private bool ScreenPointToMovementArea(Vector2 screenPoint, Camera camera, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(movementArea, screenPoint, camera, out localPoint);
    }

    private Vector2 GetPosition()
    {
        var rectTransform = transform as RectTransform;
        return rectTransform != null ? rectTransform.anchoredPosition : (Vector2)transform.position;
    }

    private Vector2 ReadMovement()
    {
        if (GameplayControlsUI.Movement.sqrMagnitude > 0.001f)
            return GameplayControlsUI.Movement;
        var keyboard = Keyboard.current;
        if (keyboard == null) return Vector2.zero;
        float x = 0f;
        float y = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;
        return new Vector2(x, y);
    }

    private void FireSingleBullet()
    {
        if (!model.IsAlive || Time.time < nextFireTime || bulletPool == null ||
            bulletSpawnPoint == null || movementArea == null) return;
        GameObject bulletObject = bulletPool.GetBullet();
        if (bulletObject == null) return;
        PlayerBullet bullet = bulletObject.GetComponent<PlayerBullet>();
        if (bullet == null)
        {
            bulletPool.ReturnBullet(bulletObject);
            return;
        }
        RectTransform bulletRect = bullet.GetComponent<RectTransform>();
        Vector3 spawnLocal = movementArea.InverseTransformPoint(bulletSpawnPoint.position);
        Vector2 anchorReference = movementArea.rect.min +
            Vector2.Scale(movementArea.rect.size, bulletRect.anchorMin);
        Vector2 startPosition = (Vector2)spawnLocal - anchorReference;
        bullet.Launch(movementArea, startPosition, bulletPool, bulletSpeed);
        nextFireTime = Time.time + fireInterval;
        if (pewSound != null) fireAudioSource.PlayOneShot(pewSound, fireVolume);
    }

    private void OnDestroy()
    {
        if (fireButton != null) fireButton.onClick.RemoveListener(FireSingleBullet);
        service?.Unregister();
    }
}
