using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RollDiceManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject SelectUI;

    [Header("Controller")]
    [SerializeField] private OverfortGames.FirstPersonController.FirstPersonController fpc;

    [Header("Weapons & Manager")]
    [SerializeField] private WeaponManager weaponManager; // assign in Inspector

    // Assign these 4 WeaponData assets in Inspector (Rifle + 3 placeholders)
    [SerializeField] private WeaponData rifleData;
    [SerializeField] private WeaponData smgData;
    [SerializeField] private WeaponData revolverData;
    [SerializeField] private WeaponData dmrData;

    public event Action OnAbilityChosen;

    public enum Ability
    {
        // Movement (non-stackable)
        Run, Sprint, Climb, Slide, GrapplingHook, WallRun,

        // Stackable buffs (can be picked repeatedly)
        PowerSurge_DamageUp,        // +20% player damage (stacks)
        VitalBoost_MaxHPUp,         // +25% player max HP (stacks)
        SoftTargeting_EnemyDmgDown, // -20% enemy damage (stacks; optional)

        // Weapon unlocks (non-stackable)
        Weapon_Rifle,
        Weapon_PlaceholderA,
        Weapon_PlaceholderB,
        Weapon_PlaceholderC
    }

    // Non-stackables are removed from pool after selection
    private static readonly Ability[] NonStackableAbilities = new[]
    {
        Ability.Run, Ability.Sprint, Ability.Climb, Ability.Slide, Ability.GrapplingHook, Ability.WallRun,
        Ability.Weapon_Rifle, Ability.Weapon_PlaceholderA, Ability.Weapon_PlaceholderB, Ability.Weapon_PlaceholderC
    };

    // Buffs remain always available (stackable)
    private static readonly Ability[] StackableBuffs = new[]
    {
        Ability.PowerSurge_DamageUp,
        Ability.VitalBoost_MaxHPUp,
        Ability.SoftTargeting_EnemyDmgDown
    };

    private readonly HashSet<Ability> _unlocked = new HashSet<Ability>();

    void Start()
    {
        if (!weaponManager) weaponManager = FindObjectOfType<WeaponManager>();
        SyncUnlockedFromFpc();
    }

    private void SyncUnlockedFromFpc()
    {
        if (!fpc) return;

        if (fpc.enableRun) _unlocked.Add(Ability.Run);
        if (fpc.enableTacticalSprint) _unlocked.Add(Ability.Sprint);
        if (fpc.enableClimb) _unlocked.Add(Ability.Climb);
        if (fpc.enableSlide) _unlocked.Add(Ability.Slide);
        if (fpc.enableGrapplingHook) _unlocked.Add(Ability.GrapplingHook);
        if (fpc.enableWallRun) _unlocked.Add(Ability.WallRun);
        // weapon unlocks start as locked; pistol is default via WeaponManager
    }

    // Called by SelectionCanvas.OnEnable()
    public void OnSelectionCanvasShown(SelectionCanvas canvas)
    {
        if (!canvas) return;

        // Pool = non-stackables not yet unlocked + all stackable buffs (always)
        List<Ability> available = NonStackableAbilities.Where(a => !_unlocked.Contains(a)).ToList();
        available.AddRange(StackableBuffs);

        Ability[] picks = PickUnique(available, 3);

        var buttons = canvas.OptionButtons;
        int count = (buttons != null) ? buttons.Length : 0;

        for (int i = 0; i < count; i++)
        {
            var btn = buttons[i];
            if (!btn) continue;

            btn.onClick.RemoveAllListeners();
            btn.interactable = i < picks.Length;

            string label = (i < picks.Length) ? ToLabel(picks[i]) : "No More Choices";

            var tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp) tmp.text = label;
            else
            {
                var legacy = btn.GetComponentInChildren<Text>();
                if (legacy) legacy.text = label;
            }

            if (i < picks.Length)
            {
                Ability choice = picks[i];
                btn.onClick.AddListener(() =>
                {
                    EnableAbility(choice);

                    // Force HUDs that listen to Balance to repaint immediately.
                    if (Balance.Instance) Balance.Instance.MarkDirty();

                    OnAbilityChosen?.Invoke();
                    canvas.HideSelf();
                });
            }
        }
    }

    private static Ability[] PickUnique(List<Ability> pool, int take)
    {
        if (pool == null || pool.Count == 0) return Array.Empty<Ability>();
        int n = Mathf.Min(take, pool.Count);
        for (int i = 0; i < n; i++)
        {
            int j = UnityEngine.Random.Range(i, pool.Count);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool.Take(n).ToArray();
    }

    private static string ToLabel(Ability a) => a switch
    {
        // Movement
        Ability.Run => "Run",
        Ability.Sprint => "Sprint",
        Ability.Climb => "Climb",
        Ability.Slide => "Slide",
        Ability.GrapplingHook => "Grappling Hook",
        Ability.WallRun => "Wall Run",

        // Buffs (stackable)
        Ability.PowerSurge_DamageUp => "+20% DMG",
        Ability.VitalBoost_MaxHPUp => "+25% Max HP",
        Ability.SoftTargeting_EnemyDmgDown => "Enemies -20% DMG",

        // Weapons
        Ability.Weapon_Rifle => "Rifle",
        Ability.Weapon_PlaceholderA => "Prototype A",
        Ability.Weapon_PlaceholderB => "Prototype B",
        Ability.Weapon_PlaceholderC => "Prototype C",

        _ => a.ToString()
    };

    private void EnableAbility(Ability a)
    {
        // Movement unlocks (one-time)
        if (a is Ability.Run or Ability.Sprint or Ability.Climb or Ability.Slide or Ability.GrapplingHook or Ability.WallRun)
        {
            if (!fpc) { Debug.LogWarning("RollDiceManager: FPC not set for movement unlock."); return; }

            switch (a)
            {
                case Ability.Run: fpc.enableRun = true; break;
                case Ability.Sprint: fpc.enableTacticalSprint = true; break;
                case Ability.Climb: fpc.enableClimb = true; break;
                case Ability.Slide: fpc.enableSlide = true; break;
                case Ability.GrapplingHook: fpc.enableGrapplingHook = true; break;
                case Ability.WallRun: fpc.enableWallRun = true; break;
            }
            _unlocked.Add(a);           // mark non-stackable as consumed
            return;
        }

        // Stackable buffs (do NOT add to _unlocked)
        switch (a)
        {
            case Ability.PowerSurge_DamageUp:
                if (Balance.Instance) Balance.Instance.AddPlayerDamagePercent(20f);
                return;

            case Ability.VitalBoost_MaxHPUp:
                if (Balance.Instance) Balance.Instance.AddPlayerMaxHealthPercent(25f);
                return;

            case Ability.SoftTargeting_EnemyDmgDown:
                if (Balance.Instance) Balance.Instance.AddEnemyDamagePercent(-20f);
                return;
        }

        // Weapon unlocks (one-time)
        if (a is Ability.Weapon_Rifle or Ability.Weapon_PlaceholderA or Ability.Weapon_PlaceholderB or Ability.Weapon_PlaceholderC)
        {
            if (!weaponManager)
            {
                Debug.LogWarning("RollDiceManager: WeaponManager not assigned.");
                return;
            }

            switch (a)
            {
                case Ability.Weapon_Rifle:
                    if (rifleData) weaponManager.AddWeapon(rifleData, equipNow: true);
                    break;
                case Ability.Weapon_PlaceholderA:
                    if (smgData) weaponManager.AddWeapon(smgData, equipNow: true);
                    break;
                case Ability.Weapon_PlaceholderB:
                    if (revolverData) weaponManager.AddWeapon(revolverData, equipNow: true);
                    break;
                case Ability.Weapon_PlaceholderC:
                    if (dmrData) weaponManager.AddWeapon(dmrData, equipNow: true);
                    break;
            }
            _unlocked.Add(a);
        }
    }

    public void ShowRollMenu()
    {
        if (SelectUI) SelectUI.SetActive(true);
        else Debug.LogWarning("RollDiceManager: SelectUI not assigned.");
    }
}
