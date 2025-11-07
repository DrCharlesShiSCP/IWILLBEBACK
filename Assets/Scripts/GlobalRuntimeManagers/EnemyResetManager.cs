using System.Collections.Generic;
using UnityEngine;

public class EnemyResetManager : MonoBehaviour
{
    private readonly List<EnemyController> _enemies = new();

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        _enemies.AddRange(FindObjectsByType<EnemyController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None));
#else
        _enemies.AddRange(FindObjectsOfType<EnemyController>(true)); // include inactive
#endif
    }

    void OnEnable() { PlayerHealth.OnPlayerRespawned += ResetAll; }
    void OnDisable() { PlayerHealth.OnPlayerRespawned -= ResetAll; }

    private void ResetAll()
    {
#if UNITY_2023_1_OR_NEWER
        var current = FindObjectsByType<EnemyController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var current = FindObjectsOfType<EnemyController>(true);
#endif
        _enemies.Clear();
        _enemies.AddRange(current);

        foreach (var e in _enemies)
        {
            if (e == null) continue;
            e.ResetToSpawn();
        }

        Debug.Log($"[EnemyResetManager] Reset {_enemies.Count} enemies.");
    }
}
