using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public int lives;
    public int gold;

    public int currentLevel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void UpdateLevel(int level)
    {
        currentLevel = level;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        gold += amount;
        //UpdateUI();
    }

    public void LoseLife(int amount = 1)
    {
        lives = Mathf.Max(0, lives - amount);
        //UpdateUI();
    }

    /*public void UpdateUI()
    {
        if (livesText != null)
            livesText.text = $"Lives: {lives}";

        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }*/
}