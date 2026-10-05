using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ControlType { Throttle, Brake, Nitro, Handbrake, SteerLeft, SteerRight }
    public ControlType control;

    public void OnPointerDown(PointerEventData eventData) => SetControl(true);
    public void OnPointerUp(PointerEventData eventData) => SetControl(false);

    private void SetControl(bool pressed)
    {
        switch (control)
        {
            case ControlType.Throttle:
                MobileInput.Throttle = pressed ? 1f : 0f;
                break;
            case ControlType.Brake:
                MobileInput.Brake = pressed ? 1f : 0f;
                break;
            case ControlType.Nitro:
                MobileInput.NitroHeld = pressed;
                break;
            case ControlType.Handbrake:
                MobileInput.HandbrakeHeld = pressed;
                break;
            case ControlType.SteerLeft:
                MobileInput.Steering = pressed ? -1f : (MobileInput.Steering < 0f ? 0f : MobileInput.Steering);
                break;
            case ControlType.SteerRight:
                MobileInput.Steering = pressed ? 1f : (MobileInput.Steering > 0f ? 0f : MobileInput.Steering);
                break;
        }
    }
}
