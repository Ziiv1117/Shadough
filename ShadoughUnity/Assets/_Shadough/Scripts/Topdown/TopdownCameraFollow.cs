using UnityEngine;

public class TopdownCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float followSmoothTime = 0.12f;
    [SerializeField] private bool snapOnStart = true;

    private Vector3 velocity;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (snapOnStart && target != null)
        {
            transform.position = GetTargetPosition();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = GetTargetPosition();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSmoothTime);
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPosition = target.position + offset;
        targetPosition.z = offset.z;
        return targetPosition;
    }
}
