# CardBeautify / 卡面美化

《Slay the Spire 2 / 杀戮尖塔2》卡牌立绘替换 MOD，提供适合手机触控的百科大全卡面切换按钮，并持久保存每张卡上次选择的卡面。

## 最新版本

- [v0.5.4](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.5.4)：再次检查“偏差认知”后确认，v0.5.3 把本来已经正确的 600×848 竖版原图先裁成 600×400 横图，随后卡牌控件又以 `KeepAspectCovered` 填充，形成二次放大与裁切。v0.5.4 移除这层单卡横向裁剪，直接使用原始竖版构图。PC v107.1 已在百科大全缩略图和卡牌详情中实机验证：摆锤、完整头发、面部、披肩与上半身均正常进入画面，日志确认加载 `v0.5.4`。

- [v0.5.3](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.5.3)：修复“偏差认知”替换卡图在横向卡牌立绘窗口中被居中裁掉头顶的问题。现在仅对这张卡使用带缓存的上部横向构图裁剪，保留摆锤、头发和面部；不会改变其他卡图，也不会增加每帧重复裁剪开销。Android v103 已在百科大全中实机验证。

- [v0.5.2](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.5.2)：修复先打开百科大全、再进入战斗后卡牌上仍显示“卡图”按钮的问题。每个按钮现在带有自身作用域守卫：卡牌节点一旦离开当前百科大全的精确网格，首帧就先隐藏并关闭触控，再释放按钮，不再依赖较慢的全局轮询清理。REDMI K80 Pro 实测百科内按钮正常显示，返回主菜单并继续战斗后按钮完全消失；离线检查 12/12 通过。

- [v0.5.1](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.5.1)：百科大全卡牌详情弹窗打开时隐藏卡图按钮，并保持每张卡的卡面选择持久化。

- [v0.5.0](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.5.0)：修复打开百科大全后，卡面按钮随复用卡牌节点泄漏到战斗、商店、牌组和牌堆界面的问题。按钮现在只允许出现在当前场景中实际拥有的百科卡牌网格内；离开百科时先同步隐藏再释放。卡图替换本身仍在所有界面生效，已保存选择不会丢失。

## 安装

下载 Release 中带平台标记的安装包，解压后将 `CardBeautify` 文件夹放入游戏 `mods/` 目录。Android v103 使用 `CardBeautify-v0.5.4-Mobile-v103.zip`，PC v107.1 使用 `CardBeautify-v0.5.4-PC-v107.1.zip`。

## 资源来源

资源原名和来源见 [`docs/README-CardBeautify-资源来源.md`](docs/README-CardBeautify-资源来源.md)。仓库只保存代码和说明，大体积资源放在 Release 安装包中。

## 历史版本

- [v0.4.9](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.4.9)
- [v0.4.8](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.4.8)
- [v0.4.7](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.4.7)
- [v0.4.6](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.4.6)
- [v0.4.2-Mobile-v103](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.4.2-Mobile-v103)
