using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TMP_Text timerText;

    private float currentTime;

    private bool timerRunning = true;

    void Update()
    {
        if (!timerRunning)
            return;

        currentTime += Time.deltaTime;

        int minutes =
            Mathf.FloorToInt(currentTime / 60);

        int seconds =
            Mathf.FloorToInt(currentTime % 60);

        int milliseconds =
            Mathf.FloorToInt((currentTime * 1000) % 1000);

        if (timerText != null)
        {
            timerText.text =
                string.Format(
                    "{0:00}:{1:00}:{2:000}",
                    minutes,
                    seconds,
                    milliseconds
                );
        }
    }

    public void StopTimer()
    {
        timerRunning = false;
    }
}