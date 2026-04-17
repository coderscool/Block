using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    public int columns = 8;
    public int rows = 8;
    public float squaresGap = 0.1f;
    public GameObject gridSquare;
    public Vector2 startPosition = new Vector2(0.0f, 0.0f);
    public float squareScale = 0.5f;
    public float everySquareOffset = 0.0f;

    private Vector2 _offset = Vector2.zero;
    private List<GameObject> _gridSquares = new List<GameObject>();

    private Block[,] gridData;
    public LevelData level;

    private Dictionary<Vector2Int, GameObject> gridDict = new Dictionary<Vector2Int, GameObject>();

    [System.Serializable]
    public class ShapeMatchPattern
    {
        public string name;
        public List<Vector2Int> offsets = new List<Vector2Int>();
        public int requiredUniqueBlocks = 2;
    }

    public List<ShapeMatchPattern> patterns = new List<ShapeMatchPattern>()
    {
        new ShapeMatchPattern()
        {
            name = "3x3",
            requiredUniqueBlocks = 2,
            offsets = new List<Vector2Int>()
            {
                new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0),
                new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1),
                new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2)
            }
        }
    };

    // ================= INIT =================

    public void Init(LevelData level)
    {
        ClearGrid();

        columns = level.cols;
        rows = level.rows;

        gridData = new Block[columns, rows];

        CreateGrid(level);
    }

    void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        _gridSquares.Clear();

        gridDict.Clear();
    }

    private void CreateGrid(LevelData level)
    {
        SpawnGridSquares(level);
        SetGridSquaresPositions();
    }

    private void SpawnGridSquares(LevelData level)
    {
        int[,] map = level.GetMap();

        for (var row = 0; row < rows; ++row)
        {
            for (var column = 0; column < columns; ++column)
            {
                if (map[row, column] == 0) continue;

                var go = Instantiate(gridSquare);
                go.transform.SetParent(this.transform);
                go.transform.localScale = new Vector3(squareScale, squareScale, squareScale);

                _gridSquares.Add(go);
                gridDict[new Vector2Int(column, row)] = go;
            }
        }
    }

    private void SetGridSquaresPositions()
    {
        var square_rect = _gridSquares[0].GetComponent<RectTransform>();

        _offset.x = square_rect.rect.width * square_rect.transform.localScale.x + everySquareOffset;
        _offset.y = square_rect.rect.height * square_rect.transform.localScale.y + everySquareOffset;

        foreach (var kvp in gridDict)
        {
            int col = kvp.Key.x;
            int row = kvp.Key.y;

            var square = kvp.Value;

            float pos_x = startPosition.x + col * (_offset.x + squaresGap);
            float pos_y = startPosition.y - row * (_offset.y + squaresGap);

            square.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(pos_x, pos_y);
        }
    }

    // ================= FIX CHÍNH Ở ĐÂY =================

    public Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        float minDistance = float.MaxValue;
        Vector2Int bestPos = new Vector2Int(-1, -1);

        foreach (var kvp in gridDict)
        {
            float dist = Vector2.Distance(
                worldPos,
                kvp.Value.GetComponent<RectTransform>().position
            );

            if (dist < minDistance)
            {
                minDistance = dist;
                bestPos = kvp.Key;
            }
        }

        return bestPos;
    }

    // ================= PLACE =================

    public void PlaceBlock(Block block)
    {
        foreach (var cell in block.cells)
        {
            int x = block.origin.x + cell.x;
            int y = block.origin.y + cell.y;

            if (!IsInside(x, y)) return;

            // 🔥 FIX MAP KHUYẾT
            if (!gridDict.ContainsKey(new Vector2Int(x, y))) return;

            if (gridData[x, y] != null) return;
        }

        foreach (var cell in block.cells)
        {
            int x = block.origin.x + cell.x;
            int y = block.origin.y + cell.y;

            gridData[x, y] = block;
        }

        CheckPattern();
    }

    // ================= MATCH =================

    void CheckPattern()
    {
        foreach (var pattern in patterns)
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (MatchShape(pattern, x, y))
                    {
                        ClearShape(pattern, x, y);
                        LevelManager.Instance.OnBlockMatched(1);
                    }
                }
            }
        }
    }

    bool MatchShape(ShapeMatchPattern pattern, int startX, int startY)
    {
        HashSet<Block> blocks = new HashSet<Block>();

        foreach (var offset in pattern.offsets)
        {
            int x = startX + offset.x;
            int y = startY + offset.y;

            if (!IsInside(x, y)) return false;

            // 🔥 FIX MAP KHUYẾT
            if (!gridDict.ContainsKey(new Vector2Int(x, y)))
                return false;

            if (gridData[x, y] == null) return false;

            blocks.Add(gridData[x, y]);
        }

        if (blocks.Count != Mathf.Max(1, pattern.requiredUniqueBlocks))
            return false;

        string id = null;
        foreach (var b in blocks)
        {
            if (b == null || string.IsNullOrEmpty(b.parentId))
                return false;

            if (id == null) id = b.parentId;
            else if (id != b.parentId) return false;
        }

        return true;
    }

    void ClearShape(ShapeMatchPattern pattern, int startX, int startY)
    {
        HashSet<Block> blocks = new HashSet<Block>();

        foreach (var offset in pattern.offsets)
        {
            int x = startX + offset.x;
            int y = startY + offset.y;

            if (gridData[x, y] != null)
            {
                blocks.Add(gridData[x, y]);
                gridData[x, y] = null;
            }
        }

        foreach (var b in blocks)
        {
            Destroy(b.gameObject);
        }
    }

    bool IsInside(int x, int y)
    {
        return x >= 0 && x < columns && y >= 0 && y < rows;
    }

    public List<RectTransform> GetAllSquares()
    {
        List<RectTransform> list = new List<RectTransform>();

        foreach (var kvp in gridDict)
        {
            list.Add(kvp.Value.GetComponent<RectTransform>());
        }

        return list;
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        if (!gridDict.ContainsKey(gridPos)) return Vector3.zero;

        return gridDict[gridPos].GetComponent<RectTransform>().position;
    }

    public string GetGridDebugString()
    {
        if (gridData == null) return "<gridData NULL>";

        StringBuilder sb = new StringBuilder();
        // Print header with column indices (optional)
        sb.Append("   ");
        for (int x = 0; x < columns; x++)
        {
            sb.AppendFormat("{0,4}", x);
        }
        sb.AppendLine();
        sb.AppendLine(new string('-', 4 + columns * 4));

        // Rows: row 0 at top (matches spawning logic)
        for (int y = 0; y < rows; y++)
        {
            sb.AppendFormat("{0,2} |", y);
            for (int x = 0; x < columns; x++)
            {
                var key = new Vector2Int(x, y);
                if (!gridDict.ContainsKey(key))
                {
                    sb.Append("   ");
                }
                else
                {
                    var b = gridData[x, y];
                    if (b == null)
                    {
                        sb.Append(" [ ]");
                    }
                    else
                    {
                        string pid = b.parentId ?? "B";
                        string shortId = pid.Length <= 2 ? pid : pid.Substring(0, 2);
                        sb.Append($"[{shortId}]");
                    }
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // Prints debug string to Unity console
    public void LogGrid()
    {
        Debug.Log("GridData:\n" + GetGridDebugString());
    }

    public void RemoveBlock(Block block)
    {
        foreach (Vector2Int cell in block.cells)
        {
            int x = block.origin.x + cell.x;
            int y = block.origin.y + cell.y;

            if (IsInside(x, y))
            {
                if (gridData[x, y] == block)
                {
                    gridData[x, y] = null;
                }
            }
        }
    }
}