extends RefCounted
## Weight transfer, brace, knee contact, then upper-body settling.
static func value(t: float,keys: Array) -> float:
	for i in range(1,keys.size()):
		if t<=keys[i][0]:
			return lerpf(keys[i-1][1],keys[i][1],smoothstep(keys[i-1][0],keys[i][0],t))
	return keys[-1][1]

static func shin(t: float) -> float:
	return deg_to_rad(value(t,[[0.0,77.9],[0.18,86.0],[0.45,98.0],[0.72,92.0],[1.0,91.0]]))

static func thigh(t: float,rest: float) -> float:
	return deg_to_rad(value(t,[[0.0,rad_to_deg(rest)],[0.18,59.0],[0.45,22.0],[0.76,-22.0],[0.84,-27.0],[1.0,-25.0]]))

static func torso(t: float) -> float:
	return deg_to_rad(value(t,[[0.0,0.0],[0.15,10.0],[0.48,13.0],[0.76,8.0],[1.0,12.0]]))

static func head(t: float) -> float:
	return deg_to_rad(value(t,[[0.0,0.0],[0.55,2.0],[0.78,6.0],[1.0,14.0]]))

static func brace(t: float) -> Vector2:
	var step:=smoothstep(0.08,0.36,t)
	return Vector2(32.0*step,-12.0*sin(PI*step))
