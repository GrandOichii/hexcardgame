@tool
extends HBoxContainer

@export var player_i: int :
	get:
		return player_i
	set(value):
		player_i = value
		$Label2.text = 'P' + str(player_i) + ' deck'

