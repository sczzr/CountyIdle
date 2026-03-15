extends Node2D

var _hover_points := PackedVector2Array()
var _style_name := "default"
var _pulse := 0.0


func _process(delta: float) -> void:
	if _hover_points.is_empty():
		return

	_pulse += delta * 4.0
	queue_redraw()


func show_hover(hex_points: PackedVector2Array, style_name: String = "default") -> void:
	_hover_points = hex_points
	_style_name = style_name
	visible = true
	queue_redraw()


func move_hover(hex_points: PackedVector2Array) -> void:
	_hover_points = hex_points
	if not visible:
		visible = true
	queue_redraw()


func hide_hover() -> void:
	_hover_points = PackedVector2Array()
	visible = false
	queue_redraw()


func _draw() -> void:
	if _hover_points.is_empty():
		return

	var pulse_alpha := 0.18 + sin(_pulse) * 0.04
	var fill_color := Color(0.93, 0.83, 0.46, pulse_alpha)
	var outline_color := Color(0.98, 0.92, 0.72, 0.88)
	if _style_name == "muted":
		fill_color = Color(0.72, 0.76, 0.88, 0.14)
		outline_color = Color(0.82, 0.87, 0.98, 0.72)

	draw_colored_polygon(_hover_points, fill_color)
	draw_polyline(_hover_points, outline_color, 2.2, true)
