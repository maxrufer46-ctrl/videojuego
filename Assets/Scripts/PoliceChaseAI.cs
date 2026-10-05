using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PoliceChaseAI : MonoBehaviour
{
    public Transform target;
    public float acceleration = 24f;
    public float maxSpeedKph = 165f;
    public float turnSpeed = 85f;
    public float catchDistance = 5f;
    public float rubberBandStrength = 0.12f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.3f, 0f);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        Vector3 flat = Vector3.ProjectOnPlane(toTarget, Vector3.up);
        if (flat.sqrMagnitude < 0.1f) return;

        Vector3 desiredDir = flat.normalized;
        float signed = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
        float turn = Mathf.Clamp(signed, -turnSpeed * Time.fixedDeltaTime, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));

        float speedKph = rb.velocity.magnitude * 3.6f;
        if (speedKph < maxSpeedKph)
        {
            float distanceBoost = Mathf.Clamp01((toTarget.magnitude - 12f) / 60f) * rubberBandStrength;
            rb.AddForce(transform.forward * acceleration * (1f + distanceBoost), ForceMode.Acceleration);
        }

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        localVelocity.x *= 0.76f;
        rb.velocity = transform.TransformDirection(localVelocity);
    }
}
