extends "res://addons/gdUnit4/src/GdUnitTestSuite.gd"

func _ensure_bus() -> Node:
    var bus = get_node_or_null("/root/EventBus")
    if bus == null:
        bus = preload("res://Game.Godot/Adapters/EventBusAdapter.cs").new()
        bus.name = "EventBus"
        get_tree().get_root().add_child(auto_free(bus))
    return bus

func test_main_scene_instantiates_and_visible() -> void:
    var scene := preload("res://Game.Godot/Scenes/Main.tscn").instantiate()
    add_child(auto_free(scene))
    await get_tree().process_frame
    assert_bool(scene.visible).is_true()

func test_settings_screen_can_load() -> void:
    var packed : PackedScene = preload("res://Game.Godot/Scenes/Screens/SettingsScreen.tscn")
    var inst := packed.instantiate()
    add_child(auto_free(inst))
    await get_tree().process_frame
    assert_bool(inst.is_inside_tree()).is_true()

func test_main_scene_routes_to_he_is_coming_prototype() -> void:
    var bus = _ensure_bus()
    var scene := preload("res://Game.Godot/Scenes/Main.tscn").instantiate()
    get_tree().get_root().add_child(auto_free(scene))
    await get_tree().process_frame

    assert_object(bus).is_not_null()
    bus.PublishSimple("ui.menu.prototype", "ut", "{\"slug\":\"He-is-Coming\"}")

    for i in range(30):
        await get_tree().process_frame

    var screen_root = scene.get_node("ScreenRoot")
    var matches = screen_root.find_children("*", "Node2D", true, false).filter(func(node): return node.name == "HeIsComingPrototype")
    var prototype = matches[0] if matches.size() > 0 else null
    assert_object(prototype).is_not_null()
