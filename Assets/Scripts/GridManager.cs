using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ===============================
// GRID MANAGER FULL FIX
// world space grid
// ===============================
public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("Grid Size")]
    public int width = 8;
    public int height = 8;

    [Header("Visual")]
    public float cellSize = 0.5f;
    public float gap = 0.05f;
    public GameObject gridPrefab;

    [Header("Runtime")]
    public GridCell[,] cells;
    public Block[,] gridData;

    public Dictionary<Vector2Int, GridCell> gridDict =
        new Dictionary<Vector2Int, GridCell>();

    [Header("Patterns")]
    public List<LevelData.ShapeMatchPattern> patterns =
        new List<LevelData.ShapeMatchPattern>();

    // ===============================
    void Awake()
    {
        instance = this;
        CreateGrid();
        CreateBoundaryWalls();
    }

    // ===============================
    // CREATE GRID
    // ===============================
    void CreateGrid()
    {
        cells = new GridCell[width, height];
        gridData = new Block[width, height];
        gridDict.Clear();

        float step = cellSize + gap;

        float startX = -(width - 1) * step / 2f;
        float startY = (height - 1) * step / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(gridPrefab, transform);
                AdjustSpriteToCell(obj);

                obj.transform.position = new Vector3(
                    startX + x * step,
                    startY - y * step,
                    0
                );

                obj.name = $"Cell_{x}_{y}";
                obj.tag = "Square";

                GridCell cell = obj.GetComponent<GridCell>();

                if (cell == null)
                    cell = obj.AddComponent<GridCell>();

                cell.x = x;
                cell.y = y;
                cell.occupied = false;

                cells[x, y] = cell;
                gridDict[new Vector2Int(x, y)] = cell;
            }
        }

    }

    void CreateBoundaryWalls()
    {
        Bounds b = GetGridBounds();

        float thickness = 0.3f;

        CreateWall(
            new Vector2(b.min.x - thickness / 2, b.center.y),
            new Vector2(thickness, b.size.y + thickness * 2)
        );

        CreateWall(
            new Vector2(b.max.x + thickness / 2, b.center.y),
            new Vector2(thickness, b.size.y + thickness * 2)
        );

        CreateWall(
            new Vector2(b.center.x, b.max.y + thickness / 2),
            new Vector2(b.size.x + thickness * 2, thickness)
        );

        CreateWall(
            new Vector2(b.center.x, b.min.y - thickness / 2),
            new Vector2(b.size.x + thickness * 2, thickness)
        );
    }

    void CreateWall(Vector2 pos, Vector2 size)
    {
        GameObject wall = new GameObject("Wall");

        wall.transform.parent = transform;
        wall.transform.position = pos;

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = size;

        Rigidbody2D rb = wall.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
    }

    public Bounds GetGridBounds()
    {
        float step = cellSize + gap;

        float totalWidth = width * step - gap;
        float totalHeight = height * step - gap;

        Vector3 center = transform.position;

        return new Bounds(
            center,
            new Vector3(totalWidth, totalHeight, 0)
        );
    }

    // ===============================
    // BASIC
    // ===============================
    public bool IsInside(int x, int y)
    {
        return x >= 0 &&
               y >= 0 &&
               x < width &&
               y < height;
    }

    public GridCell GetCell(int x, int y)
    {
        if (!IsInside(x, y))
            return null;

        return cells[x, y];
    }

    public Vector3 GetCellWorldPos(int x, int y)
    {
        GridCell c = GetCell(x, y);

        if (c == null)
            return Vector3.zero;

        return c.transform.position;
    }

    // ===============================
    // DATA
    // ===============================
    public void SetBlock(int x, int y, Block block)
    {
        if (!IsInside(x, y))
            return;

        gridData[x, y] = block;
        cells[x, y].occupied = block != null;
    }

    public Block GetBlock(int x, int y)
    {
        if (!IsInside(x, y))
            return null;

        return gridData[x, y];
    }

    public bool IsOccupied(int x, int y)
    {
        if (!IsInside(x, y))
            return true;

        return gridData[x, y] != null;
    }

    // ===============================
    // CLEAR SHAPE BY ID
    // dùng khi kéo lại shape cũ
    // ===============================
    public void ClearShape(string parentId)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Block b = gridData[x, y];

                if (b != null && b.parentId == parentId)
                {
                    gridData[x, y] = null;
                    cells[x, y].occupied = false;
                }
            }
        }
    }

    // ===============================
    // CHECK PLACE SHAPE
    // dùng để chặn đè shape khác
    // ===============================
    public bool CanPlaceShape(List<Vector2Int> coords, string ownerId)
    {
        foreach (var c in coords)
        {
            if (!IsInside(c.x, c.y))
                return false;

            Block b = gridData[c.x, c.y];

            if (b != null && b.parentId != ownerId)
                return false;
        }

        return true;
    }

    // ===============================
    // MATCH SYSTEM
    // ===============================
    public void CheckPattern()
    {
        foreach (var pattern in patterns)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (MatchShape(pattern, x, y))
                    {
                        ClearMatched(pattern, x, y);

                        if (LevelManager.Instance != null)
                            LevelManager.Instance.OnBlockMatched(1);
                    }
                }
            }
        }
    }

    bool MatchShape(LevelData.ShapeMatchPattern pattern, int startX, int startY)
    {
        HashSet<Block> uniqueBlocks = new HashSet<Block>();

        foreach (var offset in pattern.offsets)
        {
            int x = startX + offset.x;
            int y = startY + offset.y;

            if (!IsInside(x, y))
                return false;

            Block b = gridData[x, y];

            if (b == null)
                return false;

            uniqueBlocks.Add(b);
        }

        if (uniqueBlocks.Count != Mathf.Max(1, pattern.requiredUniqueBlocks))
            return false;

        string id = null;

        foreach (Block b in uniqueBlocks)
        {
            if (string.IsNullOrEmpty(b.parentId))
                return false;

            if (id == null)
                id = b.parentId;
            else if (id != b.parentId)
                return false;
        }

        return true;
    }

    void ClearMatched(LevelData.ShapeMatchPattern pattern, int startX, int startY)
    {
        HashSet<Block> destroyList = new HashSet<Block>();

        foreach (var offset in pattern.offsets)
        {
            int x = startX + offset.x;
            int y = startY + offset.y;

            if (!IsInside(x, y))
                continue;

            Block b = gridData[x, y];

            if (b != null)
            {
                destroyList.Add(b);
                gridData[x, y] = null;
                cells[x, y].occupied = false;
            }
        }

        foreach (Block b in destroyList)
        {
            if (b != null)
                Destroy(b.gameObject);
        }
    }

    // ===============================
    // DEBUG
    // ===============================
    [ContextMenu("Log Grid Map")]
    public void LogGridMap()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("===== GRID MAP =====");

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Block b = gridData[x, y];

                if (b == null)
                    sb.Append("0 ");
                else
                    sb.Append("1 ");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }

    [ContextMenu("Log Parent Id")]
    public void LogParentId()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("===== OWNER MAP =====");

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Block b = gridData[x, y];

                if (b == null)
                    sb.Append("0 ");
                else
                    sb.Append(b.parentId + " ");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }

    public void ClearBlockAt(int x, int y)
    {
        if (!IsInside(x, y)) return;

        gridData[x, y] = null;
        cells[x, y].occupied = false;
    }

    [ContextMenu("Clear All")]
    public void ClearAll()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridData[x, y] = null;
                cells[x, y].occupied = false;
            }
        }
    }

    void AdjustSpriteToCell(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // Kích thước sprite trong world units
            Vector2 spriteSize = sr.sprite.bounds.size;

            // Tính scale để sprite vừa khít cellSize
            float scaleX = cellSize / spriteSize.x;
            float scaleY = cellSize / spriteSize.y;

            obj.transform.localScale = new Vector3(scaleX, scaleY, 1);
        }
    }
}