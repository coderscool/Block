using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GridManager grid;
    public LevelData[] levels;
    public GameObject passLevel;
    public GameObject failLevel;
    //public List<ShapeImageRenderer> imageRenderer;

    public TMP_Text levelText;

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
        failLevel.SetActive(false);

        if (PlayerData.Instance == null)
        {
            Debug.LogWarning("PlayerData.Instance is null. Add PlayerData component to a persistent GameObject.");
        }

        LoadLevel(0);
    }

    public void LoadLevel(int index)
    {
        grid.ClearCurrentMap();

        currentLevel = index;

        currentBlockMatch = levels[index].blockMatchTarget;

        grid.Init(levels[index]);

        CountdownTimer timer = FindObjectOfType<CountdownTimer>();
        if (timer != null)
        {
            timer.SetTime(levels[index].time, false);
            timer.waitForFirstClick = true;
        }

        levelText.text = "Level " + (currentLevel + 1);

        SpawnShapes(levels[index]);
    }

    void SpawnShapes(LevelData level)
    {
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

        if (PlayerData.Instance != null)
            PlayerData.Instance.UpdateLevel(currentLevel);

        LoadLevel(currentLevel);
    }

    public void PassLevel()
    {
        int reward = 0;
        if (levels != null && currentLevel >= 0 && currentLevel < levels.Length)
        {
            reward = levels[currentLevel].goldReward;
        }

        if (PlayerData.Instance != null && reward > 0)
            PlayerData.Instance.AddGold(reward);

        passLevel.SetActive(true);
    }

    public void ContinueLevel()
    {
        NextLevel();
        passLevel.SetActive(false);
    }

    public void FailLevel()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.LoseLife(1);
        }

        failLevel.SetActive(true);
    }

    public void ReloadLevel()
    {
        LoadLevel(currentLevel);
        failLevel.SetActive(false);
    }
}