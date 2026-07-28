# CardBeautify v0.5.3 Android v103 实机验证

- 设备：REDMI K80 Pro
- 游戏：Android v0.103.2
- MOD：CardBeautify v0.5.3
- 卡牌：偏差认知
- 卡图选择：AnimeDefectMinimal（保存状态 `biased_cognition = AnimeDefectMinimal`）

## 验证结果

1. 游戏完成启动，CardBeautify 初始化日志显示 `loaded v0.5.3`。
2. 百科大全可通过中文搜索找到“偏差认知”。
3. 替换卡图使用单卡专用的上部横向构图：摆锤、头发与面部均进入卡牌立绘窗口。
4. 卡图按钮仍只出现在百科大全卡牌网格，已保存的卡图选择未被重置。
5. 手机安装文件 SHA-256：
   `d183c166866834f19765863222985a7cebda790ed049a13fbd1e336c81809b2b`

## 实现

对 `biased_cognition` 的纵向替换纹理创建一次性缓存的 `AtlasTexture`：

- 横向裁剪比例：1.5:1
- 起始位置：原图高度的 18.9%
- `FilterClip = true`
- 其他卡牌继续使用原有的 `KeepAspectCovered`

