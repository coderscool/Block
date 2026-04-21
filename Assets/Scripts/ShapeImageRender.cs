using UnityEngine;

// ==========================================
// SHAPE IMAGE RENDERER
// render ShapeData ngoài world space
// ==========================================
public class ShapeImageRender : MonoBehaviour
{
    public ShapeData shapeData;

    [Header("Render")]
    public float cellSize = 1f;
    public float gap = 0.05f;

    [Header("Auto")]
    public bool renderOnStart = true;
    public bool clearOldBeforeRender = true;

    void Start()
    {
        if (renderOnStart)
            RenderShape();
    }

    [ContextMenu("Render Shape")]
    public void RenderShape()
    {
        if (shapeData == null) return;

        if (clearOldBeforeRender)
            ClearChildren();

        if (shapeData.shapePrefab == null)
        {
            Debug.LogWarning("Shape Prefab missing in ShapeData");
            return;
        }

        float startX = -(shapeData.columns - 1) * (cellSize + gap) / 2f;
        float startY = (shapeData.rows - 1) * (cellSize + gap) / 2f;

        for (int y = 0; y < shapeData.rows; y++)
        {
            for (int x = 0; x < shapeData.columns; x++)
            {
                if (!shapeData.board[y].column[x])
                    continue;

                GameObject block =
                    Instantiate(shapeData.shapePrefab, transform);

                float posX = startX + x * (cellSize + gap);
                float posY = startY - y * (cellSize + gap);

                block.transform.localPosition =
                    new Vector3(posX, posY, 0);
            }
        }
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}