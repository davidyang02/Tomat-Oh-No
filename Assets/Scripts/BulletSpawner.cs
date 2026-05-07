using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns projectiles in various patterns. Supports Columns, Diagonal, Spiral, and Random.
/// Also provides SpawnPunishmentBurst() for wrong-answer consequences.
/// </summary>
public class BulletSpawner : MonoBehaviour
{
    public enum SpawnPattern { Columns, Diagonal, Spiral, Random }

    [Header("Pattern Settings")]
    public int columns = 6;
    public float spawnInterval = 0.8f;
    public float bulletSpeed = 4f;
    public float arenaWidth = 16f;
    public SpawnPattern currentPattern = SpawnPattern.Columns;

    private int waveIndex = 0;
    private float spiralAngle = 0f;
    private static Sprite sharedBulletSprite;

    public void SetActive(bool active)
    {
        CancelInvoke();
        if (active)
        {
            InvokeRepeating("SpawnWave", 0.5f, spawnInterval);
        }
    }

    public void SetPattern(SpawnPattern pattern)
    {
        currentPattern = pattern;
        spiralAngle = 0f;
    }

    void SpawnWave()
    {
        switch (currentPattern)
        {
            case SpawnPattern.Columns:  SpawnColumns(); break;
            case SpawnPattern.Diagonal: SpawnDiagonal(); break;
            case SpawnPattern.Spiral:   SpawnSpiral(); break;
            case SpawnPattern.Random:   SpawnRandom(); break;
        }
        waveIndex++;
    }

    void SpawnColumns()
    {
        float halfWidth = arenaWidth / 2f;
        float spawnY = 6f;
        bool offset = (waveIndex % 2 == 1);
        float columnWidth = arenaWidth / columns;
        float startX = -halfWidth + columnWidth / 2f;
        if (offset) startX += columnWidth / 2f;
        int count = offset ? columns - 1 : columns;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * columnWidth;
            CreateBullet(new Vector3(x, spawnY, 0f), Vector2.down);
        }
    }

    void SpawnDiagonal()
    {
        float halfWidth = arenaWidth / 2f;
        float spawnY = 6f;
        int count = 4;
        bool fromLeft = (waveIndex % 2 == 0);
        Vector2 dir = fromLeft
            ? new Vector2(0.3f, -1f).normalized
            : new Vector2(-0.3f, -1f).normalized;

        for (int i = 0; i < count; i++)
        {
            float x = fromLeft
                ? -halfWidth + i * 3.5f
                : halfWidth - i * 3.5f;
            CreateBullet(new Vector3(x, spawnY, 0f), dir);
        }
    }

    void SpawnSpiral()
    {
        int bulletsPerWave = 3;
        float angleStep = 360f / bulletsPerWave;
        float warningTime = 0.6f;

        for (int i = 0; i < bulletsPerWave; i++)
        {
            float angle = spiralAngle + i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            // Show shadow warning, then spawn bullet after delay
            StartCoroutine(SpawnWithWarning(Vector3.zero, dir, warningTime));
        }
        spiralAngle += 25f;
    }

    IEnumerator SpawnWithWarning(Vector3 position, Vector2 direction, float delay)
    {
        // Create warning indicator (bright red so it's visible on dark arena)
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.position = position;
        shadow.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        SpriteRenderer sr = shadow.AddComponent<SpriteRenderer>();
        sr.sprite = GetBulletSprite();
        sr.color = new Color(1f, 0.3f, 0.1f, 0.3f); // red-orange warning
        sr.sortingOrder = 4;

        // Pulse: shrink and brighten over time
        float elapsed = 0f;
        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / delay;
            float alpha = Mathf.Lerp(0.2f, 0.7f, t);
            float scale = Mathf.Lerp(1.8f, 0.9f, t);
            if (sr != null) sr.color = new Color(1f, 0.3f, 0.1f, alpha);
            if (shadow != null) shadow.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        if (shadow != null) Destroy(shadow);
        CreateBullet(position, direction);
    }

    void SpawnRandom()
    {
        float halfWidth = arenaWidth / 2f;
        float spawnY = 6f;
        int count = UnityEngine.Random.Range(3, 7);

        for (int i = 0; i < count; i++)
        {
            float x = UnityEngine.Random.Range(-halfWidth, halfWidth);
            CreateBullet(new Vector3(x, spawnY, 0f), Vector2.down);
        }
    }

    /// <summary>
    /// Punishment burst: ring of tomatoes from center outward.
    /// Called when the player answers incorrectly.
    /// </summary>
    public void SpawnPunishmentBurst()
    {
        int count = 8;
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            CreateBullet(Vector3.zero, dir);
        }
    }

    void CreateBullet(Vector3 position, Vector2 direction)
    {
        GameObject go = new GameObject("Bullet");
        go.transform.position = position;
        go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetBulletSprite();
        if (sharedTomatoSprite != null)
            sr.color = Color.white;
        else
            sr.color = Color.red;
        sr.sortingOrder = 5;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        Bullet bullet = go.AddComponent<Bullet>();
        bullet.direction = direction;
        bullet.speed = bulletSpeed;
    }

    private static Sprite sharedTomatoSprite;
    private static bool tomatoLoaded = false;

    static Sprite GetBulletSprite()
    {
        if (!tomatoLoaded)
        {
            tomatoLoaded = true;
            Texture2D tex = Resources.Load<Texture2D>("tomato");
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                sharedTomatoSprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    Mathf.Max(tex.width, tex.height));
            }
        }

        if (sharedTomatoSprite != null)
            return sharedTomatoSprite;

        if (sharedBulletSprite == null)
        {
            Texture2D tex = new Texture2D(4, 4);
            Color[] px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            sharedBulletSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        }
        return sharedBulletSprite;
    }
}

