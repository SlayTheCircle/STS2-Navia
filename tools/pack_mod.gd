# PCK 打包脚本(由 scripts/build-pck.sh 以 headless Godot 运行)。
# 把 <root>/localization/** 与 <root>/assets/** 打进 res://STS2-Navia/ 前缀下。
# 官方合并路径要求:res://<modId>/localization/<lang>/<table>.json
# 注意:quit() 在主循环启动前调用会被忽略,因此在 _process 里两帧退出。
extends SceneTree

const MOD_ID := "STS2-Navia"

var _root := ""
var _out := ""
var _count := 0
var _frame := 0


func _init() -> void:
	_root = OS.get_environment("NAVIA_MOD_ROOT")
	if _root == "":
		push_error("NAVIA_MOD_ROOT not set")
		_frame = -1
		return
	_out = OS.get_environment("NAVIA_MOD_PCK")
	if _out == "":
		_out = _root + "/mods-dist/" + MOD_ID + "/" + MOD_ID + ".pck"


func _process(_delta: float) -> bool:
	_frame += 1
	if _frame < 0:
		quit(1)
		return true
	if _frame == 1:
		var packer := PCKPacker.new()
		packer.pck_start(_out)
		# 素材工程:assets/STS2-Navia/** → res://STS2-Navia/**(源图 + .import 侧车)。
		# ctex 编译纹理放 pck 根的 res://.godot/imported/(侧车重定向目标,布局照 Hikari pck 实证)。
		_add_dir(packer, _root + "/localization", "res://" + MOD_ID + "/localization")
		_add_dir(packer, _root + "/assets/" + MOD_ID, "res://" + MOD_ID)
		# 全局命名空间素材(RitsuLib 无重定向补丁的内容走原版全局路径推导):
		# 纪元立绘 res://images/timeline/epoch_portraits/<id小写>.png。目录缺失自动跳过。
		_add_dir(packer, _root + "/assets/global", "res://")
		_add_dir(packer, _root + "/assets/.godot/imported", "res://.godot/imported")
		packer.flush()
		print("PCK packed: %d files -> %s" % [_count, _out])
	if _frame >= 2:
		quit(0)
	return true


func _add_dir(packer: PCKPacker, fs_dir: String, res_prefix: String) -> void:
	var dir := DirAccess.open(fs_dir)
	if dir == null:
		return
	dir.list_dir_begin()
	var name := dir.get_next()
	while name != "":
		if not name.begins_with("."):
			var fs_path := fs_dir + "/" + name
			if dir.current_is_dir():
				_add_dir(packer, fs_path, res_prefix + "/" + name)
			elif FileAccess.file_exists(fs_path + ".import"):
				# 带 .import 侧车的源文件(已由 Godot 导入成 ctex)只打包侧车,不打包源图本体
				pass
			else:
				packer.add_file(res_prefix + "/" + name, fs_path, true)
				_count += 1
		name = dir.get_next()
	dir.list_dir_end()
