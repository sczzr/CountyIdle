extends Button

# 可复用的确认印章按钮：负责悬停时的放大、旋转与朱红填充。
const SEAL_RED := Color(0.698, 0.133, 0.133, 1.0)
const PAPER_MAIN := Color(0.9569, 0.9451, 0.9176, 1.0)

@export var hover_scale := 1.08
@export var hover_rotation_degrees := 3.0
@export var hover_duration := 0.16

var _tween: Tween
var _hovered := false


func _ready() -> void:
	flat = false
	mouse_filter = Control.MOUSE_FILTER_STOP
	focus_mode = Control.FOCUS_NONE
	_sync_pivot()
	_hovered = is_hovered()
	_apply_state(_hovered)
	set_process(true)
	if not is_connected("resized", Callable(self, "_on_resized")):
		resized.connect(_on_resized)
	if not is_connected("mouse_entered", Callable(self, "_on_mouse_entered")):
		mouse_entered.connect(_on_mouse_entered)
	if not is_connected("mouse_exited", Callable(self, "_on_mouse_exited")):
		mouse_exited.connect(_on_mouse_exited)


func _notification(what: int) -> void:
	if what == NOTIFICATION_MOUSE_ENTER:
		_set_hovered(true)
	elif what == NOTIFICATION_MOUSE_EXIT:
		_set_hovered(false)


func _process(_delta: float) -> void:
	# 兜底处理：确保悬停状态和视觉同步。
	var now_hovered := is_hovered()
	if now_hovered != _hovered:
		_set_hovered(now_hovered)


func _set_hovered(value: bool) -> void:
	if _hovered == value:
		return
	_hovered = value
	_apply_state(_hovered)


func _on_resized() -> void:
	# 尺寸变化时同步中心点，避免缩放/旋转时偏移。
	_sync_pivot()




func _on_mouse_entered() -> void:
	_set_hovered(true)


func _on_mouse_exited() -> void:
	_set_hovered(false)


func _apply_state(hovered: bool) -> void:
	_apply_style(hovered)
	_apply_motion(hovered)


func _apply_style(hovered: bool) -> void:
	flat = false
	if hovered:
		# 悬停时显示朱红方印与白字。
		var hover_style := _create_fill_style(true)
		add_theme_stylebox_override("normal", hover_style)
		add_theme_stylebox_override("hover", hover_style)
		add_theme_stylebox_override("pressed", hover_style)
		add_theme_stylebox_override("focus", hover_style)
		add_theme_stylebox_override("disabled", hover_style)
		add_theme_color_override("font_color", PAPER_MAIN)
		add_theme_color_override("font_hover_color", PAPER_MAIN)
		add_theme_color_override("font_pressed_color", PAPER_MAIN)
		add_theme_color_override("font_hover_pressed_color", PAPER_MAIN)
		add_theme_color_override("font_focus_color", PAPER_MAIN)
		add_theme_color_override("font_disabled_color", PAPER_MAIN)
		_set_seal_shape_visible(false)
		return

	add_theme_stylebox_override("normal", _create_transparent_style())
	add_theme_stylebox_override("hover", _create_transparent_style())
	add_theme_stylebox_override("pressed", _create_transparent_style())
	add_theme_stylebox_override("focus", _create_transparent_style())
	add_theme_stylebox_override("disabled", _create_transparent_style())
	add_theme_color_override("font_color", SEAL_RED)
	add_theme_color_override("font_hover_color", SEAL_RED)
	add_theme_color_override("font_pressed_color", SEAL_RED)
	add_theme_color_override("font_hover_pressed_color", SEAL_RED)
	add_theme_color_override("font_focus_color", SEAL_RED)
	add_theme_color_override("font_disabled_color", SEAL_RED)
	_set_seal_shape_visible(false)


func _apply_motion(hovered: bool) -> void:
	_sync_pivot()
	_kill_tween(_tween)
	var target_scale := Vector2(hover_scale, hover_scale) if hovered else Vector2.ONE
	var target_rotation := hover_rotation_degrees if hovered else 0.0
	_tween = create_tween()
	_tween.set_parallel(true)
	_tween.tween_property(self, "scale", target_scale, hover_duration).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	_tween.tween_property(self, "rotation_degrees", target_rotation, hover_duration + 0.02).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)


func _sync_pivot() -> void:
	var current_size := size
	if current_size == Vector2.ZERO:
		current_size = custom_minimum_size
	pivot_offset = current_size * 0.5


func _set_seal_shape_visible(visible: bool) -> void:
	var seal_texture: TextureRect = get_node_or_null("SealShape")
	if seal_texture == null:
		return
	seal_texture.visible = visible
	if not visible:
		return
	# 保留可选显示时的颜色同步。
	seal_texture.modulate = PAPER_MAIN


func _create_transparent_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	style.border_width_left = 0
	style.border_width_top = 0
	style.border_width_right = 0
	style.border_width_bottom = 0
	style.shadow_size = 0
	return style


func _create_fill_style(hovered: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = SEAL_RED
	style.border_width_left = 0
	style.border_width_top = 0
	style.border_width_right = 0
	style.border_width_bottom = 0
	style.corner_radius_top_left = 2
	style.corner_radius_top_right = 2
	style.corner_radius_bottom_right = 2
	style.corner_radius_bottom_left = 2
	style.shadow_size = 10 if hovered else 0
	style.shadow_color = Color(0.45, 0.08, 0.08, 0.18)
	style.content_margin_left = 18
	style.content_margin_top = 18
	style.content_margin_right = 18
	style.content_margin_bottom = 18
	return style


func _kill_tween(tween: Tween) -> void:
	if tween != null and tween.is_running():
		tween.kill()
