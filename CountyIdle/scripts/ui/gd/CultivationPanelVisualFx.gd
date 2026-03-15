extends Node

const PAPER_MAIN := Color(0.95, 0.92, 0.84, 1.0)
const PAPER_DARK := Color(0.89, 0.85, 0.76, 1.0)
const INK_MAIN := Color(0.17, 0.15, 0.13, 1.0)
const INK_MUTED := Color(0.42, 0.37, 0.33, 1.0)
const SEAL_RED := Color(0.65, 0.16, 0.16, 1.0)
const BORDER_INK := Color(0.29, 0.25, 0.21, 1.0)

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

	for path in ["CenterLayer/DecorLayer/LeftRoller", "CenterLayer/DecorLayer/RightRoller"]:
		var roller: PanelContainer = root.get_node(path)
		roller.add_theme_stylebox_override("panel", _create_roller_style())

	var title_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/TitleLabel")
	title_label.add_theme_font_size_override("font_size", 26)
	title_label.add_theme_color_override("font_color", INK_MAIN)

	var hint_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HintLabel")
	hint_label.add_theme_color_override("font_color", INK_MUTED)
	hint_label.add_theme_font_size_override("font_size", 13)

	var footer_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/FooterHint")
	footer_label.add_theme_color_override("font_color", INK_MUTED)
	footer_label.add_theme_font_size_override("font_size", 12)

	var action_header: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ActionSection/ActionHeader")
	action_header.add_theme_color_override("font_color", INK_MAIN)
	action_header.add_theme_font_size_override("font_size", 16)

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/PopulationCard/CardMargin/CardColumn/TitleLabel",
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TechCard/CardMargin/CardColumn/TitleLabel",
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/ResourceCard/CardMargin/CardColumn/TitleLabel"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_color_override("font_color", INK_MUTED)
		label.add_theme_font_size_override("font_size", 12)

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/PopulationCard/CardMargin/CardColumn/ValueLabel",
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TechCard/CardMargin/CardColumn/ValueLabel",
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/ResourceCard/CardMargin/CardColumn/ValueLabel"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_color_override("font_color", INK_MAIN)
		label.add_theme_font_size_override("font_size", 18)

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/SkillTrainingCard/CardMargin/CardRow/InfoColumn/TitleLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/TechniquePolishCard/CardMargin/CardRow/InfoColumn/TitleLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/CraftPracticeCard/CardMargin/CardRow/InfoColumn/TitleLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/MeditationCard/CardMargin/CardRow/InfoColumn/TitleLabel"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_color_override("font_color", INK_MAIN)
		label.add_theme_font_size_override("font_size", 15)

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/SkillTrainingCard/CardMargin/CardRow/InfoColumn/DescLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/TechniquePolishCard/CardMargin/CardRow/InfoColumn/DescLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/CraftPracticeCard/CardMargin/CardRow/InfoColumn/DescLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/MeditationCard/CardMargin/CardRow/InfoColumn/DescLabel"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_color_override("font_color", INK_MUTED)
		label.add_theme_font_size_override("font_size", 12)

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/SkillTrainingCard/CardMargin/CardRow/InfoColumn/StatusLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/TechniquePolishCard/CardMargin/CardRow/InfoColumn/StatusLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/CraftPracticeCard/CardMargin/CardRow/InfoColumn/StatusLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/MeditationCard/CardMargin/CardRow/InfoColumn/StatusLabel"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_color_override("font_color", INK_MAIN)
		label.add_theme_font_size_override("font_size", 12)

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/PopulationCard",
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TechCard",
		"CenterLayer/Dialog/Margin/MainColumn/SummaryRow/ResourceCard",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/SkillTrainingCard",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/TechniquePolishCard",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/CraftPracticeCard",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/MeditationCard"
	]:
		var panel: PanelContainer = root.get_node(path)
		panel.add_theme_stylebox_override("panel", _create_inset_card_style())

	_apply_close_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton"))

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/SkillTrainingCard/CardMargin/CardRow/ActionButton",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/TechniquePolishCard/CardMargin/CardRow/ActionButton",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/CraftPracticeCard/CardMargin/CardRow/ActionButton",
		"CenterLayer/Dialog/Margin/MainColumn/ActionSection/MeditationCard/CardMargin/CardRow/ActionButton"
	]:
		_apply_action_button_style(root.get_node(path))


func play_open() -> void:
	_kill_tween()
	_backdrop.modulate.a = 0.0
	_dialog.modulate.a = 0.0
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_backdrop, "modulate:a", 1.0, 0.18)
	_current_tween.tween_property(_dialog, "modulate:a", 1.0, 0.2)


func reset_state() -> void:
	_kill_tween()
	_backdrop.modulate.a = 1.0
	_dialog.modulate.a = 1.0
	_dialog.scale = Vector2.ONE


func _apply_action_button_style(button: Button) -> void:
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_CENTER
	button.add_theme_font_size_override("font_size", 13)
	button.add_theme_stylebox_override("normal", _create_order_button_style(false))
	button.add_theme_stylebox_override("hover", _create_order_button_style(true))
	button.add_theme_stylebox_override("pressed", _create_order_button_style(true))
	button.add_theme_stylebox_override("disabled", _create_order_button_style(false, true))
	button.add_theme_color_override("font_color", INK_MAIN)
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


func _create_inset_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_DARK.r, PAPER_DARK.g, PAPER_DARK.b, 0.55)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.48, 0.42, 0.35, 0.35)
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


func _create_order_button_style(inverted: bool, disabled: bool = false) -> StyleBoxFlat:
	var border := INK_MUTED if disabled else BORDER_INK
	var background := Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.0)
	if inverted and not disabled:
		background = INK_MAIN
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = border
	style.content_margin_left = 12
	style.content_margin_top = 8
	style.content_margin_right = 12
	style.content_margin_bottom = 8
	return style


func _create_transparent_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	return style


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
