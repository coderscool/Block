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

    // 🔥 DATA GRID
    private Block[,] gridData;

    // 🎯 SHAPE MATCH (2 block L → 2x3)
    private List<Vector2Int> targetShape = new List<Vector2Int>()
    {
        new Vector2Int(0,0),
    new Vector2Int(1,0),
    new Vector2Int(2,0),

    new Vector2Int(0,1),
    new Vector2Int(1,1),
    new Vector2Int(2,1),
    };

    void Start()
    {
        CreateGrid();
        gridData = new Block[columns, rows];
    }

    // ================= GRID CREATE =================

    private void CreateGrid()
    {
        SpawnGridSquares();
        SetGridSquaresPositions();
    }

    public List<RectTransform> GetAllSquares()
    {
        List<RectTransform> list = new List<RectTransform>();

        foreach (var obj in _gridSquares)
        {
            list.Add(obj.GetComponent<RectTransform>());
        }

        return list;
    }

    private void SpawnGridSquares()
    {
        int square_index = 0;

        for (var row = 0; row < rows; ++row)
        {
            for (var column = 0; column < columns; ++column)
            {
                var go = Instantiate(gridSquare);
                go.transform.SetParent(this.transform);
                go.transform.localScale = new Vector3(squareScale, squareScale, squareScale);

                _gridSquares.Add(go);
                square_index++;
            }
        }
    }

    private void SetGridSquaresPositions()
    {
        int column_number = 0;
        int row_number = 0;
        Vector2 square_gap_number = Vector2.zero;
        bool row_moved = false;

        var square_rect = _gridSquares[0].GetComponent<RectTransform>();

        _offset.x = square_rect.rect.width * square_rect.transform.localScale.x + everySquareOffset;
        _offset.y = square_rect.rect.height * square_rect.transform.localScale.y + everySquareOffset;

        foreach (GameObject square in _gridSquares)
        {
            if (column_number + 1 > columns)
            {
                square_gap_number.x = 0;
                column_number = 0;
                row_number++;
                row_moved = false;
            }

            var pos_x_offset = _offset.x * column_number + (square_gap_number.x * squaresGap);
            var pos_y_offset = _offset.y * row_number + (square_gap_number.y * squaresGap);

            square.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(startPosition.x + pos_x_offset,
                            startPosition.y - pos_y_offset);

            column_number++;
        }
    }

    // ================= LOGIC =================

    public Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        float minDistance = float.MaxValue;
        int bestIndex = -1;

        for (int i = 0; i < _gridSquares.Count; i++)
        {
            float dist = Vector2.Distance(worldPos, _gridSquares[i].GetComponent<RectTransform>().position);

            if (dist < minDistance)
            {
                minDistance = dist;
                bestIndex = i;
            }
        }

        int x = bestIndex % columns;
        int y = bestIndex / columns;

        return new Vector2Int(x, y);
    }

    public void PlaceBlock(Block block)
    {
        // check hợp lệ
        foreach (var cell in block.cells)
        {
            int x = block.origin.x + cell.x;
            int y = block.origin.y + cell.y;

            if (!IsInside(x, y)) return;
            if (gridData[x, y] != null) return;
        }

        // place
        foreach (var cell in block.cells)
        {
            int x = block.origin.x + cell.x;
            int y = block.origin.y + cell.y;

            gridData[x, y] = block;
        }

        CheckPattern();
    }

    void CheckPattern()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (MatchShape(x, y))
                {
                    ClearShape(x, y);
                }
            }
        }
    }

    bool MatchShape(int startX, int startY)
    {
        HashSet<Block> blocks = new HashSet<Block>();

        foreach (var offset in targetShape)
        {
            int x = startX + offset.x;
            int y = startY + offset.y;

            if (!IsInside(x, y)) return false;
            if (gridData[x, y] == null) return false;

            blocks.Add(gridData[x, y]);
        }

        return blocks.Count == 2;
    }

    void ClearShape(int startX, int startY)
    {
        HashSet<Block> blocks = new HashSet<Block>();

        foreach (var offset in targetShape)
        {
            int x = startX + offset.x;
            int y = startY + offset.y;

            blocks.Add(gridData[x, y]);
            gridData[x, y] = null;
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
}