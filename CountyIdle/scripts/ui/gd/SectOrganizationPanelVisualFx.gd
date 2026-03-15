extends Node

const PAPER_BACKGROUND := Color(0.956, 0.945, 0.918, 1.0)
const INK_BLACK := Color(0.173, 0.145, 0.125, 1.0)
const INK_GRAY := Color(0.404, 0.353, 0.302, 0.95)
const CINNABAR := Color(0.620, 0.165, 0.133, 1.0)
const BORDER_GOLD := Color(0.773, 0.627, 0.349, 1.0)
const CELADON := Color(0.439, 0.553, 0.506, 1.0)

var _overlay: ColorRect
var _paper_frame: Control
var _left_column: Control
var _right_column: Control
var _current_tween: Tween


func _ready() -> void:
	var root: Node = get_parent()
	_overlay = root.get_node("Overlay")
	_paper_frame = root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame")
	_left_column = root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/LeftColumn")
	_right_column = root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/RightColumn")
	apply_theme_styles()
	reset_state()


func apply_theme_styles() -> void:
	var root := get_parent()
	root.get_node("Overlay/OuterMargin/Wrapper/TopTrim").color = BORDER_GOLD
	root.get_node("Overlay/OuterMargin/Wrapper/BottomTrim").color = BORDER_GOLD
	(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/LeftRoller") as PanelContainer).add_theme_stylebox_override("panel", _create_roller_style())
	(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/RightRoller") as PanelContainer).add_theme_stylebox_override("panel", _create_roller_style())
	(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame") as PanelContainer).add_theme_stylebox_override("panel", _create_paper_style())
	(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel") as PanelContainer).add_theme_stylebox_override("panel", _create_tone_panel_style(Color(0.96, 0.93, 0.86, 0.55)))
	(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/LeftColumn/PeakListScroll") as ScrollContainer).add_theme_stylebox_override("panel", _create_transparent_style())
	(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll") as ScrollContainer).add_theme_stylebox_override("panel", _create_transparent_style())

	for entry in [
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/HeaderRow/TitleColumn/TitleLabel", 28, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/HeaderRow/TitleColumn/SubtitleLabel", 13, INK_GRAY],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/HeaderRow/StatusColumn/StatusTitle", 12, INK_GRAY],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/HeaderRow/StatusColumn/HeaderStatusLabel", 13, CINNABAR],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/LeftColumn/LeftSectionLabel", 16, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/RightColumn/RightSectionLabel", 16, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/DepartmentSectionLabel", 16, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/DetailHeaderRow/PeakTitleLabel", 36, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/DetailHeaderRow/PeakCounterLabel", 13, CINNABAR],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakPositionRow/PeakPositionTitle", 12, INK_GRAY],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakFocusRow/PeakFocusTitle", 12, INK_GRAY],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakCoreUnitsRow/PeakCoreUnitsTitle", 12, INK_GRAY],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakSupportActiveRow/PeakSupportActiveTitle", 12, INK_GRAY],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakSupportCandidateRow/PeakSupportCandidateTitle", 12, INK_GRAY],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakPositionRow/PeakPositionValue", 13, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakFocusRow/PeakFocusValue", 13, CINNABAR],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakCoreUnitsRow/PeakCoreUnitsValue", 13, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakSupportActiveRow/PeakSupportActiveValue", 13, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/MiddleColumn/MiddleScroll/MiddleContent/SummaryPanel/SummaryMargin/SummaryColumn/PeakSupportCandidateRow/PeakSupportCandidateValue", 13, INK_BLACK],
		["Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/RightColumn/HintLabel", 12, INK_GRAY]
	]:
		_apply_label_style(entry[0], entry[1], entry[2])

	_apply_close_button_style(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/HeaderRow/CloseButton"))
	_apply_action_button_style(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/RightColumn/ActionColumn/SetSupportButton"), INK_BLACK, true)
	_apply_action_button_style(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/RightColumn/ActionColumn/ActionRow/ResetSupportButton"), CINNABAR, false)
	_apply_action_button_style(root.get_node("Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn/BodyRow/RightColumn/ActionColumn/ActionRow/OpenGovernanceButton"), CELADON, false)


func style_peak_nav_item(card: PanelContainer, title_label: Label, summary_label: Label) -> void:
	card.add_theme_stylebox_override("panel", _create_peak_nav_style(false))
	title_label.add_theme_color_override("font_color", INK_BLACK)
	title_label.add_theme_font_size_override("font_size", 16)
	summary_label.add_theme_color_override("font_color", INK_GRAY)
	summary_label.add_theme_font_size_override("font_size", 12)


func prepare_dynamic_card_margin(margin: MarginContainer) -> void:
	margin.add_theme_constant_override("margin_left", 12)
	margin.add_theme_constant_override("margin_top", 10)
	margin.add_theme_constant_override("margin_right", 12)
	margin.add_theme_constant_override("margin_bottom", 10)


func prepare_peak_nav_shell(card: PanelContainer, column: VBoxContainer) -> void:
	card.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
	column.add_theme_constant_override("separation", 4)


func apply_peak_nav_state(card: PanelContainer, title_label: Label, selected: bool) -> void:
	card.add_theme_stylebox_override("panel", _create_peak_nav_style(selected))
	title_label.add_theme_color_override("font_color", CINNABAR if selected else INK_BLACK)


func style_job_card(card: PanelContainer, title_label: Label, summary_label: Label, detail_label: Label) -> void:
	card.add_theme_stylebox_override("panel", _create_job_card_style(false))
	title_label.add_theme_color_override("font_color", INK_BLACK)
	title_label.add_theme_font_size_override("font_size", 16)
	summary_label.add_theme_color_override("font_color", INK_GRAY)
	summary_label.add_theme_font_size_override("font_size", 12)
	detail_label.add_theme_color_override("font_color", Color(0.32, 0.28, 0.23, 0.92))
	detail_label.add_theme_font_size_override("font_size", 11)


func prepare_job_card_shell(card: PanelContainer, column: VBoxContainer) -> void:
	card.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
	column.add_theme_constant_override("separation", 4)


func apply_job_card_state(card: PanelContainer, selected: bool) -> void:
	card.add_theme_stylebox_override("panel", _create_job_card_style(selected))


func style_department_card(card: PanelContainer, title_label: Label, detail_label: Label) -> void:
	card.add_theme_stylebox_override("panel", _create_department_card_style())
	title_label.add_theme_color_override("font_color", INK_BLACK)
	title_label.add_theme_font_size_override("font_size", 14)
	detail_label.add_theme_color_override("font_color", INK_GRAY)
	detail_label.add_theme_font_size_override("font_size", 12)


func prepare_department_card_shell(card: PanelContainer, column: VBoxContainer) -> void:
	card.mouse_default_cursor_shape = Control.CURSOR_ARROW
	column.add_theme_constant_override("separation", 4)


func play_open() -> void:
	_kill_tween()
	_overlay.modulate.a = 0.0
	_paper_frame.modulate.a = 0.0
	_paper_frame.scale = Vector2(0.99, 0.99)
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_overlay, "modulate:a", 1.0, 0.18)
	_current_tween.tween_property(_paper_frame, "modulate:a", 1.0, 0.2)
	_current_tween.tween_property(_paper_frame, "scale", Vector2.ONE, 0.22)


func pulse_peak_nav() -> void:
	_kill_tween()
	_left_column.scale = Vector2.ONE
	_left_column.modulate.a = 0.84
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_left_column, "scale", Vector2.ONE * 1.01, 0.08)
	_current_tween.tween_property(_left_column, "modulate:a", 1.0, 0.1)
	_current_tween.chain().tween_property(_left_column, "scale", Vector2.ONE, 0.12)


func pulse_job_cards() -> void:
	_kill_tween()
	_right_column.scale = Vector2.ONE
	_right_column.modulate.a = 0.86
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_right_column, "scale", Vector2.ONE * 1.01, 0.08)
	_current_tween.tween_property(_right_column, "modulate:a", 1.0, 0.1)
	_current_tween.chain().tween_property(_right_column, "scale", Vector2.ONE, 0.12)


func reset_state() -> void:
	_kill_tween()
	_overlay.modulate.a = 1.0
	_paper_frame.modulate.a = 1.0
	_paper_frame.scale = Vector2.ONE
	_left_column.scale = Vector2.ONE
	_left_column.modulate.a = 1.0
	_right_column.scale = Vector2.ONE
	_right_column.modulate.a = 1.0


func _apply_label_style(path: String, font_size: int, color: Color) -> void:
	var label: Label = get_parent().get_node(path)
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", color)


func _apply_close_button_style(button: Button) -> void:
	button.add_theme_font_size_override("font_size", 22)
	for state in ["normal", "hover", "pressed", "focus"]:
		button.add_theme_stylebox_override(state, _create_transparent_style())
	button.add_theme_color_override("font_color", INK_BLACK)
	button.add_theme_color_override("font_hover_color", CINNABAR)
	button.add_theme_color_override("font_pressed_color", CINNABAR)


func _apply_action_button_style(button: Button, accent_color: Color, emphasize: bool) -> void:
	button.add_theme_color_override("font_color", accent_color)
	button.add_theme_color_override("font_hover_color", accent_color)
	button.add_theme_color_override("font_pressed_color", accent_color)
	button.add_theme_color_override("font_disabled_color", Color(accent_color.r, accent_color.g, accent_color.b, 0.50))
	button.add_theme_font_size_override("font_size", 15 if emphasize else 13)
	button.add_theme_stylebox_override("normal", _create_button_style(accent_color, emphasize, false))
	button.add_theme_stylebox_override("hover", _create_button_style(accent_color, emphasize, true))
	button.add_theme_stylebox_override("pressed", _create_button_style(accent_color, emphasize, true))
	button.add_theme_stylebox_override("focus", _create_button_style(accent_color, emphasize, true))
	button.add_theme_stylebox_override("disabled", _create_disabled_button_style(accent_color, emphasize))


func _create_paper_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_BACKGROUND
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


func _create_tone_panel_style(background_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background_color
	style.border_color = Color(0.70, 0.65, 0.56, 0.58)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	return style


func _create_peak_nav_style(selected: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.93, 0.88, 0.80, 0.70) if selected else Color(1, 1, 1, 0)
	style.border_color = CINNABAR if selected else Color(0.70, 0.65, 0.56, 0.35)
	style.border_width_left = 3 if selected else 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	return style


func _create_department_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.98, 0.96, 0.90, 0.55)
	style.border_color = Color(0.70, 0.65, 0.56, 0.45)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	return style


func _create_job_card_style(selected: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.95, 0.90, 0.82, 0.85) if selected else Color(0.98, 0.96, 0.90, 0.45)
	style.border_color = Color(0.62, 0.16, 0.13, 0.82) if selected else Color(0.70, 0.65, 0.56, 0.45)
	style.border_width_left = 2 if selected else 1
	style.border_width_top = 2 if selected else 1
	style.border_width_right = 2 if selected else 1
	style.border_width_bottom = 2 if selected else 1
	return style


func _create_button_style(border_color: Color, emphasize: bool, filled: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_BACKGROUND.r, PAPER_BACKGROUND.g, PAPER_BACKGROUND.b, 0.08 if emphasize else 0.0)
	if filled:
		style.bg_color = Color(0.91, 0.84, 0.74, 0.82) if emphasize else Color(PAPER_BACKGROUND.r, PAPER_BACKGROUND.g, PAPER_BACKGROUND.b, 0.58)
	style.border_color = Color(border_color.r, border_color.g, border_color.b, 0.86 if emphasize else 0.72)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.content_margin_left = 12
	style.content_margin_top = 7
	style.content_margin_right = 12
	style.content_margin_bottom = 7
	if emphasize:
		style.shadow_size = 0 if filled else 3
		style.shadow_offset = Vector2.ZERO if filled else Vector2(2, 2)
		style.shadow_color = Color(0, 0, 0, 0.25)
	return style


func _create_disabled_button_style(border_color: Color, emphasize: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_BACKGROUND.r, PAPER_BACKGROUND.g, PAPER_BACKGROUND.b, 0.16 if emphasize else 0.08)
	style.border_color = Color(border_color.r, border_color.g, border_color.b, 0.28)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.content_margin_left = 12
	style.content_margin_top = 7
	style.content_margin_right = 12
	style.content_margin_bottom = 7
	return style


func _create_transparent_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(1, 1, 1, 0)
	return style


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
