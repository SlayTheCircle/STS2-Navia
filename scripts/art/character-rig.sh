# Native combat atlas inputs remain in ART_SOURCE_DIR; copy exact approved/supplement bytes.
mkdir -p "$DST/characters/rig"
for name in navia-master-v3 blink-v1 near-arm-backing-v1 far-arm-backing-v1 rigid-legs-v1 kneeling-train-v1; do
    if ! cmp -s "$SRC/角色骨骼/godot/运行素材/$name.png" "$DST/characters/rig/$name.png"; then
        cp "$SRC/角色骨骼/godot/运行素材/$name.png" "$DST/characters/rig/$name.png"
    fi
done
