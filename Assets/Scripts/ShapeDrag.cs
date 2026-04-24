using UnityEngine;

public class ShapeDrag : MonoBehaviour
{
    Vector3 offset;

    Collider2D[] myColliders;
    Rigidbody2D rb;

    public string destinationTag = "Square";

    ShapeImageRender shapeRender;

    Vector3 startDragPos;

    void Awake()
    {
        myColliders = GetComponentsInChildren<Collider2D>();

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        shapeRender = GetComponent<ShapeImageRender>();
    }

    Vector3 MouseWorldPosition()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouse);
    }

    void SetColliders(bool value)
    {
        foreach (var col in myColliders)
            col.enabled = value;
    }

    void OnMouseDown()
    {
        startDragPos = transform.position;

        rb.bodyType = RigidbodyType2D.Dynamic;
        offset = transform.position - MouseWorldPosition();

        ClearCurrentShapeCells();
    }

    void OnMouseDrag()
    {
        Vector2 target = MouseWorldPosition() + offset;

        if (shapeRender != null && shapeRender.shapeData != null)
        {
            var mode = shapeRender.shapeData.dragMode;

            switch (mode)
            {
                case ShapeData.DragMode.Horizontal:
                    target.y = startDragPos.y;
                    break;
                case ShapeData.DragMode.Vertical:
                    target.x = startDragPos.x;
                    break;
                case ShapeData.DragMode.Free:
                default:
                    // no constraint
                    break;
            }
        }

        rb.MovePosition(target);
    }

    void OnMouseUp()
    {
        SetColliders(false);

        // SNAP TẠI VỊ TRÍ HIỆN TẠI (đang bị cản)
        SnapAtCurrentPosition();

        // GÁN GRID MỚI
        if (CanPlaceCurrentShape())
        {
            UpdateGridData();
        }
        else
        {
            transform.position = startDragPos;
            UpdateGridData();
        }

        SetColliders(true);
        rb.bodyType = RigidbodyType2D.Kinematic;

        GridManager.instance.LogParentId();
        if (shapeRender != null && shapeRender.shapeData != null && shapeRender.shapeData.pattern != null)
        {
            GridManager.instance.CheckPattern(shapeRender.shapeData.pattern);
        }
        else
        {
            GridManager.instance.CheckPattern();
        }
    }

    void SnapAtCurrentPosition()
    {
        Transform closestBlock = GetClosestBlockToCenter();

        if (closestBlock == null)
            return;

        GridCell nearest = GetNearestCell(closestBlock.position);

        if (nearest == null)
            return;

        Vector3 blockOffset =
            transform.position - closestBlock.position;

        transform.position =
            nearest.transform.position + blockOffset;
    }

    bool CanPlaceCurrentShape()
    {
        foreach (Transform child in transform)
        {
            GridCell cell = GetNearestCell(child.position);

            if (cell == null)
                return false;

            Block exist =
                GridManager.instance.GetBlock(cell.x, cell.y);

            if (exist != null)
                return false;
        }

        return true;
    }

    Transform GetClosestBlockToCenter()
    {
        Transform closest = null;
        float minDist = 99999f;

        foreach (Transform child in transform)
        {
            float dist =
                Vector2.Distance(
                    child.position,
                    transform.position
                );

            if (dist < minDist)
            {
                minDist = dist;
                closest = child;
            }
        }

        return closest;
    }

    void UpdateGridData()
    {
        foreach (Transform child in transform)
        {
            Block b = child.GetComponent<Block>();

            GridCell nearest = GetNearestCell(child.position);

            if (nearest != null)
            {
                b.origin = new Vector2Int(nearest.x, nearest.y);

                GridManager.instance.SetBlock(
                    nearest.x,
                    nearest.y,
                    b
                );
            }
        }
    }

    GridCell GetNearestCell(Vector3 pos)
    {
        GridCell nearest = null;
        float minDist = 99999f;

        foreach (var cell in GridManager.instance.cells)
        {
            float dist =
                Vector2.Distance(pos, cell.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = cell;
            }
        }

        return nearest;
    }

    void ClearCurrentShapeCells()
    {
        foreach (Transform child in transform)
        {
            Block b = child.GetComponent<Block>();

            if (b == null) continue;

            GridManager.instance.ClearBlockAt(
                b.origin.x,
                b.origin.y
            );
        }
    }
}