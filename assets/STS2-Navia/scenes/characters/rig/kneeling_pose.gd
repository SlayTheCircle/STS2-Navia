extends RefCounted
## Pose-space deformation from articulated hip/knee/ankle targets.
## Keeps the approved texture; no whole-character image replacement.
const Legs=preload("legs/motion.gd")
var amount: float
var body: Transform2D
var head: Transform2D
var arm: Transform2D

func _init(value: float,actor: Node2D) -> void:
	amount=clampf(value,0.0,1.0)
	body=Legs.body(amount,actor)
	head=around(Vector2(630,330),preload("legs/sequence.gd").head(amount),Vector2.ZERO)
	arm=around(Vector2(775,568),deg_to_rad(-32.0*amount),Vector2.ZERO)

static func around(pivot: Vector2,angle: float,shift: Vector2) -> Transform2D:
	return Transform2D(angle,pivot+shift)*Transform2D(0,-pivot)

func map(p: Vector2,group: String) -> Vector2:
	if amount<0.000001:return p
	if group=="near_forearm_grip_parasol":
		var weight:=smoothstep(0.0,54.0,p.y-558.0+0.2*(p.x-770.0))
		return body*p.lerp(arm*p,weight)
	var q: Vector2=body*p.lerp(head*p,1.0-smoothstep(280.0,400.0,p.y))
	if group in ["rear_skirt_remainder","FarArmBacking","LegBacking"] and p.y>650.0:
		var anchor: Vector2=body*Vector2(550,650)
		var fold:=Vector2(anchor.x+(p.x-550)*0.75,anchor.y+(1335-anchor.y)*sin(clampf((p.y-650)/450.0,0,1)*PI/2))
		q=q.lerp(fold,amount*smoothstep(650.0,730.0,p.y))
	return q

func blink_position() -> Vector2:
	return body*head*Vector2(588,214)-Vector2(620,600)

func blink_rotation() -> float:
	return body.get_rotation()+head.get_rotation()
