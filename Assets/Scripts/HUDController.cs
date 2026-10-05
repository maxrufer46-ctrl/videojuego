using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public ArcadeCarController car;
    public HeatSystem heat;
    public Text speedText;
    public Text heatText;
    public Image nitroFill;

    private void Update()
    {
        if (car != null)
        {
            if (speedText != null) speedText.text = Mathf.RoundToInt(car.SpeedKph) + " km/h";
            if (nitroFill != null) nitroFill.fillAmount = car.Nitro01;
        }

        if (heat != null && heatText != null)
            heatText.text = "HEAT " + heat.HeatLevel;
    }
}
