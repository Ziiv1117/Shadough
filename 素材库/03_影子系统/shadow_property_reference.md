# 影子属性规则说明

路径：`素材库/03_影子系统/shadow_property_reference.md`

## 核心原则

- 影子素材只替换视觉表现，不改变玩法语义。
- 基础图和贴出图必须保持同一轮廓、同一角度、同一尺寸关系。
- Collider2D 以影子被剪下那一刻的数据为准，不能因为美术边缘、裂纹、高亮或贴纸效果变化。
- 贴出的影子保持原形态，不自动变成桥、钥匙、压板工具或其它实体道具。
- Reveal View 高亮、Ghost 预览和贴出边缘只负责提示状态，不赋予新属性。

## 属性视觉要求

| 属性 | 用途 | 推荐视觉表现 | 禁止事项 |
| --- | --- | --- | --- |
| `CanStandOn` | 允许玩家站立或通过 | 长条、连续、宽度清楚；轮廓可读 | 不要把树影画成实体桥；不要改变碰撞宽度 |
| `CanPress` | 触发压板或机关 | 稳定、厚重、覆盖范围清楚 | 不要因为贴出效果扩大压板接触范围 |
| `CanUnlock` | 触发锁孔检测 | 钥匙形轮廓必须清楚，头部、柄、齿可辨认 | 不要把钥匙影画成实体钥匙道具 |
| `CanAttractEnemy` | 吸引寻影兽 | 玩家影轮廓保留，允许加朱砂脉冲或金线提示 | 不要让诱敌高亮改变影子的碰撞范围 |
| `CanBlock` | 后续阻挡或遮蔽 | 轮廓必须稳定，边缘不能闪烁到影响判定 | 不要用动画缩放 alpha 轮廓 |

## 状态区分

### 未剪下的可剪影子

- 使用 `*_shadow_base.png`。
- 在 Reveal View 中可叠加 `cuttable_shadow_outline.png` 或同类边缘光。
- 允许被灯笼影响长度、方向或缩放，但只限未剪下状态。

### 携带影子

- 保存剪下瞬间的形状、缩放、旋转、Collider2D 尺寸和偏移。
- UI 使用 `shadow_inventory_icon_*.png` 表示来源类型。
- 图标只用于识别，不作为玩法判定来源。

### 贴出影子

- 使用 `*_shadow_pasted.png` 或同轮廓贴出材质。
- 可叠加 `pasted_shadow_edge.png` 表示已经成为场景实体。
- 不再随灯光实时拉长、旋转或重算碰撞。

### 贴影预览

- 使用 `shadow_preview_ghost.png`。
- 有效位置和无效位置可以通过颜色或透明度区分。
- 预览不参与碰撞，不触发机关。

## 光影变化边界

- 视觉阴影可以随光源变化，用来帮助玩家理解当前影子形态。
- 玩法影子在剪下瞬间冻结为 `ShadowItemData`。
- 贴出影子可以被强光淡化、闪烁或消散，但不应被强光改变形状。
- 基础版不做多光源实时投影、半影、遮挡叠影或实时 Collider2D 重算。

## Unity 导入建议

- Sprite Mode：`Single`
- Filter Mode：`Point`
- Compression：`None`
- Mesh Type：优先 `Full Rect`，除非手动 Collider 已经按 Tight 测试过
- Pivot：默认 `Center`，具体对象可在 Prefab 中调整
- Pixels Per Unit：与当前 `_Shadough` Sprite 规范保持一致

## 验收检查

- `tree_shadow_base.png` 和 `tree_shadow_pasted.png` 的 alpha 轮廓一致。
- `beam_shadow_base.png` 和 `beam_shadow_pasted.png` 的 alpha 轮廓一致。
- `key_shadow_base.png` 和 `key_shadow_pasted.png` 的 alpha 轮廓一致。
- `player_shadow_base.png` 和 `player_shadow_lure_pasted.png` 的 alpha 轮廓一致。
- `shadow_collision_guide.png` 中的黄色框只作为 Collider2D 对齐参考，不作为美术边缘。
