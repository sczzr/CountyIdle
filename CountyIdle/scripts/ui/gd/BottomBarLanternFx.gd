extends Node

const HOVER_SFX_PATH := "res://assets/audio/ui/hover_ink.wav"
const PULSE_DURATION := 0.18
const RESET_DURATION := 0.12
const HOVER_SCALE := 1.05
const HOVER_MODULATE := Color(0.184, 0.298, 0.345, 1.0)
const BASE_MODULATE := Color(1.0, 1.0, 1.0, 1.0)
const HOVER_SFX_COOLDOWN_MS := 80

var _button_tweens: Dictionary = {}
var _button_base_scales: Dictionary = {}
var _hover_sfx_player: AudioStreamPlayer
var _last_hover_sfx_ticks_ms := 0


func _ready() -> void:
	_initialize_hover_sfx()

	var root: Node = get_parent()
	if root == null:
		return

	var buttons: Array[Button] = []
	_collect_buttons(root, buttons)

	for button: Button in buttons:
		button.self_modulate = BASE_MODULATE
		_update_button_hover_pivot(button)
		button.resized.connect(_on_button_resized.bind(button))
		_button_base_scales[button] = button.scale
		button.mouse_entered.connect(_on_button_hover_started.bind(button))
		button.mouse_exited.connect(_on_button_hover_ended.bind(button))
		button.focus_entered.connect(_on_button_hover_started.bind(button))
		button.focus_exited.connect(_on_button_hover_ended.bind(button))


func _initialize_hover_sfx() -> void:
	_hover_sfx_player = AudioStreamPlayer.new()
	_hover_sfx_player.name = "HoverSfxPlayer"
	_hover_sfx_player.stream = load(HOVER_SFX_PATH)
	add_child(_hover_sfx_player)


func _collect_buttons(node: Node, output: Array[Button]) -> void:
	for child_node: Node in node.get_children():
		var button: Button = child_node as Button
		if button != null:
			output.append(button)

		_collect_buttons(child_node, output)


func _on_button_resized(button: Button) -> void:
	_update_button_hover_pivot(button)


func _on_button_hover_started(button: Button) -> void:
	_stop_pulse(button, false)
	_update_button_hover_pivot(button)
	_play_hover_sfx()

	var base_scale := button.scale
	if _button_base_scales.has(button):
		base_scale = _button_base_scales[button]
	else:
		_button_base_scales[button] = base_scale

	var tween := create_tween()
	tween.set_trans(Tween.TRANS_SINE)
	tween.set_ease(Tween.EASE_IN_OUT)
	tween.tween_property(button, "scale", base_scale * HOVER_SCALE, PULSE_DURATION)
	tween.parallel().tween_property(button, "self_modulate", HOVER_MODULATE, PULSE_DURATION)
	_button_tweens[button] = tween


func _on_button_hover_ended(button: Button) -> void:
	_stop_pulse(button, true)


func _stop_pulse(button: Button, smooth_reset: bool) -> void:
	if _button_tweens.has(button):
		var active_tween := _button_tweens[button] as Tween
		if active_tween != null:
			active_tween.kill()
		_button_tweens.erase(button)

	var base_scale := Vector2.ONE
	if _button_base_scales.has(button):
		base_scale = _button_base_scales[button]
	else:
		_button_base_scales[button] = base_scale

	if not smooth_reset:
		button.scale = base_scale
		button.self_modulate = BASE_MODULATE
		return

	var reset_tween := create_tween()
	reset_tween.set_trans(Tween.TRANS_SINE)
	reset_tween.set_ease(Tween.EASE_OUT)
	reset_tween.tween_property(button, "scale", base_scale, RESET_DURATION)
	reset_tween.parallel().tween_property(button, "self_modulate", BASE_MODULATE, RESET_DURATION)


func _update_button_hover_pivot(button: Button) -> void:
	button.pivot_offset = button.size * 0.5


func _play_hover_sfx() -> void:
	if _hover_sfx_player == null or _hover_sfx_player.stream == null:
		return

	var now := Time.get_ticks_msec()
	if now - _last_hover_sfx_ticks_ms < HOVER_SFX_COOLDOWN_MS:
		return

	_last_hover_sfx_ticks_ms = now
	_hover_sfx_player.stop()
	_hover_sfx_player.play()
