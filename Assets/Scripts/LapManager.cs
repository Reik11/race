using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LapManager : MonoBehaviour
{
    public static LapManager instance;

    // =================================
    // LAP
    // =================================
    [Header("Lap")]
    public int currentLap = 1;
    public int maxLap = 3;

    // =================================
    // CHECKPOINT
    // =================================
    [Header("Checkpoint")]
    public int totalCheckpoints = 4;

    private int nextCheckpoint = 0;

    // =================================
    // UI
    // =================================
    [Header("UI")]
    public TMP_Text lapText;
    public TMP_Text wrongWayText;

    // =================================
    // PLAYER
    // =================================
    [Header("Player")]
    public Transform player;

    // =================================
    // CHECKPOINT ARRAY
    // =================================
    [Header("Checkpoint Array")]
    public Transform[] checkpoints;

    // =================================
    // WRONG WAY
    // =================================
    [Header("Wrong Way")]
    public float wrongWayAngle = 140f;

    // =================================
    // FINISH UI
    // =================================
    [Header("Finish UI")]
    public GameObject finishPanel;

    // =================================
    // PRIVATE
    // =================================
    private bool raceFinished = false;

    // =================================
    // AWAKE
    // =================================
    private void Awake()
    {
        instance = this;
    }

    // =================================
    // START
    // =================================
    void Start()
    {
        UpdateLapUI();

        // sembunyikan wrong way
        if (wrongWayText != null)
        {
            wrongWayText.gameObject.SetActive(false);
        }

        // sembunyikan finish panel
        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }
    }

    // =================================
    // UPDATE
    // =================================
    void Update()
    {
        CheckWrongWay();
    }

    // =================================
    // CHECKPOINT SYSTEM
    // =================================
    public void PlayerPassedCheckpoint(int checkpointIndex)
{
    // checkpoint valid
    if (checkpointIndex == nextCheckpoint)
    {
        Debug.Log(
            "CHECKPOINT VALID → " +
            checkpointIndex
        );

        nextCheckpoint++;

        Debug.Log(
            "NEXT CHECKPOINT TARGET → " +
            nextCheckpoint
        );
    }
    else
    {
        Debug.Log(
            "CHECKPOINT SALAH / BELUM VALID → " +
            checkpointIndex +
            " | TARGET SEKARANG → " +
            nextCheckpoint
        );
    }
}

    // =================================
    // FINISH / LAP SYSTEM
    // =================================
    public void TryCompleteLap()
    {
        // race sudah selesai
        if (raceFinished)
            return;

        // semua checkpoint harus selesai
        if (nextCheckpoint >= totalCheckpoints)
        {
            currentLap++;

            Debug.Log("Lap Complete");

            // reset checkpoint
            nextCheckpoint = 0;

            UpdateLapUI();

            // finish race
            if (currentLap > maxLap)
            {
                raceFinished = true;

                Debug.Log("FINISH");

                // stop timer
                TimerManager timer =
                    FindObjectOfType<TimerManager>();

                if (timer != null)
                {
                    timer.StopTimer();
                }

                // tampilkan panel finish
                StartCoroutine(ShowFinishPanel());

                return;
            }
        }
        else
        {
            Debug.Log("Checkpoint belum lengkap");
        }
    }

    // =================================
    // UPDATE LAP UI
    // =================================
    void UpdateLapUI()
    {
        if (lapText != null)
        {
            lapText.text =
                "LAP " + currentLap + " / " + maxLap;
        }
    }

    // =================================
    // WRONG WAY SYSTEM
    // =================================
    void CheckWrongWay()
    {
        // tidak ada checkpoint
        if (checkpoints.Length == 0)
            return;

        // semua checkpoint selesai
        if (nextCheckpoint >= checkpoints.Length)
            return;

        // checkpoint target
        Transform targetCheckpoint =
            checkpoints[nextCheckpoint];

        // arah ke checkpoint
        Vector3 directionToCheckpoint =
            (targetCheckpoint.position - player.position)
            .normalized;

        // ambil rigidbody player
        Rigidbody rb =
            player.GetComponent<Rigidbody>();

        if (rb == null)
            return;

        // arah gerakan mobil
        Vector3 moveDirection =
            rb.linearVelocity.normalized;

        // jika mobil diam
        if (moveDirection.magnitude < 0.1f)
        {
            if (wrongWayText != null)
            {
                wrongWayText.gameObject.SetActive(false);
            }

            return;
        }

        // hitung angle
        float angle =
            Vector3.Angle(
                moveDirection,
                directionToCheckpoint
            );

        Debug.Log("Wrong Way Angle : " + angle);

        // WRONG WAY
        if (angle > wrongWayAngle)
        {
            if (wrongWayText != null)
            {
                wrongWayText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (wrongWayText != null)
            {
                wrongWayText.gameObject.SetActive(false);
            }
        }
    }

    // =================================
    // FINISH PANEL DELAY
    // =================================
    IEnumerator ShowFinishPanel()
    {
        // tunggu 3 detik
        yield return new WaitForSeconds(3f);

        // tampilkan panel finish
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
    }

    // =================================
    // LOAD LEVEL 2
    // =================================
    public void LoadLevel2()
    {
        SceneManager.LoadScene("Level2");
    }
}