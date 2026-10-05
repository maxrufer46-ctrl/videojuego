using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("Performance")]
    public float acceleration = 28f;
    public float reverseAcceleration = 16f;
    public float maxSpeedKph = 190f;
    public float steeringPower = 95f;
    public float brakePower = 36f;
    public float handbrakeDrag = 2.2f;

    [Header("Grip")]
    [Range(0f, 1f)] public float lateralGrip = 0.82f;
    public float downforce = 25f;
    public float normalDrag = 0.12f;

    [Header("Nitro")]
    public float nitroForce = 20f;
    public float nitroCapacity = 4f;
    public float nitroRechargePerSecond = 0.35f;

    [Header("Grounding")]
    public float groundRayLength = 1.15f;
    public LayerMask groundMask = ~0;

    public float SpeedKph { get; private set; }
    public float Nitro01 => nitroCapacity <= 0f ? 0f : nitroRemaining / nitroCapacity;
    public bool IsGrounded { get; private set; }

    private Rigidbody rb;
    private float nitroRemaining;
    private float steerInput;
    private float throttleInput;
    private float brakeInput;
    private bool nitroInput;
    private bool handbrakeInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.35f, 0.15f);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        nitroRemaining = nitroCapacity;
    }

    private void Update()
    {
        float keyboardSteer = Input.GetAxisRaw("Horizontal");
        float keyboardThrottle = Mathf.Max(0f, Input.GetAxisRaw("Vertical"));
        float keyboardBrake = Mathf.Max(0f, -Input.GetAxisRaw("Vertical"));

        steerInput = Mathf.Abs(MobileInput.Steering) > 0.01f ? MobileInput.Steering : keyboardSteer;
        throttleInput = Mathf.Max(MobileInput.Throttle, keyboardThrottle);
        brakeInput = Mathf.Max(MobileInput.Brake, keyboardBrake);
        nitroInput = MobileInput.NitroHeld || Input.GetKey(KeyCode.LeftShift);
        handbrakeInput = MobileInput.HandbrakeHeld || Input.GetKey(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        SpeedKph = rb.velocity.magnitude * 3.6f;
        IsGrounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, groundRayLength, groundMask, QueryTriggerInteraction.Ignore);

        if (!IsGrounded)
        {
            rb.AddForce(Vector3.down * downforce, ForceMode.Acceleration);
            return;
        }

        ApplyDrive();
        ApplySteering();
        ApplyGrip();
        ApplyBraking();
        ApplyNitro();
        rb.AddForce(-transform.up * downforce * Mathf.Clamp01(SpeedKph / 80f), ForceMode.Acceleration);
    }

    private void ApplyDrive()
    {
        float speedFactor = Mathf.Clamp01(1f - SpeedKph / maxSpeedKph);
        if (throttleInput > 0.01f && SpeedKph < maxSpeedKph)
            rb.AddForce(transform.forward * acceleration * throttleInput * (0.35f + 0.65f * speedFactor), ForceMode.Acceleration);

        if (brakeInput > 0.01f && Vector3.Dot(rb.velocity, transform.forward) < 1f && SpeedKph < 55f)
            rb.AddForce(-transform.forward * reverseAcceleration * brakeInput, ForceMode.Acceleration);
    }

    private void ApplySteering()
    {
        float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
        float steerScale = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 4f);
        if (steerScale < 0.05f) return;

        float direction = Mathf.Sign(forwardSpeed == 0f ? 1f : forwardSpeed);
        float angle = steerInput * steeringPower * steerScale * direction * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, angle, 0f));
    }

    private void ApplyGrip()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        float grip = handbrakeInput ? 0.35f : lateralGrip;
        localVelocity.x *= Mathf.Clamp01(grip);
        rb.velocity = transform.TransformDirection(localVelocity);
        rb.drag = handbrakeInput ? handbrakeDrag : normalDrag;
    }

    private void ApplyBraking()
    {
        if (brakeInput > 0.01f && Vector3.Dot(rb.velocity, transform.forward) > 0f)
            rb.AddForce(-rb.velocity.normalized * brakePower * brakeInput, ForceMode.Acceleration);

        if (handbrakeInput && rb.velocity.sqrMagnitude > 1f)
            rb.AddForce(-rb.velocity.normalized * 6f, ForceMode.Acceleration);
    }

    private void ApplyNitro()
    {
        if (nitroInput && throttleInput > 0.1f && nitroRemaining > 0f)
        {
            rb.AddForce(transform.forward * nitroForce, ForceMode.Acceleration);
            nitroRemaining = Mathf.Max(0f, nitroRemaining - Time.fixedDeltaTime);
        }
        else
        {
            nitroRemaining = Mathf.Min(nitroCapacity, nitroRemaining + nitroRechargePerSecond * Time.fixedDeltaTime);
        }
    }
}
