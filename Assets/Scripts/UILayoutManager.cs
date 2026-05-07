using UnityEngine;
using TMPro;

/// <summary>
/// Manages 3 UI layout modes for displaying trivia questions.
/// Switch modes at runtime to compare eye-tracking ergonomics.
/// 
/// Mode 1 - Static HUD:     Question bar fixed at top of screen (standard)
/// Mode 2 - Floating HUD:   Question text follows the player character
/// Mode 3 - Diegetic:       Question text rendered on the arena floor
/// </summary>
public class UILayoutManager : MonoBehaviour
{
    public enum UILayoutMode
    {
        StaticHUD,
        FloatingHUD,
        Diegetic
    }

    [Header("Current Mode")]
    public UILayoutMode currentMode = UILayoutMode.StaticHUD;

    [Header("References")]
    public Transform player;

    [Header("Static HUD Elements (Canvas - Screen Space)")]
    public GameObject staticHUDPanel;           // Panel at top of screen
    public TextMeshProUGUI staticQuestionText;  // TMP text in the panel

    [Header("Floating HUD Elements (World Space)")]
    public GameObject floatingHUDObject;        // World-space object that follows player
    public TextMeshPro floatingQuestionText;    // TMP text on the floating object
    public Vector2 floatingOffset = new Vector2(0f, 2f); // offset from player

    [Header("Diegetic Elements (World Space)")]
    public GameObject diegeticObject;           // Object placed on the arena floor
    public TextMeshPro diegeticQuestionText;    // TMP text on the floor object
    public Vector3 diegeticPosition = Vector3.zero; // center of arena

    // ─── Events ───
    public event System.Action<UILayoutMode> OnLayoutModeChanged;

    private string currentQuestion = "";

    void Start()
    {
        ApplyMode();
    }

    void Update()
    {
        // Floating HUD follows the player
        if (currentMode == UILayoutMode.FloatingHUD && floatingHUDObject != null && player != null)
        {
            floatingHUDObject.transform.position = (Vector2)player.position + floatingOffset;
        }
    }

    /// <summary>
    /// Set the question text across all layout modes.
    /// </summary>
    public void SetQuestion(string question)
    {
        currentQuestion = question;

        if (staticQuestionText != null) staticQuestionText.text = question;
        if (floatingQuestionText != null) floatingQuestionText.text = question;
        if (diegeticQuestionText != null) diegeticQuestionText.text = question;
    }

    /// <summary>
    /// Switch to a different UI layout mode.
    /// </summary>
    public void SetLayoutMode(UILayoutMode mode)
    {
        currentMode = mode;
        ApplyMode();
        OnLayoutModeChanged?.Invoke(mode);
    }

    /// <summary>
    /// Enable the active mode's objects, disable all others.
    /// </summary>
    void ApplyMode()
    {
        // Disable all
        if (staticHUDPanel != null) staticHUDPanel.SetActive(false);
        if (floatingHUDObject != null) floatingHUDObject.SetActive(false);
        if (diegeticObject != null) diegeticObject.SetActive(false);

        // Enable active mode
        switch (currentMode)
        {
            case UILayoutMode.StaticHUD:
                if (staticHUDPanel != null) staticHUDPanel.SetActive(true);
                break;

            case UILayoutMode.FloatingHUD:
                if (floatingHUDObject != null) floatingHUDObject.SetActive(true);
                break;

            case UILayoutMode.Diegetic:
                if (diegeticObject != null)
                {
                    diegeticObject.SetActive(true);
                    diegeticObject.transform.position = diegeticPosition;
                }
                break;
        }

        // Re-apply current question text
        SetQuestion(currentQuestion);
    }

    /// <summary>
    /// Get a human-readable label for the current mode.
    /// </summary>
    public string GetModeLabel()
    {
        switch (currentMode)
        {
            case UILayoutMode.StaticHUD: return "Static HUD (Top Bar)";
            case UILayoutMode.FloatingHUD: return "Floating HUD (Follow Player)";
            case UILayoutMode.Diegetic: return "Diegetic (On Floor)";
            default: return currentMode.ToString();
        }
    }
}
