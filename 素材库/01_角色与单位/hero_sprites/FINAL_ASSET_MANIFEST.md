# 主角精灵图最终清单

更新时间：2026-07-07

当前目录已整理完毕：`hero_sprites` 下只保留 14 套最终主角精灵图，旧版、失败版、调试版目录已删除。

## 玩法规则

- 只有放下灯后才可以剪影。
- 不存在“拿着灯剪影”的最终动作。
- 剪影前需要进入揭影视角，因此保留持灯揭影和无灯揭影两种姿态。
- 启动钟核必须是持灯状态。
- 受击分为无灯受击和持灯受击。

## 最终目录

| 序号 | 状态 | 目录 | 用途 |
| --- | --- | --- | --- |
| 01 | 持灯 | `hero_idle_lantern_4dir_v2` | 持灯四方向待机 |
| 02 | 持灯 | `hero_walk_lantern_4dir_v2` | 持灯四方向行走 |
| 03 | 无灯 | `hero_idle_no_lantern_4dir_v1` | 放下灯后的四方向待机 |
| 04 | 无灯 | `hero_walk_no_lantern_4dir_v2` | 放下灯后的四方向行走 |
| 05 | 持灯到无灯 | `hero_place_lantern_4dir_v1` | 放下灯动作 |
| 06 | 无灯到持灯 | `hero_pickup_lantern_4dir_v1` | 拾起灯动作 |
| 07 | 持灯 | `hero_reveal_focus_lantern_4dir_v1` | 持灯揭影视角/观察姿态 |
| 08 | 无灯 | `hero_reveal_focus_no_lantern_4dir_v1` | 无灯揭影视角/准备剪影姿态 |
| 09 | 无灯 | `hero_cut_shadow_no_lantern_4dir_v1` | 剪影动作 |
| 10 | 无灯 | `hero_paste_shadow_no_lantern_4dir_v1` | 贴影/放影动作 |
| 11 | 无灯 | `hero_interact_no_lantern_4dir_v1` | 通用交互/机关操作 |
| 12 | 无灯 | `hero_hurt_no_lantern_4dir_v5` | 无灯受击/失败反馈 |
| 13 | 持灯 | `hero_hurt_lantern_4dir_v1` | 持灯受击/失败反馈 |
| 14 | 持灯 | `hero_activate_core_lantern_4dir_v3` | 启动钟核/胜利反馈，持灯且只有一把剪刀 |

## 每套目录内容

每个最终目录都包含：

- `raw-sheet.png`
- `raw-sheet-clean.png`
- `sheet-transparent.png`
- 16 张拆帧 PNG
- `animation.gif`
- `prompt-used.txt`
- `source-file.txt`
- `pipeline-meta.json`

## QC 结果

- 最终目录数量：14
- 每套帧数：16
- 总帧数：224
- 每套都有 `sheet-transparent.png`
- 每套都有 `animation.gif`
- `edge_touch_frames` 均为空

## Unity 导入建议

- 使用各目录下的 `sheet-transparent.png`。
- `Sprite Mode`: `Multiple`
- 切片：4x4，每格 `256x256`
- `Filter Mode`: `Point`
- 压缩：关闭或使用无损
- Pivot：统一 bottom/feet 对齐

## 总览图

- `hero_sprites_overview.png`：14 套最终动作总览图，只用于查看和对照，不作为游戏运行精灵表导入。
