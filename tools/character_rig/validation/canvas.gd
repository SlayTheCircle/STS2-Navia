extends SceneTree
## Run with a real renderer (gl_compatibility), not headless Dummy.
## Reproduces the original room's -10 layer and reward/map foreground at zero.
var failed:=false
func _initialize() -> void:call_deferred("run")
func run() -> void:
	var pack_path:=OS.get_environment("NAVIA_MOD_PCK")
	if not pack_path.is_empty() and not ProjectSettings.load_resource_pack(pack_path):
		push_error("Cannot mount candidate PCK");quit(1);return
	var view:=SubViewport.new();view.size=Vector2i(900,850);view.render_target_update_mode=SubViewport.UPDATE_ALWAYS;root.add_child(view)
	var background:=ColorRect.new();background.size=Vector2(900,850);background.color=Color("13212a");background.z_index=-100;view.add_child(background)
	var world:=Node2D.new();world.z_index=-10;view.add_child(world)
	var actor:=load("res://STS2-Navia/scenes/characters/rig/navia_combat.tscn").instantiate() as Node2D
	world.add_child(actor);actor.position=Vector2(80,50);actor.scale=Vector2.ONE*0.52
	var player:=actor.get_node("AnimationPlayer") as AnimationPlayer;player.play("idle");player.pause();player.seek(0,true)
	await process_frame;await RenderingServer.frame_post_draw
	if nonuniform(view.get_texture().get_image())<1000:
		push_error("Canvas fixture did not render a visible character");quit(1);return
	var overlay:=ColorRect.new();overlay.size=Vector2(900,850);overlay.color=Color("5b305a");view.add_child(overlay)
	var exposed:=0
	for clip: String in ["idle","attack","cast","die","revive"]:
		player.play(clip);player.pause()
		for fraction: float in [0.0,0.5,1.0]:
			player.seek(player.get_animation(clip).length*fraction,true)
			await process_frame;await RenderingServer.frame_post_draw
			exposed+=nonuniform(view.get_texture().get_image())
	overlay.hide();world.modulate.a=0
	await process_frame;await RenderingServer.frame_post_draw
	var faded:=nonuniform(view.get_texture().get_image())
	failed=exposed!=0 or faded!=0
	print(JSON.stringify({"passed":not failed,"pixels_above_foreground":exposed,"pixels_after_ancestor_fade":faded,"poses":15,"source":"candidate PCK" if not pack_path.is_empty() else "asset project"}))
	quit(1 if failed else 0)

static func nonuniform(picture: Image) -> int:
	var bytes:=picture.get_data();var count:=0
	for offset in range(0,bytes.size(),4):
		if maxi(maxi(absi(bytes[offset]-bytes[0]),absi(bytes[offset+1]-bytes[1])),absi(bytes[offset+2]-bytes[2]))>4:count+=1
	return count
