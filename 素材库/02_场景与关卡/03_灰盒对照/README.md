# Level 01 Pixel Blockout

本目录保存当前完整地图的像素级灰盒对照，用于后续 Unity 搭建时核对可行走区域、阻挡、水体、出生点、出口和关键机关点。

这些图不是最终美术，不应直接作为游戏背景使用。最终美术以 `../05_最终地图/level01_full_map.png` 为准。

## 文件

| 文件 | 内容 |
| --- | --- |
| `level01_full_map_blockout.png` | OpenCV 像素分类灰盒图，尺寸与最终地图一致 |
| `level01_full_map_blockout_overlay.png` | 像素分类灰盒叠在最终地图上的对齐检查图 |
| `level01_full_map_pixel_outline_overlay.png` | 1px 红色外轮廓贴边检查图 |
| `level01_full_map_pixel_footprint_mask.png` | 前景占用区域黑白像素 mask |
| `level01_full_map_pixel_class_mask.png` | 原始分类 id mask |
| `level01_full_map_blockout_contact_sheet.png` | 灰盒、叠图、轮廓和 class mask 的浏览总览 |
| `level01_full_map_blockout_manifest.json` | OpenCV 分割规则、像素统计、输出文件和场景锚点坐标 |

## 颜色含义

| 颜色 | 含义 |
| --- | --- |
| 灰色区域 | 石板/室内地面候选像素 |
| 绿色区域 | 草地候选像素 |
| 蓝色区域 | 水体阻挡候选像素 |
| 黑色区域 | 墙体、阴影、道具或不可通行结构候选像素 |
| 青色区域 | 屋顶或青瓦结构候选像素 |
| 红色轮廓 | 从最终地图前景像素提取的 1px 外边界 |

## 使用建议

- Unity 对齐时同时导入 `../05_最终地图/level01_full_map.png`、`level01_full_map_blockout_overlay.png` 和 `level01_full_map_pixel_outline_overlay.png`，三者使用相同 PPU 和 Pivot。
- 用 `level01_full_map_pixel_footprint_mask.png` 和 `level01_full_map_pixel_class_mask.png` 作为像素级参考，再在 Unity 中简化成 Collider2D、Trigger 和场景锚点。
- `level01_full_map_blockout_manifest.json` 里保留了 OpenCV 分割规则、像素统计和近似场景锚点。
- 当前不保留拆分素材；后续如需独立 Sprite 或 Prefab，再从完整地图重新提取。
