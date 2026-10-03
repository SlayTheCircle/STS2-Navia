extends RefCounted
const Weights = preload("skin_weights.gd")

static func ease_keys(t: float, keys: Array) -> float:
	for i in range(1,keys.size()):
		if t <= keys[i][0]:
			var u: float = (t-keys[i-1][0])/(keys[i][0]-keys[i-1][0])
			return lerpf(keys[i-1][1],keys[i][1],0.5-0.5*cos(u*PI))
	return keys[-1][1]

static func values(kind: String, t: float) -> Dictionary:
	if kind == "idle":
		var phase := t/3.2*TAU
		return {"breath":-4.0*(0.5-0.5*cos(phase)),"hair_root":sin(phase)*0.7,"hair_tip":(sin(phase-0.45)+sin(0.45))*1.6,"skirt_root":sin(phase)*-0.4,"skirt_tip":(sin(phase-0.7)+sin(0.7))*-0.8,"arm":sin(phase)*0.25}
	var raise := ease_keys(t,[[0.0,0.0],[0.16,1.5],[0.65,-28.0],[1.0,-28.0],[1.22,-26.0],[1.86,0.0],[2.0,0.0]])
	var response := sin(t/2.0*PI)
	return {"breath":-2.0*response,"hair_root":response*-0.5,"hair_tip":response*1.3,"skirt_root":response*0.4,"skirt_tip":response*-0.7,"arm":raise}

static func library() -> AnimationLibrary:
	var lib := AnimationLibrary.new()
	for kind in ["idle","raise"]:
		var anim := Animation.new()
		anim.length = 3.2 if kind == "idle" else 2.0
		anim.loop_mode = Animation.LOOP_LINEAR if kind == "idle" else Animation.LOOP_NONE
		var tracks := {}
		for name in Weights.NAMES:
			if name == "Root":
				continue
			var pos := anim.add_track(Animation.TYPE_VALUE)
			anim.track_set_path(pos,"Skeleton2D/Root/"+name+":position")
			var rot := anim.add_track(Animation.TYPE_VALUE)
			anim.track_set_path(rot,"Skeleton2D/Root/"+name+":rotation")
			tracks[name] = [pos,rot]
		var samples := int(round(anim.length*30.0))
		for frame in range(samples+1):
			var time := frame*anim.length/samples
			var v := values(kind,time)
			for i in range(1,Weights.NAMES.size()):
				var name: String = Weights.NAMES[i]
				var dy: float = v["breath"] if name in ["Breath","HairRoot","HairTip","Arm"] else 0.0
				var rotations := {"Breath":0.0,"HairRoot":v["hair_root"],"HairTip":v["hair_tip"],"SkirtRoot":v["skirt_root"],"SkirtTip":v["skirt_tip"],"Arm":v["arm"]}
				anim.track_insert_key(tracks[name][0],time,Weights.PIVOTS[i]+Vector2(0,dy))
				anim.track_insert_key(tracks[name][1],time,deg_to_rad(rotations[name]))
		var blink := anim.add_track(Animation.TYPE_VALUE)
		anim.track_set_path(blink,"Skeleton2D/Root/Breath/Blink:modulate")
		var blink_keys: Array = [[0.0,0.0],[1.22,0.0],[1.27,1.0],[1.35,1.0],[1.40,0.0],[3.2,0.0]] if kind=="idle" else [[0.0,0.0],[2.0,0.0]]
		for key: Array in blink_keys:
			anim.track_insert_key(blink,key[0],Color(1,1,1,key[1]))
		lib.add_animation(kind,anim)
	return lib
