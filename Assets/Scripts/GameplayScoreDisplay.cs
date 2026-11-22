using TMPro;
using UnityEngine;

public sealed class GameplayScoreDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    private PlayerService playerService;

    private void Awake()
    {
        if (scoreText == null) scoreText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        playerService = PlayerService.Current;
        if (playerService == null) return;
        playerService.ScoreChanged += UpdateScore;
        UpdateScore(playerService.Score);
    }

    private void OnDestroy()
    {
        if (playerService != null) playerService.ScoreChanged -= UpdateScore;
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE {score}";
    }
}
