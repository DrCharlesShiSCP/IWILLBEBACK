using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionCanvas : MonoBehaviour
{
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private RollDiceManager manager;

    public Button[] OptionButtons => optionButtons;
    private void OnEnable()
    {
        manager = Object.FindAnyObjectByType<RollDiceManager>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(HideSelf);
        }
        if (manager != null) manager.OnSelectionCanvasShown(this);
    }

    private void OnDisable()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            btn.onClick.RemoveListener(HideSelf);
        }
    }

    public void HideSelf()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
