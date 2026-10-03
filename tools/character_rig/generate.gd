extends SceneTree
const Rig=preload("animated_rig.gd")
const Partition=preload("partition.gd")
func _initialize() -> void:
	call_deferred("run")
func run() -> void:
	var definitions_path: String=get_script().resource_path.get_base_dir()+"/motion-regions.json"
	var spec: Dictionary=JSON.parse_string(FileAccess.get_file_as_string(definitions_path))
	var actor:=Rig.build(spec,Partition.make_partition(spec))
	actor.name="Rig"
	actor.get_node("AnimationPlayer").autoplay="idle"
	own_children(actor,actor)
	var packed:=PackedScene.new()
	if packed.pack(actor)!=OK:
		push_error("Cannot pack native combat rig");quit(1);return
	var path: String="res://STS2-Navia/scenes/characters/rig/navia_combat.tscn"
	var temporary:=path.trim_suffix(".tscn")+".building.tscn"
	if ResourceSaver.save(packed,temporary)!=OK:
		push_error("Cannot save native combat rig");quit(1);return
	var content:=FileAccess.get_file_as_string(temporary)
	var identifiers:=RegEx.create_from_string('id="([^"]+)"')
	var counter:=0
	for match: RegExMatch in identifiers.search_all(content):
		counter+=1
		content=content.replace('"'+match.get_string(1)+'"','"rig_%03d"'%counter)
	if not FileAccess.file_exists(path) or FileAccess.get_file_as_string(path)!=content:
		FileAccess.open(path,FileAccess.WRITE).store_string(content)
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temporary))
	print("Native combat rig generated: "+path)
	actor.free()
	quit()
func own_children(node: Node,owner: Node) -> void:
	for child in node.get_children():
		child.owner=owner
		own_children(child,owner)
