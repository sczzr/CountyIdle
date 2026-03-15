extends Node

const HOVER_SFX_PATH := "res://assets/audio/ui/hover_ink.wav"
const PULSE_DURATION := 0.18
const RESET_DURATION := 0.12
const HOVER_SCALE := 1.05
const HOVER_MODULATE := Color(0.184, 0.298, 0.345, 1.0)
const BASE_MODULATE := Color(1.0, 1.0, 1.0, 1.0)
const HOVER_SFX_COOLDOWN_MS := 80
const DROPDOWN_PAPER_COLOR := Color(0.95, 0.92, 0.84, 1.0)
const DROPDOWN_INK_COLOR := Color(0.17, 0.15, 0.13, 1.0)
const DROPDOWN_BORDER_COLOR := Color(0.29, 0.25, 0.21, 0.95)

var _button_tweens: Dictionary = {}
var _button_base_scales: Dictionary = {}
var _hover_locked_buttons: Dictionary = {}
var _bound_buttons: Dictionary = {}
var _hover_sfx_player: AudioStreamPlayer
var _last_hover_sfx_ticks_ms := 0


func _ready() -> void:
	_initialize_hover_sfx()


func _exit_tree() -> void:
	for active_tween in _button_tweens.values():
		var tween: Tween = active_tween as Tween
		if tween != null:
			tween.kill()

	_button_tweens.clear()


func bind_hover_fx() -> void:
	var root: Node = get_parent()
	if root == null:
		return

	var buttons: Array[Button] = []
	_collect_buttons(root, buttons)

	for button: Button in buttons:
		if _is_bottom_bar_button(button):
			continue
		if _bound_buttons.has(button):
			continue

		button.self_modulate = BASE_MODULATE
		_update_button_hover_pivot(button)
		_button_base_scales[button] = button.scale
		button.resized.connect(_on_button_resized.bind(button))
		button.mouse_entered.connect(_on_button_hover_started.bind(button))
		button.mouse_exited.connect(_on_button_hover_ended.bind(button))
		button.focus_entered.connect(_on_button_hover_started.bind(button))
		button.focus_exited.connect(_on_button_hover_ended.bind(button))
		_bound_buttons[button] = true

		var option_button: OptionButton = button as OptionButton
		if option_button != null:
			_style_option_popup(option_button)
			_register_option_button_hover_lock(option_button)


func _initialize_hover_sfx() -> void:
	_hover_sfx_player = AudioStreamPlayer.new()
	_hover_sfx_player.name = "HoverSfxPlayer"
	_hover_sfx_player.stream = load(HOVER_SFX_PATH)
	_hover_sfx_player.bus = &"Master"
	_hover_sfx_player.volume_db = -8.0
	add_child(_hover_sfx_player)


func _collect_buttons(node: Node, output: Array[Button]) -> void:
	for child_node: Node in node.get_children():
		var button: Button = child_node as Button
		if button != null:
			output.append(button)

		_collect_buttons(child_node, output)


func _is_bottom_bar_button(button: Button) -> bool:
	var current := button as Node
	while current != null:
		if String(current.name) == "BottomBar":
			return true
		current = current.get_parent()

	return false


func _on_button_resized(button: Button) -> void:
	_update_button_hover_pivot(button)


func _on_button_hover_started(button: Button) -> void:
	_stop_pulse(button, false)
	_update_button_hover_pivot(button)
	_play_hover_sfx()

	var base_scale: Vector2 = button.scale
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
	if _hover_locked_buttons.has(button):
		return

	if _button_tweens.has(button):
		var active_tween: Tween = _button_tweens[button] as Tween
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


func _register_option_button_hover_lock(option_button: OptionButton) -> void:
	if _hover_locked_buttons.has(StringName("registered_" + str(option_button.get_instance_id()))):
		return

	var popup: PopupMenu = option_button.get_popup()
	option_button.button_down.connect(_lock_option_button_hover.bind(option_button))
	option_button.pressed.connect(_lock_option_button_hover.bind(option_button))
	popup.about_to_popup.connect(_on_option_popup_about_to_popup.bind(option_button, popup))
	popup.popup_hide.connect(_on_option_popup_hide.bind(option_button))
	_hover_locked_buttons[StringName("registered_" + str(option_button.get_instance_id()))] = true


func _on_option_popup_about_to_popup(option_button: OptionButton, popup: PopupMenu) -> void:
	_lock_option_button_hover(option_button)
	_align_option_popup(option_button, popup)


func _on_option_popup_hide(option_button: OptionButton) -> void:
	_hover_locked_buttons.erase(option_button)
	_stop_pulse(option_button, true)


func _style_option_popup(option_button: OptionButton) -> void:
	var popup: PopupMenu = option_button.get_popup()
	popup.add_theme_stylebox_override("panel", _create_dropdown_panel_style())
	popup.add_theme_stylebox_override("hover", _create_dropdown_hover_style())
	popup.add_theme_stylebox_override("separator", _create_dropdown_separator_style())
	popup.add_theme_color_override("font_color", DROPDOWN_INK_COLOR)
	popup.add_theme_color_override("font_hover_color", DROPDOWN_PAPER_COLOR)
	popup.add_theme_color_override("font_disabled_color", Color(DROPDOWN_INK_COLOR.r, DROPDOWN_INK_COLOR.g, DROPDOWN_INK_COLOR.b, 0.45))


func _lock_option_button_hover(option_button: OptionButton) -> void:
	_hover_locked_buttons[option_button] = true
	_ensure_hover_visual(option_button)


func _ensure_hover_visual(button: Button) -> void:
	_update_button_hover_pivot(button)

	if _button_tweens.has(button):
		var active_tween: Tween = _button_tweens[button] as Tween
		if active_tween != null:
			active_tween.kill()
		_button_tweens.erase(button)

	var base_scale: Vector2 = button.scale
	if _button_base_scales.has(button):
		base_scale = _button_base_scales[button]
	else:
		_button_base_scales[button] = base_scale

	button.scale = base_scale * HOVER_SCALE
	button.self_modulate = HOVER_MODULATE


func _align_option_popup(option_button: OptionButton, popup: PopupMenu) -> void:
	var rect: Rect2 = option_button.get_global_rect()
	var target_pos: Vector2 = rect.position + Vector2(0.0, rect.size.y)
	var target_width := int(round(rect.size.x))

	popup.position = Vector2i(int(round(target_pos.x)), int(round(target_pos.y)))
	popup.min_size = Vector2i(target_width, int(round(popup.min_size.y)))
	popup.size = Vector2i(target_width, int(round(popup.size.y)))


func _create_dropdown_panel_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = DROPDOWN_PAPER_COLOR
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = DROPDOWN_BORDER_COLOR
	style.content_margin_left = 8
	style.content_margin_top = 6
	style.content_margin_right = 8
	style.content_margin_bottom = 6
	return style


func _create_dropdown_hover_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(HOVER_MODULATE.r, HOVER_MODULATE.g, HOVER_MODULATE.b, 0.22)
	return style


func _create_dropdown_separator_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(DROPDOWN_BORDER_COLOR.r, DROPDOWN_BORDER_COLOR.g, DROPDOWN_BORDER_COLOR.b, 0.45)
	return style


func _play_hover_sfx() -> void:
	if _hover_sfx_player == null or _hover_sfx_player.stream == null:
		return

	var now := Time.get_ticks_msec()
	if now - _last_hover_sfx_ticks_ms < HOVER_SFX_COOLDOWN_MS:
		return

	_last_hover_sfx_ticks_ms = now
	_hover_sfx_player.stop()
	_hover_sfx_player.play()

