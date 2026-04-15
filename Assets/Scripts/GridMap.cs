using System.Collections.Generic;
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
            name = "3x2",
            requiredUniqueBlocks = 2,
            offsets = new List<Vector2Int>()
            {
                new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0),
                new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1),
            }
        }
    };

    // ================= INIT =================

    public void Init(LevelData level)
    {
        columns = level.cols;
        rows = level.rows;

        gridData = new Block[columns, rows];

        CreateGrid(level);
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
}