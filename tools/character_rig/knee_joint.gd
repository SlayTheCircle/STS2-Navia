extends RefCounted
## One continuous stocking surface replaces the two rounded overlapping caps.
static func create(texture: Texture2D,near: bool,length: float) -> Polygon2D:
	var joint:=Polygon2D.new();joint.name="KneeJoint";joint.position=Vector2(0,length)
	var center:=Vector2(375,430) if near else Vector2(790,430)
	var radius:=40.0 if near else 36.0
	var polygon:=PackedVector2Array();var uv:=PackedVector2Array()
	for i in 48:
		var direction:=Vector2.from_angle(i*TAU/48.0)
		polygon.append(direction*radius);uv.append(center+direction*60.0)
	joint.polygon=polygon;joint.uv=uv;joint.texture=texture
	joint.texture_filter=CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS
	var shader:=Shader.new()
	shader.code="shader_type canvas_item; uniform vec2 center; uniform float fold=0.0; void fragment(){vec2 p=(UV*vec2(1152.0,1366.0)-center)/60.0;COLOR.a*=1.0-smoothstep(0.88,1.0,length(p));float crease=exp(-pow((p.x+0.48)/0.12,2.0))*exp(-pow(p.y/0.52,2.0));COLOR.rgb*=1.0-0.18*fold*crease;}"
	var material:=ShaderMaterial.new();material.shader=shader;material.resource_local_to_scene=true
	material.set_shader_parameter("center",center);joint.material=material
	return joint
