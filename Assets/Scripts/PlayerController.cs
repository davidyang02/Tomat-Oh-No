using UnityEngine;
using System;

/// <summary>
/// Player controller with 3 swappable movement modes:
///   - SnapToGrid: discrete tile-based movement
///   - Analog: smooth fluid movement (like Binding of Isaac)
///   - Dash: slow base speed + fast dash on Space
/// 
/// Switch modes at runtime via SetMovementMode() or the ModeSwitcher UI.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ─── Movement Mode Enum ───
    public enum MovementMode
    {
        SnapToGrid,
        Analog,
        Dash
    }

    [Header("Current Mode")]
    public MovementMode currentMode = MovementMode.Analog;

    [Header("Analog Settings")]
    public float analogSpeed = 6f;

    [Header("Snap-to-Grid Settings")]
    public float gridSize = 1f;
    public float snapCooldown = 0.15f; // seconds between grid moves

    [Header("Dash Settings")]
    public float dashBaseSpeed = 3f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    [Header("Health")]
    public int maxHP = 10;
    public int currentHP;
    public float invincibilityDuration = 2f;

    [Header("Arena Bounds (world units)")]
    public float arenaMinX = -8f;
    public float arenaMaxX = 8f;
    public float arenaMinY = -4f;
    public float arenaMaxY = 4f;

    // ─── Events ───
    public event Action<int> OnHPChanged;         // passes current HP
    public event Action OnPlayerDied;
    public event Action<MovementMode> OnModeChanged;

    // ─── Private state ───
    private float snapTimer;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;
    private bool isInvincible;
    private float invincibilityTimer;
    private SpriteRenderer spriteRenderer;
    private AudioClip dashClip;

    void Awake()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        dashClip = Resources.Load<AudioClip>("dash03");
    }

    void Start()
    {
        // Any additional start logic can go here
    }

    void Update()
    {
        // Invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            // Blink effect
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = Mathf.FloorToInt(invincibilityTimer * 10f) % 2 == 0;
            }
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null) spriteRenderer.enabled = true;
            }
        }

        // Movement based on current mode
        switch (currentMode)
        {
            case MovementMode.Analog:
                HandleAnalogMovement();
                break;
            case MovementMode.SnapToGrid:
                HandleSnapMovement();
                break;
            case MovementMode.Dash:
                HandleDashMovement();
                break;
        }

        // Clamp to arena
        ClampPosition();
    }

    // ─── ANALOG: Smooth fluid movement ───
    void HandleAnalogMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(h, v).normalized;
        transform.Translate(input * analogSpeed * Time.deltaTime);
    }

    // ─── SNAP-TO-GRID: Retro discrete movement ───
    void HandleSnapMovement()
    {
        snapTimer -= Time.deltaTime;
        if (snapTimer > 0f) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = Vector2.zero;
        // Prioritize horizontal, then vertical
        if (Mathf.Abs(h) > 0.1f)
            move = new Vector2(Mathf.Sign(h) * gridSize, 0f);
        else if (Mathf.Abs(v) > 0.1f)
            move = new Vector2(0f, Mathf.Sign(v) * gridSize);

        if (move != Vector2.zero)
        {
            transform.position += (Vector3)move;
            // Snap to grid center after moving
            SnapPositionToGrid();
            snapTimer = snapCooldown;
        }
    }

    /// <summary>
    /// Snaps the player position to the nearest grid cell center.
    /// Grid cells are centered at (0.5, 0.5), (1.5, 0.5), etc.
    /// </summary>
    void SnapPositionToGrid()
    {
        Vector3 pos = transform.position;
        // Offset by half gridSize so we snap to cell centers, not intersections
        pos.x = Mathf.Floor(pos.x / gridSize) * gridSize + gridSize * 0.5f;
        pos.y = Mathf.Floor(pos.y / gridSize) * gridSize + gridSize * 0.5f;
        transform.position = pos;
    }

    // ─── DASH: Slow base + fast burst ───
    void HandleDashMovement()
    {
        dashCooldownTimer -= Time.deltaTime;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(h, v).normalized;

        // Dash activation
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && input != Vector2.zero)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = input;
            dashCooldownTimer = dashCooldown;
            if (dashClip != null) AudioSource.PlayClipAtPoint(dashClip, transform.position, 0.6f);
        }

        // Dash movement
        if (isDashing)
        {
            transform.Translate(dashDirection * dashSpeed * Time.deltaTime);
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        else
        {
            // Slow base movement
            transform.Translate(input * dashBaseSpeed * Time.deltaTime);
        }
    }

    // ─── Clamp to arena bounds ───
    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, arenaMinX, arenaMaxX);
        pos.y = Mathf.Clamp(pos.y, arenaMinY, arenaMaxY);
        transform.position = pos;
    }

    // ─── Damage handling ───
    public void TakeDamage()
    {
        if (isInvincible) return;

        currentHP--;
        OnHPChanged?.Invoke(currentHP);

        if (currentHP <= 0)
        {
            OnPlayerDied?.Invoke();
            gameObject.SetActive(false);
        }
        else
        {
            // Start invincibility
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    public void Heal()
    {
        if (currentHP < maxHP)
        {
            currentHP++;
            OnHPChanged?.Invoke(currentHP);
        }
    }

    // ─── Mode switching (called by ModeSwitcher UI) ───
    public void SetMovementMode(MovementMode mode)
    {
        currentMode = mode;
        // Reset state when switching
        isDashing = false;
        dashTimer = 0f;
        snapTimer = 0f;

        // Snap position to grid when entering SnapToGrid mode
        if (mode == MovementMode.SnapToGrid)
        {
            SnapPositionToGrid();
        }

        OnModeChanged?.Invoke(mode);
    }

    public void ResetPlayer()
    {
        currentHP = maxHP;
        transform.position = Vector3.zero;
        gameObject.SetActive(true);
        isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        OnHPChanged?.Invoke(currentHP);
    }
}
