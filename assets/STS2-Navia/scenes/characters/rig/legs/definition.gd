extends RefCounted
## Atlas anchors are fixed authoring data. Playback never scales or deforms a limb.
const TEXTURE="res://STS2-Navia/images/characters/rig/rigid-legs-v1.png"
const FLOOR=1330.0
const NEAR={"hip":Vector2(574,660),"knee":Vector2(520,985),"lower_angle":deg_to_rad(96.2),"upper_region":Rect2(250,20,280,565),"upper_anchor":Vector2(398,25),"upper_joint":Vector2(365,510),"lower_region":Rect2(230,600,320,730),"lower_anchor":Vector2(361,675)}
const FAR={"hip":Vector2(684,686),"knee":Vector2(738,978),"lower_angle":deg_to_rad(77.9),"upper_region":Rect2(630,20,300,565),"upper_anchor":Vector2(750,25),"upper_joint":Vector2(788,510),"lower_region":Rect2(650,600,370,730),"lower_anchor":Vector2(788,675)}

static func upper_length(definition: Dictionary) -> float:
	return definition["hip"].distance_to(definition["knee"])
