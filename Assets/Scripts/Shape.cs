using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public GameObject squareShapeImage;
    public ShapeData CurrentShapeData;

    private List<GameObject> _currentShape = new List<GameObject>();

    private void Start()
    {
        RequestNewShape(CurrentShapeData);
    }

    public void RequestNewShape(ShapeData shapeData)
    {
        CreateShape(shapeData);
    }

    private void CreateShape(ShapeData shapeData)
    {
        CurrentShapeData = shapeData;

        int totalSquareNumber = GetNumberOfSquares(shapeData);

        // Tạo đủ số lượng ô
        while (_currentShape.Count < totalSquareNumber)
        {
            _currentShape.Add(Instantiate(squareShapeImage, transform));
        }

        // Reset tất cả
        foreach (var square in _currentShape)
        {
            square.transform.localPosition = Vector3.zero;
            square.SetActive(false);
        }

        // Lấy kích thước ô
        var rect = squareShapeImage.GetComponent<RectTransform>();
        Vector2 moveDistance = new Vector2(
            rect.rect.width * rect.localScale.x,
            rect.rect.height * rect.localScale.y
        );

        int index = 0;

        for (int row = 0; row < shapeData.rows; row++)
        {
            for (int col = 0; col < shapeData.columns; col++)
            {
                if (shapeData.board[row].column[col])
                {
                    var square = _currentShape[index];
                    square.SetActive(true);

                    float x = GetXPosition(shapeData, col, moveDistance);
                    float y = GetYPosition(shapeData, row, moveDistance);

                    square.GetComponent<RectTransform>().localPosition = new Vector2(x, y);

                    index++;
                }
            }
        }
    }

    // Đếm số ô active
    private int GetNumberOfSquares(ShapeData shapeData)
    {
        int count = 0;

        foreach (var row in shapeData.board)
        {
            foreach (var cell in row.column)
            {
                if (cell) count++;
            }
        }

        return count;
    }

    // Tính vị trí X
    private float GetXPosition(ShapeData shapeData, int column, Vector2 moveDistance)
    {
        float center = (shapeData.columns - 1) / 2f;
        return (column - center) * moveDistance.x;
    }

    // Tính vị trí Y
    private float GetYPosition(ShapeData shapeData, int row, Vector2 moveDistance)
    {
        float center = (shapeData.rows - 1) / 2f;
        return (center - row) * moveDistance.y;
    }
}