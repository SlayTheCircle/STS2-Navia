extends RefCounted
const Definition=preload("res://STS2-Navia/scenes/characters/rig/legs/definition.gd")

static func add_to(actor: Node2D) -> void:
	var texture:=load(Definition.TEXTURE) as Texture2D
	var picture:=texture.get_image()
	for near: bool in [true,false]:
		var spec: Dictionary=Definition.NEAR if near else Definition.FAR
		var skeleton:=Skeleton2D.new();skeleton.name="NearLeg" if near else "FarLeg"
		skeleton.set_meta("navia_layer",30 if near else 25);actor.add_child(skeleton)
		var upper:=Bone2D.new();upper.name="Thigh";upper.position=spec["hip"]
		upper.rotation=(spec["knee"]-spec["hip"]).angle()-PI/2
		upper.rest=upper.transform;upper.set_autocalculate_length_and_angle(false);upper.set_length(Definition.upper_length(spec));skeleton.add_child(upper)
		var lower:=Bone2D.new();lower.name="ShinBoot";lower.position=Vector2(0,upper.get_length())
		lower.rotation=spec["lower_angle"]-(spec["knee"]-spec["hip"]).angle()
		lower.rest=lower.transform;lower.set_autocalculate_length_and_angle(false);lower.set_length(250);upper.add_child(lower)
		var support:=opaque_hull(picture,spec["lower_region"],spec["lower_anchor"])
		var bottom:=0.0
		for point: Vector2 in support:bottom=maxf(bottom,point.rotated(spec["lower_angle"]-PI/2).y)
		var lower_scale: float=(Definition.FLOOR-spec["knee"].y)/bottom
		skeleton.set_meta("sole_outline",support);skeleton.set_meta("lower_scale",lower_scale)
		lower.add_child(sprite(texture,spec["lower_region"],spec["lower_anchor"],lower_scale,0.0,spec["lower_anchor"],false))
		var upper_scale: float=upper.get_length()/spec["upper_anchor"].distance_to(spec["upper_joint"])
		upper.add_child(sprite(texture,spec["upper_region"],spec["upper_anchor"],upper_scale,PI/2-(spec["upper_joint"]-spec["upper_anchor"]).angle(),spec["upper_joint"],true))

		upper.add_child(preload("knee_joint.gd").create(texture,near,upper.get_length()))

static func sprite(texture: Texture2D,region: Rect2,anchor: Vector2,factor: float,angle: float,joint: Vector2,upper: bool) -> Node2D:
	var attachment:=Node2D.new();attachment.name="Attachment";attachment.rotation=angle;attachment.scale=Vector2.ONE*factor
	var picture:=Sprite2D.new();picture.name="Art";picture.texture=texture;picture.centered=false
	picture.region_enabled=true;picture.region_rect=region;picture.position=region.position-anchor
	picture.texture_filter=CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS
	# Trim the rounded ends; the dedicated knee surface bridges the joint.
	var shader:=Shader.new()
	shader.code="shader_type canvas_item; uniform vec2 joint; uniform bool upper; void fragment(){vec2 p=UV*vec2(1152.0,1366.0)-joint;COLOR.a*=upper?(1.0-smoothstep(-32.0,-10.0,p.y)):smoothstep(10.0,32.0,p.y);}"
	var material:=ShaderMaterial.new();material.shader=shader
	material.set_shader_parameter("joint",joint);material.set_shader_parameter("upper",upper);picture.material=material
	attachment.add_child(picture)
	return attachment

static func opaque_hull(picture: Image,region: Rect2,anchor: Vector2) -> PackedVector2Array:
	var edges:=PackedVector2Array()
	for y in range(int(region.position.y),mini(int(region.end.y),picture.get_height()),2):
		var first:=-1;var last:=-1
		for x in range(int(region.position.x),mini(int(region.end.x),picture.get_width())):
			if picture.get_pixel(x,y).a>0.5:
				if first<0:first=x
				last=x
		if first>=0:edges.append(Vector2(first,y)-anchor);edges.append(Vector2(last,y)-anchor)
	return Geometry2D.convex_hull(edges)
