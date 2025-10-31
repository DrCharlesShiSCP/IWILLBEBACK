using System.Collections;
using UnityEngine;
using TMPro;

public class BuffsHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text multipliersText;
    [Header("Formatting")]
    [SerializeField] private int decimals = 2;

    private float _lastPDmg = -1f, _lastPMax = -1f, _lastEDmg = -1f;

    // Track which Balance we subscribed to; rebind if the singleton changes.
    private Balance _subscribedTo;

    void OnEnable()
    {
        StartCoroutine(AutoBindLoop());
        Refresh(); // initial paint even if Balance is not ready yet
    }

    void OnDisable()
    {
        Unsubscribe();
        StopAllCoroutines();
    }

    private IEnumerator AutoBindLoop()
    {
        var wait = new WaitForSeconds(0.2f);
        while (true)
        {
            // If there is a Balance and we're not subscribed to this exact instance, (re)subscribe.
            if (Balance.Instance != null && Balance.Instance != _subscribedTo)
            {
                Unsubscribe();
                _subscribedTo = Balance.Instance;
                _subscribedTo.OnValuesChanged += Refresh;
                // Force an immediate refresh on rebinding.
                Refresh();
            }
            yield return wait;
        }
    }

    private void Unsubscribe()
    {
        if (_subscribedTo != null)
        {
            _subscribedTo.OnValuesChanged -= Refresh;
            _subscribedTo = null;
        }
    }

    public void Refresh()
    {
        if (!multipliersText) return;

        float pd = Balance.Instance ? Balance.Instance.playerDamageMultiplier : 1f;
        float pm = Balance.Instance ? Balance.Instance.playerMaxHealthMultiplier : 1f;
        float ed = Balance.Instance ? Balance.Instance.enemyDamageMultiplier : 1f;

        if (Mathf.Approximately(pd, _lastPDmg) &&
            Mathf.Approximately(pm, _lastPMax) &&
            Mathf.Approximately(ed, _lastEDmg))
        {
            return; // no visual change needed
        }

        _lastPDmg = pd; _lastPMax = pm; _lastEDmg = ed;

        string fmt = "F" + Mathf.Clamp(decimals, 0, 4);
        multipliersText.text = $"DMG x{pd.ToString(fmt)}  |  MaxHP x{pm.ToString(fmt)}  |  EnemyDmg x{ed.ToString(fmt)}";
    }
}
