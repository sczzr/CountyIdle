extends Node

const PAPER_MAIN := Color(0.95, 0.92, 0.84, 1.0)
const PAPER_DARK := Color(0.89, 0.85, 0.76, 1.0)
const INK_MAIN := Color(0.17, 0.15, 0.13, 1.0)
const INK_MUTED := Color(0.42, 0.37, 0.33, 1.0)
const SEAL_RED := Color(0.65, 0.16, 0.16, 1.0)
const BORDER_INK := Color(0.29, 0.25, 0.21, 1.0)

var _dialog: PanelContainer
var _preview_frame: Control
var _preview_texture: TextureRect
var _preview_hint: Label
var _slot_detail_label: Label
var _name_row: Control
var _action_row_primary: Control
var _action_row_secondary: Control
var _action_row_tertiary: Control
var _detail_nodes: Array[Control] = []
var _current_tween: Tween


func _ready() -> void:
	var root: Node = get_parent()
	_dialog = root.get_node("CenterLayer/Dialog")
	_preview_frame = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/PreviewFrame")
	_preview_texture = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/PreviewFrame/PreviewMargin/PreviewColumn/PreviewTexture")
	_preview_hint = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/PreviewFrame/PreviewMargin/PreviewColumn/PreviewHintLabel")
	_slot_detail_label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/SlotDetailLabel")
	_name_row = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/NameRow")
	_action_row_primary = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowPrimary")
	_action_row_secondary = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowSecondary")
	_action_row_tertiary = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowTertiary")
	_detail_nodes = [_slot_detail_label, _name_row, _action_row_primary, _action_row_secondary, _action_row_tertiary]
	apply_theme_styles()
	reset_state()


func apply_theme_styles() -> void:
	var root := get_parent()
	_dialog.add_theme_stylebox_override("panel", _create_paper_style())
	_preview_frame.add_theme_stylebox_override("panel", _create_note_style())

	for path in ["CenterLayer/DecorLayer/LeftRoller", "CenterLayer/DecorLayer/RightRoller"]:
		var roller: PanelContainer = root.get_node(path)
		roller.add_theme_stylebox_override("panel", _create_roller_style())

	var title_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/TitleLabel")
	title_label.add_theme_font_size_override("font_size", 26)
	title_label.add_theme_color_override("font_color", INK_MAIN)

	var mode_label: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ModeLabel")
	mode_label.add_theme_font_size_override("font_size", 14)
	mode_label.add_theme_color_override("font_color", SEAL_RED)

	var slot_list_title: Label = root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlotListTitle")
	slot_list_title.add_theme_font_size_override("font_size", 16)
	slot_list_title.add_theme_color_override("font_color", INK_MAIN)

	_slot_detail_label.add_theme_color_override("font_color", INK_MAIN)
	_preview_hint.add_theme_color_override("font_color", INK_MUTED)
	_preview_hint.add_theme_font_size_override("font_size", 13)

	for path in [
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/DetailTitle",
		"CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/NameRow/SlotNameLabel"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_font_size_override("font_size", 15)
		label.add_theme_color_override("font_color", INK_MAIN)

	_apply_close_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton"))
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CloseFooterButton"), false)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowPrimary/SaveSelectedButton"), false)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowPrimary/LoadSelectedButton"), false)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowSecondary/CreateSlotButton"), false)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowSecondary/RenameSlotButton"), false)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowSecondary/CopySlotButton"), false)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowTertiary/DeleteSlotButton"), true)
	_apply_ink_button_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowTertiary/RefreshButton"), false)

	_apply_field_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/FilterRow/FilterOptionButton"))
	_apply_field_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/FilterRow/SortOptionButton"))
	_apply_line_edit_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/NameRow/SlotNameEdit"))
	_apply_item_list_style(root.get_node("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlotList"))


func transition_to_preview() -> void:
	_kill_tween()
	_preview_frame.scale = Vector2(0.992, 0.992)
	_preview_texture.modulate.a = 0.0
	_preview_hint.modulate.a = 0.0
	_slot_detail_label.modulate.a = 0.78
	_name_row.modulate.a = 0.82
	_action_row_primary.modulate.a = 0.82
	_action_row_secondary.modulate.a = 0.82
	_action_row_tertiary.modulate.a = 0.82
	_name_row.scale = Vector2(0.996, 0.996)

	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_preview_texture, "modulate:a", 1.0, 0.18)
	_current_tween.tween_property(_preview_hint, "modulate:a", 1.0, 0.18)
	_current_tween.tween_property(_preview_frame, "scale", Vector2.ONE, 0.20)
	_current_tween.tween_property(_slot_detail_label, "modulate:a", 1.0, 0.16)
	_current_tween.tween_property(_name_row, "modulate:a", 1.0, 0.16)
	_current_tween.tween_property(_action_row_primary, "modulate:a", 1.0, 0.16)
	_current_tween.tween_property(_action_row_secondary, "modulate:a", 1.0, 0.16)
	_current_tween.tween_property(_action_row_tertiary, "modulate:a", 1.0, 0.16)
	_current_tween.tween_property(_name_row, "scale", Vector2.ONE, 0.18)


func transition_to_empty() -> void:
	_kill_tween()
	_preview_frame.scale = Vector2.ONE
	_preview_texture.modulate.a = 0.38
	_preview_hint.modulate.a = 1.0
	_slot_detail_label.modulate.a = 1.0
	_name_row.modulate.a = 1.0
	_action_row_primary.modulate.a = 0.92
	_action_row_secondary.modulate.a = 0.92
	_action_row_tertiary.modulate.a = 0.92
	_name_row.scale = Vector2.ONE


func pulse_on_select() -> void:
	_kill_tween()
	_preview_frame.scale = Vector2.ONE
	_slot_detail_label.scale = Vector2.ONE
	_action_row_primary.scale = Vector2.ONE

	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_preview_frame, "scale", Vector2.ONE * 1.012, 0.08)
	_current_tween.tween_property(_slot_detail_label, "scale", Vector2.ONE * 1.006, 0.08)
	_current_tween.tween_property(_action_row_primary, "scale", Vector2.ONE * 1.004, 0.08)
	_current_tween.chain().tween_property(_preview_frame, "scale", Vector2.ONE, 0.12)
	_current_tween.parallel().tween_property(_slot_detail_label, "scale", Vector2.ONE, 0.12)
	_current_tween.parallel().tween_property(_action_row_primary, "scale", Vector2.ONE, 0.12)


func reset_state() -> void:
	_kill_tween()
	_preview_frame.scale = Vector2.ONE
	_preview_texture.modulate.a = 1.0
	_preview_hint.modulate.a = 1.0
	_slot_detail_label.scale = Vector2.ONE
	_name_row.scale = Vector2.ONE
	_action_row_primary.scale = Vector2.ONE
	for node: Control in _detail_nodes:
		node.modulate.a = 1.0


func _apply_ink_button_style(button: Button, destructive: bool) -> void:
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


func _apply_item_list_style(item_list: ItemList) -> void:
	item_list.add_theme_stylebox_override("panel", _create_note_style())
	item_list.add_theme_stylebox_override("focus", _create_field_style(true))
	item_list.add_theme_stylebox_override("cursor", _create_selection_style())
	item_list.add_theme_stylebox_override("cursor_unfocused", _create_selection_style())
	item_list.add_theme_color_override("font_color", INK_MAIN)
	item_list.add_theme_color_override("font_selected_color", PAPER_MAIN)
	item_list.add_theme_color_override("guide_color", Color(BORDER_INK.r, BORDER_INK.g, BORDER_INK.b, 0.25))
	item_list.add_theme_constant_override("h_separation", 8)
	item_list.add_theme_constant_override("v_separation", 6)


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


func _create_roller_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.29, 0.19, 0.13, 1.0)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.14, 0.09, 0.05, 1.0)
	return style


func _create_note_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_DARK
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.64, 0.58, 0.50, 1.0)
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


func _create_selection_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = INK_MAIN
	return style


func _create_transparent_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	return style


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
