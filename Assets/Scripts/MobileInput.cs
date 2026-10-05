using UnityEngine;

public static class MobileInput
{
    public static float Steering { get; set; }
    public static float Throttle { get; set; }
    public static float Brake { get; set; }
    public static bool NitroHeld { get; set; }
    public static bool HandbrakeHeld { get; set; }

    public static void ResetAll()
    {
        Steering = 0f;
        Throttle = 0f;
        Brake = 0f;
        NitroHeld = false;
        HandbrakeHeld = false;
    }
}
