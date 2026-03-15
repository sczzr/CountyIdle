extends TileMapLayer

@export var plain_tiles: Array[Vector2i] = [
	Vector2i(0, 1),
	Vector2i(2, 1),
	Vector2i(3, 1),
]

@export var spirit_tiles: Array[Vector2i] = [
	Vector2i(1, 1),
	Vector2i(2, 0),
	Vector2i(2, 1),
]

@export var rugged_tiles: Array[Vector2i] = [
	Vector2i(0, 0),
	Vector2i(1, 1),
	Vector2i(3, 1),
]

@export var snow_tiles: Array[Vector2i] = [
	Vector2i(2, 0),
]

@export var deep_water_tiles: Array[Vector2i] = [
	Vector2i(1, 0),
	Vector2i(3, 0),
]

@export var shallow_water_tiles: Array[Vector2i] = [
	Vector2i(1, 0),
	Vector2i(3, 0),
]
