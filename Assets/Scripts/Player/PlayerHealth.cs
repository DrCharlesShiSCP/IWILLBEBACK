using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    [Header("UI")]
    [SerializeField] private TMP_Text healthText;   // existing HP label (optional)
    [SerializeField] private TMP_Text deathText;    // ¡û drag a TMP_Text here to show death count

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    public float currentHealth;
    private int deathCount = 0;                     // ¡û death counter

    private RollDiceManager manager;
    private CharacterController cc;
    private Health enemyHealth;
    public AudioSource audioSource;
    public AudioClip deathClip;

    public static event System.Action OnPlayerRespawned;
    void Awake()
    {
        audioSource = FindAnyObjectByType<AudioSource>();
        cc = GetComponent<CharacterController>();
    }
    private float EffectiveMaxHealth()
    {
        float mult = Balance.Instance ? Balance.Instance.playerMaxHealthMultiplier : 1f;
        return maxHealth * mult;
    }

    void Start()
    {
        currentHealth = EffectiveMaxHealth(); 
        // currentHealth = maxHealth;
        UpdateHealthUI();
        UpdateDeathUI();
        manager = Object.FindAnyObjectByType<RollDiceManager>();
        if (manager != null)
        {
            manager.OnAbilityChosen += HandleAbilityChosen;
        }
        else
        {
            Debug.LogWarning("PlayerHealth: No RollDiceManager found in scene.");
        }
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.OnAbilityChosen -= HandleAbilityChosen;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        // increment deaths and update UI immediately
        deathCount++;
        UpdateDeathUI();

        // Pause and open selection UI
        Time.timeScale = 0f;

        if (manager != null) manager.ShowRollMenu();
        else Debug.LogWarning("PlayerHealth: No RollDiceManager to show selection UI.");
    }

    public void HandleAbilityChosen()
    {
        // Teleport to respawn and fully heal
        audioSource.PlayOneShot(deathClip);
        if (respawnPoint != null)
        {
            bool wasEnabled = cc && cc.enabled;
            if (cc) cc.enabled = false;

            transform.position = respawnPoint.position;
            // transform.rotation = respawnPoint.rotation; // uncomment if you want to reset facing

            if (cc && wasEnabled) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("PlayerHealth: RespawnPoint not assigned. Staying in place.");
        }

        currentHealth = EffectiveMaxHealth();

        FindAnyObjectByType<HealthTracker>().ReactivateAllHealthObjects();

        UpdateHealthUI();
        Time.timeScale = 1f;
        OnPlayerRespawned?.Invoke();
    }

    private void UpdateHealthUI()
    {
        float maxHP = EffectiveMaxHealth();
        currentHealth = Mathf.Min(currentHealth, maxHP);
        if (healthText) healthText.text = $"HP: {Mathf.Max(0f, currentHealth):0}/{maxHP:0}";
    }

    private void UpdateDeathUI()
    {
        if (deathText) deathText.text = $"Deaths: {deathCount}";
    }
}
