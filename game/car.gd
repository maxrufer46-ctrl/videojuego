extends CharacterBody3D

var enabled_control := false
var speed := 0.0
var nitro := 1.0
var touch_throttle := 0.0
var touch_brake := 0.0
var touch_left := 0.0
var touch_right := 0.0
var touch_nitro := false
var touch_handbrake := false

const MAX_SPEED := 58.0
const MAX_REVERSE := 12.0
const ACCEL := 26.0
const BRAKE := 30.0
const COAST := 6.0
const TURN_RATE := 1.9
const NITRO_EXTRA := 24.0

func set_control(action: String, pressed: bool) -> void:
	match action:
		"accelerate": touch_throttle = 1.0 if pressed else 0.0
		"brake": touch_brake = 1.0 if pressed else 0.0
		"left": touch_left = 1.0 if pressed else 0.0
		"right": touch_right = 1.0 if pressed else 0.0
		"nitro": touch_nitro = pressed
		"handbrake": touch_handbrake = pressed

func speed_kph() -> int:
	return int(abs(speed) * 3.6)

func _physics_process(delta: float) -> void:
	if not enabled_control:
		speed = move_toward(speed, 0.0, COAST * delta)
		velocity = -transform.basis.z * speed
		move_and_slide()
		return

	var throttle := max(Input.get_action_strength("accelerate"), touch_throttle)
	var brake_input := max(Input.get_action_strength("brake"), touch_brake)
	var steer := (Input.get_action_strength("steer_right") + touch_right) - (Input.get_action_strength("steer_left") + touch_left)
	steer = clamp(steer, -1.0, 1.0)
	var using_nitro := (Input.is_action_pressed("nitro") or touch_nitro) and throttle > 0.1 and nitro > 0.02

	if throttle > 0.0:
		speed = move_toward(speed, MAX_SPEED, ACCEL * throttle * delta)
	elif brake_input > 0.0:
		if speed > 1.0:
			speed = move_toward(speed, 0.0, BRAKE * brake_input * delta)
		else:
			speed = move_toward(speed, -MAX_REVERSE, ACCEL * 0.6 * brake_input * delta)
	else:
		speed = move_toward(speed, 0.0, COAST * delta)

	if using_nitro:
		speed = move_toward(speed, MAX_SPEED + NITRO_EXTRA, 28.0 * delta)
		nitro = max(0.0, nitro - 0.22 * delta)
	else:
		nitro = min(1.0, nitro + 0.07 * delta)

	var handbrake := Input.is_action_pressed("handbrake") or touch_handbrake
	var steer_scale := clamp(abs(speed) / 8.0, 0.0, 1.0)
	var turn_mult := 1.35 if handbrake else 1.0
	if abs(speed) > 0.5:
		rotate_y(-steer * TURN_RATE * turn_mult * steer_scale * delta * sign(speed))

	if handbrake:
		speed = move_toward(speed, 0.0, 10.0 * delta)

	velocity = -transform.basis.z * speed
	velocity.y = -3.0
	move_and_slide()
