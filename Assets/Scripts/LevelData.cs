using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int rows;
    public int cols;

    [TextArea(5, 10)]
    public string mapString;

    public int blockMatchTarget;

    public ShapeData[] levelShapes;

    [System.Serializable]
    public class ShapeMatchPattern
    {
        public string name;
        public List<Vector2Int> offsets = new List<Vector2Int>();
        public int requiredUniqueBlocks = 2;
    }

    [Header("Patterns")]
    public List<ShapeMatchPattern> patterns =
        new List<ShapeMatchPattern>();

    public int[,] GetMap()
    {
        int[,] map = new int[rows, cols];

        string[] lines = mapString.Split('\n');

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                map[r, c] = lines[r][c] - '0';
            }
        }

        return map;
    }
}
