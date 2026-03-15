extends Control

const INK_GRAY := Color(0.478, 0.478, 0.478, 1.0)

var _stats: Array = []
var _axis_labels: Array[Label] = []


func _ready() -> void:
	custom_minimum_size = Vector2(320, 320)
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	size_flags_horizontal = Control.SIZE_EXPAND_FILL


func set_stats(stats: Array) -> void:
	_stats = stats.duplicate(true)
	_ensure_labels()
	_layout_labels()
	queue_redraw()


func _notification(what: int) -> void:
	if what == NOTIFICATION_RESIZED:
		_layout_labels()
		queue_redraw()


func _draw() -> void:
	if _stats.size() < 3:
		return

	var center: Vector2 = size / 2.0
	var radius: float = minf(size.x, size.y) * 0.30
	var directions: Array[Vector2] = _build_directions(_stats.size())

	draw_circle(center, radius * 1.04, Color(0.12, 0.10, 0.08, 0.02))

	for ring in range(1, 6):
		var ring_factor: float = float(ring) / 5.0
		var ring_points: Array[Vector2] = []
		for direction in directions:
			ring_points.append(center + direction * radius * ring_factor)
		draw_polyline(_to_closed_loop(ring_points), Color(0.48, 0.48, 0.45, 0.35), 1.2, true)

	for direction in directions:
		draw_line(center, center + direction * radius, Color(0.46, 0.45, 0.42, 0.42), 1.0, true)

	var data_points: Array[Vector2] = []
	for index in range(_stats.size()):
		var stat: Dictionary = _stats[index]
		var value: float = clampf(float(stat.get("value", 0)), 0.0, 100.0)
		data_points.append(center + directions[index] * radius * (value / 100.0))

	draw_colored_polygon(data_points, Color(0.12, 0.11, 0.10, 0.05))
	draw_polyline(_to_closed_loop(data_points), Color(0.18, 0.18, 0.18, 0.92), 2.0, true)

	for point in data_points:
		draw_circle(point, 3.2, Color(0.18, 0.18, 0.18, 0.88))


func _ensure_labels() -> void:
	while _axis_labels.size() < _stats.size():
		var label := Label.new()
		label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		label.add_theme_font_size_override("font_size", 12)
		label.add_theme_color_override("font_color", INK_GRAY)
		add_child(label)
		_axis_labels.append(label)

	for index in range(_axis_labels.size()):
		var label := _axis_labels[index]
		if index < _stats.size():
			var stat: Dictionary = _stats[index]
			label.text = str(stat.get("label", ""))
			label.visible = true
		else:
			label.visible = false


func _layout_labels() -> void:
	if _stats.is_empty():
		return

	var center: Vector2 = size / 2.0
	var radius: float = minf(size.x, size.y) * 0.38
	var directions: Array[Vector2] = _build_directions(_stats.size())

	for index in range(_stats.size()):
		var label := _axis_labels[index]
		label.size = label.get_combined_minimum_size()
		label.position = center + directions[index] * radius - label.size / 2.0


func _build_directions(count: int) -> Array[Vector2]:
	var directions: Array[Vector2] = []
	for index in range(count):
		var angle: float = (-PI / 2.0) + (TAU * float(index) / float(count))
		directions.append(Vector2(cos(angle), sin(angle)))
	return directions


func _to_closed_loop(points: Array[Vector2]) -> PackedVector2Array:
	var result: PackedVector2Array = PackedVector2Array()
	for point in points:
		result.append(point)
	if not points.is_empty():
		result.append(points[0])
	return result
