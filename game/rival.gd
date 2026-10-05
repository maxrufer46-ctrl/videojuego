extends CharacterBody3D

var active := false
var finished := false
var race_speed := 35.0

func _physics_process(_delta: float) -> void:
	if not active or finished:
		velocity = Vector3.ZERO
		return

	velocity = Vector3(0.0, -0.5, -race_speed)
	move_and_slide()
	if global_position.z < -175.0:
		finished = true
		velocity = Vector3.ZERO
