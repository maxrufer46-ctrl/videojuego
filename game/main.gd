extends Node3D

var player
var police
var rival
var camera: Camera3D
var hud_speed: Label
var hud_heat: Label
var hud_status: Label
var nitro_bar: ProgressBar
var heat := 1.0
var race_started := false
var race_finished := false
var start_z := 10.0
var finish_z := -170.0
var active_touches := {}

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color(0.025, 0.03, 0.055))
	_build_environment()
	_build_city()
	_build_player()
	_build_police()
	_build_rival()
	_build_hud()
	_start_countdown()

func _input(event: InputEvent) -> void:
	if player == null: return
	if event is InputEventScreenTouch:
		if event.pressed:
			active_touches[event.index] = event.position
		else:
			active_touches.erase(event.index)
		_apply_touch_controls()
	elif event is InputEventScreenDrag:
		active_touches[event.index] = event.position
		_apply_touch_controls()

func _apply_touch_controls() -> void:
	var left := false
	var right := false
	var gas := false
	var brake := false
	var nitro_touch := false
	var drift := false
	for p in active_touches.values():
		var pos: Vector2 = p
		if pos.x < 180 and pos.y > 500: left = true
		elif pos.x < 360 and pos.y > 500: right = true
		if pos.x > 1070 and pos.y > 500: gas = true
		if pos.x > 770 and pos.x < 960 and pos.y > 535: brake = true
		if pos.x > 940 and pos.x < 1120 and pos.y > 410 and pos.y < 550: nitro_touch = true
		if pos.x > 770 and pos.x < 950 and pos.y > 410 and pos.y < 550: drift = true
	player.set_control("left", left)
	player.set_control("right", right)
	player.set_control("accelerate", gas)
	player.set_control("brake", brake)
	player.set_control("nitro", nitro_touch)
	player.set_control("handbrake", drift)

func _process(delta: float) -> void:
	if player == null: return
	if race_started and not race_finished:
		heat = min(5.0, heat + 0.025 * delta)
		if player.global_position.z < finish_z:
			race_finished = true
			player.enabled_control = false
			police.enabled_chase = false
			rival.active = false
			hud_status.text = "YOU ESCAPED!"
	hud_speed.text = str(player.speed_kph()) + " km/h"
	hud_heat.text = "HEAT " + str(int(ceil(heat))) + "   " + _stars(int(ceil(heat)))
	nitro_bar.value = player.nitro * 100.0
	if camera:
		var desired: Vector3 = player.global_transform * Vector3(0, 4.3, 8.5)
		camera.global_position = camera.global_position.lerp(desired, 1.0 - exp(-7.0 * delta))
		camera.fov = lerp(camera.fov, 82.0 if player.speed_kph() > 120 else 74.0, 1.0 - exp(-3.0 * delta))
		var look: Vector3 = player.global_position + (-player.global_transform.basis.z * 5.0) + Vector3.UP
		camera.look_at(look, Vector3.UP)

func _stars(count: int) -> String:
	var s := ""
	for i in range(5): s += "★" if i < count else "☆"
	return s

func _build_environment() -> void:
	var world := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color(0.025, 0.03, 0.055)
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.25, 0.30, 0.45)
	env.ambient_light_energy = 0.8
	world.environment = env
	add_child(world)
	var moon := DirectionalLight3D.new()
	moon.rotation_degrees = Vector3(-48, -25, 0)
	moon.light_color = Color(0.65, 0.75, 1.0)
	moon.light_energy = 1.1
	moon.shadow_enabled = true
	add_child(moon)

func _mat(color: Color, emission := Color(0,0,0)) -> StandardMaterial3D:
	var m := StandardMaterial3D.new()
	m.albedo_color = color
	m.roughness = 0.72
	if emission != Color(0,0,0):
		m.emission_enabled = true
		m.emission = emission
		m.emission_energy_multiplier = 2.5
	return m

func _box(name: String, pos: Vector3, size: Vector3, color: Color) -> StaticBody3D:
	var body := StaticBody3D.new()
	body.name = name
	body.position = pos
	var mesh := MeshInstance3D.new()
	var box := BoxMesh.new()
	box.size = size
	mesh.mesh = box
	mesh.material_override = _mat(color)
	body.add_child(mesh)
	var col := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = size
	col.shape = shape
	body.add_child(col)
	add_child(body)
	return body

func _build_city() -> void:
	_box("Road", Vector3(0,-0.35,-80), Vector3(18,0.5,220), Color(0.055,0.06,0.07))
	_box("SidewalkL", Vector3(-11,-0.1,-80), Vector3(4,0.5,220), Color(0.18,0.18,0.2))
	_box("SidewalkR", Vector3(11,-0.1,-80), Vector3(4,0.5,220), Color(0.18,0.18,0.2))
	for z in range(20, -190, -12):
		_box("Line", Vector3(0,0.01,z), Vector3(0.18,0.03,5), Color(0.9,0.82,0.45))
		for side in [-1,1]:
			var x := float(side) * (16.0 + float((abs(z) / 12)) * 0.08)
			var h := 7.0 + float((abs(z) / 12) % 5) * 2.5
			_box("Building", Vector3(x,h/2.0-0.1,z), Vector3(8,h,9), Color(0.09,0.1,0.14))
	for z in range(10, -180, -30):
		for side in [-1,1]:
			var lamp := OmniLight3D.new()
			lamp.position = Vector3(side*8.5,4,z)
			lamp.light_color = Color(0.2,0.55,1.0)
			lamp.light_energy = 2.2
			lamp.omni_range = 11.0
			add_child(lamp)
	_box("FinishL", Vector3(-8.5,1.8,finish_z), Vector3(0.5,3.6,0.5), Color(1,0.2,0.1))
	_box("FinishR", Vector3(8.5,1.8,finish_z), Vector3(0.5,3.6,0.5), Color(1,0.2,0.1))
	_box("FinishTop", Vector3(0,3.5,finish_z), Vector3(17.5,0.45,0.5), Color(1,0.65,0.05))

func _car_visual(root: Node3D, color: Color, police_car := false) -> void:
	var body := MeshInstance3D.new()
	var mesh := BoxMesh.new()
	mesh.size = Vector3(1.9,0.55,4.2)
	body.mesh = mesh
	body.position.y = 0.55
	body.material_override = _mat(color)
	root.add_child(body)
	var hood := MeshInstance3D.new()
	var hood_mesh := BoxMesh.new()
	hood_mesh.size = Vector3(1.78,0.18,1.45)
	hood.mesh = hood_mesh
	hood.position = Vector3(0,0.84,-1.18)
	hood.material_override = _mat(color.lightened(0.08))
	root.add_child(hood)
	var rear := MeshInstance3D.new()
	var rear_mesh := BoxMesh.new()
	rear_mesh.size = Vector3(1.82,0.2,1.0)
	rear.mesh = rear_mesh
	rear.position = Vector3(0,0.82,1.45)
	rear.material_override = _mat(color.darkened(0.08))
	root.add_child(rear)
	var cabin := MeshInstance3D.new()
	var cab := BoxMesh.new()
	cab.size = Vector3(1.55,0.5,1.9)
	cabin.mesh = cab
	cabin.position = Vector3(0,1.0,0.15)
	cabin.material_override = _mat(Color(0.035,0.055,0.08))
	root.add_child(cabin)
	for x in [-0.62,0.62]:
		var head := OmniLight3D.new()
		head.position = Vector3(x,0.72,-2.15)
		head.light_color = Color(0.78,0.9,1.0)
		head.light_energy = 1.2
		head.omni_range = 7.0
		root.add_child(head)
	for x in [-0.68,0.68]:
		var tail := MeshInstance3D.new()
		var tail_mesh := BoxMesh.new()
		tail_mesh.size = Vector3(0.42,0.16,0.08)
		tail.mesh = tail_mesh
		tail.position = Vector3(x,0.68,2.12)
		tail.material_override = _mat(Color(0.2,0.02,0.02), Color(1.0,0.03,0.01))
		root.add_child(tail)
	for x in [-1.0,1.0]:
		for z in [-1.35,1.35]:
			var wheel := MeshInstance3D.new()
			var cyl := CylinderMesh.new()
			cyl.top_radius = 0.38
			cyl.bottom_radius = 0.38
			cyl.height = 0.3
			wheel.mesh = cyl
			wheel.rotation_degrees.z = 90
			wheel.position = Vector3(x,0.35,z)
			wheel.material_override = _mat(Color(0.015,0.015,0.018))
			root.add_child(wheel)
	if police_car:
		var bar := MeshInstance3D.new()
		var bm := BoxMesh.new()
		bm.size = Vector3(1.3,0.12,0.28)
		bar.mesh = bm
		bar.position = Vector3(0,1.35,0.05)
		bar.material_override = _mat(Color(0.15,0.15,0.2), Color(0.2,0.4,1.0))
		root.add_child(bar)

func _setup_car(script_path: String, pos: Vector3, color: Color, cop := false):
	var car := CharacterBody3D.new()
	car.set_script(load(script_path))
	car.position = pos
	var col := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(1.9,1.1,4.2)
	col.shape = shape
	col.position.y = 0.55
	car.add_child(col)
	_car_visual(car,color,cop)
	add_child(car)
	return car

func _build_player() -> void:
	player = _setup_car("res://game/car.gd", Vector3(0,0.2,start_z), Color(0.08,0.55,0.95))
	camera = Camera3D.new()
	camera.fov = 74
	camera.position = player.position + Vector3(0,4.3,8.5)
	add_child(camera)

func _build_police() -> void:
	police = _setup_car("res://game/police.gd", Vector3(0,0.2,23), Color(0.12,0.12,0.14), true)
	police.target = player

func _build_rival() -> void:
	rival = _setup_car("res://game/rival.gd", Vector3(3,0.2,start_z-2), Color(0.95,0.2,0.12))

func _label(parent: Node, text: String, pos: Vector2, size: int) -> Label:
	var l := Label.new()
	l.text = text
	l.position = pos
	l.add_theme_font_size_override("font_size", size)
	l.add_theme_color_override("font_color", Color.WHITE)
	parent.add_child(l)
	return l

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 10
	add_child(layer)
	hud_speed = _label(layer, "0 km/h", Vector2(1020,40), 36)
	hud_heat = _label(layer, "HEAT 1  ★☆☆☆☆", Vector2(35,35), 27)
	hud_status = _label(layer, "STREET HEAT", Vector2(480,70), 38)
	_label(layer, "v0.3 • MULTITOUCH", Vector2(520,118), 15)
	_label(layer, "NITRO", Vector2(1020,92), 18)
	nitro_bar = ProgressBar.new()
	nitro_bar.position = Vector2(1020,120)
	nitro_bar.size = Vector2(200,20)
	nitro_bar.max_value = 100
	nitro_bar.value = 100
	nitro_bar.show_percentage = false
	layer.add_child(nitro_bar)
	_touch_button(layer, "◀", Vector2(40,540), Vector2(135,135), "left")
	_touch_button(layer, "▶", Vector2(190,540), Vector2(135,135), "right")
	_touch_button(layer, "BRAKE", Vector2(800,565), Vector2(140,105), "brake")
	_touch_button(layer, "GAS", Vector2(1090,540), Vector2(150,135), "accelerate")
	_touch_button(layer, "DRIFT", Vector2(800,445), Vector2(140,90), "handbrake")
	_touch_button(layer, "NITRO", Vector2(955,445), Vector2(140,90), "nitro")
	var restart := Button.new()
	restart.text = "RESTART"
	restart.position = Vector2(560,640)
	restart.size = Vector2(160,55)
	restart.pressed.connect(_restart)
	layer.add_child(restart)

func _touch_button(parent: Node, text: String, pos: Vector2, size: Vector2, action: String) -> void:
	var b := Button.new()
	b.text = text
	b.position = pos
	b.size = size
	b.add_theme_font_size_override("font_size", 22)
	b.modulate = Color(1,1,1,0.78)
	b.button_down.connect(func(): player.set_control(action,true))
	b.button_up.connect(func(): player.set_control(action,false))
	parent.add_child(b)

func _start_countdown() -> void:
	hud_status.text = "3"
	await get_tree().create_timer(0.8).timeout
	hud_status.text = "2"
	await get_tree().create_timer(0.8).timeout
	hud_status.text = "1"
	await get_tree().create_timer(0.8).timeout
	hud_status.text = "GO!  ESCAPE THE COPS"
	player.enabled_control = true
	police.enabled_chase = true
	rival.active = true
	race_started = true
	await get_tree().create_timer(1.3).timeout
	if not race_finished: hud_status.text = "BEAT THE RED CAR • REACH THE GATE"

func _restart() -> void:
	get_tree().reload_current_scene()
