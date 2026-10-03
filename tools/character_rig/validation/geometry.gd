extends RefCounted
## UI containment and rigid limb invariants target the two reported defects.
const BONES=["NearLeg/Thigh","NearLeg/Thigh/ShinBoot","FarLeg/Thigh","FarLeg/Thigh/ShinBoot"]
static func canvas_contained(node: Node,check: Callable) -> void:
	if node is CanvasItem:check.call(node.z_index==0,"Rig escaped creature canvas layer: "+str(node.name))
	for child in node.get_children():canvas_contained(child,check)

static func pose(actor: Node2D) -> Dictionary:
	var result: Dictionary={}
	for path: String in BONES:result[path]=(actor.get_node(path) as Node2D).global_transform
	return result

static func same_pose(actor: Node2D,reference: Dictionary,check: Callable,message: String) -> void:
	for path: String in BONES:check.call((actor.get_node(path) as Node2D).global_transform.is_equal_approx(reference[path]),message+": "+path)

static func descent(actor: Node2D,check: Callable) -> int:
	var reference: Dictionary={}
	for path: String in BONES:
		var bone:=actor.get_node(path) as Bone2D
		var art:=bone.get_node("Attachment/Art") as Sprite2D
		reference[path]={"basis":art.global_transform,"region":art.region_rect,"texture":art.texture,"length":bone.get_length()}
	var min_angle:=INF;var max_angle:=-INF
	var first_knee: Vector2=actor.get_node("FarLeg/Thigh/ShinBoot").global_position
	var travel:=0.0
	var samples:=0
	for step in 201:
		actor.set("collapse",step/200.0)
		for path: String in BONES:
			var bone:=actor.get_node(path) as Bone2D
			var art:=bone.get_node("Attachment/Art") as Sprite2D
			var transform:=art.global_transform
			var original: Transform2D=reference[path]["basis"]
			check.call(absf(transform.x.length()-original.x.length())<0.00001 and absf(transform.y.length()-original.y.length())<0.00001,"Limb stretched during descent: "+path)
			check.call(absf(transform.x.dot(transform.y))<0.00001,"Limb sheared during descent: "+path)
			check.call(transform.determinant()>0.0 and bone.scale.is_equal_approx(Vector2.ONE),"Limb inverted/scaled: "+path)
			check.call(art.texture==reference[path]["texture"] and art.region_rect==reference[path]["region"] and art.modulate.a==1.0,"Limb texture switched/faded: "+path)
			check.call(bone.get_length()==reference[path]["length"],"Bone length changed: "+path)
			samples+=1
		var support:=actor.get_node("FarLeg/Thigh/ShinBoot") as Bone2D
		min_angle=minf(min_angle,support.global_rotation);max_angle=maxf(max_angle,support.global_rotation)
		travel=maxf(travel,support.global_position.distance_to(first_knee))
		var foot_bottom:=-INF
		for p: Vector2 in actor.get_node("FarLeg").get_meta("sole_outline"):
			foot_bottom=maxf(foot_bottom,(support.global_transform*(p*float(actor.get_node("FarLeg").get_meta("lower_scale")))).y)
		check.call(foot_bottom<=1330.1 and foot_bottom>=1300.0,"Support boot penetrated floor or floated away")
		for leg: String in ["NearLeg","FarLeg"]:
			var thigh:=actor.get_node(leg+"/Thigh") as Bone2D
			check.call(thigh.get_node("KneeJoint").global_position.distance_to(thigh.get_node("ShinBoot").global_position)<0.001,"Knee connector detached")
	check.call(max_angle-min_angle>deg_to_rad(10.0) and travel>40.0,"Support shin froze during collapse")
	actor.set("collapse",0.0)
	return samples
