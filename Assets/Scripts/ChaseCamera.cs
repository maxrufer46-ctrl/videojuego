using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.7f, -7.2f);
    public float positionSmooth = 7f;
    public float rotationSmooth = 9f;
    public float lookAhead = 4f;

    private Rigidbody targetRb;

    private void Start()
    {
        if (target != null) targetRb = target.GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-positionSmooth * Time.deltaTime));

        Vector3 velocityLook = targetRb != null ? targetRb.velocity * 0.08f : Vector3.zero;
        Vector3 lookPoint = target.position + target.forward * lookAhead + velocityLook;
        Quaternion desiredRot = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime));
    }
}
