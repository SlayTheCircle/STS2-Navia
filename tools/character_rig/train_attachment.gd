extends RefCounted
const Partition=preload("partition.gd")
const Rig=preload("animated_rig.gd")
static func add_to(actor: Node2D) -> void:
	var spec: Dictionary={"size":[1536,1024],"regions_front_first":[{"id":"KneelingTrain","points":[[1030,40],[1170,85],[1320,45],[1330,155],[1370,240],[1450,330],[1450,465],[1370,600],[1450,740],[1410,855],[1270,870],[1090,943],[550,975],[335,925],[200,855],[100,850],[45,770],[195,767],[260,700],[390,700],[450,660],[600,555],[735,465],[885,280],[970,120]]}],"remainder":{"id":"unused"}}
	var group: Dictionary=Partition.make_partition(spec)[0]
	var mesh:=Rig.make_mesh(group,load("res://STS2-Navia/images/characters/rig/kneeling-train-v1.png"),null,false)
	mesh.set_meta("navia_layer",18)
	mesh.modulate.a=0.0
	var shader:=Shader.new()
	shader.code="shader_type canvas_item; void fragment(){if(COLOR.a<0.05){discard;}}"
	var material:=ShaderMaterial.new();material.resource_local_to_scene=true;material.shader=shader;mesh.material=material
	actor.add_child(mesh)
static func map(p: Vector2,pose: RefCounted) -> Vector2:
	var anchor: Vector2=pose.body*Vector2(550,620)
	var floor_y: float=lerpf(1110.0,1335.0,pose.amount)
	var sy: float=(floor_y-anchor.y)/850.0
	return anchor+Vector2((p.x-1150.0)*0.45,(p.y-100.0)*sy)
