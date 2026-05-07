using UnityEngine;

/// <summary>
/// Draws a visible grid on the arena floor using LineRenderer.
/// The grid helps players see tile boundaries, especially useful
/// for the Snap-to-Grid movement mode.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class GridRenderer : MonoBehaviour
{
    [Header("Grid Settings")]
    public float gridSize = 1f;
    public int gridWidth = 16;   // number of columns
    public int gridHeight = 10;  // number of rows
    public Color gridColor = new Color(0.3f, 0.3f, 0.5f, 0.3f);
    public float lineWidth = 0.02f;

    void Start()
    {
        DrawGrid();
    }

    void DrawGrid()
    {
        // Calculate the total world-space dimensions
        float totalWidth = gridWidth * gridSize;
        float totalHeight = gridHeight * gridSize;
        float startX = -totalWidth / 2f;
        float startY = -totalHeight / 2f;

        // Draw vertical lines
        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = startX + x * gridSize;
            CreateLine("GridV_" + x,
                new Vector3(xPos, startY, 0),
                new Vector3(xPos, startY + totalHeight, 0));
        }

        // Draw horizontal lines
        for (int y = 0; y <= gridHeight; y++)
        {
            float yPos = startY + y * gridSize;
            CreateLine("GridH_" + y,
                new Vector3(startX, yPos, 0),
                new Vector3(startX + totalWidth, yPos, 0));
        }
    }

    void CreateLine(string name, Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.sortingOrder = -5;

        // Use a simple unlit material
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridColor;
        lr.endColor = gridColor;
    }

    /// <summary>
    /// Snaps a world position to the nearest grid center.
    /// </summary>
    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        float snappedX = Mathf.Round(worldPos.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(worldPos.y / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, worldPos.z);
    }

    /// <summary>
    /// Get the grid bounds (useful for clamping).
    /// </summary>
    public Rect GetGridBounds()
    {
        float totalWidth = gridWidth * gridSize;
        float totalHeight = gridHeight * gridSize;
        return new Rect(-totalWidth / 2f, -totalHeight / 2f, totalWidth, totalHeight);
    }
}
