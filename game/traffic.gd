extends CharacterBody3D

var lane_x := 0.0
var cruise_speed := 18.0
var direction_sign := -1.0
var min_z := -185.0
var max_z := 35.0

func setup(x: float, z: float, speed_value: float, dir: float) -> void:
	lane_x = x
	global_position = Vector3(x,0.2,z)
	cruise_speed = speed_value
	direction_sign = dir

func _physics_process(delta: float) -> void:
	global_position.z += cruise_speed * direction_sign * delta
	global_position.x = lane_x
	global_position.y = 0.2
	if global_position.z < min_z:
		global_position.z = max_z
	elif global_position.z > max_z:
		global_position.z = min_z
