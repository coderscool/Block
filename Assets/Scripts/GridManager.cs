using UnityEngine;

// ===============================
// GRID MANAGER
// spawn grid world space
// ===============================
public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public int width = 8;
    public int height = 8;

    public float cellSize = 1f;
    public float gap = 0.05f;

    public GameObject gridPrefab;

    private GridCell[,] cells;

    void Awake()
    {
        instance = this;
        CreateGrid();
    }

    void CreateGrid()
    {
        cells = new GridCell[width, height];

        float startX = -(width - 1) * (cellSize + gap) / 2f;
        float startY = (height - 1) * (cellSize + gap) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(gridPrefab, transform);

                obj.transform.position = new Vector3(
                    startX + x * (cellSize + gap),
                    startY - y * (cellSize + gap),
                    0
                );

                obj.tag = "Square";

                GridCell cell = obj.GetComponent<GridCell>();
                if (cell == null) cell = obj.AddComponent<GridCell>();

                cell.x = x;
                cell.y = y;
                cell.occupied = false;

                cells[x, y] = cell;
            }
        }
    }

    public GridCell GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return null;

        return cells[x, y];
    }
}