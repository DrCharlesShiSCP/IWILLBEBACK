using System.Collections.Generic;
using UnityEngine;

public class HealthTracker : MonoBehaviour
{
    // Holds all found Health objects and their original positions
    private readonly Dictionary<GameObject, Vector3> trackedObjects = new();

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        // Include inactive objects
        var allHealthObjects = FindObjectsByType<Health>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var allHealthObjects = FindObjectsOfType<Health>(true);
#endif

        trackedObjects.Clear();
        foreach (var h in allHealthObjects)
        {
            if (h == null) continue;
            trackedObjects[h.gameObject] = h.transform.position;
        }

        Debug.Log($"[HealthTracker] Recorded {trackedObjects.Count} Health objects.");
    }

    /// <summary>
    /// Checks all recorded objects. If any are inactive, sets them active again.
    /// Optionally, you can also reset their position to the original recorded position.
    /// </summary>
    public void ReactivateAllHealthObjects(bool resetPositions = false)
    {
        int reactivated = 0;

        foreach (var kvp in trackedObjects)
        {
            GameObject obj = kvp.Key;
            if (obj == null) continue;

            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                if (resetPositions)
                {
                    obj.transform.position = kvp.Value;
                }
                reactivated++;
            }
        }

        Debug.Log($"[HealthTracker] Reactivated {reactivated} inactive Health objects.");
    }
}
