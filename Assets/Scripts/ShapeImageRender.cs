using UnityEngine;
using static ShapeData;

public class ShapeImageRender : MonoBehaviour
{
    public ShapeData shapeData;

    public float cellSize = 1f;
    public float gap = 0.05f;

    public string parentId = "Shape_01";
    public int indexId = 0;

    public Sprite[] imagePieces;

    void Start()
    {
        // nếu quên nhập thì lấy tên object
        if (string.IsNullOrEmpty(parentId))
            parentId = gameObject.name;

        SpawnShapeAsParent();
    }

    void SpawnShapeAsParent()
    {
        if (shapeData == null) return;
        if (shapeData.shapePrefab == null) return;

        GridCell firstCell =
            GridManager.instance.GetCell(shapeData.axisX, shapeData.axisY);

        if (firstCell == null) return;

        transform.position = firstCell.transform.position;

        for (int y = 0; y < shapeData.rows; y++)
        {
            for (int x = 0; x < shapeData.columns; x++)
            {
                if (!shapeData.board[y].column[x])
                    continue;

                int gridX = shapeData.axisX + x;
                int gridY = shapeData.axisY + y;

                if (!GridManager.instance.IsInside(gridX, gridY))
                    continue;

                GameObject block =
                    Instantiate(shapeData.shapePrefab, transform);

                SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
                if (sr == null) sr = block.AddComponent<SpriteRenderer>();

                int index = y * shapeData.columns + x;
                if (index < imagePieces.Length)
                    sr.sprite = imagePieces[index];

                AdjustSpriteToCell(block);

                BoxCollider2D col = block.GetComponent<BoxCollider2D>();
                if (col != null && sr != null)
                {
                    Vector2 spriteSize = sr.sprite.bounds.size;

                    // Giảm collider xuống 90% so với sprite
                    float shrinkFactor = 0.95f; // chỉnh tuỳ ý, ví dụ 0.95 hoặc 0.8

                    col.size = spriteSize * shrinkFactor;
                    col.offset = Vector2.zero;
                }

                float posX = x * (cellSize + gap);
                float posY = -y * (cellSize + gap);

                block.transform.localPosition =
                    new Vector3(posX, posY, 0);

                Block b = block.GetComponent<Block>();

                if (b == null)
                    b = block.AddComponent<Block>();

                // GÁN ID CHUẨN
                b.parentId = parentId;
                b.indexId = indexId;

                b.origin = new Vector2Int(gridX, gridY);

                GridManager.instance.SetBlock(gridX, gridY, b);
            }
        }

        Debug.Log("Spawn shape id = " + parentId);
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