extends RefCounted
const Partition = preload("partition.gd")
const Weights = preload("skin_weights.gd")
const Motion = preload("native_motion.gd")
const Backings=preload("backing_regions.gd")

static func make_mesh(group: Dictionary, texture: Texture2D, fields: RefCounted, use_weights: bool) -> Polygon2D:
	var mesh := Polygon2D.new()
	mesh.name = group["id"]
	mesh.texture = texture
	mesh.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS
	var vertices := PackedVector2Array()
	var triangles := []
	var weights := []
	for _i in Weights.NAMES.size():
		weights.append(PackedFloat32Array())
	for poly: PackedVector2Array in group["polygons"]:
		var start := vertices.size()
		var indices := Geometry2D.triangulate_polygon(poly)
		assert(not indices.is_empty())
		for p in poly:
			vertices.append(p)
			if use_weights:
				var values: PackedFloat32Array = fields.at_point(p,group["id"])
				for i in values.size():
					weights[i].append(values[i])
		for i in range(0,indices.size(),3):
			triangles.append(PackedInt32Array([start+indices[i],start+indices[i+1],start+indices[i+2]]))
	mesh.polygon=vertices
	mesh.uv=vertices
	mesh.polygons=triangles
	if use_weights:
		mesh.skeleton=NodePath("../Skeleton2D")
		for i in Weights.NAMES.size():
			mesh.add_bone(NodePath("Root" if i==0 else "Root/"+Weights.NAMES[i]),weights[i])
	return mesh

static func build(spec: Dictionary, groups: Array) -> Node2D:
	var root := Node2D.new()
	root.name="NaviaAnimated"
	root.set_script(preload("res://STS2-Navia/scenes/characters/rig/rig_driver.gd"))
	var skeleton := Skeleton2D.new()
	skeleton.name="Skeleton2D"
	root.add_child(skeleton)
	var base: Bone2D
	for i in Weights.NAMES.size():
		var bone := Bone2D.new()
		bone.name=Weights.NAMES[i]
		bone.position=Weights.PIVOTS[i]
		bone.rest=bone.transform
		bone.set_autocalculate_length_and_angle(false)
		bone.set_length(50)
		if i==0:
			skeleton.add_child(bone)
			base=bone
		else:
			base.add_child(bone)
	var texture := load(spec["texture"]) as Texture2D
	var fields := Weights.new(spec)
	Backings.add_to(root,groups,texture,func(group,art):return make_mesh(group,art,fields,false))
	var order := {"rear_hair":-40,"rear_skirt_remainder":-35,"back_leg":-25,"front_leg":-20,"far_upper_arm":-10,"far_forearm":-9,"torso_front_skirt":0,"head_hat":10,"near_upper_arm":15,"near_forearm_grip_parasol":16}
	var clean_shader:=Shader.new()
	clean_shader.code="shader_type canvas_item; uniform float replacement=0.0; uniform float replace_y=1130.0; void fragment(){if(UV.y*1365.0>=1130.0){discard;}COLOR.a*=1.0-replacement*smoothstep(replace_y-15.0,replace_y+15.0,UV.y*1365.0);if(COLOR.a<0.01){discard;}}"
	var clean_material:=ShaderMaterial.new()
	clean_material.resource_local_to_scene=true
	clean_material.shader=clean_shader
	for group: Dictionary in groups:
		if group["id"] in ["front_leg","back_leg"]:continue
		var mesh:=make_mesh(group,texture,fields,true)
		mesh.set_meta("source_group",group["id"])
		mesh.set_meta("navia_layer",order[group["id"]]+50)
		mesh.material=clean_material.duplicate()
		(mesh.material as ShaderMaterial).set_shader_parameter("replace_y",600.0 if group["id"]=="rear_skirt_remainder" else 1130.0)
		root.add_child(mesh)
	preload("rigid_legs.gd").add_to(root)
	preload("train_attachment.gd").add_to(root)
	var blink := Sprite2D.new()
	blink.name="Blink"
	blink.texture=load("res://STS2-Navia/images/characters/rig/blink-v1.png")
	blink.region_enabled=true
	blink.region_rect=Rect2(588,214,100,64)
	blink.centered=false
	blink.position=Vector2(588,214)-Weights.PIVOTS[1]
	skeleton.set_meta("navia_layer",100)
	blink.modulate.a=0.0
	blink.texture_filter=CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS
	var blink_shader := Shader.new()
	blink_shader.code="shader_type canvas_item; void fragment(){vec2 p=UV*vec2(1152.0,1366.0);float edge=min(min(p.x-588.0,688.0-p.x),min(p.y-214.0,278.0-p.y));COLOR.a*=smoothstep(0.0,4.0,edge);}"
	var blink_material := ShaderMaterial.new()
	blink_material.shader=blink_shader
	blink.material=blink_material
	base.get_node("Breath").add_child(blink)
	var player := AnimationPlayer.new()
	player.name="AnimationPlayer"
	root.add_child(player)
	player.add_animation_library("",Motion.library(groups))
	preload("res://STS2-Navia/scenes/characters/rig/draw_order.gd").sort_children(root)
	return root
