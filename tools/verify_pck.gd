# 验证真实 PCK 的纹理、尺寸、透明边缘与本地化；不代替游戏运行时验收。
extends SceneTree

const TEXTURES := [
	"res://STS2-Navia/images/energy/navia_spark.png",
	"res://STS2-Navia/images/energy/navia_energy_glow.png",
	"res://STS2-Navia/images/cards/TravelLight.png",
	"res://STS2-Navia/images/cards/VolleyFire.png",
	"res://STS2-Navia/images/cards/LightningReload.png",
	"res://STS2-Navia/images/relics/RosulaEmblem.png",
	"res://STS2-Navia/images/relics/RosulaEmblem_outline.png",
	"res://STS2-Navia/images/potions/Fonta.png",
	"res://STS2-Navia/images/powers/LoadPower.png",
	"res://STS2-Navia/images/powers/CannonadePower.png",
	"res://STS2-Navia/images/powers/ReinforcedBarrelPower.png",
	"res://STS2-Navia/images/powers/GuidedBombardmentPower.png",
	"res://STS2-Navia/images/powers/GreedyGunfirePower.png",
	"res://STS2-Navia/images/powers/GoldenTouchPower.png",
	"res://STS2-Navia/images/powers/BalanceTheBooksPower.png",
	"res://STS2-Navia/images/powers/CollectInterestPower.png",
	"res://STS2-Navia/images/powers/CoveringFirePower.png",
	"res://STS2-Navia/images/powers/ArmsDealerPower.png",
	"res://STS2-Navia/images/powers/AccidentalBlastPower.png",
	"res://STS2-Navia/images/powers/DangerousRetrofitFillPower.png",
	"res://STS2-Navia/images/enchantments/navia_support.png",
	"res://STS2-Navia/images/energy/navia_energy_big.png",
	"res://STS2-Navia/images/energy/navia_energy_text.png",
	"res://STS2-Navia/images/events/HometownMemory.png",
	"res://STS2-Navia/images/events/ForeignBall.png",
	"res://STS2-Navia/images/events/HeavyRain.png",
	"res://images/timeline/epoch_portraits/sts2_navia_epoch_1.png",
	"res://images/timeline/epoch_portraits/sts2_navia_epoch_4.png",
	"res://STS2-Navia/images/timeline/sts2_navia_epoch_1_thumb.png",
]
const DIMENSIONS := {
	"res://STS2-Navia/images/energy/navia_spark.png": Vector2i(64, 64),
	"res://STS2-Navia/images/energy/navia_energy_glow.png": Vector2i(128, 128),
	"res://STS2-Navia/images/cards/LightningReload.png": Vector2i(606, 852),
	"res://images/timeline/epoch_portraits/sts2_navia_epoch_1.png": Vector2i(1672, 941),
	"res://STS2-Navia/images/timeline/sts2_navia_epoch_1_thumb.png": Vector2i(272, 174),
	"res://STS2-Navia/images/energy/navia_energy_big.png": Vector2i(256, 256),
	"res://STS2-Navia/images/energy/navia_energy_text.png": Vector2i(24, 24),
	"res://STS2-Navia/images/enchantments/navia_support.png": Vector2i(256, 256),
}
const TRANSPARENT_TEXTURES := [
	"res://STS2-Navia/images/powers/DangerousRetrofitFillPower.png",
	"res://STS2-Navia/images/energy/navia_spark.png",
	"res://STS2-Navia/images/energy/navia_energy_glow.png",
	"res://STS2-Navia/images/enchantments/navia_support.png",
	"res://STS2-Navia/images/powers/CollectInterestPower.png",
	"res://STS2-Navia/images/powers/CoveringFirePower.png",
	"res://STS2-Navia/images/powers/ArmsDealerPower.png",
	"res://STS2-Navia/images/powers/AccidentalBlastPower.png",
	"res://STS2-Navia/images/energy/navia_energy_big.png",
	"res://STS2-Navia/images/energy/navia_energy_text.png",
]

var _frame := 0

func _process(_delta: float) -> bool:
	_frame += 1
	if _frame != 1:
		return true
	var pck := OS.get_environment("NAVIA_MOD_PCK")
	if pck.is_empty() or not ProjectSettings.load_resource_pack(pck, true):
		push_error("PCK 挂载失败")
		quit(1)
		return true
	var failures := 0
	for path in TEXTURES:
		failures += _verify_texture(path)
	# 标准版 Godot 不执行游戏 C# 脚本，仅确认新能量计场景确实入包。
	if not FileAccess.file_exists("res://STS2-Navia/scenes/combat/navia_energy_counter.tscn"):
		push_error("PCK 缺能量计场景")
		failures += 1
	if not FileAccess.file_exists("res://STS2-Navia/scenes/vfx/navia_card_trail.tscn"):
		push_error("PCK 缺专属拖尾场景")
		failures += 1
	for lang in ["zhs", "eng"]:
		for table in ["cards", "events", "epochs", "ancients"]:
			failures += _verify_localization(lang, table)
	if failures == 0:
		print("PCK 代表性纹理、尺寸、透明边缘与本地化可解析；未验证游戏模型、角色场景或机制。")
	quit(0 if failures == 0 else 1)
	return true

func _verify_texture(path: String) -> int:
	if not ResourceLoader.exists(path):
		push_error("PCK 缺纹理: " + path)
		return 1
	var texture := ResourceLoader.load(path, "Texture2D") as Texture2D
	if texture == null or texture.get_width() <= 0 or texture.get_height() <= 0:
		push_error("PCK 纹理不可解析: " + path)
		return 1
	# 保护竖卡裁框、纪元缩略图和内联能量图的实际显示尺寸。
	var expected: Vector2i = DIMENSIONS.get(path, Vector2i(256, 256) if path.contains("/powers/") else Vector2i.ZERO)
	if expected != Vector2i.ZERO and Vector2i(texture.get_width(), texture.get_height()) != expected:
		push_error("PCK 纹理尺寸错误: " + path)
		return 1
	# 新透明图标若被导出为实底，在游戏小图槽里会出现方框。
	if path in TRANSPARENT_TEXTURES:
		var pixels := texture.get_image()
		if pixels == null or pixels.is_empty() or pixels.get_pixel(0, 0).a > 0.01:
			push_error("PCK 图标缺透明边缘: " + path)
			return 1
	return 0

func _verify_localization(lang: String, table: String) -> int:
	var path := "res://STS2-Navia/localization/%s/%s.json" % [lang, table]
	if not FileAccess.file_exists(path):
		push_error("PCK 缺本地化: " + path)
		return 1
	var parsed = JSON.parse_string(FileAccess.get_file_as_string(path))
	if not parsed is Dictionary or parsed.is_empty():
		push_error("PCK 本地化不可解析: " + path)
		return 1
	return 0
