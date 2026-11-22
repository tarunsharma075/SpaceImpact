using System;
using UnityEngine;

/// <summary>Runtime data for the player. Does not depend on Unity scene objects.</summary>
[Serializable]
public sealed class PlayerModel
{
    [SerializeField, Min(1)] private int maximumHealth = 3;
    [SerializeField, Min(0)] private float movementSpeed = 5f;

    public int MaximumHealth => Mathf.Max(1, maximumHealth);
    public int CurrentHealth { get; private set; }
    public int Score { get; private set; }
    public float MovementSpeed => Mathf.Max(0f, movementSpeed);
    public bool IsAlive => CurrentHealth > 0;

    public event Action<int, int> HealthChanged;
    public event Action<int> ScoreChanged;
    public event Action Died;

    public PlayerModel(int health = 3, float speed = 5f)
    {
        maximumHealth = Mathf.Max(1, health);
        movementSpeed = Mathf.Max(0f, speed);
        CurrentHealth = maximumHealth;
    }

    public void Initialize()
    {
        CurrentHealth = MaximumHealth;
        Score = 0;
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0) return;
        int previous = CurrentHealth;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (previous == CurrentHealth) return;
        HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
        if (CurrentHealth == 0) Died?.Invoke();
    }

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0) return;
        int previous = CurrentHealth;
        CurrentHealth = Mathf.Min(MaximumHealth, CurrentHealth + amount);
        if (previous != CurrentHealth) HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        Score += amount;
        ScoreChanged?.Invoke(Score);
    }
}
