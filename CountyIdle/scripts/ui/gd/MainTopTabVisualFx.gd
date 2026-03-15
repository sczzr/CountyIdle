extends Node

var _top_tab_row: Control
var _current_tween: Tween


func _ready() -> void:
	_top_tab_row = get_parent().get_node("TopTabRow")
	reset_state()


func play_tab_emphasis(_tab_name: String) -> void:
	_kill_tween()
	_top_tab_row.scale = Vector2.ONE
	_top_tab_row.modulate.a = 0.9
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_top_tab_row, "scale", Vector2.ONE * 1.01, 0.07)
	_current_tween.tween_property(_top_tab_row, "modulate:a", 1.0, 0.1)
	_current_tween.chain().tween_property(_top_tab_row, "scale", Vector2.ONE, 0.12)


func reset_state() -> void:
	_kill_tween()
	_top_tab_row.scale = Vector2.ONE
	_top_tab_row.modulate.a = 1.0


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
