extends RefCounted
const Definition=preload("definition.gd")
const Sequence=preload("sequence.gd")
const PIVOT=Vector2(620,600)

static func support(amount: float,chain: Skeleton2D) -> Array:
	var spec: Dictionary=Definition.FAR
	var lower_angle:=Sequence.shin(amount)
	var calf: Vector2=Vector2(0,440)*float(chain.get_meta("lower_scale"))
	var ankle_x: float=spec["knee"].x+calf.rotated(spec["lower_angle"]-PI/2).x
	var brace:=Sequence.brace(amount)
	var knee:=Vector2(ankle_x+brace.x-calf.rotated(lower_angle-PI/2).x,Definition.FLOOR-bottom(chain,lower_angle)+brace.y)
	var upper_angle:=Sequence.thigh(amount,(spec["knee"]-spec["hip"]).angle())
	return [knee-Vector2.from_angle(upper_angle)*Definition.upper_length(spec),knee,upper_angle,lower_angle]

static func body(amount: float,actor: Node2D) -> Transform2D:
	var hip: Vector2=support(amount,actor.get_node("FarLeg"))[0]
	var rotation:=Sequence.torso(amount)
	return Transform2D(rotation,hip-(Definition.FAR["hip"]-PIVOT).rotated(rotation))*Transform2D(0,-PIVOT)

static func apply(actor: Node2D,amount: float) -> void:
	if not actor.has_node("NearLeg"):return
	var torso:=body(amount,actor)
	for near: bool in [true,false]:
		var spec: Dictionary=Definition.NEAR if near else Definition.FAR
		var chain:=actor.get_node("NearLeg" if near else "FarLeg") as Skeleton2D
		var upper:=chain.get_node("Thigh") as Bone2D
		var lower:=upper.get_node("ShinBoot") as Bone2D
		var hip: Vector2=torso*spec["hip"]
		var upper_angle: float
		var lower_angle: float
		if near:
			var final_hip: Vector2=body(1.0,actor)*spec["hip"]
			var target_angle:=asin(clampf((Definition.FLOOR-40.0-final_hip.y)/Definition.upper_length(spec),-1.0,1.0))
			upper_angle=lerp_angle((spec["knee"]-spec["hip"]).angle(),target_angle,smoothstep(0.10,0.80,amount))
			var contact_angle:=asin(clampf((Definition.FLOOR-40.0-hip.y)/Definition.upper_length(spec),-1.0,1.0))
			upper_angle=lerp_angle(upper_angle,contact_angle,smoothstep(0.62,0.80,amount))
			lower_angle=deg_to_rad(Sequence.value(amount,[[0.0,96.2],[0.2,115.0],[0.5,150.0],[0.76,185.0],[1.0,186.0]]))
		else:
			var standing:=support(amount,chain)
			upper_angle=standing[2];lower_angle=standing[3]
		var knee:=hip+Vector2.from_angle(upper_angle)*Definition.upper_length(spec)
		if near:lower_angle=clear_floor(chain,knee,lower_angle,amount)
		upper.position=hip;upper.rotation=upper_angle-PI/2
		lower.rotation=lower_angle-upper_angle
		var joint:=upper.get_node("KneeJoint") as Polygon2D
		joint.rotation=lower.rotation*0.5
		(joint.material as ShaderMaterial).set_shader_parameter("fold",smoothstep(0.15,1.6,absf(lower.rotation)))

static func clear_floor(chain: Skeleton2D,knee: Vector2,angle: float,amount: float) -> float:
	var floor_y:=Definition.FLOOR-8.0*sin(PI*amount)
	if knee.y+bottom(chain,angle)<=floor_y:return angle
	var lo:=angle;var hi:=deg_to_rad(190.0)
	for _step in 24:
		var mid: float=(lo+hi)*0.5
		if knee.y+bottom(chain,mid)>floor_y:lo=mid
		else:hi=mid
	return lerpf(angle,hi,smoothstep(0.0,0.08,amount))

static func bottom(chain: Skeleton2D,angle: float) -> float:
	var result:=-INF
	for point: Vector2 in chain.get_meta("sole_outline"):
		result=maxf(result,point.rotated(angle-PI/2).y)
	return result*float(chain.get_meta("lower_scale"))
