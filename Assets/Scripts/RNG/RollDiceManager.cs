using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class RollDiceManager : MonoBehaviour
{
    public GameObject SelectUI;
    [SerializeField] private OverfortGames.FirstPersonController.FirstPersonController fpc;

    public enum Ability { Run, Sprint, Climb, Slide, GrapplingHook, WallRun }

    //Master pool
    private static readonly Ability[] Pool = new[]
    {
        Ability.Run, Ability.Sprint, Ability.Climb, Ability.Slide, Ability.GrapplingHook, Ability.WallRun
    };

    //Called
    public void OnSelectionCanvasShown(SelectionCanvas canvas)
    {
        if (canvas == null) return;

        //choose 3
        var picks = PickThreeUnique(Pool);

        //apply
        var buttons = canvas.OptionButtons;
        for (int i = 0; i < buttons.Length; i++)
        {
            var btn = buttons[i];
            if (btn == null) continue;

            //clear
            btn.onClick.RemoveAllListeners();

            //set label
            var tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = ToLabel(picks[i]);
            else
            {
                var legacy = btn.GetComponentInChildren<Text>();
                if (legacy != null) legacy.text = ToLabel(picks[i]);
            }

            //capturevar 
            Ability choice = picks[i];

            //clicked
            btn.onClick.AddListener(() =>
            {
                EnableAbility(choice);
                canvas.HideSelf();
            });
        }
    }


    private static Ability[] PickThreeUnique(Ability[] source)
    {
        Ability[] arr = (Ability[])source.Clone();
        var rng = UnityEngine.Random.state;
        for (int i = 0; i < 3; i++)
        {
            int j = UnityEngine.Random.Range(i, arr.Length);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return new[] { arr[0], arr[1], arr[2] };
    }

    private static string ToLabel(Ability a) => a switch
    {
        Ability.GrapplingHook => "Grappling Hook",
        Ability.WallRun => "Wall Run",
        _ => a.ToString()
    };

    private void EnableAbility(Ability a)
    {
        if (fpc == null)
        {
            Debug.LogWarning("RollDiceManager: FirstPersonController reference not set.");
            return;
        }

        switch (a)
        {
            case Ability.Run:
                fpc.enableRun = true;
                break;
            case Ability.Sprint:
                fpc.enableTacticalSprint = true;
                break;
            case Ability.Climb:
                fpc.enableClimb = true;
                break;
            case Ability.Slide:
                fpc.enableSlide = true;
                break;
            case Ability.GrapplingHook:
                fpc.enableGrapplingHook = true;
                break;
            case Ability.WallRun:
                fpc.enableWallRun = true;
                break;
        }
    }

    public void ShowRollMenu()
    {
        SelectUI.SetActive(true);
    }
}
