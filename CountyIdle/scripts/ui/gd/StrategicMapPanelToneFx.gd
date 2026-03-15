extends Node

const DEFAULT_TITLE_COLOR := Color(0.93, 0.90, 0.80, 1.0)
const DEFAULT_TERRAIN_TINT := Color(1.0, 1.0, 1.0, 1.0)

var _title_label: Label
var _terrain_layer: CanvasItem


func _ready() -> void:
	var root: Node = get_parent()
	_title_label = root.get_node("Label")
	_terrain_layer = root.get_node_or_null("WorldTerrainTileLayer")
	reset_title_tone()
	reset_terrain_tint()


func apply_title_tone(accent: Color) -> void:
	_title_label.add_theme_color_override("font_color", accent)


func reset_title_tone() -> void:
	_title_label.add_theme_color_override("font_color", DEFAULT_TITLE_COLOR)


func apply_terrain_tint(tint: Color) -> void:
	if _terrain_layer == null:
		return

	_terrain_layer.modulate = tint


func reset_terrain_tint() -> void:
	if _terrain_layer == null:
		return

	_terrain_layer.modulate = DEFAULT_TERRAIN_TINT
