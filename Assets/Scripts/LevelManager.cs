using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GridManager grid;
    public LevelData[] levels;
    public GameObject passLevel;
    //public List<ShapeImageRenderer> imageRenderer;

    int currentLevel = 0;

    private int currentBlockMatch;

    [Header("Shape Spawn")]
  
    public Transform shapeParent;

    private List<ShapeImageRender> currentShapes =
        new List<ShapeImageRender>();


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
        grid.ClearCurrentMap();

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
            GameObject obj = new GameObject("Shape"); // tạo object trống
            obj.transform.SetParent(shapeParent);

            ShapeImageRender renderer = obj.AddComponent<ShapeImageRender>();
            renderer.shapeData = data;
            renderer.parentId = data.parentId;
            renderer.indexId = data.indexId;
            renderer.imagePieces = data.imagePieces; // nếu có sprite pieces trong LevelData
            renderer.Init();

            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // đặt kinematic

            // Thêm script ShapeDrag
            obj.AddComponent<ShapeDrag>();

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