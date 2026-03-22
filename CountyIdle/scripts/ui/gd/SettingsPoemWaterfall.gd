extends Control

# 机宜卷背景诗文瀑布：只负责低透明竖排诗句的缓慢漂流，不触碰任何业务逻辑。
const DEFAULT_POEMS := [
	"北冥有鱼其名为鲲",
	"天地与我并生",
	"上善若水利万物",
	"道可道非常道"
]

var _poem_labels: Array[Label] = []


func _ready() -> void:
	# 诗句只承担背景意境，因此初始化时即打散位置与速度，避免齐步下落过于机械。
	randomize()
	for child in get_children():
		if child is Label:
			var label := child as Label
			_poem_labels.append(label)
			label.text = _format_vertical_text(DEFAULT_POEMS.pick_random())
			label.set_meta("speed", randf_range(10.0, 22.0))
			label.position.y = randf_range(-360.0, size.y * 0.5)
			label.rotation = randf_range(-0.02, 0.02)


func _process(delta: float) -> void:
	# 让每列诗文以不同速度匀速下落；离开底部后回到顶部继续循环。
	for label in _poem_labels:
		var speed := float(label.get_meta("speed", 14.0))
		label.position.y += speed * delta
		if label.position.y > size.y + 260.0:
			label.position.y = -randf_range(220.0, 420.0)
			label.text = _format_vertical_text(DEFAULT_POEMS.pick_random())
			label.set_meta("speed", randf_range(10.0, 22.0))


func _format_vertical_text(text: String) -> String:
	# 将诗句转换为竖排感更强的换行串，便于直接用普通 Label 呈现。
	var builder := PackedStringArray()
	for char in text:
		builder.append(char)
	return "\n".join(builder)
