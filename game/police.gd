extends CharacterBody3D

var target: Node3D
var enabled_chase := false
var chase_speed := 31.0

func _physics_process(delta: float) -> void:
	if not enabled_chase or target == null:
		velocity = Vector3.ZERO
		return

	var to_target := target.global_position - global_position
	to_target.y = 0.0
	if to_target.length() < 1.0:
		velocity = Vector3.ZERO
		return

	var wanted := atan2(-to_target.x, -to_target.z)
	rotation.y = lerp_angle(rotation.y, wanted, 2.2 * delta)
	var distance_bonus := clamp((to_target.length() - 18.0) / 70.0, 0.0, 1.0) * 10.0
	velocity = -transform.basis.z * (chase_speed + distance_bonus)
	velocity.y = -0.5
	move_and_slide()
