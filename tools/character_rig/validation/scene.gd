extends SceneTree
const Geometry=preload("geometry.gd")
var failed:=false
func check(ok: bool,message: String) -> void:
	if not ok:failed=true;push_error(message)
func _initialize() -> void:
	call_deferred("run")
func run() -> void:
	var pack_path:=OS.get_environment("NAVIA_MOD_PCK")
	if not pack_path.is_empty():
		check(ProjectSettings.load_resource_pack(pack_path),"Cannot mount candidate PCK")
	var scene:=load("res://STS2-Navia/scenes/characters/rig/navia_combat.tscn") as PackedScene
	check(scene!=null,"Native rig cannot load")
	if failed:quit(1);return
	var a:=scene.instantiate();var b:=scene.instantiate()
	root.add_child(a);root.add_child(b)
	await process_frame
	var player:=a.get_node("AnimationPlayer") as AnimationPlayer
	b.get_node("AnimationPlayer").pause()
	player.play("idle");player.pause();player.seek(0,true)
	var baseline: Dictionary={}
	for child in a.get_children():
		if child is Polygon2D:baseline[child.name]=child.polygon.duplicate()
	Geometry.canvas_contained(a,check)
	var resting_legs:=Geometry.pose(a)
	var rigid_samples:=Geometry.descent(a,check)
	var samples:=0
	for clip: String in ["idle","raise","attack","cast","hurt","low_health_loop","relaxed_loop","die","revive"]:
		check(player.has_animation(clip),"Missing clip: "+clip)
		var animation:=player.get_animation(clip)
		check(animation.loop_mode==(Animation.LOOP_LINEAR if clip in ["idle","low_health_loop","relaxed_loop"] else Animation.LOOP_NONE),"Incorrect loop: "+clip)
		player.play(clip);player.pause()
		for t: float in [0.0,animation.length*0.35,animation.length*0.65,animation.length]:
			player.seek(t,true)
			for child in a.get_children():
				if child is Polygon2D:
					for p: Vector2 in child.polygon:
						check(is_finite(p.x) and is_finite(p.y) and absf(p.x)<2000 and absf(p.y)<2000,"Invalid deformed point")
						samples+=1
	player.play("die");player.pause();player.seek(2,true)
	for name: String in ["torso_front_skirt","rear_skirt_remainder","ArmBacking"]:
		var ma: ShaderMaterial=a.get_node(name).material
		var mb: ShaderMaterial=b.get_node(name).material
		check(ma!=mb,"Shader state shared between actors: "+name)
		check(float(mb.get_shader_parameter("replacement"))==0.0,"Other actor lost its standing attachment")
	Geometry.same_pose(b,resting_legs,check,"Other actor leg pose changed")
	for leg: String in ["NearLeg","FarLeg"]:
		var joint_a: ShaderMaterial=a.get_node(leg+"/Thigh/KneeJoint").material
		var joint_b: ShaderMaterial=b.get_node(leg+"/Thigh/KneeJoint").material
		check(joint_a!=joint_b and float(joint_b.get_shader_parameter("fold"))==0.0,"Knee fold state shared between actors")
	var dead_legs:=Geometry.pose(a)
	var dead: Dictionary={}
	for child in a.get_children():
		if child is Polygon2D:dead[child.name]=child.polygon.duplicate()
	player.play("revive");player.pause();player.seek(0,true)
	for child in a.get_children():
		if child is Polygon2D:check(child.polygon==dead[child.name],"Revive entry jump")
	Geometry.same_pose(a,dead_legs,check,"Revive entry bone jump")
	player.seek(1.6,true)
	Geometry.same_pose(a,resting_legs,check,"Revive did not reset bones")
	for child in a.get_children():
		if child is Polygon2D:check(child.polygon==baseline[child.name],"Revive did not restore mesh")
	player.play("die");player.pause();player.seek(2,true)
	player.play("idle");player.advance(0)
	for child in a.get_children():
		if child is Polygon2D:check(child.polygon==baseline[child.name],"Death geometry leaked into idle")
	check(int(a.get_node("FarLeg").get_meta("navia_layer"))==25,"Raised leg draw order was not reset")
	Geometry.same_pose(a,resting_legs,check,"Death bone pose leaked into idle")
	for item in [["attack",0.15,-30.5],["cast",0.4,-29.4]]:
		player.play(item[0]);player.pause();player.seek(item[1],true)
		check(absf(a.get_node("Skeleton2D/Root/Arm").rotation_degrees-item[2])<0.001,"Impact pose drift")
	print(JSON.stringify({"passed":not failed,"source":"candidate PCK" if not pack_path.is_empty() else "asset project","vertex_samples":samples,"rigid_limb_samples":rigid_samples,"checks":["saved rig load","nine clips and loop flags","finite meshes","201 descent poses with no limb shear, stretch or texture swap","support shin movement and floor contact","connected knee surfaces","creature canvas containment","death-revive continuity","pose/depth reset","impact timing","per-actor material isolation"]}))
	a.free();b.free()
	quit(1 if failed else 0)
