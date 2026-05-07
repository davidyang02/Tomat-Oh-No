using UnityEngine;

/// <summary>
/// Individual bullet projectile. Moves in a direction and destroys itself off-screen.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 5f;
    public Vector2 direction = Vector2.down;

    private Camera mainCam;
    private float destroyMargin = 1f; // extra buffer beyond screen edge

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // Move in the assigned direction
        transform.Translate(direction.normalized * speed * Time.deltaTime);

        // Destroy if off-screen
        if (IsOffScreen())
        {
            Destroy(gameObject);
        }
    }

    bool IsOffScreen()
    {
        Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);
        return viewportPos.x < -destroyMargin || viewportPos.x > 1f + destroyMargin ||
               viewportPos.y < -destroyMargin || viewportPos.y > 1f + destroyMargin;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Notify the player controller of the hit
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage();
            }
            // Splat at player location (fades away)
            if (SplatEffect.Instance != null) SplatEffect.Instance.SpawnHitSplat(other.transform.position);
            Destroy(gameObject);
        }
    }
}
