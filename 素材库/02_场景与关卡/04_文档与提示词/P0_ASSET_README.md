# P0 场景模块素材

本批次只负责 `02_场景与关卡` 的场景素材，不包含角色、敌人、技能、UI 或 Unity 逻辑。

## 预览

- `../00_当前总览/p0_scene_assets_contact_sheet.png`：本批次全部素材总览。
- `p0_scene_assets_manifest.json`：机器可读的生成批次文件清单。
- `P0_PROMPT_MANIFEST.md`：三张源图的生成提示词记录。

## 已生成 Tile

| 文件 | 用途 |
| --- | --- |
| `../02_环境拆分素材/tiles/stone_path_tile.png` | 室外可行走石板路 |
| `../02_环境拆分素材/tiles/blue_brick_floor_tile.png` | 府邸/机关室内地面 |
| `../02_环境拆分素材/tiles/water_channel_tile.png` | 水渠主体 |
| `../02_环境拆分素材/tiles/water_bank_left.png` | 水渠左岸过渡 |
| `../02_环境拆分素材/tiles/water_bank_right.png` | 水渠右岸过渡 |
| `../02_环境拆分素材/tiles/broken_bridge_edge.png` | 断桥/不可直接通行边缘 |

## 已生成墙体

| 文件 | 用途 |
| --- | --- |
| `../02_环境拆分素材/walls/timber_wall.png` | 木梁墙体，作为不可通行边界或院墙段 |

## 已生成道具

| 文件 | 用途 |
| --- | --- |
| `../02_环境拆分素材/props/ancient_tree_trunk.png` | 第一关树影来源，树干独立素材 |
| `../02_环境拆分素材/props/ancient_tree_canopy.png` | 第一关树影来源，树冠独立素材 |
| `../02_环境拆分素材/props/timber_beam_source.png` | 第三关压板机关的梁影来源 |
| `../02_环境拆分素材/props/key_ornament_source.png` | 第二关锁门机关的钥匙纹样影源 |

## 使用边界

- 当前文件是美术素材库资产，不是 Unity Prefab。
- `props` 和 `walls` 里的正式素材已经带透明通道。
- `*_source_*` 文件保留为源图，方便以后重切或修边。
- 后续进 Unity 时，建议 Tile 进入 Tilemap，墙体/树/木梁/钥匙纹样作为独立 Sprite 或 Prefab。
