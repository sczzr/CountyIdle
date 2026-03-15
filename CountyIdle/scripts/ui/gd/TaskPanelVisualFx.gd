extends Node

const PAPER_MAIN := Color(0.95, 0.92, 0.84, 1.0)
const PAPER_DARK := Color(0.89, 0.85, 0.76, 1.0)
const INK_MAIN := Color(0.17, 0.15, 0.13, 1.0)
const INK_MUTED := Color(0.42, 0.37, 0.33, 1.0)
const SEAL_RED := Color(0.64, 0.19, 0.14, 1.0)
const BORDER_INK := Color(0.29, 0.25, 0.21, 1.0)
const SIDEBAR_ACTIVE := Color(0.63, 0.19, 0.14, 1.0)

var _overlay: ColorRect
var _frame: Control
var _policy_tab: Control
var _season_tab: Control
var _rules_tab: Control
var _affairs_tab: Control
var _affairs_detail_panel: Control
var _current_tween: Tween


func _ready() -> void:
	var root: Node = get_parent()
	_overlay = root.get_node("Overlay")
	_frame = root.get_node("Overlay/Center/Frame")
	_policy_tab = root.get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab")
	_season_tab = root.get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab")
	_rules_tab = root.get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab")
	_affairs_tab = root.get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab")
	_affairs_detail_panel = root.get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsDetailPanel")
	apply_theme_styles()
	reset_state()


func apply_theme_styles() -> void:
	_apply_panel_style("Overlay/Center/Frame", _create_frame_style())
	_apply_panel_style("Overlay/Center/Frame/RootColumn/HeaderPanel", _create_header_style())
	_apply_panel_style("Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel", _create_sidebar_panel_style())

	for path in [
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/PolicySummaryPanel",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/SeasonSummaryPanel",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/RulesSummaryPanel",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsSummaryPanel"
	]:
		_apply_panel_style(path, _create_summary_style())

	for path in [
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/DevelopmentCard",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LawCard",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/TalentCard",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LockedCard",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/QuarterDecreeCard",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/AffairsRuleCard",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DoctrineRuleCard",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DisciplineRuleCard",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard"
	]:
		_apply_panel_style(path, _create_card_style())

	for path in [
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsListPanel",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsDetailPanel"
	]:
		_apply_panel_style(path, _create_inner_paper_style())

	for path in [
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentCapsule",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LawCard/LawCardMargin/LawCardRow/LawCapsule",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/TalentCard/TalentCardMargin/TalentCardRow/TalentCapsule",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LockedCard/LockedCardMargin/LockedCardRow/LockedStateCapsule",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeCapsule",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleCapsule",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleCapsule",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleCapsule"
	]:
		_apply_panel_style(path, _create_control_capsule_style())

	for entry in [
		["Overlay/Center/Frame/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/TitleLabel", 24, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/StatRow/ContributionRow/ContributionTitle", 15, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/StatRow/SpiritStoneRow/SpiritStoneTitle", 15, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/StatRow/ContributionRow/ContributionValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/StatRow/SpiritStoneRow/SpiritStoneValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/PolicySummaryPanel/PolicySummaryMargin/PolicySummaryColumn/PolicySummaryTitle", 15, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/PolicySummaryPanel/PolicySummaryMargin/PolicySummaryColumn/PolicySummaryLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/SeasonSummaryPanel/SeasonSummaryMargin/SeasonSummaryColumn/SeasonSummaryTitle", 15, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/SeasonSummaryPanel/SeasonSummaryMargin/SeasonSummaryColumn/SeasonSummaryLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/RulesSummaryPanel/RulesSummaryMargin/RulesSummaryColumn/RulesSummaryTitle", 15, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/RulesSummaryPanel/RulesSummaryMargin/RulesSummaryColumn/RulesSummaryLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsSummaryPanel/AffairsSummaryMargin/AffairsSummaryColumn/AffairsSummaryTitle", 15, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsSummaryPanel/AffairsSummaryMargin/AffairsSummaryColumn/AffairsSummaryLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentInfoColumn/DevelopmentTitle", 18, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentInfoColumn/DevelopmentHintLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentCapsule/DevelopmentCapsuleRow/DevelopmentValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LawCard/LawCardMargin/LawCardRow/LawInfoColumn/LawTitle", 18, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LawCard/LawCardMargin/LawCardRow/LawInfoColumn/LawHintLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LawCard/LawCardMargin/LawCardRow/LawCapsule/LawCapsuleRow/LawValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/TalentCard/TalentCardMargin/TalentCardRow/TalentInfoColumn/TalentTitle", 18, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/TalentCard/TalentCardMargin/TalentCardRow/TalentInfoColumn/TalentHintLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/TalentCard/TalentCardMargin/TalentCardRow/TalentCapsule/TalentCapsuleRow/TalentValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LockedCard/LockedCardMargin/LockedCardRow/LockedInfoColumn/LockedTitle", 18, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LockedCard/LockedCardMargin/LockedCardRow/LockedInfoColumn/LockedDescription", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LockedCard/LockedCardMargin/LockedCardRow/LockedStateCapsule/LockedStateLabel", 14, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeInfoColumn/QuarterDecreeTitle", 18, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeInfoColumn/QuarterDecreeHintLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeCapsule/QuarterDecreeCapsuleRow/QuarterDecreeValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleInfoColumn/AffairsRuleTitle", 18, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleInfoColumn/AffairsRuleHintLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleCapsule/AffairsRuleCapsuleRow/AffairsRuleValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleInfoColumn/DoctrineRuleTitle", 18, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleInfoColumn/DoctrineRuleHintLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleCapsule/DoctrineRuleCapsuleRow/DoctrineRuleValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleInfoColumn/DisciplineRuleTitle", 18, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleInfoColumn/DisciplineRuleHintLabel", 13, INK_MUTED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleCapsule/DisciplineRuleCapsuleRow/DisciplineRuleValueLabel", 16, SEAL_RED],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsListTitle", 15, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsDetailPanel/AffairsDetailMargin/DetailLabel", 13, INK_MAIN],
		["Overlay/Center/Frame/RootColumn/HintLabel", 12, INK_MUTED]
	]:
		_apply_label_style(entry[0], entry[1], entry[2])

	var hint_label: Label = get_parent().get_node("Overlay/Center/Frame/RootColumn/HintLabel")
	hint_label.add_theme_constant_override("line_spacing", 1)

	var close_button: Button = get_parent().get_node("Overlay/Center/Frame/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/CloseButton")
	close_button.add_theme_font_size_override("font_size", 24)
	close_button.add_theme_color_override("font_color", INK_MUTED)
	close_button.add_theme_color_override("font_hover_color", SEAL_RED)
	close_button.add_theme_color_override("font_pressed_color", SEAL_RED)
	for state in ["normal", "hover", "pressed", "focus"]:
		close_button.add_theme_stylebox_override(state, _create_transparent_style())

	for path in [
		"Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel/SidebarColumn/PolicyTabButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel/SidebarColumn/SeasonTabButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel/SidebarColumn/RulesTabButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel/SidebarColumn/AffairsTabButton"
	]:
		_apply_sidebar_button_style(get_parent().get_node(path))

	for path in [
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentCapsule/DevelopmentCapsuleRow/DevelopmentPrevButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentCapsule/DevelopmentCapsuleRow/DevelopmentNextButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LawCard/LawCardMargin/LawCardRow/LawCapsule/LawCapsuleRow/LawPrevButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/LawCard/LawCardMargin/LawCardRow/LawCapsule/LawCapsuleRow/LawNextButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/TalentCard/TalentCardMargin/TalentCardRow/TalentCapsule/TalentCapsuleRow/TalentPrevButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/PolicyTab/TalentCard/TalentCardMargin/TalentCardRow/TalentCapsule/TalentCapsuleRow/TalentNextButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeCapsule/QuarterDecreeCapsuleRow/QuarterDecreePrevButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/SeasonTab/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeCapsule/QuarterDecreeCapsuleRow/QuarterDecreeNextButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleCapsule/AffairsRuleCapsuleRow/AffairsRulePrevButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleCapsule/AffairsRuleCapsuleRow/AffairsRuleNextButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleCapsule/DoctrineRuleCapsuleRow/DoctrineRulePrevButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleCapsule/DoctrineRuleCapsuleRow/DoctrineRuleNextButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleCapsule/DisciplineRuleCapsuleRow/DisciplineRulePrevButton",
		"Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/RulesTab/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleCapsule/DisciplineRuleCapsuleRow/DisciplineRuleNextButton"
	]:
		_apply_arrow_button_style(get_parent().get_node(path))

	_apply_footer_action_button_style(get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsActionRow/MinusOneButton"), false)
	_apply_footer_action_button_style(get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsActionRow/PlusOneButton"), false)
	_apply_footer_action_button_style(get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsActionRow/PlusFiveButton"), false)
	_apply_footer_action_button_style(get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsActionRow/ResetButton"), true)

	var task_list: ItemList = get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsListPanel/AffairsListPanelMargin/TaskList")
	task_list.add_theme_stylebox_override("panel", _create_transparent_style())
	task_list.add_theme_stylebox_override("cursor", _create_selection_style())
	task_list.add_theme_stylebox_override("cursor_unfocused", _create_selection_style())
	task_list.add_theme_color_override("font_color", INK_MAIN)
	task_list.add_theme_color_override("font_selected_color", PAPER_MAIN)
	task_list.add_theme_constant_override("h_separation", 6)
	task_list.add_theme_constant_override("v_separation", 5)

	var affairs_split: HSplitContainer = get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/ContentMargin/ContentScroll/ContentStack/AffairsTab/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit")
	affairs_split.split_offset = 280


func play_open() -> void:
	_kill_tween()
	_overlay.modulate.a = 0.0
	_frame.modulate.a = 0.0
	_frame.scale = Vector2.ONE
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_overlay, "modulate:a", 1.0, 0.18)
	_current_tween.tween_property(_frame, "modulate:a", 1.0, 0.2)


func play_tab_switch(tab_name: String) -> void:
	_kill_tween()
	var target_tab: Control = _resolve_tab(tab_name)
	target_tab.modulate.a = 0.74
	target_tab.scale = Vector2.ONE
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(target_tab, "modulate:a", 1.0, 0.14)


func apply_tab_button_state(tab_name: String) -> void:
	var buttons: Dictionary = {
		"Policy": get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel/SidebarColumn/PolicyTabButton") as Button,
		"Season": get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel/SidebarColumn/SeasonTabButton") as Button,
		"Rules": get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel/SidebarColumn/RulesTabButton") as Button,
		"Affairs": get_parent().get_node("Overlay/Center/Frame/RootColumn/BodyRow/SidebarPanel/SidebarColumn/AffairsTabButton") as Button
	}

	for key in buttons.keys():
		var button: Button = buttons[key] as Button
		button.add_theme_color_override("font_color", INK_MAIN if key == tab_name else INK_MUTED)


func pulse_task_detail() -> void:
	_kill_tween()
	_affairs_detail_panel.scale = Vector2.ONE
	_affairs_detail_panel.modulate.a = 0.86
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_affairs_detail_panel, "modulate:a", 1.0, 0.1)


func reset_state() -> void:
	_kill_tween()
	_overlay.modulate.a = 1.0
	_frame.modulate.a = 1.0
	_frame.scale = Vector2.ONE
	_affairs_detail_panel.scale = Vector2.ONE
	_affairs_detail_panel.modulate.a = 1.0
	for tab in [_policy_tab, _season_tab, _rules_tab, _affairs_tab]:
		tab.modulate.a = 1.0
		tab.scale = Vector2.ONE


func _resolve_tab(tab_name: String) -> Control:
	match tab_name:
		"Season":
			return _season_tab
		"Rules":
			return _rules_tab
		"Affairs":
			return _affairs_tab
		_:
			return _policy_tab


func _apply_panel_style(path: String, style: StyleBox) -> void:
	var panel := get_parent().get_node(path)
	if panel is Control:
		panel.add_theme_stylebox_override("panel", style)


func _apply_label_style(path: String, font_size: int, color: Color) -> void:
	var label: Label = get_parent().get_node(path)
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", color)


func _apply_sidebar_button_style(button: Button) -> void:
	button.add_theme_font_size_override("font_size", 18)
	button.add_theme_color_override("font_color", INK_MUTED)
	button.add_theme_color_override("font_hover_color", INK_MAIN)
	button.add_theme_color_override("font_pressed_color", INK_MAIN)
	button.add_theme_stylebox_override("normal", _create_sidebar_item_style(false, false))
	button.add_theme_stylebox_override("hover", _create_sidebar_item_style(false, true))
	button.add_theme_stylebox_override("pressed", _create_sidebar_item_style(true, false))
	button.add_theme_stylebox_override("focus", _create_sidebar_item_style(false, true))


func _apply_arrow_button_style(button: Button) -> void:
	button.add_theme_font_size_override("font_size", 22)
	button.add_theme_color_override("font_color", INK_MUTED)
	button.add_theme_color_override("font_hover_color", SEAL_RED)
	button.add_theme_color_override("font_pressed_color", SEAL_RED)
	for state in ["normal", "hover", "pressed", "focus"]:
		button.add_theme_stylebox_override(state, _create_transparent_style())


func _apply_footer_action_button_style(button: Button, accent: bool) -> void:
	button.add_theme_font_size_override("font_size", 13)
	button.add_theme_color_override("font_color", SEAL_RED if accent else INK_MAIN)
	button.add_theme_color_override("font_hover_color", PAPER_MAIN)
	button.add_theme_color_override("font_pressed_color", PAPER_MAIN)
	button.add_theme_stylebox_override("normal", _create_footer_button_style(accent, false))
	button.add_theme_stylebox_override("hover", _create_footer_button_style(accent, true))
	button.add_theme_stylebox_override("pressed", _create_footer_button_style(accent, true))
	button.add_theme_stylebox_override("focus", _create_footer_button_style(accent, true))


func _create_frame_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_MAIN
	style.border_color = BORDER_INK
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.shadow_color = Color(0.0, 0.0, 0.0, 0.45)
	style.shadow_size = 24
	return style


func _create_header_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_DARK.r, PAPER_DARK.g, PAPER_DARK.b, 0.72)
	return style


func _create_sidebar_panel_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.38)
	return style


func _create_sidebar_item_style(active: bool, hover: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	if active:
		style.bg_color = Color(PAPER_DARK.r, PAPER_DARK.g, PAPER_DARK.b, 0.52)
	elif hover:
		style.bg_color = Color(PAPER_DARK.r, PAPER_DARK.g, PAPER_DARK.b, 0.30)
	style.border_width_left = 4
	style.border_width_bottom = 1
	style.border_color = SIDEBAR_ACTIVE if active else Color(0.82, 0.76, 0.67, 0.82)
	style.content_margin_left = 24
	style.content_margin_right = 16
	return style


func _create_summary_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(1, 1, 1, 0.40)
	style.border_color = Color(0.77, 0.71, 0.62, 0.95)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	return style


func _create_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.88)
	style.border_color = Color(0.71, 0.64, 0.54, 0.96)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.shadow_color = Color(0.58, 0.50, 0.41, 0.25)
	style.shadow_size = 2
	return style


func _create_control_capsule_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_DARK.r, PAPER_DARK.g, PAPER_DARK.b, 0.70)
	style.border_color = Color(0.78, 0.69, 0.57, 0.9)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.corner_radius_top_left = 24
	style.corner_radius_top_right = 24
	style.corner_radius_bottom_right = 24
	style.corner_radius_bottom_left = 24
	return style


func _create_inner_paper_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.95, 0.92, 0.84, 0.74)
	style.border_color = Color(0.78, 0.71, 0.61, 0.92)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	return style


func _create_footer_button_style(accent: bool, active: bool) -> StyleBoxFlat:
	var border := SEAL_RED if accent else BORDER_INK
	var style := StyleBoxFlat.new()
	style.bg_color = border if active else Color(0, 0, 0, 0)
	style.border_color = border
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.content_margin_left = 12
	style.content_margin_top = 8
	style.content_margin_right = 12
	style.content_margin_bottom = 8
	return style


func _create_selection_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = INK_MAIN
	return style


func _create_transparent_style() -> StyleBoxEmpty:
	return StyleBoxEmpty.new()


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null

