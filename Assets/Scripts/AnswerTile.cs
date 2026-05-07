using UnityEngine;
using TMPro;

/// <summary>
/// An answer tile in the arena. The player walks into it to select an answer.
/// Displays answer text via a child TextMeshPro component.
/// </summary>
public class AnswerTile : MonoBehaviour
{
    [Header("Settings")]
    public int answerIndex; // which answer this tile represents (0-3)
    public Color correctColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);
    public Color wrongColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);

    private Color defaultColor;
    private TextMeshPro label;
    private SpriteRenderer spriteRenderer;

    // Tile colors matching HUD option text colors
    private static readonly Color[] tileColors = {
        new Color(0.9f, 0.3f, 0.3f, 0.85f),  // A = Red
        new Color(0.3f, 0.5f, 0.9f, 0.85f),  // B = Blue
        new Color(0.3f, 0.8f, 0.3f, 0.85f),  // C = Green
        new Color(0.9f, 0.7f, 0.2f, 0.85f)   // D = Yellow
    };

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        label = GetComponentInChildren<TextMeshPro>();
        // Use white as default so custom tile sprites show their true colors
        defaultColor = Color.white;
        SetColor(defaultColor);
    }

    /// <summary>
    /// Set the display text for this answer tile.
    /// </summary>
    public void SetAnswerText(string text)
    {
        if (label != null)
        {
            label.text = text;
        }
    }

    /// <summary>
    /// Get the display text.
    /// </summary>
    public string GetAnswerText()
    {
        return label != null ? label.text : "";
    }

    private float triggerCooldown = 0f;

    void Update()
    {
        if (triggerCooldown > 0f) triggerCooldown -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerCooldown > 0f) return;
        if (other.CompareTag("Player"))
        {
            triggerCooldown = 2f; // prevent re-triggering for 2 seconds
            // Notify the GameManager that this answer was selected
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.OnAnswerSelected(answerIndex);
            }
        }
    }

    /// <summary>
    /// Flash the tile to show correct/wrong feedback.
    /// </summary>
    public void ShowCorrect()
    {
        SetColor(correctColor);
    }

    public void ShowWrong()
    {
        SetColor(wrongColor);
    }

    public void ResetColor()
    {
        SetColor(defaultColor);
    }

    void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}
