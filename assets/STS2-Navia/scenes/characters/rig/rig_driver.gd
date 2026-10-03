extends Node2D
const Train=preload("train_deformation.gd")
const Legs=preload("legs/motion.gd")
const Pose=preload("kneeling_pose.gd")
@export var collapse: float=0.0:
	set(value):
		if is_equal_approx(collapse,value) and not source.is_empty():return
		collapse=value
		apply_pose()
var source: Dictionary={}
func _ready() -> void:
	apply_pose()
func apply_pose() -> void:
	if not has_node("Skeleton2D/Root/Breath/Blink"):return
	if source.is_empty():
		for child in get_children():
			if child is Polygon2D:source[child.name]=child.uv.duplicate()
	var pose:=Pose.new(collapse,self)
	Legs.apply(self,clampf(collapse,0.0,1.0))
	for child in get_children():
		if child is Polygon2D:
			var group: String=child.get_meta("source_group",str(child.name))
			var points:=PackedVector2Array()
			for p: Vector2 in source[child.name]:
				points.append(Train.map(p,pose) if child.name=="KneelingTrain" else pose.map(p,group))
			if str(child.name).begins_with("Kneeling"):
				child.modulate.a=smoothstep(0.42,0.48,collapse)
			elif child.material is ShaderMaterial:
				(child.material as ShaderMaterial).set_shader_parameter("replacement",smoothstep(0.42,0.48,collapse))
			child.polygon=points
	var blink:=get_node("Skeleton2D/Root/Breath/Blink") as Sprite2D
	blink.position=pose.blink_position()
	blink.rotation=pose.blink_rotation()
