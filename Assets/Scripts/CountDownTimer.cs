using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    public float timeRemaining = 60f; 
    public bool isRunning = true;

    public bool waitForFirstClick = true;

    // internal flag để biết đã bắt đầu do click chưa
    private bool startedByClick = false;

    public TMP_Text timerText;

    void Update()
    {
        // Nếu chưa chạy, kiểm tra click để bắt đầu (nếu bật waitForFirstClick)
        if (!isRunning)
        {
            if (waitForFirstClick && !startedByClick && Input.GetMouseButtonDown(0))
            {
                isRunning = true;
                startedByClick = true;
            }
            else
            {
                return;
            }
        }

        if (isRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                isRunning = false;
                Debug.Log("Hết giờ!");
                LevelManager.Instance.FailLevel();
            }
        }
    }

    void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void SetTime(float time, bool startImmediately = false)
    {
        timeRemaining = time;
        UpdateTimerDisplay(timeRemaining);
        isRunning = startImmediately;
        startedByClick = startImmediately;
    }
}