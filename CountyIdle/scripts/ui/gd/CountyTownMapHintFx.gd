extends Node

const DEFAULT_HINT_COLOR := Color(0.176471, 0.145098, 0.12549, 1.0)

var _hint_label: Label


func _ready() -> void:
	var root: Node = get_parent()
	_hint_label = root.get_node("MapHintLabel")
	reset_hint_tone()


func apply_hint_tone(accent: Color) -> void:
	_hint_label.add_theme_color_override("font_color", accent)


func reset_hint_tone() -> void:
	_hint_label.add_theme_color_override("font_color", DEFAULT_HINT_COLOR)
