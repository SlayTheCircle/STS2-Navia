extends RefCounted
const Weights=preload("skin_weights.gd")
const Approved=preload("approved_motion.gd")
const Deformation=preload("deformation_tracks.gd")

static func specs() -> Dictionary:
	return JSON.parse_string(FileAccess.get_file_as_string("res://../tools/character_rig/clips.json"))

static func values(spec: Dictionary,t: float) -> Dictionary:
	var v: Dictionary
	if spec.has("approved"):
		v=Approved.values(spec["approved"],t)
	else:
		v={"breath":0.0,"hair_root":0.0,"hair_tip":0.0,"skirt_root":0.0,"skirt_tip":0.0,"arm":0.0}
	v["body_x"]=0.0
	v["body_angle"]=0.0
	for key: String in spec.get("keys",{}):
		v[key]=Approved.ease_keys(t,spec["keys"][key])
	return v

static func library(groups: Array) -> AnimationLibrary:
	var lib:=AnimationLibrary.new()
	var definitions:=specs()
	for kind: String in definitions:
		var spec: Dictionary=definitions[kind]
		var anim:=Animation.new()
		anim.length=spec["length"]
		anim.loop_mode=Animation.LOOP_LINEAR if spec["loop"] else Animation.LOOP_NONE
		var tracks: Dictionary={}
		for name: String in Weights.NAMES:
			if name=="Root":continue
			var pos:=anim.add_track(Animation.TYPE_VALUE)
			anim.track_set_path(pos,"Skeleton2D/Root/"+name+":position")
			var rot:=anim.add_track(Animation.TYPE_VALUE)
			anim.track_set_path(rot,"Skeleton2D/Root/"+name+":rotation")
			tracks[name]=[pos,rot]
		var times: Array=[0.0,anim.length]
		for f in range(int(ceil(anim.length*30))):times.append(minf(f/30.0,anim.length))
		for keys: Array in spec.get("keys",{}).values():
			for key: Array in keys:times.append(float(key[0]))
		times.sort()
		var last: float=-1
		for t: float in times:
			if absf(t-last)<0.00001:continue
			last=t
			insert_pose(anim,tracks,t,values(spec,t))
		var blink:=anim.add_track(Animation.TYPE_VALUE)
		anim.track_set_path(blink,"Skeleton2D/Root/Breath/Blink:modulate")
		for key: Array in spec["blink"]:
			anim.track_insert_key(blink,key[0],Color(1,1,1,key[1]))
		Deformation.add(anim,spec,groups)
		lib.add_animation(kind,anim)
	return lib

static func insert_pose(anim: Animation,tracks: Dictionary,t: float,v: Dictionary) -> void:
	var angle:=deg_to_rad(float(v["body_angle"]))
	var shift:=Vector2(v["body_x"],v["breath"])
	var anchor: Vector2=Weights.PIVOTS[1]
	var rotations: Dictionary={"Breath":0.0,"HairRoot":v["hair_root"],"HairTip":v["hair_tip"],"SkirtRoot":v["skirt_root"],"SkirtTip":v["skirt_tip"],"Arm":v["arm"]}
	for i in range(1,Weights.NAMES.size()):
		var name: String=Weights.NAMES[i]
		var upper: bool=name in ["Breath","HairRoot","HairTip","Arm"]
		var pos: Vector2=anchor+(Weights.PIVOTS[i]-anchor).rotated(angle)+shift if upper else Weights.PIVOTS[i]
		var rotation: float=deg_to_rad(float(rotations[name]))+(angle if upper else 0.0)
		anim.track_insert_key(tracks[name][0],t,pos)
		anim.track_insert_key(tracks[name][1],t,rotation)
