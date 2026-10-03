extends RefCounted

static func specifications(groups: Array) -> Array:
	var by_id: Dictionary={}
	for group: Dictionary in groups:by_id[group["id"]]=group
	var results:=[]
	for definition in [
		["ArmBacking",["near_forearm_grip_parasol"],"near-arm-backing-v1",51],
		["FarArmBacking",["far_forearm","far_upper_arm"],"far-arm-backing-v1",17]]:
		var polygons: Array=[]
		for id: String in definition[1]:polygons.append_array(by_id[id]["polygons"])
		results.append({"id":definition[0],"polygons":polygons,"texture":"res://STS2-Navia/images/characters/rig/"+definition[2]+".png","z":definition[3]})
	return results

static func add_to(root: Node2D,groups: Array,original: Texture2D,factory: Callable) -> void:
	var shader:=Shader.new()
	shader.code="shader_type canvas_item; uniform sampler2D original; uniform float replacement=0.0; void fragment(){vec4 c=texture(TEXTURE,UV);c.a*=texture(original,UV).a;c.a*=1.0-replacement*smoothstep(640.0,700.0,UV.y*1366.0);if(c.a<0.01){discard;}COLOR=c;}"
	for definition: Dictionary in specifications(groups):
		var mesh: Polygon2D=factory.call(definition,load(definition["texture"]))
		mesh.name=definition["id"]
		mesh.set_meta("navia_layer",definition["z"])
		var material:=ShaderMaterial.new()
		material.resource_local_to_scene=true
		material.shader=shader
		material.set_shader_parameter("original",original)
		mesh.material=material
		root.add_child(mesh)
