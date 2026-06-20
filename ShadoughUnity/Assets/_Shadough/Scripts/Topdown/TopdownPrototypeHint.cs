using UnityEngine;

public class TopdownPrototypeHint : MonoBehaviour
{
    [SerializeField] private bool showHint = true;
    [SerializeField] private Vector2 hintPosition = new Vector2(12f, 12f);
    [SerializeField] private Vector2 hintSize = new Vector2(340f, 112f);

    private void OnGUI()
    {
        if (!showHint)
        {
            return;
        }

        string hintText =
            "W/A/S/D: Move\n" +
            "Hold Shift: Reveal\n" +
            "G: Plant / Retrieve Lantern\n" +
            "E: Cut while Revealing / Use Clock Core\n" +
            "Q: Cut Self Shadow while Revealing\n" +
            "F: Paste";

        GUI.Label(new Rect(hintPosition.x, hintPosition.y, hintSize.x, hintSize.y), hintText);
    }
}
