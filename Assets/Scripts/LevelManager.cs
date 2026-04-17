using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GridMap grid;
    public LevelData[] levels;
    public GameObject passLevel;
    //public List<ShapeImageRenderer> imageRenderer;

    int currentLevel = 0;

    private int currentBlockMatch;

    [Header("Shape Spawn")]
  
    public Transform shapeParent;

    private List<ShapeImageRenderer> currentShapes =
        new List<ShapeImageRenderer>();


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

        SpawnShapes(levels[index]);
    }

    void SpawnShapes(LevelData level)
    {
        // Xóa shape cũ
        foreach (Transform child in shapeParent)
        {
            Destroy(child.gameObject);
        }

        currentShapes.Clear();

        // Tạo shape mới theo level
        foreach (ShapeData data in level.levelShapes)
        {
            GameObject obj =
                Instantiate(data.shapePrefab, shapeParent);

            ShapeImageRenderer renderer =
                obj.GetComponent<ShapeImageRenderer>();

            renderer.shapeData = data;
            renderer.Init();

            currentShapes.Add(renderer);
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