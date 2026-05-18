extends "res://addons/gdUnit4/src/GdUnitTestSuite.gd"

func test_main_menu_scene_instantiates() -> void:
    var scene := preload("res://Game.Godot/Scenes/UI/MainMenu.tscn").instantiate()
    add_child(auto_free(scene))
    await get_tree().process_frame
    assert_bool(scene.visible).is_true()

func test_main_menu_has_prototype_button() -> void:
    var scene := preload("res://Game.Godot/Scenes/UI/MainMenu.tscn").instantiate()
    add_child(auto_free(scene))
    await get_tree().process_frame
    var btn := scene.get_node_or_null("VBox/BtnPrototype")
    assert_object(btn).is_not_null()
