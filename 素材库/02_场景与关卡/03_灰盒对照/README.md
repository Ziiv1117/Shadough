# Level Blockouts

本目录是 P0 关卡灰盒对照图，用于后续 Unity 搭建时核对路径、阻挡、交互点、触发区和出入口。

这些图不是最终美术，不应直接作为游戏背景使用。

## 文件

| 文件 | 内容 |
| --- | --- |
| `blockout_01_outside_river.png` | 第一关：府邸外、河道、树影桥区域 |
| `blockout_02_mansion_key_lock.png` | 第二关：府邸院内、钥匙影和锁门区域 |
| `blockout_03_tower_press_plate.png` | 第三关：灯塔/钟塔入口、压板机关区域 |
| `blockout_04_lure_corridor.png` | 第四关：诱饵通道、寻影兽活动占位区域 |
| `blockout_05_final_clock_core.png` | 第五关：最终钟核房间 |
| `level_blockouts_contact_sheet.png` | 五张灰盒总览 |
| `level_blockouts_manifest.json` | 对应的坐标、区域、出入口和说明 |

## 颜色含义

| 颜色 | 含义 |
| --- | --- |
| 绿色圆点 | 玩家出生点 |
| 黄色圆点 | 出口/下一关入口 |
| 橙色块 | 可交互物或交互源 |
| 紫色区域 | 触发区 |
| 深紫区域 | 危险/寻影兽活动占位区 |
| 深色块 | 阻挡物 |
| 蓝色区域 | 水体阻挡 |

## 使用建议

- 用 `level_blockouts_manifest.json` 作为 Unity 初搭时的摆放参考。
- 美术替换时优先参考 `../01_房间效果图` 里的当前房间效果图。
- 交互物最终应从 `../02_环境拆分素材` 中拆成独立 Sprite 或 Prefab。
