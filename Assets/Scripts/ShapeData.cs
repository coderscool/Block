using UnityEngine;

[CreateAssetMenu(fileName = "New Shape", menuName = "Game/Shape Data")]
public class ShapeData : ScriptableObject
{
    public int axisX = 0;
    public int axisY = 0;

    public GameObject shapePrefab;

    public Sprite[] imagePieces;

    public string parentId;
    public int indexId;

    [System.Serializable]
    public class Row
    {
        public bool[] column;

        public Row() { }

        public Row(int size)
        {
            CreateRow(size);
        }

        public void CreateRow(int size)
        {
            column = new bool[size];
        }

        public void ClearRow()
        {
            if (column == null) return;

            for (int i = 0; i < column.Length; i++)
            {
                column[i] = false;
            }
        }
    }

    public int columns = 0;
    public int rows = 0;

    public Row[] board;

    public void Clear()
    {
        if (board == null) return;

        for (int i = 0; i < board.Length; i++)
        {
            board[i].ClearRow();
        }
    }

    public void CreateNewBoard()
    {
        board = new Row[rows];

        for (int i = 0; i < rows; i++)
        {
            board[i] = new Row(columns);
        }
    }
}