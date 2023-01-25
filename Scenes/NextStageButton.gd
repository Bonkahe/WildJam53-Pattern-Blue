extends Button


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_death():
	SceneManager.set_recorded_scene(scene)
	SceneManager.change_scene("deathscreen", fade_out_options, fade_in_options, general_options)
	print(get_tree().root.get_child_count());
