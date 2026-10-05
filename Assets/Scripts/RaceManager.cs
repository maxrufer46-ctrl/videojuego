using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public int totalCheckpoints = 4;
    public int lapsToWin = 2;
    public int CurrentCheckpoint { get; private set; }
    public int CurrentLap { get; private set; } = 1;
    public bool RaceFinished { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (RaceFinished) return;
        RaceCheckpoint checkpoint = other.GetComponent<RaceCheckpoint>();
        if (checkpoint == null || checkpoint.index != CurrentCheckpoint) return;

        CurrentCheckpoint++;
        if (CurrentCheckpoint >= totalCheckpoints)
        {
            CurrentCheckpoint = 0;
            CurrentLap++;
            if (CurrentLap > lapsToWin)
            {
                RaceFinished = true;
                CurrentLap = lapsToWin;
            }
        }
    }
}
