# CardBeautify v0.5.4 PC v107.1 实机验证

- 游戏：Steam PC v0.107.1
- MOD：CardBeautify v0.5.4
- 卡牌：偏差认知
- 卡图：自动选择的 AnimeDefectMinimal 竖版原图（600×848）
- 验证位置：百科大全卡牌网格与卡牌详情

## 问题原因

v0.5.3 对“偏差认知”的竖版替换图额外创建了一个 600×400 的横向 `AtlasTexture`。卡牌控件随后又以 `KeepAspectCovered` 填充竖向卡框，因此同一张图片被连续裁切两次，最终在详情页出现明显放大、人物身体和构图被截断的问题。

## 修复

1. 删除 `BiasedCognitionPortraitCrops` 缓存。
2. 删除 `GetDisplayTexture` 中针对 `biased_cognition` 的单卡 `AtlasTexture` 裁剪。
3. 缩略图和详情页均直接使用资源包内原始 600×848 竖版纹理。
4. 保留已有的百科大全按钮作用域限制和每张卡图选择持久化。

## 验证结果

1. 百科大全搜索“偏差认知”后，缩略图不再使用错误的二次放大裁剪。
2. 点开详情后，摆锤、完整头发、面部、披肩和上半身均处于卡框内，原图竖版构图恢复。
3. `godot.log` 明确记录：
   `loaded v0.5.4: Biased Cognition keeps its native portrait aspect instead of applying a second landscape crop`
4. CardBeautify 初始化与资源包挂载成功；未出现 CardBeautify 异常。
5. 源码与 PC/Android 两套编译产物离线检查 14/14 通过，两个 ZIP 完整性测试均通过。

## 安装包校验

- `CardBeautify-v0.5.4-Mobile-v103.zip`
  - SHA-256：`92F8F3A540A580E0E573AB7DCAA947CE6E6EE9CEA4687989AE58B9B3F1C053CF`
- `CardBeautify-v0.5.4-PC-v107.1.zip`
  - SHA-256：`4858D36912165FD8FE4DA09E3F23527937F02CB94558875DF9664AFFDE10B199`
