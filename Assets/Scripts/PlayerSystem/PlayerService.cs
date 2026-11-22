using System;
using UnityEngine;

/// <summary>Plain C# API for other gameplay systems that need player state or actions.</summary>
public sealed class PlayerService
{
    public static PlayerService Current { get; private set; }

    private readonly PlayerModel model;
    private readonly Func<Vector2> getPosition;

    public int Health => model.CurrentHealth;
    public int MaximumHealth => model.MaximumHealth;
    public int Score => model.Score;
    public bool IsAlive => model.IsAlive;
    public Vector2 Position => getPosition != null ? getPosition() : Vector2.zero;

    public event Action<int, int> HealthChanged
    {
        add => model.HealthChanged += value;
        remove => model.HealthChanged -= value;
    }
    public event Action<int> ScoreChanged
    {
        add => model.ScoreChanged += value;
        remove => model.ScoreChanged -= value;
    }
    public event Action Died
    {
        add => model.Died += value;
        remove => model.Died -= value;
    }

    internal PlayerService(PlayerModel playerModel, Func<Vector2> positionProvider)
    {
        model = playerModel;
        getPosition = positionProvider;
    }

    internal void Register() => Current = this;
    internal void Unregister() { if (Current == this) Current = null; }

    public void TakeDamage(int amount) => model.TakeDamage(amount);
    public void Heal(int amount) => model.Heal(amount);
    public void AddScore(int amount) => model.AddScore(amount);
}
