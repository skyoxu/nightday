extends "res://addons/gdUnit4/src/GdUnitTestSuite.gd"

var _bus: Node
var _received := false
var _etype := ""

func _ensure_bus() -> void:
    _bus = get_node_or_null("/root/EventBus")
    if _bus == null:
        _bus = preload("res://Game.Godot/Adapters/EventBusAdapter.cs").new()
        _bus.name = "EventBus"
        get_tree().get_root().add_child(auto_free(_bus))
    var callable := Callable(self, "_on_evt")
    if not _bus.is_connected("DomainEventEmitted", callable):
        _bus.connect("DomainEventEmitted", callable)

func _on_evt(type, _source, _data_json, _id, _spec, _ct, _ts) -> void:
    _received = true
    _etype = str(type)

func test_main_menu_emits_start() -> void:
    _ensure_bus()
    _received = false
    _etype = ""
    var menu = preload("res://Game.Godot/Scenes/UI/MainMenu.tscn").instantiate()
    add_child(auto_free(menu))
    await get_tree().process_frame
    var btn = menu.get_node("VBox/BtnPlay")
    btn.emit_signal("pressed")
    await get_tree().process_frame
    assert_bool(_received).is_true()
    assert_str(_etype).is_equal("ui.menu.start")

func test_main_menu_emits_prototype_start() -> void:
    _ensure_bus()
    _received = false
    _etype = ""
    var menu = preload("res://Game.Godot/Scenes/UI/MainMenu.tscn").instantiate()
    add_child(auto_free(menu))
    await get_tree().process_frame
    var btn = menu.get_node("VBox/BtnPrototype")
    btn.emit_signal("pressed")
    for i in range(5):
        await get_tree().process_frame
        if _received and _etype == "ui.menu.prototype":
            break
    assert_bool(_received).is_true()
    assert_str(_etype).contains("ui.menu.prototype")
