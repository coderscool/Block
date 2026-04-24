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

    public float time = 60f;

    public ShapeData[] levelShapes;

    public ShapeMatchPattern[] patterns;

    [Header("Rewards")]
    public int goldReward = 10;

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
