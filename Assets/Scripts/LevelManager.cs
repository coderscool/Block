using Microsoft.Unity.VisualStudio.Editor;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GridMap grid;
    public LevelData[] levels;
    public GameObject passLevel;
    public List<ShapeImageRenderer> imageRenderer;

    int currentLevel = 0;

    private int currentBlockMatch;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        passLevel.SetActive(false);
        LoadLevel(0);
    }

    public void LoadLevel(int index)
    {
        currentLevel = index;

        currentBlockMatch = levels[index].blockMatchTarget;

        grid.Init(levels[index]);

        foreach (ShapeImageRenderer renderer in imageRenderer) 
        {
            renderer.Init();
        }
    }

    public void OnBlockMatched(int amount)
    {
        currentBlockMatch -= amount;

        Debug.Log("Remain: " + currentBlockMatch);

        if (currentBlockMatch <= 0)
        {
            PassLevel();
        }
    }

    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel >= levels.Length)
            currentLevel = 0;

        LoadLevel(currentLevel);
    }

    public void PassLevel()
    {
        passLevel.SetActive(true);
    }

    public void ContinueLevel()
    {
        NextLevel();
        passLevel.SetActive(false);
    }

    public void ReloadLevel()
    {
        LoadLevel(currentLevel);
        passLevel.SetActive(false);
    }
}