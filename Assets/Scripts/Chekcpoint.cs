using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LapManager.instance
                .PlayerPassedCheckpoint(checkpointIndex);
        }
    }
}