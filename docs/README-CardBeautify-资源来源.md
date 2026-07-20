# CardBeautify 资源来源说明

本文档只说明 **CardBeautify** 中卡面图片资源的来源、原名和整合方式，方便二次分发时保留原作者署名。

当前整合版 Mod：`CardBeautify` / `Card Beautify`  
当前资源包：`CardBeautifyAssets.pck`  
当前目录索引：`Data/CardBeautifyCatalog.catalog`

## 资源来源一览

| CardBeautify 内显示名称 | 短按钮名 | 当前收录数量 | 原始来源 / 原名 | 原作者 / 署名 | 原始文件或目录名 | 备注 |
|---|---:|---:|---|---|---|---|
| `Original` | `Orig` | 130 | 用户提供的卡面合集压缩包 `卡面美化合集(战士+猎手+机器人).zip` | `CapePisces` | 原 Mod 名 / id：`CardReplaceMod1`；原文件：`CardReplaceMod1.dll`、`CardReplaceMod1.json`、`CardReplaceMod1.pck` | 已提取并重新打包到 `res://CardBeautifyPacked/Original/CardReplaceMod1/`。 |
| `Anime Minimal` | `AniMin` | 88 | Steam Workshop 订阅 Mod `Anime Defect Cards` | 原 manifest 写法：`榛戞礊椋為奔 Painttist (FlyingFish)`；通常署名为 `Painttist (FlyingFish)` | 原 Mod id：`AnimeDefectCards`；原目录：`card_portraits/minimal/defect`；原文件：`AnimeDefectCards.dll`、`AnimeDefectCards.json`、`AnimeDefectCards.pck` | 作为机器人卡“minimal”风格包导入。 |
| `Anime Spire` | `AniSpi` | 83 | Steam Workshop 订阅 Mod `Anime Defect Cards` | 原 manifest 写法：`榛戞礊椋為奔 Painttist (FlyingFish)`；通常署名为 `Painttist (FlyingFish)` | 原 Mod id：`AnimeDefectCards`；原目录：`card_portraits/spire/defect`；原文件：`AnimeDefectCards.dll`、`AnimeDefectCards.json`、`AnimeDefectCards.pck` | 作为机器人卡“spire”风格包导入。 |
| `Aduare` | `Adu` | 30 | 制作时整理的 `Aduare` 卡面资源组 | `Aduare` | 原资源组名：`Aduare`；导入目录：`generated/assets/card_art/`；文件名格式：`MegaCrit.Sts2.Core.Models.Cards.<CardClass>_card_art.png` | 保留原资源组名与原文件命名格式。 |
| `Diana` | `Dia` | 90 | 制作时整理的 `Diana` 卡面资源组 | `Diana111` | 原资源组名：`Diana`；导入目录：`generated/assets/card_art/`；文件名格式：`MegaCrit.Sts2.Core.Models.Cards.<CardClass>_card_art.png` | 保留原资源组名与原文件命名格式。 |

## 原始 manifest 记录

### CardReplaceMod1

```json
{
  "id": "CardReplaceMod1",
  "name": "CardReplaceMod1",
  "author": "CapePisces",
  "description": "Slay the Spire 2 mod created from a template for use with BaseLib",
  "version": "v0.0.0",
  "has_pck": true,
  "has_dll": true,
  "dependencies": [],
  "affects_gameplay": false
}
```

### AnimeDefectCards

```json
{
  "id": "AnimeDefectCards",
  "name": "Anime Defect Cards",
  "author": "榛戞礊椋為奔 Painttist (FlyingFish)",
  "version": "0.10.2",
  "min_game_version": "0.103.0",
  "has_pck": true,
  "has_dll": true,
  "dependencies": [],
  "affects_gameplay": false
}
```

> 注：`AnimeDefectCards.json` 中作者字段含有乱码文本；本说明保留 manifest 中的原始写法，并同时保留 `Painttist (FlyingFish)` 署名。

## CardBeautify 的整合改动

- 将多个来源的卡面资源统一重新打包到 `CardBeautifyAssets.pck`。
- 使用 `Data/CardBeautifyCatalog.catalog` 建立“卡牌 id -> 可选卡面包 -> 图片路径”的索引。
- 添加/调整了每张卡牌详情页内的切换按钮，按钮尺寸和触控区域已针对手机触控放大，并尽量贴近《杀戮尖塔》UI 风格。
- 本 Mod 只替换卡牌肖像显示，不修改卡牌数值、战斗逻辑、掉落、商店或存档内容。
- CardBeautify 没有包含来源 Mod 的原始逻辑；只使用并注明其图片资源来源。

## 致谢

感谢以下作者/资源组提供原始卡面资源：

- `CapePisces` / `CardReplaceMod1`
- `Painttist (FlyingFish)` / `Anime Defect Cards`
- `Aduare`
- `Diana111`

整合与手机 UI 适配：`Yummn` / `CardBeautify`。