using UnityEngine;

public class TopdownGoalTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string successMessage = "Topdown prototype success";
    [SerializeField] private bool showGuiMessage = true;
    [SerializeField] private Vector2 messageSize = new Vector2(320f, 32f);

    private bool completed;

    public bool Completed => completed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed)
        {
            return;
        }

        if (other.CompareTag(playerTag))
        {
            completed = true;
            Debug.Log(successMessage);
        }
    }

    private void OnGUI()
    {
        if (!completed || !showGuiMessage)
        {
            return;
        }

        Rect messageRect = new Rect(
            (Screen.width - messageSize.x) * 0.5f,
            24f,
            messageSize.x,
            messageSize.y);

        GUI.Label(messageRect, "Prototype Complete");
    }
}
