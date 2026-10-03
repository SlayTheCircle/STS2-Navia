extends RefCounted
static func map(p: Vector2,pose: RefCounted) -> Vector2:
	var anchor: Vector2=pose.body*Vector2(550,620)
	var floor_y: float=lerpf(1110.0,1335.0,pose.amount)
	var sy: float=(floor_y-anchor.y)/850.0
	return anchor+Vector2((p.x-1150.0)*0.45,(p.y-100.0)*sy)
