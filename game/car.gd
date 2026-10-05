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

const MAX_SPEED := 62.0
const MAX_REVERSE := 14.0
const ACCEL := 30.0
const BRAKE := 34.0
const COAST := 5.0
const TURN_RATE := 1.75
const NITRO_EXTRA := 25.0

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
		return

	var throttle: float = max(Input.get_action_strength("accelerate"), touch_throttle)
	var brake_input: float = max(Input.get_action_strength("brake"), touch_brake)
	var steer: float = (Input.get_action_strength("steer_right") + touch_right) - (Input.get_action_strength("steer_left") + touch_left)
	steer = clamp(steer, -1.0, 1.0)
	var using_nitro: bool = (Input.is_action_pressed("nitro") or touch_nitro) and throttle > 0.05 and nitro > 0.01

	if throttle > 0.05:
		speed = move_toward(speed, MAX_SPEED, ACCEL * throttle * delta)
	elif brake_input > 0.05:
		if speed > 0.5:
			speed = move_toward(speed, 0.0, BRAKE * delta)
		else:
			speed = move_toward(speed, -MAX_REVERSE, ACCEL * 0.55 * delta)
	else:
		speed = move_toward(speed, 0.0, COAST * delta)

	if using_nitro:
		speed = move_toward(speed, MAX_SPEED + NITRO_EXTRA, 34.0 * delta)
		nitro = max(0.0, nitro - 0.24 * delta)
	else:
		nitro = min(1.0, nitro + 0.08 * delta)

	var handbrake: bool = Input.is_action_pressed("handbrake") or touch_handbrake
	if abs(speed) > 0.25:
		var steer_strength: float = clamp(abs(speed) / 14.0, 0.28, 1.0)
		rotate_y(-steer * TURN_RATE * (1.35 if handbrake else 1.0) * steer_strength * delta * sign(speed))

	# Arcade translation is deliberate: it guarantees immediate response on Android
	# and avoids the prototype being locked by physics contacts.
	global_position += (-global_transform.basis.z) * speed * delta
	global_position.y = 0.2
