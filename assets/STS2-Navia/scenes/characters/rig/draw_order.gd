extends RefCounted
## Keep every drawable in the creature's inherited canvas layer.
## Paint ranks affect only sibling order, never the surrounding room/UI.
static func sort_children(actor: Node2D) -> void:
	var children:=actor.get_children()
	children.sort_custom(func(a: Node,b: Node):return int(a.get_meta("navia_layer",100))<int(b.get_meta("navia_layer",100)))
	for i in children.size():actor.move_child(children[i],i)
