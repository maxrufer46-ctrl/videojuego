using UnityEngine;

public class HeatSystem : MonoBehaviour
{
    [Range(0f, 5f)] public float heat = 1f;
    public float heatGainPerSecond = 0.06f;
    public float heatLossPerSecond = 0.08f;
    public bool inPursuit = true;

    public int HeatLevel => Mathf.Clamp(Mathf.CeilToInt(heat), 1, 5);

    private void Update()
    {
        if (inPursuit)
            heat = Mathf.Min(5f, heat + heatGainPerSecond * Time.deltaTime);
        else
            heat = Mathf.Max(1f, heat - heatLossPerSecond * Time.deltaTime);
    }
}
