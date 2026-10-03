extends RefCounted
## Shared coordinate fields keep breathing continuous at region boundaries.
const NAMES := ["Root", "Breath", "HairRoot", "HairTip", "SkirtRoot", "SkirtTip", "Arm"]
const PIVOTS := [Vector2.ZERO, Vector2(620,600), Vector2(390,400), Vector2(260,570), Vector2(420,650), Vector2(250,900), Vector2(775,568)]
var edges: Array = []

func _init(spec: Dictionary) -> void:
	for region: Dictionary in spec["regions_front_first"]:
		var ps: Array = region["points"]
		for i in ps.size():
			edges.append([Vector2(ps[i][0],ps[i][1]),Vector2(ps[(i+1)%ps.size()][0],ps[(i+1)%ps.size()][1])])

func boundary_fade(p: Vector2) -> float:
	var nearest := 10000.0
	for edge: Array in edges:
		nearest = minf(nearest,p.distance_to(Geometry2D.get_closest_point_to_segment(p,edge[0],edge[1])))
	return smoothstep(0.0,28.0,nearest)

func at_point(p: Vector2, group: String) -> PackedFloat32Array:
	var breath := clampf((820.0-p.y)/220.0,0.0,1.0)
	var arm := 0.0
	var hair := 0.0
	var skirt := 0.0
	if group == "near_forearm_grip_parasol":
		arm = smoothstep(0.0,54.0,p.y-558.0+0.2*(p.x-770.0))
		breath = 1.0
	elif group == "rear_hair":
		hair = 0.85 * (1.0-smoothstep(260.0,430.0,p.x)) * smoothstep(320.0,500.0,p.y) * boundary_fade(p)
	elif group == "rear_skirt_remainder":
		# The residual region also contains small boot-outline pixels. Never
		# let cloth weights reach the ankle/boot area below the actual skirt.
		skirt = 0.85 * (1.0-smoothstep(280.0,530.0,p.x)) * smoothstep(650.0,890.0,p.y) * (1.0-smoothstep(1060.0,1130.0,p.y)) * boundary_fade(p)
	var remainder := 1.0-arm-hair-skirt
	var hair_tip := smoothstep(440.0,650.0,p.y)
	var skirt_tip := smoothstep(790.0,1050.0,p.y)
	return PackedFloat32Array([remainder*(1.0-breath),remainder*breath,hair*(1.0-hair_tip),hair*hair_tip,skirt*(1.0-skirt_tip),skirt*skirt_tip,arm])
