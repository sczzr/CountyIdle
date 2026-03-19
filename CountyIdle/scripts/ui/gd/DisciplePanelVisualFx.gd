extends Node

const BG_DARK := Color(0.027, 0.033, 0.030, 0.96)
const PANEL_DARK := Color(0.075, 0.085, 0.081, 0.90)
const PANEL_EDGE := Color(0.420, 0.384, 0.267, 0.45)
const GOLD := Color(0.894, 0.753, 0.310, 1.0)
const GOLD_DIM := Color(0.420, 0.384, 0.267, 1.0)
const INK_MAIN := Color(0.910, 0.922, 0.914, 1.0)
const INK_MUTED := Color(0.557, 0.596, 0.580, 1.0)
const INK_DIM := Color(0.420, 0.384, 0.267, 0.85)
const ACCENT := Color(0.247, 0.851, 0.659, 1.0)
const DANGER := Color(0.851, 0.282, 0.220, 1.0)

# 维持国风手札气质的字体与形状素材。
const FONT_TITLE := preload("res://assets/ui/fonts/MaShanZheng-Regular.ttf")
const FONT_BODY := preload("res://assets/ui/fonts/NotoSerifSC[wght].ttf")
const TAG_POLY_TEXTURE := preload("res://assets/ui/shapes/tag_poly.svg")

var _backdrop: ColorRect
var _tree_page: Control
var _profile_page: Control
var _roster_panel: Control
var _profile_row: Control
var _back_button: Button
var _close_button: Button
var _current_tween: Tween


func _ready() -> void:
	# 视觉层只注入样式与动效，权威数据仍由 C# 面板维护。
	var root: Node = get_parent()
	_backdrop = root.get_node("Backdrop")
	_tree_page = root.get_node("ScreenMargin/ScreenRoot/TreePage")
	_profile_page = root.get_node("ScreenMargin/ScreenRoot/ProfilePage")
	_roster_panel = root.get_node("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel")
	_profile_row = root.get_node("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow")
	_back_button = root.get_node("TopOverlay/BackButton")
	_close_button = root.get_node("TopOverlay/CloseButton")
	apply_theme_styles()
	reset_state()


func apply_theme_styles() -> void:
	var root := get_parent()
	_backdrop.color = BG_DARK

	var hex_grid := root.get_node_or_null("HexGrid") as TextureRect
	if hex_grid != null:
		hex_grid.self_modulate = Color(1, 1, 1, 0.08)

	for path in [
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/SummaryPanel",
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/FilterPanel",
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/DebugPanel",
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel",
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TreeCountBadge",
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/SectRootCenter/SectRootCard",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/MiddlePanel",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/RealmBox",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/QiSeaBox",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/TraitPanel",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/LogPanel"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_panel_glass_style())

	_apply_stylebox_safe(
		root,
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/CombatTag",
		"panel",
		_create_combat_tag_style()
	)
	_apply_stylebox_safe(
		root,
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/RootCircleWrap/RootCircle",
		"panel",
		_create_circle_style()
	)
	_apply_stylebox_safe(
		root,
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TitleGroup/TitleSeal",
		"panel",
		_create_seal_style()
	)
	_apply_stylebox_safe(
		root,
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/PeakColumnTemplate/PeakCard",
		"panel",
		_create_peak_card_style()
	)
	_apply_stylebox_safe(
		root,
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/HallGroupTemplate/HallCard",
		"panel",
		_create_hall_card_style()
	)

	_style_option_button(root, "ScreenMargin/ScreenRoot/TreePage/TreeColumn/FilterPanel/FilterMargin/FilterColumn/FilterOption")
	_style_option_button(root, "ScreenMargin/ScreenRoot/TreePage/TreeColumn/FilterPanel/FilterMargin/FilterColumn/SortOption")

	for path in [
		"TopOverlay/BackButton",
		"TopOverlay/CloseButton",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionGrid/DirectiveNoneButton",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionGrid/DirectiveOuterButton",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionGrid/DirectiveStewardButton",
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionGrid/CultivationJumpButton",
		"ScreenMargin/ScreenRoot/TreePage/TreeColumn/DebugPanel/DebugMargin/DebugRow/RandomRosterButton"
	]:
		_style_button(root, path)
	style_roster_card(root.get_node("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/DiscipleCardTemplate"), false)

	_style_progress_bar(root, "ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/RealmBox/RealmMargin/RealmColumn/RealmProgress", GOLD)
	_style_progress_bar(root, "ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/QiSeaBox/QiSeaMargin/QiSeaColumn/QiSeaProgress", ACCENT)

	for entry in [
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TitleGroup/TitleColumn/TitleLabel", 26, GOLD, true],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TitleGroup/TitleColumn/SubtitleLabel", 12, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TitleGroup/TitleSeal/SealLabel", 24, GOLD, true],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TreeCountBadge/TreeCountLabel", 12, GOLD, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/SectRootCenter/SectRootCard/SectRootMargin/SectRootColumn/SectRoleLabel", 11, GOLD_DIM, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/SectRootCenter/SectRootCard/SectRootMargin/SectRootColumn/SectTitleLabel", 26, GOLD, true],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/SectRootCenter/SectRootCard/SectRootMargin/SectRootColumn/SectMetaLabel", 11, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/SummaryPanel/SummaryMargin/SummaryColumn/SummaryTitle", 13, GOLD_DIM, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/SummaryPanel/SummaryMargin/SummaryColumn/SummaryLabel", 12, INK_MAIN, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/SummaryPanel/SummaryMargin/SummaryColumn/GovernanceLabel", 11, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/ProfileName", 30, INK_MAIN, true],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/ProfileMeta", 12, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/ProfileStatus", 12, INK_DIM, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/RootCircleWrap/RootCircle/RootCircleLabel", 12, GOLD, true],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveHeader/DirectiveStatus", 12, INK_MAIN, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveHeader/DirectiveEffect", 11, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/RealmBox/RealmMargin/RealmColumn/RealmStatus", 12, INK_MAIN, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/RealmBox/RealmMargin/RealmColumn/RealmHint", 11, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/QiSeaBox/QiSeaMargin/QiSeaColumn/QiSeaHint", 11, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/CombatTag/CombatMargin/CombatColumn/CombatMain", 20, DANGER, true],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/CombatTag/CombatMargin/CombatColumn/CombatHint", 11, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/TraitPanel/TraitMargin/TraitColumn/TraitTitle", 12, GOLD_DIM, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionTitle", 12, GOLD_DIM, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/LogPanel/LogMargin/LogColumn/LogHeaderRow/LogTitle", 12, INK_MAIN, false],
		["ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/LogPanel/LogMargin/LogColumn/LogSummary", 12, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/HintLabel", 11, INK_MUTED, false]
	]:
		_apply_label_style(root, entry[0], entry[1], entry[2], entry[3])

	for entry in [
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/PeakColumnTemplate/PeakCard/PeakMargin/PeakColumn/PeakTagLabel", 10, ACCENT, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/PeakColumnTemplate/PeakCard/PeakMargin/PeakColumn/PeakTitleLabel", 18, INK_MAIN, true],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/PeakColumnTemplate/PeakCard/PeakMargin/PeakColumn/PeakCountLabel", 11, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/HallGroupTemplate/HallCard/HallMargin/HallColumn/HallTitleLabel", 13, GOLD, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/HallGroupTemplate/HallCard/HallMargin/HallColumn/HallMetaLabel", 10, INK_MUTED, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/DiscipleCardTemplate/CardMargin/CardColumn/DiscipleBadgeLabel", 10, DANGER, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/DiscipleCardTemplate/CardMargin/CardColumn/DiscipleNameLabel", 16, INK_MAIN, true],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/DiscipleCardTemplate/CardMargin/CardColumn/DiscipleRealmLabel", 11, ACCENT, false],
		["ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/DiscipleCardTemplate/CardMargin/CardColumn/DiscipleDutyLabel", 10, INK_MUTED, false]
	]:
		_apply_label_style(root, entry[0], entry[1], entry[2], entry[3])

	var branch_line := root.get_node_or_null("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/BranchWrap/BranchLine") as ColorRect
	if branch_line != null:
		branch_line.color = Color(GOLD.r, GOLD.g, GOLD.b, 0.22)
	var root_connector := root.get_node_or_null("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/RootConnectorCenter/RootConnector") as ColorRect
	if root_connector != null:
		root_connector.color = Color(GOLD.r, GOLD.g, GOLD.b, 0.35)

	for metric_key in ["Insight", "Potential", "Health", "Craft", "Mood", "HeartState", "Combat", "Execution", "Contribution"]:
		_apply_metric_tile_style(root, metric_key)


func play_open() -> void:
	# 开场轻微淡入，让宗门大谱有“翻卷”气质。
	if _tree_page == null or _profile_page == null:
		return
	_fade_in_page(_profile_page if _profile_page.visible else _tree_page)


func switch_to_tree() -> void:
	# 切回宗门大谱时做轻微淡入。
	if _tree_page == null:
		return
	_fade_in_page(_tree_page)


func switch_to_profile() -> void:
	# 进入命谱详情时做轻微淡入。
	if _profile_page == null:
		return
	_fade_in_page(_profile_page)


func pulse_roster_refresh() -> void:
	# 名册刷新时做一次轻微脉冲，避免界面僵硬。
	if _roster_panel == null:
		return
	_kill_tween()
	_roster_panel.scale = Vector2.ONE
	_current_tween = create_tween()
	_current_tween.tween_property(_roster_panel, "scale", Vector2(1.02, 1.02), 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	_current_tween.tween_property(_roster_panel, "scale", Vector2.ONE, 0.18).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)


func transition_profile_card() -> void:
	# 详情卡片更新时做轻微抬升，提示已切换目标。
	if _profile_row == null:
		return
	_kill_tween()
	_profile_row.scale = Vector2.ONE
	_current_tween = create_tween()
	_current_tween.tween_property(_profile_row, "scale", Vector2(1.01, 1.01), 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	_current_tween.tween_property(_profile_row, "scale", Vector2.ONE, 0.18).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)


func apply_metric_value_tone(value_label: Label, value: int) -> void:
	# 数值越高越显色，突出天赋差异。
	var color := INK_MAIN
	if value >= 85:
		color = DANGER
	elif value >= 70:
		color = GOLD
	elif value <= 30:
		color = INK_MUTED
	value_label.add_theme_color_override("font_color", color)
	value_label.add_theme_font_override("font", FONT_BODY)
	value_label.add_theme_font_size_override("font_size", 20)


func style_trait_tag(panel: PanelContainer, label: Label) -> void:
	# 性情标签使用金线边框强化玉简感。
	panel.add_theme_stylebox_override("panel", _create_tag_style())
	label.add_theme_font_override("font", FONT_BODY)
	label.add_theme_font_size_override("font_size", 12)
	label.add_theme_color_override("font_color", GOLD)


func reset_state() -> void:
	# 恢复默认透明度与缩放，避免上次动效残留。
	if _tree_page != null:
		_tree_page.modulate = Color(1, 1, 1, 1)
	if _profile_page != null:
		_profile_page.modulate = Color(1, 1, 1, 1)
	if _roster_panel != null:
		_roster_panel.scale = Vector2.ONE
	if _profile_row != null:
		_profile_row.scale = Vector2.ONE


func _fade_in_page(page: Control) -> void:
	_kill_tween()
	page.modulate.a = 0.0
	_current_tween = create_tween()
	_current_tween.tween_property(page, "modulate:a", 1.0, 0.22).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)


func _apply_stylebox_safe(root: Node, path: String, theme_type: String, style: StyleBox) -> void:
	var control := root.get_node_or_null(path) as Control
	if control == null:
		push_warning("DisciplePanelVisualFx missing node: %s" % path)
		return
	control.add_theme_stylebox_override(theme_type, style)


func _apply_label_style(root: Node, path: String, font_size: int, color: Color, use_title_font: bool) -> void:
	var label := root.get_node_or_null(path) as Label
	if label == null:
		push_warning("DisciplePanelVisualFx missing label: %s" % path)
		return
	label.add_theme_font_override("font", FONT_TITLE if use_title_font else FONT_BODY)
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", color)


func _style_button(root: Node, path: String) -> void:
	var button := root.get_node_or_null(path) as Button
	if button == null:
		return
	button.add_theme_font_override("font", FONT_BODY)
	button.add_theme_font_size_override("font_size", 12)
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_hover_color", GOLD)
	button.add_theme_color_override("font_pressed_color", GOLD)
	button.add_theme_color_override("font_disabled_color", INK_MUTED)
	button.add_theme_stylebox_override("normal", _create_button_style(false))
	button.add_theme_stylebox_override("hover", _create_button_style(true))
	button.add_theme_stylebox_override("pressed", _create_button_style(true))
	button.add_theme_stylebox_override("disabled", _create_button_style(false, true))


func style_peak_card(panel: PanelContainer) -> void:
	if panel == null:
		return
	panel.add_theme_stylebox_override("panel", _create_peak_card_style())


func style_hall_card(panel: PanelContainer) -> void:
	if panel == null:
		return
	panel.add_theme_stylebox_override("panel", _create_hall_card_style())


func style_roster_card(panel: PanelContainer, is_selected: bool) -> void:
	if panel == null:
		return
	panel.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
	panel.add_theme_stylebox_override("panel", _create_roster_card_style(is_selected))


func _style_option_button(root: Node, path: String) -> void:
	var option_button := root.get_node_or_null(path) as OptionButton
	if option_button == null:
		return
	option_button.add_theme_font_override("font", FONT_BODY)
	option_button.add_theme_font_size_override("font_size", 12)
	option_button.add_theme_color_override("font_color", INK_MAIN)
	option_button.add_theme_stylebox_override("normal", _create_button_style(false))
	option_button.add_theme_stylebox_override("hover", _create_button_style(true))
	option_button.add_theme_stylebox_override("pressed", _create_button_style(true))
	option_button.add_theme_stylebox_override("focus", _create_button_style(true))


func _style_roster_tree(tree: Tree) -> void:
	tree.add_theme_color_override("font_color", INK_MAIN)
	tree.add_theme_color_override("font_selected_color", GOLD)
	tree.add_theme_color_override("guide_color", Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.55))
	tree.add_theme_color_override("relationship_line_color", Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.45))
	tree.add_theme_stylebox_override("selected", _create_selection_style())
	tree.add_theme_stylebox_override("selected_focus", _create_selection_style(true))
	tree.add_theme_stylebox_override("cursor", _create_selection_style())
	tree.add_theme_stylebox_override("cursor_unfocused", _create_selection_style())
	tree.add_theme_stylebox_override("panel", _create_panel_glass_style(0.35))


func _style_progress_bar(root: Node, path: String, fill_color: Color) -> void:
	var progress := root.get_node_or_null(path) as ProgressBar
	if progress == null:
		return
	progress.add_theme_stylebox_override("background", _create_progress_bg())
	progress.add_theme_stylebox_override("fill", _create_progress_fill(fill_color))
	progress.custom_minimum_size = Vector2(0, 12)


func _apply_metric_tile_style(root: Node, key: String) -> void:
	var tile_path := "ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/MiddlePanel/MiddleMargin/MiddleColumn/MetricGrid/%sTile" % key
	_apply_stylebox_safe(root, tile_path, "panel", _create_metric_tile_style())
	var title_label := root.get_node_or_null("%s/%sMargin/%sColumn/%sTitle" % [tile_path, key, key, key]) as Label
	var value_label := root.get_node_or_null("%s/%sMargin/%sColumn/%sValue" % [tile_path, key, key, key]) as Label
	if title_label != null:
		title_label.add_theme_font_override("font", FONT_BODY)
		title_label.add_theme_font_size_override("font_size", 11)
		title_label.add_theme_color_override("font_color", INK_MUTED)
		title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	if value_label != null:
		value_label.add_theme_font_override("font", FONT_BODY)
		value_label.add_theme_font_size_override("font_size", 20)
		value_label.add_theme_color_override("font_color", INK_MAIN)
		value_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER


func _create_panel_glass_style(alpha: float = 0.82) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PANEL_DARK.r, PANEL_DARK.g, PANEL_DARK.b, alpha)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = PANEL_EDGE
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	style.shadow_color = Color(0, 0, 0, 0.45)
	style.shadow_size = 12
	return style


func _create_metric_tile_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PANEL_DARK.r, PANEL_DARK.g, PANEL_DARK.b, 0.65)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.35)
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	return style


func _create_peak_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.09, 0.12, 0.11, 0.92)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.55)
	style.corner_radius_top_left = 5
	style.corner_radius_top_right = 5
	style.corner_radius_bottom_right = 5
	style.corner_radius_bottom_left = 5
	style.shadow_color = Color(0, 0, 0, 0.25)
	style.shadow_size = 8
	return style


func _create_hall_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PANEL_DARK.r, PANEL_DARK.g, PANEL_DARK.b, 0.76)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.42)
	style.corner_radius_top_left = 5
	style.corner_radius_top_right = 5
	style.corner_radius_bottom_right = 5
	style.corner_radius_bottom_left = 5
	return style


func _create_roster_card_style(is_selected: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	var bg_alpha := 0.08
	var border_alpha := 0.24
	var shadow_alpha := 0.10
	if is_selected:
		bg_alpha = 0.18
		border_alpha = 0.72
		shadow_alpha = 0.26
	style.bg_color = Color(0.10, 0.12, 0.11, bg_alpha)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	if is_selected:
		style.border_width_left = 2
		style.border_width_top = 2
		style.border_width_right = 2
		style.border_width_bottom = 2
	style.border_color = Color(GOLD.r, GOLD.g, GOLD.b, border_alpha)
	style.corner_radius_top_left = 5
	style.corner_radius_top_right = 5
	style.corner_radius_bottom_right = 5
	style.corner_radius_bottom_left = 5
	style.content_margin_left = 0
	style.content_margin_top = 0
	style.content_margin_right = 0
	style.content_margin_bottom = 0
	style.shadow_color = Color(GOLD.r, GOLD.g, GOLD.b, shadow_alpha)
	style.shadow_size = 8 if is_selected else 4
	return style


func _create_button_style(is_hovered: bool, is_disabled: bool = false) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	var bg_alpha := 0.08 if is_hovered else 0.04
	var border_alpha := 0.45 if is_hovered else 0.25
	if is_disabled:
		bg_alpha = 0.02
		border_alpha = 0.12
	style.bg_color = Color(0.10, 0.12, 0.11, bg_alpha)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, border_alpha)
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	style.content_margin_left = 8
	style.content_margin_top = 4
	style.content_margin_right = 8
	style.content_margin_bottom = 4
	return style


func _create_selection_style(is_focus: bool = false) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.12 if is_focus else 0.08)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.55 if is_focus else 0.35)
	return style


func _create_progress_bg() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0.35)
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	return style


func _create_progress_fill(color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	style.shadow_color = Color(color.r, color.g, color.b, 0.4)
	style.shadow_size = 6
	return style


func _create_circle_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.10, 0.12, 0.11, 0.75)
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.border_color = GOLD
	style.corner_radius_top_left = 999
	style.corner_radius_top_right = 999
	style.corner_radius_bottom_right = 999
	style.corner_radius_bottom_left = 999
	return style


func _create_seal_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.18)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = GOLD
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	return style


func _create_combat_tag_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(DANGER.r, DANGER.g, DANGER.b, 0.08)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = DANGER
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	return style


func _create_tag_style() -> StyleBox:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.06)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.7)
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	return style


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
