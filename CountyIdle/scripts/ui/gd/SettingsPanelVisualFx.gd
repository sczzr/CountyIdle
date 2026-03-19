extends Node

const PAPER_MAIN := Color(0.95, 0.92, 0.84, 1.0)
const PAPER_DARK := Color(0.89, 0.85, 0.76, 1.0)
const INK_MAIN := Color(0.17, 0.15, 0.13, 1.0)
const INK_MUTED := Color(0.42, 0.37, 0.33, 1.0)
const SEAL_RED := Color(0.65, 0.16, 0.16, 1.0)
const BORDER_INK := Color(0.29, 0.25, 0.21, 1.0)
# 复用全局主题资源，避免设置卷再次退回运行时纯色手柄。
const SLIDER_GRABBER_ICON := preload("res://assets/ui/icons/slider_grabber.svg")
const SLIDER_GRABBER_HIGHLIGHT_ICON := preload("res://assets/ui/icons/slider_grabber_hl.svg")
# 低分辨率下设置项会落入滚动区，因此统一维护设置区路径前缀。
const SETTINGS_ROWS_PATH := "CenterLayer/Dialog/Margin/MainColumn/SettingsScroll/SettingsRows"

var _backdrop: ColorRect
var _dialog: Control
var _current_tween: Tween


func _ready() -> void:
	var root: Node = get_parent()
	_backdrop = root.get_node("Backdrop")
	_dialog = root.get_node("CenterLayer/Dialog")
	apply_theme_styles()
	reset_state()


func apply_theme_styles() -> void:
	var root := get_parent()
	var dialog: PanelContainer = root.get_node("CenterLayer/Dialog")
	dialog.add_theme_stylebox_override("panel", _create_paper_style())

	# 装饰卷轴改为容器行，路径需包含 RollerRow。
	for path in ["CenterLayer/DecorLayer/RollerRow/LeftRoller", "CenterLayer/DecorLayer/RollerRow/RightRoller"]:
		var roller: PanelContainer = root.get_node(path)
		roller.add_theme_stylebox_override("panel", _create_roller_style())

	var title_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/TitleLabel")
	title_label.add_theme_font_size_override("font_size", 26)
	title_label.add_theme_color_override("font_color", INK_MAIN)

	var hint_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HintLabel")
	hint_label.add_theme_color_override("font_color", INK_MUTED)
	hint_label.add_theme_font_size_override("font_size", 13)

	for path in [
		"%s/InstantHeader" % SETTINGS_ROWS_PATH,
		"%s/SavedHeader" % SETTINGS_ROWS_PATH,
		"%s/LanguageRow/LanguageLabel" % SETTINGS_ROWS_PATH,
		"%s/ResolutionRow/ResolutionLabel" % SETTINGS_ROWS_PATH,
		"%s/FullscreenRow/FullscreenLabel" % SETTINGS_ROWS_PATH,
		"%s/ZoomRow/ZoomLabel" % SETTINGS_ROWS_PATH,
		"%s/VolumeRow/VolumeLabel" % SETTINGS_ROWS_PATH,
		"%s/ShortcutHeader" % SETTINGS_ROWS_PATH,
		"%s/OpenSettingsKeyRow/OpenSettingsKeyLabel" % SETTINGS_ROWS_PATH,
		"%s/OpenWarehouseKeyRow/OpenWarehouseKeyLabel" % SETTINGS_ROWS_PATH,
		"%s/ToggleExplorationKeyRow/ToggleExplorationKeyLabel" % SETTINGS_ROWS_PATH,
		"%s/ToggleSpeedKeyRow/ToggleSpeedKeyLabel" % SETTINGS_ROWS_PATH,
		"%s/QuickSaveKeyRow/QuickSaveKeyLabel" % SETTINGS_ROWS_PATH,
		"%s/QuickLoadKeyRow/QuickLoadKeyLabel" % SETTINGS_ROWS_PATH,
		"%s/QuickResetKeyRow/QuickResetKeyLabel" % SETTINGS_ROWS_PATH
	]:
		var label: Label = root.get_node(path)
		label.add_theme_color_override("font_color", INK_MAIN)
		var is_header := label.name.ends_with("Header")
		label.add_theme_font_size_override("font_size", 16 if is_header else 14)

	for path in [
		"%s/ZoomRow/ZoomValue" % SETTINGS_ROWS_PATH,
		"%s/VolumeRow/VolumeValue" % SETTINGS_ROWS_PATH
	]:
		var value_label: Label = root.get_node(path)
		value_label.add_theme_color_override("font_color", INK_MAIN)
		value_label.add_theme_font_size_override("font_size", 14)

	_apply_close_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton"))

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/FooterRow/CancelButton",
		"CenterLayer/Dialog/Margin/MainColumn/FooterRow/ApplyButton",
		"%s/OpenSettingsKeyRow/OpenSettingsKeyOption" % SETTINGS_ROWS_PATH,
		"%s/OpenWarehouseKeyRow/OpenWarehouseKeyOption" % SETTINGS_ROWS_PATH,
		"%s/ToggleExplorationKeyRow/ToggleExplorationKeyOption" % SETTINGS_ROWS_PATH,
		"%s/ToggleSpeedKeyRow/ToggleSpeedKeyOption" % SETTINGS_ROWS_PATH,
		"%s/QuickSaveKeyRow/QuickSaveKeyOption" % SETTINGS_ROWS_PATH,
		"%s/QuickLoadKeyRow/QuickLoadKeyOption" % SETTINGS_ROWS_PATH,
		"%s/QuickResetKeyRow/QuickResetKeyOption" % SETTINGS_ROWS_PATH
	]:
		_apply_action_button_style(root.get_node(path), false)

	for path in [
		"%s/LanguageRow/LanguageOption" % SETTINGS_ROWS_PATH,
		"%s/ResolutionRow/ResolutionOption" % SETTINGS_ROWS_PATH
	]:
		_apply_field_style(root.get_node(path))

	_apply_slider_style(root.get_node("%s/ZoomRow/ZoomSlider" % SETTINGS_ROWS_PATH))
	_apply_slider_style(root.get_node("%s/VolumeRow/VolumeSlider" % SETTINGS_ROWS_PATH))


func play_open() -> void:
	_kill_tween()
	_backdrop.modulate.a = 0.0
	_dialog.modulate.a = 0.0
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_backdrop, "modulate:a", 1.0, 0.18)
	_current_tween.tween_property(_dialog, "modulate:a", 1.0, 0.2)


func pulse_shortcut(button_path: String) -> void:
	if not get_parent().has_node(button_path):
		return

	_kill_tween()
	var button: Control = get_parent().get_node(button_path)
	button.modulate.a = 0.82
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(button, "modulate:a", 1.0, 0.1)


func reset_state() -> void:
	_kill_tween()
	_backdrop.modulate.a = 1.0
	_dialog.modulate.a = 1.0
	_dialog.scale = Vector2.ONE


func _apply_action_button_style(button: Button, destructive: bool) -> void:
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	button.add_theme_font_size_override("font_size", 14)
	button.add_theme_stylebox_override("normal", _create_order_button_style(destructive, false))
	button.add_theme_stylebox_override("hover", _create_order_button_style(destructive, true))
	button.add_theme_stylebox_override("pressed", _create_order_button_style(destructive, true))
	button.add_theme_stylebox_override("disabled", _create_order_button_style(false, false, true))
	button.add_theme_color_override("font_color", SEAL_RED if destructive else INK_MAIN)
	button.add_theme_color_override("font_hover_color", PAPER_MAIN)
	button.add_theme_color_override("font_pressed_color", PAPER_MAIN)
	button.add_theme_color_override("font_disabled_color", INK_MUTED)


func _apply_close_button_style(button: Button) -> void:
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_CENTER
	button.add_theme_font_size_override("font_size", 22)
	button.add_theme_stylebox_override("normal", _create_transparent_style())
	button.add_theme_stylebox_override("hover", _create_transparent_style())
	button.add_theme_stylebox_override("pressed", _create_transparent_style())
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_hover_color", SEAL_RED)
	button.add_theme_color_override("font_pressed_color", SEAL_RED)


func _apply_field_style(button: BaseButton) -> void:
	button.add_theme_stylebox_override("normal", _create_field_style(false))
	button.add_theme_stylebox_override("hover", _create_field_style(true))
	button.add_theme_stylebox_override("pressed", _create_field_style(true))
	button.add_theme_stylebox_override("focus", _create_field_style(true))
	button.add_theme_font_size_override("font_size", 13)
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_hover_color", INK_MAIN)
	button.add_theme_color_override("font_pressed_color", INK_MAIN)


func _apply_slider_style(slider: Range) -> void:
	slider.add_theme_stylebox_override("slider", _create_slider_track_style())
	slider.add_theme_stylebox_override("grabber_area", _create_transparent_style())
	slider.add_theme_stylebox_override("grabber_area_highlight", _create_transparent_style())
	slider.add_theme_icon_override("grabber", SLIDER_GRABBER_ICON)
	slider.add_theme_icon_override("grabber_highlight", SLIDER_GRABBER_HIGHLIGHT_ICON)
	slider.add_theme_icon_override("grabber_disabled", SLIDER_GRABBER_ICON)


func _create_paper_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_MAIN
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.48, 0.42, 0.35, 0.45)
	style.shadow_color = Color(0, 0, 0, 0.35)
	style.shadow_size = 10
	return style


func _create_roller_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.29, 0.19, 0.13, 1.0)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.14, 0.09, 0.05, 1.0)
	return style


func _create_field_style(focused: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.75 if focused else 0.35)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = SEAL_RED if focused else BORDER_INK
	style.content_margin_left = 10
	style.content_margin_top = 8
	style.content_margin_right = 10
	style.content_margin_bottom = 8
	return style


func _create_order_button_style(destructive: bool, inverted: bool, disabled: bool = false) -> StyleBoxFlat:
	var border := INK_MUTED if disabled else (SEAL_RED if destructive else BORDER_INK)
	var background := Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.0)
	if inverted and not disabled:
		background = SEAL_RED if destructive else INK_MAIN
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = border
	style.content_margin_left = 12
	style.content_margin_top = 10
	style.content_margin_right = 12
	style.content_margin_bottom = 10
	return style


func _create_slider_track_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_DARK
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = BORDER_INK
	style.content_margin_top = 4
	style.content_margin_bottom = 4
	return style


func _create_transparent_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	return style


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
