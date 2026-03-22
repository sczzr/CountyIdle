extends Node

const PAPER_MAIN := Color(0.95, 0.92, 0.84, 1.0)
const PAPER_DARK := Color(0.91, 0.88, 0.80, 1.0)
const PAPER_WARM := Color(0.97, 0.95, 0.90, 1.0)
const INK_MAIN := Color(0.17, 0.15, 0.13, 1.0)
const INK_MUTED := Color(0.42, 0.37, 0.33, 1.0)
const SEAL_RED := Color(0.70, 0.13, 0.13, 1.0)
const JADE_SOFT := Color(0.78, 0.88, 0.82, 1.0)
const BORDER_INK := Color(0.29, 0.25, 0.21, 1.0)

var _dialog: PanelContainer
var _scroll_frame: PanelContainer
var _slip_shelf_frame: PanelContainer
var _preview_frame: Control
var _preview_texture: TextureRect
var _preview_hint: Label
var _ink_wash_top: ColorRect
var _ink_wash_bottom: ColorRect
var _preview_seal_label: Label
var _slot_title_label: Label
var _slot_updated_label: Label
var _slot_detail_label: Label
var _slot_updated_detail_label: Label
var _name_row: Control
var _action_row_primary: Control
var _action_row_secondary: Control
var _detail_nodes: Array[Control] = []
var _stat_nodes: Array[Label] = []
var _current_tween: Tween


func _ready() -> void:
	var root: Node = get_parent()
	_dialog = root.get_node("CenterLayer/Dialog")
	_scroll_frame = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame")
	_slip_shelf_frame = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlipShelfFrame")
	_preview_frame = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame")
	_preview_texture = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame/PreviewMargin/PreviewColumn/PreviewTexture")
	_preview_hint = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame/PreviewMargin/PreviewColumn/PreviewHintLabel")
	_ink_wash_top = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame/InkWashTop")
	_ink_wash_bottom = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame/InkWashBottom")
	_preview_seal_label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame/PreviewSealLabel")
	_slot_title_label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ScrollTitleRow/ScrollTitleStack/SlotTitleLabel")
	_slot_updated_label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ScrollTitleRow/ScrollTitleStack/SlotUpdatedLabel")
	_slot_detail_label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/DetailBody/SlotDetailLabel")
	_slot_updated_detail_label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/DetailBody/SlotUpdatedDetailLabel")
	_name_row = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/NameRow")
	_action_row_primary = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary")
	_action_row_secondary = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowSecondary")
	_detail_nodes = [_slot_title_label, _slot_updated_label, _slot_detail_label, _slot_updated_detail_label, _name_row, _action_row_primary, _action_row_secondary]
	_stat_nodes = [
		root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/PopulationStatCard/PopulationStatValueLabel"),
		root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/GoldStatCard/GoldStatValueLabel"),
		root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/ExplorationStatCard/ExplorationStatValueLabel")
	]
	apply_theme_styles()
	reset_state()


func apply_theme_styles() -> void:
	var root := get_parent()
	_dialog.add_theme_stylebox_override("panel", _create_paper_style())
	_scroll_frame.add_theme_stylebox_override("panel", _create_scroll_style())
	_slip_shelf_frame.add_theme_stylebox_override("panel", _create_shelf_style())
	_preview_frame.add_theme_stylebox_override("panel", _create_preview_style())

	for path in ["CenterLayer/DecorLayer/RollerRow/LeftRoller", "CenterLayer/DecorLayer/RollerRow/RightRoller"]:
		var roller: PanelContainer = root.get_node(path)
		roller.add_theme_stylebox_override("panel", _create_roller_style())

	var title_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/TitleStack/TitleLabel")
	title_label.add_theme_font_size_override("font_size", 28)
	title_label.add_theme_color_override("font_color", INK_MAIN)

	var subtitle_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/TitleStack/SubtitleLabel")
	subtitle_label.add_theme_font_size_override("font_size", 12)
	subtitle_label.add_theme_color_override("font_color", INK_MUTED)

	var quote_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/QuoteLabel")
	quote_label.add_theme_font_size_override("font_size", 12)
	quote_label.add_theme_color_override("font_color", INK_MUTED)

	var hint_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HintLabel")
	hint_label.add_theme_font_size_override("font_size", 13)
	hint_label.add_theme_color_override("font_color", INK_MUTED)

	var mode_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ModeLabel")
	mode_label.add_theme_font_size_override("font_size", 14)
	mode_label.add_theme_color_override("font_color", SEAL_RED)

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlotListTitle",
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/DetailTitle",
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ScrollTitleRow/ScrollTagLabel",
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/NameRow/SlotNameLabel"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_font_size_override("font_size", 15)
		label.add_theme_color_override("font_color", INK_MAIN)

	var shelf_caption: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/ShelfCaptionLabel")
	shelf_caption.add_theme_font_size_override("font_size", 12)
	shelf_caption.add_theme_color_override("font_color", INK_MUTED)

	var shelf_seal_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlipShelfFrame/ShelfSealLabel")
	shelf_seal_label.add_theme_font_size_override("font_size", 12)
	shelf_seal_label.add_theme_color_override("font_color", Color(0.58, 0.22, 0.20, 0.88))

	_slot_title_label.add_theme_font_size_override("font_size", 24)
	_slot_title_label.add_theme_color_override("font_color", INK_MAIN)
	_slot_updated_label.add_theme_font_size_override("font_size", 13)
	_slot_updated_label.add_theme_color_override("font_color", INK_MUTED)
	_slot_detail_label.add_theme_font_size_override("font_size", 14)
	_slot_detail_label.add_theme_color_override("font_color", INK_MAIN)
	_slot_updated_detail_label.add_theme_font_size_override("font_size", 12)
	_slot_updated_detail_label.add_theme_color_override("font_color", INK_MUTED)
	_preview_hint.add_theme_font_size_override("font_size", 13)
	_preview_hint.add_theme_color_override("font_color", INK_MUTED)
	_preview_seal_label.add_theme_font_size_override("font_size", 13)
	_preview_seal_label.add_theme_color_override("font_color", Color(0.58, 0.20, 0.18, 0.92))
	_preview_seal_label.rotation_degrees = -6.0
	_ink_wash_top.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_ink_wash_bottom.mouse_filter = Control.MOUSE_FILTER_IGNORE

	for stat_label in _stat_nodes:
		stat_label.add_theme_font_size_override("font_size", 18)
		stat_label.add_theme_color_override("font_color", INK_MAIN)
		stat_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_LEFT

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/PopulationStatCard/PopulationStatCaption",
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/GoldStatCard/GoldStatCaption",
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/ExplorationStatCard/ExplorationStatCaption"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_font_size_override("font_size", 11)
		label.add_theme_color_override("font_color", INK_MUTED)

	_apply_close_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton"))
	_apply_footer_seal_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CloseFooterButton"))
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/NameRow/RenameSlotButton"), false, false, true)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary/DeleteSlotButton"), true, false)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary/CopySlotButton"), false, false)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary/SaveSelectedButton"), false, true)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary/LoadSelectedButton"), false, true)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowSecondary/CreateSlotButton"), false, false, true)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowSecondary/RefreshButton"), false, false, true)

	_apply_field_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/FilterRow/FilterOptionButton"))
	_apply_field_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/FilterRow/SortOptionButton"))
	_apply_line_edit_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/NameRow/SlotNameEdit"))


func transition_to_preview() -> void:
	_kill_tween()
	_preview_frame.scale = Vector2.ONE
	_scroll_frame.scale = Vector2.ONE
	_preview_texture.modulate.a = 0.0
	_preview_hint.modulate.a = 0.0
	_preview_seal_label.modulate.a = 0.0
	_ink_wash_top.modulate.a = 0.0
	_ink_wash_bottom.modulate.a = 0.0
	for node in _detail_nodes:
		node.modulate.a = 0.80
	for stat_label in _stat_nodes:
		stat_label.modulate.a = 0.75

	_current_tween = create_tween().set_parallel(true)
	_current_tween.tween_property(_preview_texture, "modulate:a", 1.0, 0.24)
	_current_tween.tween_property(_preview_hint, "modulate:a", 0.0, 0.18)
	_current_tween.tween_property(_preview_seal_label, "modulate:a", 1.0, 0.22)
	_current_tween.tween_property(_ink_wash_top, "modulate:a", 1.0, 0.28)
	_current_tween.tween_property(_ink_wash_bottom, "modulate:a", 1.0, 0.28)
	for node in _detail_nodes:
		_current_tween.tween_property(node, "modulate:a", 1.0, 0.18)
	for stat_label in _stat_nodes:
		_current_tween.tween_property(stat_label, "modulate:a", 1.0, 0.18)


func transition_to_empty() -> void:
	_kill_tween()
	_preview_frame.scale = Vector2.ONE
	_scroll_frame.scale = Vector2.ONE
	_preview_texture.modulate.a = 0.10
	_preview_hint.modulate.a = 1.0
	_preview_seal_label.modulate.a = 0.42
	_ink_wash_top.modulate.a = 0.76
	_ink_wash_bottom.modulate.a = 0.76
	for node in _detail_nodes:
		node.modulate.a = 1.0
	for stat_label in _stat_nodes:
		stat_label.modulate.a = 1.0


func pulse_on_select() -> void:
	_kill_tween()
	_scroll_frame.scale = Vector2.ONE
	_preview_frame.scale = Vector2.ONE
	_slot_title_label.position = Vector2.ZERO

	_current_tween = create_tween().set_parallel(true)
	_current_tween.tween_property(_scroll_frame, "scale", Vector2(1.012, 1.006), 0.12)
	_current_tween.tween_property(_preview_frame, "scale", Vector2(1.014, 1.014), 0.12)
	_current_tween.tween_property(_slot_title_label, "position", Vector2(6.0, 0.0), 0.12)
	_current_tween.tween_property(_preview_seal_label, "rotation_degrees", -10.0, 0.12)
	_current_tween.chain().tween_property(_scroll_frame, "scale", Vector2.ONE, 0.18)
	_current_tween.parallel().tween_property(_preview_frame, "scale", Vector2.ONE, 0.18)
	_current_tween.parallel().tween_property(_slot_title_label, "position", Vector2.ZERO, 0.18)
	_current_tween.parallel().tween_property(_preview_seal_label, "rotation_degrees", -6.0, 0.18)


func reset_state() -> void:
	_kill_tween()
	_preview_frame.scale = Vector2.ONE
	_scroll_frame.scale = Vector2.ONE
	_preview_texture.modulate.a = 1.0
	_preview_hint.modulate.a = 1.0
	_preview_seal_label.modulate.a = 1.0
	_preview_seal_label.rotation_degrees = -6.0
	_ink_wash_top.modulate.a = 1.0
	_ink_wash_bottom.modulate.a = 1.0
	for node in _detail_nodes:
		node.modulate.a = 1.0
	for stat_label in _stat_nodes:
		stat_label.modulate.a = 1.0


func _apply_ink_button_style(button: Button, destructive: bool, emphasized: bool, compact: bool = false) -> void:
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_CENTER
	button.add_theme_font_size_override("font_size", 12 if compact else (14 if emphasized else 13))
	button.add_theme_stylebox_override("normal", _create_order_button_style(destructive, false, emphasized, compact))
	button.add_theme_stylebox_override("hover", _create_order_button_style(destructive, true, emphasized, compact))
	button.add_theme_stylebox_override("pressed", _create_order_button_style(destructive, true, emphasized, compact))
	button.add_theme_stylebox_override("disabled", _create_order_button_style(false, false, emphasized, compact, true))
	button.add_theme_color_override("font_color", SEAL_RED if destructive or emphasized else INK_MAIN)
	button.add_theme_color_override("font_hover_color", PAPER_MAIN)
	button.add_theme_color_override("font_pressed_color", PAPER_MAIN)
	button.add_theme_color_override("font_disabled_color", INK_MUTED)


func _apply_footer_seal_button_style(button: Button) -> void:
	# 留影录卷尾主动作改复用机宜卷印章组件，保留“合卷”这一卷册家族动作语义。
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_CENTER
	button.add_theme_font_size_override("font_size", 24)
	for state in ["normal", "hover", "pressed", "focus", "disabled"]:
		button.add_theme_stylebox_override(state, _create_transparent_style())
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_hover_color", SEAL_RED)
	button.add_theme_color_override("font_pressed_color", SEAL_RED)
	button.add_theme_color_override("font_disabled_color", INK_MUTED)
	var seal_shape: TextureRect = button.get_node_or_null("SealShape")
	if seal_shape != null:
		seal_shape.modulate = Color(0.63, 0.19, 0.17, 0.92)


func _apply_field_style(button: BaseButton) -> void:
	button.add_theme_stylebox_override("normal", _create_field_style(false))
	button.add_theme_stylebox_override("hover", _create_field_style(true))
	button.add_theme_stylebox_override("pressed", _create_field_style(true))
	button.add_theme_stylebox_override("focus", _create_field_style(true))
	button.add_theme_font_size_override("font_size", 13)
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_hover_color", INK_MAIN)
	button.add_theme_color_override("font_pressed_color", INK_MAIN)


func _apply_line_edit_style(line_edit: LineEdit) -> void:
	line_edit.add_theme_stylebox_override("normal", _create_field_style(false))
	line_edit.add_theme_stylebox_override("focus", _create_field_style(true))
	line_edit.add_theme_stylebox_override("read_only", _create_field_style(false))
	line_edit.add_theme_color_override("font_color", INK_MAIN)
	line_edit.add_theme_color_override("font_placeholder_color", INK_MUTED)
	line_edit.add_theme_constant_override("minimum_character_width", 12)


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
	style.shadow_color = Color(0, 0, 0, 0.18)
	style.shadow_size = 10
	return style


func _create_scroll_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_WARM
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.64, 0.56, 0.44, 0.65)
	style.shadow_color = Color(0, 0, 0, 0.12)
	style.shadow_size = 8
	return style


func _create_preview_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.93, 0.92, 0.90, 1.0)
	style.border_width_left = 10
	style.border_width_top = 10
	style.border_width_right = 10
	style.border_width_bottom = 10
	style.border_color = Color(0.99, 0.98, 0.95, 1.0)
	style.shadow_color = Color(0, 0, 0, 0.15)
	style.shadow_size = 16
	return style


func _create_shelf_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_DARK.r, PAPER_DARK.g, PAPER_DARK.b, 0.52)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(JADE_SOFT.r, JADE_SOFT.g, JADE_SOFT.b, 0.68)
	style.shadow_color = Color(0.08, 0.08, 0.06, 0.10)
	style.shadow_size = 8
	return style


func _create_roller_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.22, 0.18, 0.14, 1.0)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.12, 0.09, 0.06, 1.0)
	return style


func _create_field_style(focused: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.78 if focused else 0.44)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = SEAL_RED if focused else BORDER_INK
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	style.content_margin_left = 10
	style.content_margin_top = 8
	style.content_margin_right = 10
	style.content_margin_bottom = 8
	return style


func _create_order_button_style(destructive: bool, inverted: bool, emphasized: bool, compact: bool, disabled: bool = false) -> StyleBoxFlat:
	var border := INK_MUTED if disabled else (SEAL_RED if destructive or emphasized else BORDER_INK)
	var background := Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.0)
	if inverted and not disabled:
		background = SEAL_RED if destructive or emphasized else INK_MAIN
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_width_left = 0 if compact and not inverted else 1
	style.border_width_top = 1
	style.border_width_right = 0 if compact and not inverted else 1
	style.border_width_bottom = 0 if compact and not inverted else 1
	style.border_color = border
	style.corner_radius_top_left = 2 if compact else 8
	style.corner_radius_top_right = 2 if compact else 8
	style.corner_radius_bottom_right = 2 if compact else 8
	style.corner_radius_bottom_left = 2 if compact else 8
	style.shadow_color = Color(0, 0, 0, 0.10 if disabled else 0.16)
	style.shadow_size = 6 if emphasized else 4
	style.content_margin_left = 8 if compact else 12
	style.content_margin_top = 6 if compact else 10
	style.content_margin_right = 8 if compact else 12
	style.content_margin_bottom = 6 if compact else 10
	return style


func _create_transparent_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	return style


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
