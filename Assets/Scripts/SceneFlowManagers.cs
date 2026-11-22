using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>Creates the scene flow managers before the first scene is shown.</summary>
public static class SceneFlowBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateManagers()
    {
        var root = new GameObject("Scene Flow Managers");
        Object.DontDestroyOnLoad(root);
        root.AddComponent<SplashScreenManager>();
        root.AddComponent<MainMenuManager>();
        root.AddComponent<GameScreenManager>();
    }
}

public sealed class SplashScreenManager : MonoBehaviour
{
    public static SplashScreenManager Instance { get; private set; }
    [SerializeField] private float splashDuration = 15f;
    private float shownAt;
    private bool advanced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "SplashScreen") return;
        shownAt = Time.unscaledTime;
        advanced = false;
        var prompt = GameObject.Find("Text (TMP)")?.GetComponent<TMP_Text>();
        if (prompt != null) prompt.text = "TAP OR CLICK TO CONTINUE";
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "SplashScreen" || advanced) return;
        bool pressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                       (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) ||
                       (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame);
        if (pressed ||
            Time.unscaledTime - shownAt >= splashDuration)
            ContinueToMenu();
    }

    private void ContinueToMenu()
    {
        if (advanced) return;
        advanced = true;
        SceneManager.LoadScene("MainMenu");
    }
}

public sealed class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu") return;
        var play = GameObject.Find("Play")?.GetComponent<Button>();
        if (play == null) return;
        play.onClick.RemoveListener(PlayGame);
        play.onClick.AddListener(PlayGame);
    }

    public void PlayGame() => SceneManager.LoadScene("Game");
}

public sealed class GameScreenManager : MonoBehaviour
{
    public static GameScreenManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game") AddBackButton();
    }

    private void AddBackButton()
    {
        var canvas = System.Array.Find(FindObjectsOfType<Canvas>(), item => item.name == "MainGameCanvas");
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        var buttonObject = new GameObject("Back To Main Menu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas.transform, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(24, -24);
        rect.sizeDelta = new Vector2(150, 54);
        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.05f, 0.12f, 0.22f, 0.9f);
        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(ReturnToMenu);

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "←  BACK";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 24;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    public void ReturnToMenu() => SceneManager.LoadScene("MainMenu");
}
