using UnityEngine;

public class LeverController : MonoBehaviour
{
    [SerializeField] private bool isActivated;
    [SerializeField] private DoorController targetDoor;

    public bool IsActivated => isActivated;
    public DoorController TargetDoor => targetDoor;

    public void Activate()
    {
        if (isActivated)
        {
            return;
        }

        isActivated = true;

        if (targetDoor != null)
        {
            targetDoor.Open();
        }

        Debug.Log("Lever activated: " + name);
    }

    public void Deactivate()
    {
        isActivated = false;

        if (targetDoor != null)
        {
            targetDoor.Close();
        }

        Debug.Log("Lever deactivated: " + name);
    }
}
