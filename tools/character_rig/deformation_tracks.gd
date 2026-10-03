extends RefCounted
static func add(anim: Animation,spec: Dictionary,_groups: Array) -> void:
	var track:=anim.add_track(Animation.TYPE_VALUE)
	anim.track_set_path(track,":collapse")
	if not spec.has("collapse"):
		anim.value_track_set_update_mode(track,Animation.UPDATE_DISCRETE)
		anim.track_insert_key(track,0.0,0.0)
		return
	anim.track_set_interpolation_type(track,Animation.INTERPOLATION_CUBIC)
	for key: Array in spec["collapse"]:anim.track_insert_key(track,key[0],key[1])
