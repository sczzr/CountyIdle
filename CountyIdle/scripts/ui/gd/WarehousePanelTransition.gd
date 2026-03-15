extends Node

const PAPER_MAIN := Color(0.95, 0.92, 0.84, 1.0)
const PAPER_DARK := Color(0.89, 0.85, 0.76, 1.0)
const INK_MAIN := Color(0.17, 0.15, 0.13, 1.0)
const INK_MUTED := Color(0.42, 0.37, 0.33, 1.0)
const SEAL_RED := Color(0.65, 0.16, 0.16, 1.0)
const BORDER_INK := Color(0.29, 0.25, 0.21, 1.0)
const ACCENT_GOLD := Color(0.72, 0.53, 0.04, 1.0)
const ACCENT_BLUE := Color(0.19, 0.33, 0.54, 1.0)
const ACTIVE_SLOT_MODULATE := Color(1.0, 1.0, 1.0, 1.0)
const INACTIVE_SLOT_MODULATE := Color(1.0, 1.0, 1.0, 0.45)
const TOKEN_INACTIVE_MODULATE := Color(1.0, 1.0, 1.0, 0.62)
const TOKEN_GLYPH_INACTIVE_MODULATE := Color(1.0, 1.0, 1.0, 0.70)
const NAME_INACTIVE_MODULATE := Color(1.0, 1.0, 1.0, 0.62)
const TYPE_INACTIVE_MODULATE := Color(1.0, 1.0, 1.0, 0.50)
const AMOUNT_INACTIVE_MODULATE := Color(1.0, 1.0, 1.0, 0.70)

var _paper: Control
var _left_roller: Control
var _right_roller: Control
var _backdrop: Control
var _status_section: PanelContainer
var _capacity_bar_frame: Panel
var _capacity_bar: ProgressBar
var _capacity_tick_overlay: TextureRect
var _chain_info_frame: PanelContainer
var _warning_stamp_label: Label
var _hint_label: Label
var _warehouse_status_value: Label
var _capacity_value_label: Label
var _tier_zero_chain_status_value: Label
var _tab_buttons: Dictionary = {}
var _current_tween: Tween


func _ready() -> void:
	var root: Node = get_parent()
	_paper = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper")
	_left_roller = root.get_node("CenterLayer/LedgerWrapper/FrameRow/LeftRoller")
	_right_roller = root.get_node("CenterLayer/LedgerWrapper/FrameRow/RightRoller")
	_backdrop = root.get_node("Backdrop")
	_status_section = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection")
	_capacity_bar_frame = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/CapacityBarFrame")
	_capacity_bar = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/CapacityBarFrame/CapacityBar")
	_capacity_tick_overlay = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/CapacityBarFrame/CapacityTickOverlay")
	_chain_info_frame = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/ChainInfoFrame")
	_warning_stamp_label = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/StatusRow/WarningStampLabel")
	_hint_label = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/StatusRow/StatusTextColumn/HintLabel")
	_warehouse_status_value = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/StatusRow/StatusTextColumn/WarehouseStatusValue")
	_capacity_value_label = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/StatusRow/CapacityValueLabel")
	_tier_zero_chain_status_value = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/ChainInfoFrame/ChainInfoMargin/TierZeroStatusValue")
	_tab_buttons = {
		"All": root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/AllTabButton") as Button,
		"Basic": root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/BasicTabButton") as Button,
		"Materials": root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/MaterialsTabButton") as Button,
		"Crafted": root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/CraftedTabButton") as Button
	}
	apply_theme_styles()
	reset_state()


func apply_theme_styles() -> void:
	var root := get_parent()

	_paper.add_theme_stylebox_override("panel", _create_paper_style())
	_left_roller.add_theme_stylebox_override("panel", _create_roller_style())
	_right_roller.add_theme_stylebox_override("panel", _create_roller_style())
	_status_section.add_theme_stylebox_override("panel", _create_warning_panel_style(Color(SEAL_RED.r, SEAL_RED.g, SEAL_RED.b, 0.06), SEAL_RED))
	_capacity_bar_frame.add_theme_stylebox_override("panel", _create_capacity_frame_style(SEAL_RED))
	_capacity_bar.add_theme_stylebox_override("background", _create_capacity_bar_background_style())
	_capacity_bar.add_theme_stylebox_override("fill", _create_capacity_bar_fill_style(SEAL_RED))
	_capacity_bar.show_percentage = false
	_capacity_bar.min_value = 0.0
	_capacity_bar.max_value = 100.0
	_capacity_bar.value = 0.0
	_capacity_tick_overlay.texture = _create_capacity_tick_texture()
	_capacity_tick_overlay.stretch_mode = TextureRect.STRETCH_TILE
	_capacity_tick_overlay.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	_capacity_tick_overlay.modulate = Color(0.2, 0.17, 0.13, 0.22)
	_chain_info_frame.add_theme_stylebox_override("panel", _create_note_style())

	var title_label: Label = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/HeaderRow/TitleGroup/TitleLabel")
	title_label.add_theme_font_size_override("font_size", 26)
	title_label.add_theme_color_override("font_color", INK_MAIN)

	var subtitle_label: Label = root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/HeaderRow/TitleGroup/SubtitleLabel")
	subtitle_label.add_theme_font_size_override("font_size", 14)
	subtitle_label.add_theme_color_override("font_color", INK_MUTED)

	_warning_stamp_label.rotation = -0.08
	_warning_stamp_label.add_theme_font_size_override("font_size", 14)
	_warning_stamp_label.add_theme_color_override("font_color", SEAL_RED)
	_hint_label.add_theme_font_size_override("font_size", 14)
	_warehouse_status_value.add_theme_font_size_override("font_size", 12)
	_warehouse_status_value.add_theme_color_override("font_color", INK_MUTED)
	_capacity_value_label.add_theme_font_size_override("font_size", 20)
	_capacity_value_label.add_theme_color_override("font_color", INK_MAIN)
	_tier_zero_chain_status_value.add_theme_font_size_override("font_size", 13)
	_tier_zero_chain_status_value.add_theme_color_override("font_color", INK_MAIN)

	for path in [
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ManufactureSection/ManufactureTitle",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/BuildSection/BuildTitle",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/ChainTitle"
	]:
		var label: Label = root.get_node(path)
		label.add_theme_font_size_override("font_size", 18)
		label.add_theme_color_override("font_color", INK_MAIN)

	_apply_close_button_style(root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/HeaderRow/CloseButton"))
	_apply_order_button_style(root.get_node("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ManufactureSection/LockedForgeButton"), true)
	for path in [
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/BuildSection/UpgradeButton",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ManufactureSection/CraftToolsButton",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/BuildSection/BuildWorkshopButton",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/BuildSection/BuildAdministrationButton",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/BuildForestryChainButton",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/BuildMasonryChainButton",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/BuildMedicinalChainButton",
		"CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/BuildFiberChainButton"
	]:
		_apply_order_button_style(root.get_node(path), false)

	apply_tab_button_state("All")


func apply_tab_button_state(tab_name: String) -> void:
	for key in _tab_buttons.keys():
		var button: Button = _tab_buttons[key] as Button
		_apply_tab_button_style(button, key == tab_name)


func apply_capacity_visual(load_rate: float) -> void:
	var accent: Color = _resolve_capacity_accent(load_rate)
	var background: Color = _resolve_capacity_background(load_rate)
	_status_section.add_theme_stylebox_override("panel", _create_warning_panel_style(background, accent))
	_capacity_bar_frame.add_theme_stylebox_override("panel", _create_capacity_frame_style(accent))
	_capacity_bar.add_theme_stylebox_override("fill", _create_capacity_bar_fill_style(accent))
	_capacity_bar.value = clampf(load_rate, 0.0, 100.0)
	_hint_label.add_theme_color_override("font_color", accent)
	_warning_stamp_label.add_theme_color_override("font_color", accent)
	_capacity_value_label.add_theme_color_override("font_color", INK_MAIN if load_rate >= 90.0 else accent)


func style_resource_slot(card, token, token_glyph, name_label, type_label, amount_label, accent_color: Color) -> void:
	if card is PanelContainer:
		card.add_theme_stylebox_override("panel", _create_slot_style())
	if token is PanelContainer:
		token.add_theme_stylebox_override("panel", _create_token_style(accent_color))
	if token_glyph is Label:
		token_glyph.add_theme_font_size_override("font_size", 20)
		token_glyph.add_theme_color_override("font_color", PAPER_MAIN)
	if name_label is Label:
		name_label.add_theme_font_size_override("font_size", 17)
		name_label.add_theme_color_override("font_color", INK_MAIN)
	if type_label is Label:
		type_label.add_theme_font_size_override("font_size", 11)
		type_label.add_theme_color_override("font_color", INK_MUTED)
	if amount_label is Label:
		amount_label.add_theme_font_size_override("font_size", 24)
		amount_label.add_theme_color_override("font_color", INK_MAIN)


func apply_resource_slot_state(card, token, token_glyph, name_label, type_label, amount_label, has_amount: bool) -> void:
	if card is CanvasItem:
		card.modulate = ACTIVE_SLOT_MODULATE if has_amount else INACTIVE_SLOT_MODULATE
	if token is CanvasItem:
		token.modulate = ACTIVE_SLOT_MODULATE if has_amount else TOKEN_INACTIVE_MODULATE
	if token_glyph is CanvasItem:
		token_glyph.modulate = ACTIVE_SLOT_MODULATE if has_amount else TOKEN_GLYPH_INACTIVE_MODULATE
	if name_label is CanvasItem:
		name_label.modulate = ACTIVE_SLOT_MODULATE if has_amount else NAME_INACTIVE_MODULATE
	if type_label is CanvasItem:
		type_label.modulate = ACTIVE_SLOT_MODULATE if has_amount else TYPE_INACTIVE_MODULATE
	if amount_label is CanvasItem:
		amount_label.modulate = ACTIVE_SLOT_MODULATE if has_amount else AMOUNT_INACTIVE_MODULATE


func play_open() -> void:
	_kill_tween()
	reset_state()
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_backdrop, "modulate:a", 1.0, 0.18).from(0.0)
	_current_tween.tween_property(_paper, "modulate:a", 1.0, 0.18).from(0.0)
	_current_tween.tween_property(_paper, "scale", Vector2.ONE, 0.22).from(Vector2(0.985, 0.985))
	_current_tween.tween_property(_left_roller, "modulate:a", 1.0, 0.16).from(0.0)
	_current_tween.tween_property(_right_roller, "modulate:a", 1.0, 0.16).from(0.0)


func play_tab_switch(_tab_name: String) -> void:
	_kill_tween()
	_current_tween = create_tween()
	_current_tween.tween_property(_paper, "scale", Vector2.ONE * 1.008, 0.08)
	_current_tween.tween_property(_paper, "scale", Vector2.ONE, 0.12)


func play_invalid_feedback() -> void:
	_kill_tween()
	var original_position: Vector2 = _paper.position
	_current_tween = create_tween()
	_current_tween.tween_property(_paper, "position:x", original_position.x - 6.0, 0.04)
	_current_tween.tween_property(_paper, "position:x", original_position.x + 6.0, 0.05)
	_current_tween.tween_property(_paper, "position:x", original_position.x, 0.04)


func reset_state() -> void:
	_kill_tween()
	_backdrop.modulate.a = 1.0
	_paper.modulate.a = 1.0
	_left_roller.modulate.a = 1.0
	_right_roller.modulate.a = 1.0
	_paper.scale = Vector2.ONE


func _apply_tab_button_style(button: Button, active: bool) -> void:
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_CENTER
	button.add_theme_font_size_override("font_size", 16)
	button.add_theme_stylebox_override("normal", _create_tab_style(active))
	button.add_theme_stylebox_override("hover", _create_tab_style(true))
	button.add_theme_stylebox_override("pressed", _create_tab_style(true))
	button.add_theme_stylebox_override("disabled", _create_tab_style(false))
	button.add_theme_color_override("font_color", INK_MAIN if active else INK_MUTED)
	button.add_theme_color_override("font_hover_color", INK_MAIN)
	button.add_theme_color_override("font_pressed_color", INK_MAIN)
	button.add_theme_color_override("font_disabled_color", Color(INK_MUTED.r, INK_MUTED.g, INK_MUTED.b, 0.7))


func _apply_order_button_style(button: Button, locked: bool) -> void:
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	button.add_theme_font_size_override("font_size", 15)
	button.add_theme_stylebox_override("normal", _create_order_button_style(locked, false))
	button.add_theme_stylebox_override("hover", _create_order_button_style(locked, true))
	button.add_theme_stylebox_override("pressed", _create_order_button_style(locked, true))
	button.add_theme_stylebox_override("disabled", _create_order_button_style(true, false))
	button.add_theme_color_override("font_color", INK_MUTED if locked else INK_MAIN)
	button.add_theme_color_override("font_hover_color", INK_MUTED if locked else PAPER_MAIN)
	button.add_theme_color_override("font_pressed_color", INK_MUTED if locked else PAPER_MAIN)
	button.add_theme_color_override("font_disabled_color", INK_MUTED)


func _apply_close_button_style(button: Button) -> void:
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_CENTER
	button.add_theme_font_size_override("font_size", 24)
	button.add_theme_stylebox_override("normal", _create_transparent_style())
	button.add_theme_stylebox_override("hover", _create_transparent_style())
	button.add_theme_stylebox_override("pressed", _create_transparent_style())
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_hover_color", SEAL_RED)
	button.add_theme_color_override("font_pressed_color", SEAL_RED)


func _resolve_capacity_accent(load_rate: float) -> Color:
	if load_rate >= 90.0:
		return SEAL_RED
	if load_rate >= 70.0:
		return ACCENT_GOLD
	return ACCENT_BLUE


func _resolve_capacity_background(load_rate: float) -> Color:
	if load_rate >= 90.0:
		return Color(SEAL_RED.r, SEAL_RED.g, SEAL_RED.b, 0.06)
	if load_rate >= 70.0:
		return Color(ACCENT_GOLD.r, ACCENT_GOLD.g, ACCENT_GOLD.b, 0.06)
	return Color(0.0, 0.0, 0.0, 0.02)


func _create_paper_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_MAIN
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.48, 0.42, 0.35, 0.45)
	style.corner_radius_top_left = 2
	style.corner_radius_top_right = 2
	style.corner_radius_bottom_right = 2
	style.corner_radius_bottom_left = 2
	style.shadow_color = Color(0.0, 0.0, 0.0, 0.35)
	style.shadow_size = 12
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


func _create_warning_panel_style(background_color: Color, border_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background_color
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = border_color
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


func _create_tab_style(active: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_DARK if active else Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.0)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 0
	style.border_color = BORDER_INK if active else Color(0.0, 0.0, 0.0, 0.0)
	style.content_margin_left = 12
	style.content_margin_top = 8
	style.content_margin_right = 12
	style.content_margin_bottom = 8
	return style


func _create_slot_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.32)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(BORDER_INK.r, BORDER_INK.g, BORDER_INK.b, 0.55)
	style.corner_radius_top_left = 2
	style.corner_radius_top_right = 2
	style.corner_radius_bottom_right = 2
	style.corner_radius_bottom_left = 2
	return style


func _create_token_style(accent_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(accent_color.r, accent_color.g, accent_color.b, 0.95)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.12, 0.10, 0.08, 0.85)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	style.shadow_color = Color(0.0, 0.0, 0.0, 0.35)
	style.shadow_size = 6
	return style


func _create_capacity_frame_style(accent_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_DARK.r, PAPER_DARK.g, PAPER_DARK.b, 0.35)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(accent_color.r, accent_color.g, accent_color.b, 0.9)
	return style


func _create_capacity_bar_background_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_DARK.r, PAPER_DARK.g, PAPER_DARK.b, 0.55)
	return style


func _create_capacity_bar_fill_style(accent_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = accent_color
	return style


func _create_capacity_tick_texture() -> Texture2D:
	const WIDTH := 12
	const HEIGHT := 6
	var image := Image.create(WIDTH, HEIGHT, false, Image.FORMAT_RGBA8)
	image.fill(Color(0.0, 0.0, 0.0, 0.0))
	for y in range(HEIGHT):
		image.set_pixel(WIDTH - 1, y, Color(0.0, 0.0, 0.0, 0.35))
	return ImageTexture.create_from_image(image)


func _create_order_button_style(locked: bool, active: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = INK_MAIN if active and not locked else Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.0)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(INK_MUTED.r, INK_MUTED.g, INK_MUTED.b, 0.65) if locked else INK_MAIN
	style.content_margin_left = 14
	style.content_margin_top = 12
	style.content_margin_right = 14
	style.content_margin_bottom = 12
	return style


func _create_transparent_style() -> StyleBoxEmpty:
	return StyleBoxEmpty.new()


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
