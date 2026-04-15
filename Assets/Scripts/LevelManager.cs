using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GridMap grid;
    public LevelData[] levels;

    int currentLevel = 0;

    void Start()
    {
        LoadLevel(0);
    }

    public void LoadLevel(int index)
    {
        currentLevel = index;
        grid.Init(levels[index]);
    }

    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel >= levels.Length)
            currentLevel = 0;

        LoadLevel(currentLevel);
    }
}