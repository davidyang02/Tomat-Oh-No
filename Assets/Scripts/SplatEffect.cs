using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages tomato splat effects on the audience when the boss takes damage.
/// Attach to a persistent GameObject in the scene.
/// </summary>
public class SplatEffect : MonoBehaviour
{
    public static SplatEffect Instance { get; private set; }

    private Sprite splatSprite;
    private AudioClip splat01Clip;
    private AudioClip splat02Clip;
    private List<GameObject> persistentSplats = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        splatSprite = CreateSplatSprite();
        splat01Clip = Resources.Load<AudioClip>("splat01");
        splat02Clip = Resources.Load<AudioClip>("splat02");
        Debug.Log("SplatEffect initialized! splat01: " + (splat01Clip != null) + " splat02: " + (splat02Clip != null));
    }

    /// <summary>
    /// Clear all persistent audience splats.
    /// </summary>
    public void ClearSplats()
    {
        foreach (GameObject go in persistentSplats)
        {
            if (go != null) Destroy(go);
        }
        persistentSplats.Clear();
    }

    /// <summary>
    /// Call this to spawn a splat on a random audience member.
    /// </summary>
    public void SpawnSplat()
    {
        Debug.Log("SpawnSplat called!");

        // Find the Crowd parent
        GameObject crowdParent = GameObject.Find("Crowd");
        if (crowdParent == null)
        {
            Debug.LogWarning("Crowd not found!");
            return;
        }

        // Get all crowd member renderers
        SpriteRenderer[] members = crowdParent.GetComponentsInChildren<SpriteRenderer>();
        Debug.Log($"Found {members.Length} crowd renderers");
        if (members.Length <= 1) return; // only backdrop

        // Pick a random member (skip backdrop at index 0)
        int idx = Random.Range(1, members.Length);
        Vector3 pos = members[idx].transform.position;
        Debug.Log($"Splatting at position: {pos}");

        // Play audience splat sound
        if (splat01Clip != null) AudioSource.PlayClipAtPoint(splat01Clip, pos, 0.7f);

        StartCoroutine(AnimateSplat(pos));
    }

    IEnumerator AnimateSplat(Vector3 position)
    {
        GameObject splat = new GameObject("Splat");
        splat.transform.position = position;
        splat.transform.localScale = Vector3.zero;
        persistentSplats.Add(splat);

        SpriteRenderer sr = splat.AddComponent<SpriteRenderer>();
        sr.sprite = splatSprite;
        sr.color = new Color(0.95f, 0.3f, 0.1f, 0.5f); // translucent tomato orange
        sr.sortingOrder = 15; // definitely on top

        // Phase 1: Scale up quickly (splat!)
        float duration = 0.2f;
        float elapsed = 0f;
        float targetScale = 2.5f; // bigger splat

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = targetScale * (1f + 0.3f * Mathf.Sin(t * Mathf.PI));
            splat.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        splat.transform.localScale = new Vector3(targetScale, targetScale, 1f);

        // Splat persists — no fade out
    }

    /// <summary>
    /// Spawn a splat at a specific position (player hit). This one fades away.
    /// </summary>
    public void SpawnHitSplat(Vector3 position)
    {
        // Play player hit splat sound
        if (splat02Clip != null) AudioSource.PlayClipAtPoint(splat02Clip, position, 0.7f);

        StartCoroutine(AnimateHitSplat(position));
    }

    IEnumerator AnimateHitSplat(Vector3 position)
    {
        GameObject splat = new GameObject("HitSplat");
        splat.transform.position = position;
        splat.transform.localScale = Vector3.zero;

        SpriteRenderer sr = splat.AddComponent<SpriteRenderer>();
        sr.sprite = splatSprite;
        sr.color = new Color(0.95f, 0.3f, 0.1f, 0.5f);
        sr.sortingOrder = 15;

        // Scale up
        float duration = 0.15f;
        float elapsed = 0f;
        float targetScale = 0.8f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = targetScale * (1f + 0.3f * Mathf.Sin(t * Mathf.PI));
            splat.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        splat.transform.localScale = new Vector3(targetScale, targetScale, 1f);

        // Hold briefly
        yield return new WaitForSeconds(0.3f);

        // Fade out
        duration = 0.8f;
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            sr.color = new Color(0.95f, 0.3f, 0.1f, 0.5f * (1f - t));
            yield return null;
        }

        Destroy(splat);
    }

    Sprite CreateSplatSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color tomato = new Color(0.95f, 0.3f, 0.1f, 1f);
        Color tomatoDark = new Color(0.8f, 0.2f, 0.08f, 1f);
        Color pulp = new Color(1f, 0.55f, 0.3f, 1f);
        Color seed = new Color(0.95f, 0.85f, 0.4f, 1f);

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        float cx = size / 2f;
        float cy = size / 2f;
        float radius = size * 0.35f;

        Vector2[] seeds = {
            new Vector2(cx - 4, cy + 2), new Vector2(cx + 3, cy - 3),
            new Vector2(cx + 1, cy + 4), new Vector2(cx - 2, cy - 2),
            new Vector2(cx + 5, cy + 1),
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist < radius)
                {
                    if (dist < radius * 0.3f)
                        pixels[y * size + x] = pulp;
                    else if (dist < radius * 0.6f)
                        pixels[y * size + x] = tomatoDark;
                    else
                        pixels[y * size + x] = tomato;

                    foreach (Vector2 s in seeds)
                    {
                        float sdx = x - s.x;
                        float sdy = y - s.y;
                        if (sdx * sdx + sdy * sdy < 2.5f)
                            pixels[y * size + x] = seed;
                    }
                }
                else if (dist < radius * 1.5f)
                {
                    float angle = Mathf.Atan2(dy, dx);
                    float noise = Mathf.Sin(angle * 7f) * 0.3f + Mathf.Sin(angle * 11f) * 0.2f
                                + Mathf.Cos(angle * 5f) * 0.15f;
                    if (dist < radius * (1f + noise))
                        pixels[y * size + x] = tomato;
                }
            }
        }

        // Draw 6-point green star stem (calyx) in the center
        Color stemGreen = new Color(0.2f, 0.5f, 0.1f, 1f);
        Color stemDark = new Color(0.15f, 0.38f, 0.08f, 1f);
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI / 3f; // 60 degrees apart
            for (float t = 0; t < 5f; t += 0.5f)
            {
                int px = Mathf.RoundToInt(cx + Mathf.Cos(angle) * t);
                int py = Mathf.RoundToInt(cy + Mathf.Sin(angle) * t);
                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    pixels[py * size + px] = (t < 2f) ? stemDark : stemGreen;
                    // Thicken the stem lines
                    if (px + 1 < size) pixels[py * size + px + 1] = stemGreen;
                    if (py + 1 < size) pixels[(py + 1) * size + px] = stemGreen;
                }
            }
        }
        // Small dark center dot
        for (int dy2 = -1; dy2 <= 1; dy2++)
            for (int dx2 = -1; dx2 <= 1; dx2++)
                pixels[((int)cy + dy2) * size + (int)cx + dx2] = stemDark;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
