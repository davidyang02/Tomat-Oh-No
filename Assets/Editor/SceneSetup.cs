using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor script that auto-builds the entire Bullet Hell Trivia test scene.
/// Run from: Unity menu bar → Tools → Setup Bullet Hell Trivia Scene
/// </summary>
public class SceneSetup : EditorWindow
{
    [MenuItem("Tools/Setup Bullet Hell Trivia Scene")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Scene",
            "This will create all GameObjects for the Bullet Hell Trivia prototype.\n\nContinue?",
            "Yes", "Cancel"))
        {
            return;
        }

        // ═══════════════════════════════════════════
        // 0. CLEANUP — Remove old objects if re-running
        // ═══════════════════════════════════════════
        string[] objectsToClean = {
            "Arena", "Grid", "Player", "BulletSpawner", "Canvas",
            "FloatingHUD", "DiegeticText", "──── Managers ────",
            "Tile_A", "Tile_B", "Tile_C", "Tile_D", "ButtonPanel",
            "OptionsPanel", "EventSystem", "StartPanel",
            "Curtain", "Crowd", "VictoryPanel", "CurtainLeft", "CurtainRight",
            "SideFillLeft", "SideFillRight", "StageLip", "StageHighlight", "StageShadow",
            "LeftSidePanel", "RightSidePanel"
        };
        foreach (string objName in objectsToClean)
        {
            GameObject existing = GameObject.Find(objName);
            while (existing != null)
            {
                DestroyImmediate(existing);
                existing = GameObject.Find(objName);
            }
        }
        // Also clean up any leftover bullet prefab instances
        Bullet[] leftoverBullets = Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (Bullet b in leftoverBullets) DestroyImmediate(b.gameObject);

        Debug.Log("🧹 Cleaned up old objects.");

        // ═══════════════════════════════════════════
        // 1. CAMERA (create if missing, ALWAYS set orthographic)
        // ═══════════════════════════════════════════
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        // ALWAYS force orthographic for 2D game
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.3f, 0.04f, 0.06f); // dark curtain red
        cam.orthographicSize = 8f;
        cam.transform.position = new Vector3(0, 1.5f, -10);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        // Full-screen background behind everything
        GameObject bgQuad = GameObject.Find("ScreenBG");
        if (bgQuad != null) DestroyImmediate(bgQuad);
        bgQuad = new GameObject("ScreenBG");
        bgQuad.transform.position = new Vector3(0f, 0f, 5f); // behind everything
        bgQuad.transform.localScale = new Vector3(40f, 40f, 1f);
        SpriteRenderer bgSr = bgQuad.AddComponent<SpriteRenderer>();
        bgSr.sprite = MakeWhiteSprite();
        bgSr.color = new Color(0.3f, 0.04f, 0.06f, 1f); // dark curtain red
        bgSr.sortingOrder = -10;

        // ═══════════════════════════════════════════
        // 2. ARENA BACKGROUND + CURTAIN + CROWD
        // ═══════════════════════════════════════════
        GameObject arena = CreateSprite("Arena", Vector3.zero, new Vector3(30f, 20f, 1f),
            new Color(0.451f, 0.09f, 0.176f, 1f), sortingOrder: -10); // #73172d


        // ─── Stage Curtains (separate left and right PNGs) ───
        Sprite leftCurtainSprite = LoadSprite("left curtain");
        Sprite rightCurtainSprite = LoadSprite("right curtain");

        if (leftCurtainSprite != null)
        {
            float sw = leftCurtainSprite.bounds.size.x;
            float sh = leftCurtainSprite.bounds.size.y;
            // Scale to fill the height of the screen and be ~3-4 units wide
            float scaleY = 18f / sh;
            float scaleX = scaleY; // maintain aspect ratio

            GameObject leftCurtain = new GameObject("CurtainLeft");
            leftCurtain.transform.position = new Vector3(-8f, 0f, 0f);
            leftCurtain.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            SpriteRenderer lsr = leftCurtain.AddComponent<SpriteRenderer>();
            lsr.sprite = leftCurtainSprite;
            lsr.color = Color.white;
            lsr.sortingOrder = 12;
        }

        if (rightCurtainSprite != null)
        {
            float sw = rightCurtainSprite.bounds.size.x;
            float sh = rightCurtainSprite.bounds.size.y;
            float scaleY = 18f / sh;
            float scaleX = scaleY;

            GameObject rightCurtain = new GameObject("CurtainRight");
            rightCurtain.transform.position = new Vector3(8f, 0f, 0f);
            rightCurtain.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            SpriteRenderer rsr = rightCurtain.AddComponent<SpriteRenderer>();
            rsr.sprite = rightCurtainSprite;
            rsr.color = Color.white;
            rsr.sortingOrder = 12;
        }

        // ─── Side Panels (tightly overlapping curtain strips) ───
        if (leftCurtainSprite != null)
        {
            float sh = leftCurtainSprite.bounds.size.y;
            float sw = leftCurtainSprite.bounds.size.x;
            float scaleY = 18f / sh;
            float stripWidth = sw * scaleY;
            float overlap = stripWidth * 0.25f; // very tight overlap

            for (float x = -8f; x > -20f; x -= overlap)
            {
                GameObject strip = new GameObject("SideFillLeft");
                strip.transform.position = new Vector3(x, 0f, 0f);
                strip.transform.localScale = new Vector3(scaleY, scaleY, 1f);
                SpriteRenderer ssr = strip.AddComponent<SpriteRenderer>();
                ssr.sprite = leftCurtainSprite;
                ssr.color = Color.white;
                ssr.sortingOrder = 11;
            }
        }

        if (rightCurtainSprite != null)
        {
            float sh = rightCurtainSprite.bounds.size.y;
            float sw = rightCurtainSprite.bounds.size.x;
            float scaleY = 18f / sh;
            float stripWidth = sw * scaleY;
            float overlap = stripWidth * 0.25f;

            for (float x = 8f; x < 20f; x += overlap)
            {
                GameObject strip = new GameObject("SideFillRight");
                strip.transform.position = new Vector3(x, 0f, 0f);
                strip.transform.localScale = new Vector3(scaleY, scaleY, 1f);
                SpriteRenderer ssr = strip.AddComponent<SpriteRenderer>();
                ssr.sprite = rightCurtainSprite;
                ssr.color = Color.white;
                ssr.sortingOrder = 11;
            }
        }

        // ─── Crowd Audience (tiled at top of screen) ───
        Sprite crowdSprite = LoadSprite("crowd");
        if (crowdSprite != null)
        {
            float crowdW = crowdSprite.bounds.size.x;
            float crowdH = crowdSprite.bounds.size.y;
            float crowdScale = 1.4f;
            float scaledW = crowdW * crowdScale;
            float scaledH = crowdH * crowdScale;

            int rows = 3;
            float startY = 5.6f;
            float startX = -7.5f;
            float endX = 7.5f;

            GameObject crowdParent = new GameObject("Crowd");
            crowdParent.AddComponent<SplatEffect>();

            // Dark backdrop behind the crowd (near black)
            GameObject backdrop = new GameObject("CrowdBackdrop");
            backdrop.transform.SetParent(crowdParent.transform);
            backdrop.transform.position = new Vector3(0f, 6.5f, 0f);
            backdrop.transform.localScale = new Vector3(16f, 3.5f, 1f);
            SpriteRenderer bsr = backdrop.AddComponent<SpriteRenderer>();
            bsr.sprite = MakeWhiteSprite();
            bsr.color = new Color(0.12f, 0.1f, 0.1f, 1f); // very dark, near black
            bsr.sortingOrder = 6;

            // Each row gets smaller (perspective) and slightly darker
            float[] rowScales = { 1.0f, 0.85f, 0.7f };
            Color[] rowTints = {
                new Color(0.7f, 0.6f, 0.55f, 1f),
                new Color(0.5f, 0.43f, 0.4f, 1f),
                new Color(0.35f, 0.3f, 0.28f, 1f)
            };

            for (int row = 0; row < rows; row++)
            {
                float rScale = crowdScale * rowScales[row];
                float rScaledW = crowdW * rScale;
                float y = startY + row * (scaledH * 0.45f); // tight packing
                float x = startX;
                if (row % 2 == 1) x += rScaledW * 0.4f;

                while (x < endX)
                {
                    GameObject member = new GameObject("CrowdMember");
                    member.transform.SetParent(crowdParent.transform);
                    member.transform.position = new Vector3(x, y, 0f);
                    member.transform.localScale = new Vector3(rScale, rScale, 1f);
                    SpriteRenderer msr = member.AddComponent<SpriteRenderer>();
                    msr.sprite = crowdSprite;
                    msr.color = rowTints[row];
                    msr.sortingOrder = 9 - row; // all above backdrop (6)
                    x += rScaledW * 0.85f;
                }
            }
        }

        // ═══════════════════════════════════════════
        // 2.5 TILED GRID FLOOR (using tile.png)
        // ═══════════════════════════════════════════
        Sprite tileGridSprite = LoadSprite("tile");
        GameObject gridObj = new GameObject("Grid");
        if (tileGridSprite != null)
        {
            int gridW = 16;
            int gridH = 10;
            for (int x = 0; x < gridW; x++)
            {
                for (int y = 0; y < gridH; y++)
                {
                    float px = -gridW / 2f + x + 0.5f;
                    float py = -gridH / 2f + y + 0.5f;
                    GameObject cell = CreateSpriteWithTexture(
                        "GridCell", new Vector3(px, py, 0),
                        new Vector3(1f, 1f, 1f), tileGridSprite, sortingOrder: -8);
                    cell.transform.SetParent(gridObj.transform);
                }
            }
        }
        else
        {
            // Fallback: line grid
            gridObj.AddComponent<LineRenderer>();
            GridRenderer grid = gridObj.AddComponent<GridRenderer>();
            grid.gridSize = 1f;
            grid.gridWidth = 16;
            grid.gridHeight = 10;
            grid.gridColor = new Color(0.3f, 0.3f, 0.5f, 0.25f);
            grid.lineWidth = 0.02f;
        }

        // ═══════════════════════════════════════════
        // 3. PLAYER
        // ═══════════════════════════════════════════
        // Load crowdhat sprite as player character, otherwise fallback to green square
        Sprite playerSprite = LoadSprite("crowdhat");
        GameObject player;
        if (playerSprite != null)
        {
            player = CreateSpriteWithTexture("Player", Vector3.zero,
                new Vector3(1.2f, 1.2f, 1f), playerSprite, sortingOrder: 5);
        }
        else
        {
            player = CreateSprite("Player", Vector3.zero, new Vector3(0.8f, 0.8f, 1f),
                new Color(0.1f, 1f, 0.3f, 1f), sortingOrder: 5);
        }
        player.tag = "Player";

        BoxCollider2D playerCol = player.AddComponent<BoxCollider2D>();
        playerCol.isTrigger = true;

        Rigidbody2D playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        PlayerController pc = player.AddComponent<PlayerController>();
        // Set bounds to match the grid (16 wide × 10 tall, centered)
        pc.arenaMinX = -8f;
        pc.arenaMaxX = 8f;
        pc.arenaMinY = -5f;
        pc.arenaMaxY = 5f;
        pc.gridSize = 1f; // match the visual grid

        // ═══════════════════════════════════════════
        // 4. BULLET SPAWNER (creates bullets from code, no prefab needed)
        // ═══════════════════════════════════════════
        GameObject spawnerObj = new GameObject("BulletSpawner");
        spawnerObj.transform.position = Vector3.zero;
        BulletSpawner spawner = spawnerObj.AddComponent<BulletSpawner>();
        spawner.columns = 6;
        spawner.spawnInterval = 0.8f;
        spawner.bulletSpeed = 4f;
        spawner.arenaWidth = 16f;

        // ═══════════════════════════════════════════
        // 6. ANSWER TILES (4 corners)
        // ═══════════════════════════════════════════
        Vector3[] tilePositions = new Vector3[]
        {
            new Vector3(-5f, 3f, 0f),   // A - top left
            new Vector3(5f, 3f, 0f),    // B - top right
            new Vector3(-5f, -3f, 0f),  // C - bottom left
            new Vector3(5f, -3f, 0f)    // D - bottom right
        };
        string[] tileLabels = { "A", "B", "C", "D" };
        string[] tileSpriteNames = { "tileA", "tileB", "tileC", "tileD" };
        Color[] tileColors = {
            new Color(0.3f, 0.7f, 0.3f, 0.85f),  // A = Green (matches tileA)
            new Color(0.7f, 0.2f, 0.2f, 0.85f),  // B = Red/Maroon (matches tileB)
            new Color(0.3f, 0.5f, 0.9f, 0.85f),  // C = Blue (matches tileC)
            new Color(0.9f, 0.8f, 0.2f, 0.85f)   // D = Yellow (matches tileD)
        };
        AnswerTile[] tiles = new AnswerTile[4];

        for (int i = 0; i < 4; i++)
        {
            Sprite tileSprite = LoadSprite(tileSpriteNames[i]);
            GameObject tile;
            if (tileSprite != null)
            {
                tile = CreateSpriteWithTexture("Tile_" + tileLabels[i], tilePositions[i],
                    new Vector3(2f, 2f, 1f), tileSprite, sortingOrder: 1);
            }
            else
            {
                tile = CreateSprite("Tile_" + tileLabels[i], tilePositions[i],
                    new Vector3(1.8f, 1.8f, 1f), tileColors[i], sortingOrder: 1);
            }

            BoxCollider2D tileCol = tile.AddComponent<BoxCollider2D>();
            tileCol.isTrigger = true;

            AnswerTile at = tile.AddComponent<AnswerTile>();
            at.answerIndex = i;
            tiles[i] = at;
        }

        // ═══════════════════════════════════════════
        // 7. UI CANVAS (Screen-Space)
        // ═══════════════════════════════════════════
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem is REQUIRED for button clicks
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var scaler = canvasObj.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // ─── Static HUD Panel (top bar — question + answer options) ───
        GameObject staticPanel = new GameObject("StaticHUDPanel");
        staticPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform spRT = staticPanel.AddComponent<RectTransform>();
        spRT.anchorMin = new Vector2(0, 1);
        spRT.anchorMax = new Vector2(1, 1);
        spRT.pivot = new Vector2(0.5f, 1f);
        spRT.anchoredPosition = new Vector2(0, 0);
        spRT.sizeDelta = new Vector2(0, 70);
        spRT.offsetMin = new Vector2(0, spRT.offsetMin.y);
        spRT.offsetMax = new Vector2(0, spRT.offsetMax.y);
        UnityEngine.UI.Image spImg = staticPanel.AddComponent<UnityEngine.UI.Image>();
        spImg.color = new Color(0f, 0f, 0f, 0.85f);

        // Question text (fills the static panel)
        TextMeshProUGUI staticQText = CreateUIText(staticPanel.transform, "StaticQuestionText",
            "Question goes here", 28, TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(0, 60));
        staticQText.fontStyle = FontStyles.Bold;
        RectTransform sqRT = staticQText.rectTransform;
        sqRT.anchorMin = new Vector2(0.02f, 0f);
        sqRT.anchorMax = new Vector2(0.98f, 1f);
        sqRT.offsetMin = Vector2.zero;
        sqRT.offsetMax = Vector2.zero;

        // ─── Answer Options Panel (ALWAYS VISIBLE, sits below question bar) ───
        GameObject optionsPanel = new GameObject("OptionsPanel");
        optionsPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform opRT = optionsPanel.AddComponent<RectTransform>();
        opRT.anchorMin = new Vector2(0, 1);
        opRT.anchorMax = new Vector2(1, 1);
        opRT.pivot = new Vector2(0.5f, 1f);
        opRT.anchoredPosition = new Vector2(0, -80); // right below the question bar
        opRT.sizeDelta = new Vector2(0, 60);
        opRT.offsetMin = new Vector2(0, opRT.offsetMin.y);
        opRT.offsetMax = new Vector2(0, opRT.offsetMax.y);
        UnityEngine.UI.Image opImg = optionsPanel.AddComponent<UnityEngine.UI.Image>();
        opImg.color = new Color(0f, 0f, 0f, 0.7f);

        // Answer options A-D (children of OptionsPanel, always visible)
        Color[] optionColors = {
            new Color(0.3f, 0.7f, 0.3f),  // A = Green (matches tileA)
            new Color(0.7f, 0.2f, 0.2f),  // B = Red/Maroon (matches tileB)
            new Color(0.3f, 0.5f, 0.9f),  // C = Blue (matches tileC)
            new Color(0.9f, 0.8f, 0.2f)   // D = Yellow (matches tileD)
        };
        string[] optLabels = { "A", "B", "C", "D" };
        TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            float xMin = i * 0.25f;
            float xMax = (i + 1) * 0.25f;

            TextMeshProUGUI optText = CreateUIText(optionsPanel.transform,
                "OptionText_" + optLabels[i],
                optLabels[i] + ") Answer", 20, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.zero);
            optText.fontStyle = FontStyles.Bold;
            RectTransform oRT = optText.rectTransform;
            oRT.anchorMin = new Vector2(xMin + 0.01f, 0f);
            oRT.anchorMax = new Vector2(xMax - 0.01f, 1f);
            oRT.offsetMin = Vector2.zero;
            oRT.offsetMax = Vector2.zero;
            optText.color = optionColors[i];
            optionTexts[i] = optText;
        }

        // ─── Boss HP Bar (top right, replaces score) ───
        GameObject bossHPPanel = new GameObject("BossHPPanel");
        bossHPPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform bhpRT = bossHPPanel.AddComponent<RectTransform>();
        bhpRT.anchorMin = new Vector2(1, 1);
        bhpRT.anchorMax = new Vector2(1, 1);
        bhpRT.pivot = new Vector2(1, 1);
        bhpRT.anchoredPosition = new Vector2(-20, -115);
        bhpRT.sizeDelta = new Vector2(250, 50);

        // Boss HP label
        TextMeshProUGUI bossHPLabel = CreateUIText(bossHPPanel.transform, "BossHPLabel",
            "Boss: 10/10", 18, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.zero);
        bossHPLabel.fontStyle = FontStyles.Bold;
        RectTransform blRT = bossHPLabel.rectTransform;
        blRT.anchorMin = new Vector2(0, 0.6f);
        blRT.anchorMax = new Vector2(1, 1f);
        blRT.offsetMin = Vector2.zero;
        blRT.offsetMax = Vector2.zero;

        // HP bar background
        GameObject barBg = new GameObject("BossHPBarBg");
        barBg.transform.SetParent(bossHPPanel.transform, false);
        RectTransform bgRT = barBg.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0);
        bgRT.anchorMax = new Vector2(1, 0.55f);
        bgRT.offsetMin = new Vector2(5, 2);
        bgRT.offsetMax = new Vector2(-5, -2);
        UnityEngine.UI.Image bgImg = barBg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // HP bar fill
        GameObject barFill = new GameObject("BossHPBarFill");
        barFill.transform.SetParent(barBg.transform, false);
        RectTransform fillRT = barFill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        UnityEngine.UI.Image fillImg = barFill.AddComponent<UnityEngine.UI.Image>();
        fillImg.color = new Color(0.9f, 0.2f, 0.2f, 1f);
        fillImg.type = UnityEngine.UI.Image.Type.Filled;
        fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;

        // ─── Timer Text (top center, below options bar — prominent) ───
        GameObject timerPanel = new GameObject("TimerPanel");
        timerPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform tpRT = timerPanel.AddComponent<RectTransform>();
        tpRT.anchorMin = new Vector2(0.5f, 1);
        tpRT.anchorMax = new Vector2(0.5f, 1);
        tpRT.pivot = new Vector2(0.5f, 1);
        tpRT.anchoredPosition = new Vector2(0, -110);
        tpRT.sizeDelta = new Vector2(140, 50);
        UnityEngine.UI.Image tpBg = timerPanel.AddComponent<UnityEngine.UI.Image>();
        tpBg.color = new Color(0f, 0f, 0f, 0.7f);

        TextMeshProUGUI timerText = CreateUIText(timerPanel.transform, "TimerText",
            "12s", 36, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.zero);
        timerText.fontStyle = FontStyles.Bold;
        RectTransform ttRT = timerText.rectTransform;
        ttRT.anchorMin = Vector2.zero;
        ttRT.anchorMax = Vector2.one;
        ttRT.offsetMin = Vector2.zero;
        ttRT.offsetMax = Vector2.zero;

        // ─── HP Text (top left) ───
        TextMeshProUGUI hpText = CreateUIText(canvasObj.transform, "HPText",
            "♥ ♥ ♥ ♥ ♥", 32, TextAlignmentOptions.TopLeft,
            new Vector2(20, -115), new Vector2(300, 50));
        hpText.rectTransform.anchorMin = new Vector2(0, 1);
        hpText.rectTransform.anchorMax = new Vector2(0, 1);
        hpText.rectTransform.pivot = new Vector2(0, 1);
        hpText.color = new Color(1f, 0.3f, 0.3f);

        // ─── Feedback Text (center, big) ───
        TextMeshProUGUI feedbackText = CreateUIText(canvasObj.transform, "FeedbackText",
            "Correct!", 56, TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(800, 120));
        feedbackText.fontStyle = FontStyles.Bold;
        feedbackText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        feedbackText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        feedbackText.gameObject.SetActive(false);

        // ─── Game Over Panel ───
        GameObject gameOverPanel = CreateUIPanel(canvasObj.transform, "GameOverPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(600, 300),
            new Color(0f, 0f, 0f, 0.85f));
        gameOverPanel.SetActive(false);

        TextMeshProUGUI goText = CreateUIText(gameOverPanel.transform, "GameOverText",
            "GAME OVER\n\nPress [R] to Restart", 32, TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(550, 250));

        // ─── Victory Panel ───
        GameObject victoryPanel = CreateUIPanel(canvasObj.transform, "VictoryPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(600, 300),
            new Color(0f, 0.15f, 0f, 0.9f));
        victoryPanel.SetActive(false);

        TextMeshProUGUI vicText = CreateUIText(victoryPanel.transform, "VictoryText",
            "YOU WIN!\nBoss Defeated!\n\nPress [R] to Play Again", 32, TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(550, 250));
        vicText.color = new Color(0.3f, 1f, 0.3f);

        // ─── Start Screen Panel (centered overlay) ───
        GameObject startPanel = new GameObject("StartPanel");
        startPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform stRT = startPanel.AddComponent<RectTransform>();
        stRT.anchorMin = Vector2.zero;
        stRT.anchorMax = Vector2.one;
        stRT.offsetMin = Vector2.zero;
        stRT.offsetMax = Vector2.zero;
        UnityEngine.UI.Image stBg = startPanel.AddComponent<UnityEngine.UI.Image>();
        stBg.color = new Color(0f, 0f, 0f, 0.85f);

        // Title text
        TextMeshProUGUI titleText = CreateUIText(startPanel.transform, "TitleText",
            "Tomat-oh No!", 52, TextAlignmentOptions.Center,
            new Vector2(0, 80), new Vector2(600, 80));
        titleText.color = new Color(0.9f, 0.3f, 0.3f);
        titleText.fontStyle = FontStyles.Bold;

        // Subtitle
        TextMeshProUGUI subtitleText = CreateUIText(startPanel.transform, "SubtitleText",
            "A Bullet Hell Trivia Game", 24, TextAlignmentOptions.Center,
            new Vector2(0, 30), new Vector2(500, 40));
        subtitleText.color = new Color(0.8f, 0.8f, 0.8f);

        // Start button
        Button startBtn = CreateModeButton(startPanel.transform, "StartBtn", "Start",
            new Vector2(0.38f, 0.3f), new Vector2(0.62f, 0.42f));
        ColorBlock scb = startBtn.colors;
        scb.normalColor = new Color(0.3f, 0.8f, 0.4f);
        scb.highlightedColor = new Color(0.35f, 0.85f, 0.45f);
        scb.pressedColor = new Color(0.25f, 0.7f, 0.35f);
        startBtn.colors = scb;
        TextMeshProUGUI startBtnTxt = startBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (startBtnTxt != null)
        {
            startBtnTxt.color = Color.white;
            startBtnTxt.fontStyle = FontStyles.Bold;
            startBtnTxt.fontSize = 28;
        }

        // ─── Button Panel (bottom of screen — Static HUD + Floating) ───
        GameObject buttonPanel = new GameObject("ButtonPanel");
        buttonPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform bpRT = buttonPanel.AddComponent<RectTransform>();
        bpRT.anchorMin = new Vector2(0, 0);
        bpRT.anchorMax = new Vector2(1, 0);
        bpRT.pivot = new Vector2(0.5f, 0);
        bpRT.anchoredPosition = Vector2.zero;
        bpRT.sizeDelta = new Vector2(0, 55);

        UnityEngine.UI.Image bpBg = buttonPanel.AddComponent<UnityEngine.UI.Image>();
        bpBg.color = new Color(0f, 0f, 0f, 0.6f);

        // Label
        TextMeshProUGUI qdLabel = CreateUIText(buttonPanel.transform, "QDLabel",
            "Question Display:", 14, TextAlignmentOptions.MidlineLeft,
            Vector2.zero, Vector2.zero);
        RectTransform qdRT = qdLabel.rectTransform;
        qdRT.anchorMin = new Vector2(0.01f, 0.1f);
        qdRT.anchorMax = new Vector2(0.18f, 0.9f);
        qdRT.offsetMin = Vector2.zero;
        qdRT.offsetMax = Vector2.zero;
        qdLabel.color = new Color(0.8f, 0.8f, 0.8f);

        // Static HUD & Floating buttons
        string[] uiLabels = { "Static HUD", "Floating" };
        Button[] uiButtons = new Button[2];
        for (int i = 0; i < 2; i++)
        {
            float xMin = 0.19f + i * 0.16f;
            float xMax = xMin + 0.15f;
            uiButtons[i] = CreateModeButton(buttonPanel.transform, "UIBtn_" + i, uiLabels[i],
                new Vector2(xMin, 0.1f), new Vector2(xMax, 0.9f));
        }

        // Reset button (right side)
        Button resetBtn = CreateModeButton(buttonPanel.transform, "ResetBtn", "Reset",
            new Vector2(0.85f, 0.1f), new Vector2(0.98f, 0.9f));
        ColorBlock rcb = resetBtn.colors;
        rcb.normalColor = new Color(0.9f, 0.3f, 0.3f);
        resetBtn.colors = rcb;
        TextMeshProUGUI resetTxt = resetBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (resetTxt != null) resetTxt.color = Color.white;

        // Pause button
        Button pauseBtn = CreateModeButton(buttonPanel.transform, "PauseBtn", "Pause [P]",
            new Vector2(0.68f, 0.1f), new Vector2(0.83f, 0.9f));
        ColorBlock pcb = pauseBtn.colors;
        pcb.normalColor = new Color(0.85f, 0.65f, 0.1f);
        pcb.highlightedColor = new Color(0.9f, 0.7f, 0.15f);
        pcb.pressedColor = new Color(0.75f, 0.55f, 0.08f);
        pauseBtn.colors = pcb;
        TextMeshProUGUI pauseTxt = pauseBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (pauseTxt != null) pauseTxt.color = Color.white;

        // Hidden mode label
        TextMeshProUGUI uiModeLabel = CreateUIText(canvasObj.transform, "UIModeLabel",
            "", 1, TextAlignmentOptions.Center, Vector2.zero, Vector2.zero);
        uiModeLabel.gameObject.SetActive(false);

        // ═══════════════════════════════════════════
        // 8. FLOATING HUD (World Space - follows player)
        // ═══════════════════════════════════════════
        GameObject floatingHUD = new GameObject("FloatingHUD");
        floatingHUD.transform.position = new Vector3(0, 2f, 0);
        TextMeshPro floatingText = CreateWorldText(floatingHUD.transform, "FloatingQuestionText",
            "Question here", 3f, new Vector2(8f, 1.5f));
        floatingHUD.SetActive(false); // starts hidden

        // ═══════════════════════════════════════════
        // 9. MANAGERS
        // ═══════════════════════════════════════════
        GameObject managers = new GameObject("──── Managers ────");

        // UILayoutManager (Static + Floating, no diegetic)
        UILayoutManager ulm = managers.AddComponent<UILayoutManager>();
        ulm.player = player.transform;
        ulm.staticHUDPanel = staticPanel;
        ulm.staticQuestionText = staticQText;
        ulm.floatingHUDObject = floatingHUD;
        ulm.floatingQuestionText = floatingText;

        // GameManager
        GameManager gm = managers.AddComponent<GameManager>();
        gm.player = pc;
        gm.bulletSpawner = spawner;
        gm.uiLayoutManager = ulm;
        gm.answerTiles = tiles;
        gm.hpText = hpText;
        gm.timerText = timerText;
        gm.feedbackText = feedbackText;
        gm.bossHPLabelText = bossHPLabel;
        gm.bossHPFillImage = fillImg;
        gm.gameOverPanel = gameOverPanel;
        gm.victoryPanel = victoryPanel;
        gm.startPanel = startPanel;
        gm.optionTexts = optionTexts;

        // Wire start button to GameManager
        startBtn.onClick.AddListener(gm.StartGame);

        // ModeSwitcher (finds buttons by name at runtime)
        ModeSwitcher ms = managers.AddComponent<ModeSwitcher>();
        ms.playerController = pc;
        ms.uiLayoutManager = ulm;
        ms.uiModeLabel = uiModeLabel;

        // Wire pause button to GameManager
        pauseBtn.onClick.AddListener(gm.TogglePause);

        // ═══════════════════════════════════════════
        // DONE
        // ═══════════════════════════════════════════
        Debug.Log("✅ Bullet Hell Trivia scene setup complete! Press Play to test.");
        EditorUtility.DisplayDialog("Setup Complete!",
            "Scene is ready!\n\n" +
            "Press PLAY to test.\n\n" +
            "Controls:\n" +
            "  WASD / Arrows = Move\n" +
            "  Buttons at bottom = Switch modes\n" +
            "  P = Pause/Resume\n" +
            "  R = Reset",
            "Got it!");
    }

    // ═══════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════

    static GameObject CreateSprite(string name, Vector3 position, Vector3 scale, Color color, int sortingOrder = 0)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = MakeWhiteSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return obj;
    }

    static GameObject CreateSpriteWithTexture(string name, Vector3 position, Vector3 scale, Sprite sprite, int sortingOrder = 0)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        return obj;
    }

    static Sprite LoadSprite(string resourceName)
    {
        Texture2D tex = Resources.Load<Texture2D>(resourceName);
        if (tex == null)
        {
            Debug.LogWarning($"Could not load sprite: {resourceName}");
            return null;
        }
        tex.filterMode = FilterMode.Point; // pixel art — no blurring
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), Mathf.Max(tex.width, tex.height));
    }

    static Sprite MakeWhiteSprite()
    {
        // Create a 4x4 white texture to use as a basic square sprite
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    static GameObject CreateUIPanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        UnityEngine.UI.Image img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = color;

        return panel;
    }

    static TextMeshProUGUI CreateUIText(Transform parent, string name,
        string text, float fontSize, TextAlignmentOptions alignment,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        return tmp;
    }

    static TextMeshPro CreateWorldText(Transform parent, string name,
        string text, float fontSize, Vector2 rectSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = Vector3.zero;

        TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = rectSize;
        tmp.sortingOrder = 8;

        return tmp;
    }

    static void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }

    static UnityEngine.UI.Button CreateModeButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(2, 2);
        rt.offsetMax = new Vector2(-2, -2);

        UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.white;

        UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;

        // Button label
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(4, 2);
        trt.offsetMax = new Vector2(-4, -2);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.15f, 0.15f, 0.15f);
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 10;
        tmp.fontSizeMax = 16;

        return btn;
    }
}
