using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// In-game UI that lets the tester switch between:
///   - UI Layout modes (Static / Floating)
/// Movement is always Dash. Supports both keyboard shortcuts and clickable UI buttons.
/// Finds buttons by name at runtime to avoid serialization issues.
/// </summary>
public class ModeSwitcher : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public UILayoutManager uiLayoutManager;

    [Header("UI Text Elements")]
    public TextMeshProUGUI uiModeLabel;

    [Header("UI Buttons (assigned at runtime)")]
    public Button resetButton;

    // Colors for active/inactive buttons
    private Color activeColor = new Color(0.3f, 0.8f, 0.4f);
    private Button staticBtn;
    private Button floatingBtn;

    void Start()
    {
        // Force Dash mode on start
        if (playerController != null)
            playerController.SetMovementMode(PlayerController.MovementMode.Dash);

        // Find buttons by name at runtime (serialized refs don't persist)
        GameObject staticObj = GameObject.Find("UIBtn_0");
        GameObject floatingObj = GameObject.Find("UIBtn_1");
        GameObject resetObj = GameObject.Find("ResetBtn");

        if (staticObj != null)
        {
            staticBtn = staticObj.GetComponent<Button>();
            if (staticBtn != null) staticBtn.onClick.AddListener(OnClickStaticHUD);
        }
        if (floatingObj != null)
        {
            floatingBtn = floatingObj.GetComponent<Button>();
            if (floatingBtn != null) floatingBtn.onClick.AddListener(OnClickFloatingHUD);
        }
        if (resetObj != null)
        {
            resetButton = resetObj.GetComponent<Button>();
            if (resetButton != null) resetButton.onClick.AddListener(OnClickReset);
        }

        UpdateLabels();
        UpdateButtonHighlights();
    }

    void Update()
    {
        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetUIMode(UILayoutManager.UILayoutMode.StaticHUD);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetUIMode(UILayoutManager.UILayoutMode.FloatingHUD);

        if (Input.GetKeyDown(KeyCode.R))
            ResetTest();
    }

    // ─── Public methods for UI buttons ───
    public void OnClickStaticHUD()   { SetUIMode(UILayoutManager.UILayoutMode.StaticHUD); }
    public void OnClickFloatingHUD() { SetUIMode(UILayoutManager.UILayoutMode.FloatingHUD); }
    public void OnClickReset()       { ResetTest(); }

    void SetUIMode(UILayoutManager.UILayoutMode mode)
    {
        if (uiLayoutManager != null)
        {
            uiLayoutManager.SetLayoutMode(mode);
            UpdateLabels();
            UpdateButtonHighlights();
        }
    }

    void ResetTest()
    {
        if (playerController != null)
            playerController.ResetPlayer();

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ResetGame();
    }

    void UpdateLabels()
    {
        if (uiModeLabel != null && uiLayoutManager != null)
            uiModeLabel.text = "UI: " + uiLayoutManager.GetModeLabel();
    }

    void UpdateButtonHighlights()
    {
        if (uiLayoutManager == null) return;

        int activeUI = (int)uiLayoutManager.currentMode;
        Button[] buttons = { staticBtn, floatingBtn };

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            bool isActive = (i == activeUI);
            ColorBlock cb = buttons[i].colors;
            cb.normalColor = isActive ? activeColor : Color.white;
            cb.highlightedColor = isActive ? activeColor : new Color(0.9f, 0.9f, 0.9f);
            buttons[i].colors = cb;

            TextMeshProUGUI txt = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
        }
    }
}
