using System.Collections.Generic;
using UnityEngine;

public class ShapeImageRenderer : MonoBehaviour
{
    public int columns = 8;
    public int rows = 8;

    [Header("Data")]
    public ShapeData shapeData;
    public string parentId;

    [Header("Render")]
    public GameObject cellPrefab;
    public Sprite[] imagePieces;

    [Header("Settings")]
    public float cellSize = 1f;
    public Vector3 shapeSelectedScale = Vector3.one * 1.1f;

    private List<GameObject> cells = new List<GameObject>();
    private Dictionary<Transform, Vector2Int> cellCoords = new Dictionary<Transform, Vector2Int>();

    private Transform tf;
    private Camera cam;
    private GridMap grid;

    private Vector3 startScale;
    private Vector3 startPos;
    private Vector3 dragOffset;
    private bool dragging = false;

    Vector3 offset;
    Collider2D collider2D;
    public string destnationtag = "Square";

    void Awake()
    {
        tf = transform;
        cam = Camera.main;
        grid = FindObjectOfType<GridMap>();
        startScale = tf.localScale;
        collider2D = GetComponent<Collider2D>();
    }

    public void Init()
    {
        CreateShape();
        PlaceAtGrid(new Vector2Int(shapeData.axisX, shapeData.axisY));
    }

    void CreateShape()
    {
        float offsetX = (shapeData.columns - 1) / 2f;
        float offsetY = (shapeData.rows - 1) / 2f;

        for (int row = 0; row < shapeData.rows; row++)
        {
            for (int col = 0; col < shapeData.columns; col++)
            {
                if (!shapeData.board[row].column[col]) continue;

                GameObject cell = Instantiate(cellPrefab, tf);

                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if (sr == null) sr = cell.AddComponent<SpriteRenderer>();

                int index = row * shapeData.columns + col;
                if (index < imagePieces.Length)
                    sr.sprite = imagePieces[index];

                float x = (col - offsetX) * cellSize;
                float y = (offsetY - row) * cellSize;

                cell.transform.localPosition = new Vector3(x, y, 0);
                cell.transform.localScale = cellPrefab.transform.localScale;

                cells.Add(cell);
                cellCoords[cell.transform] = new Vector2Int(col, row);
            }
        }
    }

    Vector3 MouseWorldPosition()
    {
        var mouse = Input.mousePosition;
        mouse.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouse);
    }
    void OnMouseDown()
    {
        offset = transform.position - MouseWorldPosition();
    }

    void OnMouseDrag()
    {
        transform.position = MouseWorldPosition() + offset;
    }

    void OnMouseUp()
    {
        collider2D.enabled = false;
        var ray = Camera.main.transform.position;
        var rayDi = MouseWorldPosition() - Camera.main.transform.position;
        RaycastHit2D hitinfo;
        if(hitinfo = Physics2D.Raycast(ray, rayDi))
        {
            if(hitinfo.collider.tag == destnationtag)
            {
                transform.position = hitinfo.transform.position + new Vector3(0,0,-0.01f);
            }
        }
        collider2D.enabled = true;
    }

    void SnapToGrid()
    {
        Transform pivot = GetBestPivotCell();
        if (pivot == null) return;

        Transform closest = GetClosestSquare();
        if (closest == null)
        {
            tf.position = startPos;
            return;
        }

        Vector3 delta = closest.position - pivot.position;

        if (CheckCollision(tf.position + delta))
        {
            tf.position = startPos;
            return;
        }

        tf.position += delta;
    }

    bool CheckCollision(Vector3 targetPos)
    {
        Vector3 old = tf.position;
        tf.position = targetPos;

        foreach (GameObject c in cells)
        {
            Collider2D hit = Physics2D.OverlapBox(
                c.transform.position,
                Vector2.one * 0.8f,
                0f
            );

            if (hit != null && hit.transform.root != tf)
            {
                tf.position = old;
                return true;
            }
        }

        tf.position = old;
        return false;
    }

    Transform GetClosestSquare()
    {
        Transform pivot = GetBestPivotCell();
        if (pivot == null) return null;

        float min = 999f;
        Transform best = null;

        foreach (Transform sq in grid.GetAllSquares())
        {
            float d = Vector2.Distance(pivot.position, sq.position);
            if (d < min)
            {
                min = d;
                best = sq;
            }
        }

        return best;
    }

    Transform GetBestPivotCell()
    {
        if (cells.Count == 0) return null;

        Vector2 center = Vector2.zero;
        foreach (var c in cells)
            center += (Vector2)c.transform.position;

        center /= cells.Count;

        float min = 999f;
        Transform best = null;

        foreach (var c in cells)
        {
            float d = Vector2.Distance(c.transform.position, center);
            if (d < min)
            {
                min = d;
                best = c.transform;
            }
        }

        return best;
    }

    void SetSorting(int order)
    {
        foreach (var c in cells)
        {
            SpriteRenderer sr = c.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = order;
        }
    }

    public void PlaceAtGrid(Vector2Int origin)
    {
        Transform pivot = cells[0].transform;
        Vector3 target = grid.GetWorldPosition(origin);
        Vector3 delta = target - pivot.position;
        tf.position += delta;
    }
}