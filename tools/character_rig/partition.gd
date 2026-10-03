extends RefCounted
## Non-destructive UV partition of an approved atlas. No pixels are redrawn.

static func points(values: Array) -> PackedVector2Array:
	var result := PackedVector2Array()
	for value in values:
		result.append(Vector2(value[0], value[1]))
	return result

static func area(poly: PackedVector2Array) -> float:
	var total := 0.0
	for i in poly.size():
		total += poly[i].cross(poly[(i + 1) % poly.size()])
	return absf(total) * 0.5

static func make_partition(spec: Dictionary) -> Array:
	var remaining: Array[PackedVector2Array] = []
	var size := Vector2i(spec["size"][0], spec["size"][1])
	# Small convex seed cells prevent the clipper from returning holes.
	for y in range(0, size.y, 48):
		for x in range(0, size.x, 48):
			var right := mini(x + 48, size.x)
			var bottom := mini(y + 48, size.y)
			remaining.append(PackedVector2Array([Vector2(x,y),Vector2(right,y),Vector2(right,bottom)]))
			remaining.append(PackedVector2Array([Vector2(x,y),Vector2(right,bottom),Vector2(x,bottom)]))
	var result := []
	for region: Dictionary in spec["regions_front_first"]:
		var mask := points(region["points"])
		var selected: Array[PackedVector2Array] = []
		var next: Array[PackedVector2Array] = []
		for cell in remaining:
			for part in Geometry2D.intersect_polygons(cell, mask):
				if area(part) > 0.001:
					selected.append(part)
			for part in Geometry2D.clip_polygons(cell, mask):
				if area(part) > 0.001:
					next.append(part)
		remaining = next
		var group := region.duplicate()
		group["polygons"] = selected
		result.append(group)
	var leftover: Dictionary = spec["remainder"].duplicate()
	leftover["polygons"] = remaining
	result.append(leftover)
	return result

static func build(groups: Array, texture: Texture2D, exploded := 0.0) -> Node2D:
	var actor := Node2D.new()
	actor.name = "NaviaParts"
	var skeleton := Skeleton2D.new()
	skeleton.name = "Skeleton2D"
	actor.add_child(skeleton)
	for group: Dictionary in groups:
		var origin := Vector2(group["pivot"][0], group["pivot"][1])
		var bone := Bone2D.new()
		bone.name = group["id"]
		bone.position = origin
		bone.rest = bone.transform
		bone.set_autocalculate_length_and_angle(false)
		bone.set_length(40)
		skeleton.add_child(bone)
		var mesh := Polygon2D.new()
		mesh.name = "Region"
		mesh.texture = texture
		mesh.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS
		var vertices := PackedVector2Array()
		var uv := PackedVector2Array()
		var triangles := []
		for poly: PackedVector2Array in group["polygons"]:
			var base := vertices.size()
			var indices := Geometry2D.triangulate_polygon(poly)
			assert(not indices.is_empty(), "Non-triangulable region")
			for p in poly:
				vertices.append(p - origin)
				uv.append(p)
			for i in range(0, indices.size(), 3):
				triangles.append(PackedInt32Array([base+indices[i],base+indices[i+1],base+indices[i+2]]))
		mesh.polygon = vertices
		mesh.uv = uv
		mesh.polygons = triangles
		bone.add_child(mesh)
		bone.position += Vector2(group["explode"][0],group["explode"][1]) * exploded
	return actor
