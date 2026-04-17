using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShapeImageRenderer : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public int columns = 8;
    public int rows = 8;
    [Header("Data")]
    public ShapeData shapeData;
    [Tooltip("ID cha dùng để so khớp khi clear pattern. Nếu rỗng sẽ lấy tên parent transform.")]
    public string parentId;

    [Header("Render")]
    public GameObject cellPrefab;
    public Sprite[] imagePieces;

    [Header("Settings")]
    public float cellSize = 100f;

    private List<GameObject> cells = new List<GameObject>();

    public Vector3 shapeSelectedScale;
    public Vector2 offset = new Vector2(0f, 700f);
    private Vector3 _shapeStartScale;
    private RectTransform _transform;
    private bool _shapeDraggable = true;
    private Canvas _canvas;
    private GridMap _grid;
    private readonly Dictionary<RectTransform, Vector2Int> _cellCoordByRect = new Dictionary<RectTransform, Vector2Int>();

    private string ResolveParentId()
    {
        if (!string.IsNullOrWhiteSpace(parentId))
        {
            return parentId.Trim();
        }

        if (transform.parent != null)
        {
            return transform.parent.name;
        }

        return gameObject.name;
    }

    void Awake()
    {
        _transform = GetComponent<RectTransform>();
        _shapeStartScale = GetComponent<RectTransform>().localScale;
        _canvas = GetComponentInParent<Canvas>();
        _shapeDraggable = true;
        _grid = FindObjectOfType<GridMap>();
    }

    public void Init()
    {
        if (!ValidateData()) return;

        CreateShape();

        // 🔥 đặt vào vị trí grid (ví dụ ô (3,2))
        PlaceAtGrid(new Vector2Int(shapeData.axisX, shapeData.axisY));
    }

    private bool ValidateData()
    {
        if (shapeData == null)
        {
            Debug.LogError("shapeData NULL");
            return false;
        }

        if (shapeData.board == null)
        {
            Debug.LogError("board NULL");
            return false;
        }

        if (imagePieces == null || imagePieces.Length == 0)
        {
            Debug.LogError("imagePieces chưa gán");
            return false;
        }

        return true;
    }

    private void CreateShape()
    {
        int index = 0;

        float offsetX = (shapeData.columns - 1) / 2f;
        float offsetY = (shapeData.rows - 1) / 2f;

        _cellCoordByRect.Clear();
        for (int row = 0; row < shapeData.rows; row++)
        {
            for (int col = 0; col < shapeData.columns; col++)
            {
                if (!shapeData.board[row].column[col]) continue;

                GameObject cell = Instantiate(cellPrefab, transform);

                int spriteIndex = row * shapeData.columns + col;

                if (spriteIndex >= imagePieces.Length)
                {
                    Debug.LogError("Thiếu sprite trong imagePieces!");
                    continue;
                }

                var img = cell.GetComponent<Image>();
                img.sprite = imagePieces[spriteIndex];

                float x = (col - offsetX) * cellSize;
                float y = (offsetY - row) * cellSize;

                RectTransform rt = cell.GetComponent<RectTransform>();

                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.localScale = Vector3.one;

                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                cells.Add(cell);
                _cellCoordByRect[rt] = new Vector2Int(col, row);
                index++;
            }
        }

        Debug.Log("Shape render xong!");
    }

    private RectTransform GetClosestSquare()
    {
        float minDistance = float.MaxValue;
        RectTransform closest = null;

        foreach (var square in _grid.GetAllSquares())
        {
            float distance = Vector2.Distance(
                _transform.position,
                square.position
            );

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = square;
            }
        }

        return closest;
    }

    private RectTransform GetClosestCellToPosition(Vector2 targetPosition)
    {
        float minDistance = float.MaxValue;
        RectTransform closest = null;

        foreach (var cell in cells)
        {
            if (cell == null) continue;

            var cellRect = cell.GetComponent<RectTransform>();
            float distance = Vector2.Distance(cellRect.position, targetPosition);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = cellRect;
            }
        }

        return closest;
    }

    List<Vector2Int> GetCellsRelativeToPivot(Vector2Int pivotCoord)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int row = 0; row < shapeData.rows; row++)
        {
            for (int col = 0; col < shapeData.columns; col++)
            {
                if (!shapeData.board[row].column[col]) continue;

                // GridMap uses y increasing downward, same as row here.
                result.Add(new Vector2Int(col - pivotCoord.x, row - pivotCoord.y));
            }
        }

        return result;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }

    private Vector2 _dragOffset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _transform.localScale = shapeSelectedScale;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var localMousePos
        );

        _dragOffset = _transform.anchoredPosition - localMousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        _transform.anchoredPosition = pos + _dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _transform.localScale = _shapeStartScale;

        if (_grid == null || cells.Count == 0) return;

        var closestSquare = GetClosestSquare();
        if (closestSquare == null) return;

        var closestCell = GetClosestCellToPosition(closestSquare.position);
        if (closestCell == null) return;

        var snapDelta = (Vector2)closestSquare.position - (Vector2)closestCell.position;
        _transform.position += (Vector3)snapDelta;

        Vector2Int origin = _grid.GetGridPositionFromWorld(closestSquare.position);

        if (!_cellCoordByRect.TryGetValue(closestCell, out var pivotCoord))
        {
            pivotCoord = Vector2Int.zero;
        }

        Block block = gameObject.GetComponent<Block>();
        if (block == null) block = gameObject.AddComponent<Block>();
        block.cells = GetCellsRelativeToPivot(pivotCoord);
        block.SetOrigin(origin);
        block.SetParentId(ResolveParentId());

        _grid.PlaceBlock(block);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void PlaceAtGrid(Vector2Int origin)
    {
        if (_grid == null || cells.Count == 0) return;

        // 🔥 chọn 1 cell làm pivot (ví dụ: cell đầu tiên)
        RectTransform pivotCell = cells[0].GetComponent<RectTransform>();

        if (!_cellCoordByRect.TryGetValue(pivotCell, out var pivotCoord))
        {
            pivotCoord = Vector2Int.zero;
        }

        // 🔥 vị trí world của ô grid
        Vector3 targetWorldPos = _grid.GetWorldPosition(origin);

        // 🔥 vị trí hiện tại của pivot cell
        Vector3 pivotWorldPos = pivotCell.position;

        // 🔥 dịch shape
        Vector3 delta = targetWorldPos - pivotWorldPos;
        _transform.position += delta;

        // 🔥 tạo block để lưu vào gridData
        Block block = gameObject.GetComponent<Block>();
        if (block == null) block = gameObject.AddComponent<Block>();

        block.cells = GetCellsRelativeToPivot(pivotCoord);
        block.SetOrigin(origin);
        block.SetParentId(ResolveParentId());

        _grid.PlaceBlock(block);
    }
}