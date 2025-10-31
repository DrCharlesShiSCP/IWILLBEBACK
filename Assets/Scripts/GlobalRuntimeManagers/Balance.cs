using UnityEngine;
using System;

public class Balance : MonoBehaviour
{
    public static Balance Instance { get; private set; }

    [Header("Runtime Multipliers / Bonuses")]
    [Range(0f, 10f)] public float enemyDamageMultiplier = 1f;      // e.g., 0.8f = -20% enemy damage
    [Range(0f, 10f)] public float playerMaxHealthMultiplier = 1f;  // e.g., 1.25f = +25% max HP
    [Range(0f, 10f)] public float playerDamageMultiplier = 1f;     // e.g., 1.20f = +20% player damage

    public event Action OnValuesChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddPlayerDamagePercent(float percent)
    {
        playerDamageMultiplier *= 1f + (percent / 100f);
        OnValuesChanged?.Invoke();
    }

    public void AddPlayerMaxHealthPercent(float percent)
    {
        playerMaxHealthMultiplier *= 1f + (percent / 100f);
        OnValuesChanged?.Invoke();
    }

    public void AddEnemyDamagePercent(float percent)
    {
        enemyDamageMultiplier *= 1f + (percent / 100f);
        OnValuesChanged?.Invoke();
    }

    // Call this when you need the HUD to repaint even if numbers didn’t change
    public void MarkDirty() => OnValuesChanged?.Invoke();
}
