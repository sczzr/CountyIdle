extends PanelContainer

## 主 HUD 底栏只负责表现层：令箭副卷展开、倍速珠切换与简单的提示文案。

const SUB_MENUS := {
	"WarehouseQuickButton": {
		"title": "库房卷",
		"items": [
			{"icon": "谷", "name": "灵谷仓"},
			{"icon": "戒", "name": "纳戒阁"},
			{"icon": "箱", "name": "行囊入库"}
		]
	},
	"TaskQuickButton": {
		"title": "中枢卷",
		"items": [
			{"icon": "令", "name": "发展方向"},
			{"icon": "律", "name": "门规法令"},
			{"icon": "才", "name": "育才部署"}
		]
	},
	"DiscipleQuickButton": {
		"title": "谱系卷",
		"items": [
			{"icon": "谱", "name": "宗门大谱"},
			{"icon": "修", "name": "弟子命谱"},
			{"icon": "候", "name": "候补指令"}
		]
	},
	"ConstructionQuickButton": {
		"title": "天工卷",
		"items": [
			{"icon": "舍", "name": "院域营建"},
			{"icon": "阵", "name": "阵枢落点"},
			{"icon": "队", "name": "建造队列"}
		]
	},
	"OrganizationQuickButton": {
		"title": "政令卷",
		"items": [
			{"icon": "峰", "name": "峰脉扶持"},
			{"icon": "命", "name": "命名录"},
			{"icon": "政", "name": "宗务导向"}
		]
	}
}

@onready var _sub_menu_panel: PanelContainer = $SubMenuWrap/SubMenuPanel
@onready var _sub_title: Label = $SubMenuWrap/SubMenuPanel/SubMenuVBox/HeaderRow/SubTitle
@onready var _item_grid: GridContainer = $SubMenuWrap/SubMenuPanel/SubMenuVBox/ItemGrid
@onready var _close_button: Button = $SubMenuWrap/SubMenuPanel/SubMenuVBox/HeaderRow/CloseButton
@onready var _token_buttons := [
	$BarPadding/MainRow/QuickActionRow/WarehouseQuickButton,
	$BarPadding/MainRow/QuickActionRow/TaskQuickButton,
	$BarPadding/MainRow/QuickActionRow/DiscipleQuickButton,
	$BarPadding/MainRow/QuickActionRow/ConstructionQuickButton,
	$BarPadding/MainRow/QuickActionRow/OrganizationQuickButton
]
@onready var _time_beads := [
	$BarPadding/MainRow/SpeedRow/SpeedX1Button,
	$BarPadding/MainRow/SpeedRow/SpeedX2Button,
	$BarPadding/MainRow/SpeedRow/SpeedX4Button
]

var _sub_menu_tween: Tween


func _ready() -> void:
	_sub_menu_panel.visible = false
	_sub_menu_panel.modulate.a = 0.0
	_sub_menu_panel.scale = Vector2(0.95, 0.95)
	_close_button.pressed.connect(_on_close_pressed)

	for token in _token_buttons:
		token.toggled.connect(_on_token_toggled.bind(token))

	for bead in _time_beads:
		bead.pressed.connect(_on_time_bead_pressed.bind(bead))


func _on_token_toggled(is_pressed: bool, token: BaseButton) -> void:
	if is_pressed:
		_open_sub_menu(token.name)
	else:
		var any_pressed := false
		for other in _token_buttons:
			if other.button_pressed:
				any_pressed = true
				break

		if not any_pressed:
			_close_sub_menu()


func _open_sub_menu(token_name: String) -> void:
	var payload: Dictionary = SUB_MENUS.get(token_name, {
		"title": "未录卷轴",
		"items": [{"icon": "无", "name": "尚未参透"}]
	})

	_sub_title.text = str(payload.get("title", "未录卷轴"))
	for child in _item_grid.get_children():
		child.queue_free()

	var items: Array = payload.get("items", [])
	for item in items:
		var item_data: Dictionary = item
		_item_grid.add_child(_build_item_card(str(item_data.get("icon", "无")), str(item_data.get("name", "尚未参透"))))

	if _sub_menu_tween != null and _sub_menu_tween.is_running():
		_sub_menu_tween.kill()

	_sub_menu_panel.visible = true
	_sub_menu_panel.position.y = 24.0
	_sub_menu_panel.scale = Vector2(0.95, 0.95)
	_sub_menu_panel.modulate.a = 0.0
	_sub_menu_tween = create_tween().set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)
	_sub_menu_tween.set_parallel(true)
	_sub_menu_tween.tween_property(_sub_menu_panel, "modulate:a", 1.0, 0.24)
	_sub_menu_tween.tween_property(_sub_menu_panel, "position:y", 0.0, 0.28)
	_sub_menu_tween.tween_property(_sub_menu_panel, "scale", Vector2.ONE, 0.28)


func _close_sub_menu() -> void:
	if not _sub_menu_panel.visible:
		return

	if _sub_menu_tween != null and _sub_menu_tween.is_running():
		_sub_menu_tween.kill()

	_sub_menu_tween = create_tween().set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_IN)
	_sub_menu_tween.set_parallel(true)
	_sub_menu_tween.tween_property(_sub_menu_panel, "modulate:a", 0.0, 0.2)
	_sub_menu_tween.tween_property(_sub_menu_panel, "position:y", 20.0, 0.2)
	_sub_menu_tween.tween_property(_sub_menu_panel, "scale", Vector2(0.96, 0.96), 0.2)
	_sub_menu_tween.chain().tween_callback(func() -> void:
		_sub_menu_panel.visible = false
	)


func _on_close_pressed() -> void:
	for token in _token_buttons:
		token.button_pressed = false

	_close_sub_menu()


func _on_time_bead_pressed(bead: BaseButton) -> void:
	for other in _time_beads:
		other.button_pressed = (other == bead)


func _build_item_card(icon_text: String, name_text: String) -> PanelContainer:
	var card := PanelContainer.new()
	card.custom_minimum_size = Vector2(132, 88)
	card.theme_override_styles.panel = _build_card_style(Color(0.98, 0.97, 0.93, 0.76), Color(0.55, 0.45, 0.29, 0.24))

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 10)
	margin.add_theme_constant_override("margin_top", 10)
	margin.add_theme_constant_override("margin_right", 10)
	margin.add_theme_constant_override("margin_bottom", 10)
	card.add_child(margin)

	var vbox := VBoxContainer.new()
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	vbox.add_theme_constant_override("separation", 6)
	margin.add_child(vbox)

	var icon_bg := PanelContainer.new()
	icon_bg.custom_minimum_size = Vector2(34, 34)
	icon_bg.theme_override_styles.panel = _build_card_style(Color(0.80, 0.86, 0.82, 1.0), Color(0.55, 0.66, 0.59, 0.5), 17)
	vbox.add_child(icon_bg)

	var icon_label := Label.new()
	icon_label.text = icon_text
	icon_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	icon_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	icon_label.add_theme_font_size_override("font_size", 18)
	icon_bg.add_child(icon_label)

	var name_label := Label.new()
	name_label.text = name_text
	name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	name_label.add_theme_font_size_override("font_size", 13)
	vbox.add_child(name_label)

	return card


func _build_card_style(fill_color: Color, border_color: Color, radius: int = 8) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = fill_color
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = border_color
	style.corner_radius_top_left = radius
	style.corner_radius_top_right = radius
	style.corner_radius_bottom_left = radius
	style.corner_radius_bottom_right = radius
	return style
