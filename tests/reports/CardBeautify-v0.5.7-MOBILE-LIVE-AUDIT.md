# CardBeautify v0.5.7 Android v110.1 实机验证

- 设备：REDMI K80 Pro
- 游戏：Slay the Spire 2 v0.110.1
- 模组：CardBeautify v0.5.7

## 结果

- PASS：手机安装 DLL SHA-256 与 v110.1 构建产物一致。
- PASS：游戏正常启动并进入百科大全。
- PASS：冰之长枪 Anime 卡图的焦点裁剪在网格与详情页一致。
- PASS：冰之长枪的人物头部和冰枪主体未被上沿裁掉。
- PASS：偏差认知详情页保留摆锤、头发和面部，未回退到几何中心切片。
- PASS：测试后恢复 `ice_lance: Diana`，逐卡选择存储未丢失。
