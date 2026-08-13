# 卡面美化（Card Beautify）

这是一个只改画面、不改数值的《杀戮尖塔 2》卡图替换 MOD。它给百科大全增加了适合触屏使用的“卡图”按钮，可以为同一张牌切换不同立绘；选择结果会按卡牌保存，之后在百科、详情页和局内卡牌上继续使用。

项目由 CapePisces、Yummn、Aduare、FlyingFish、Diana111 共同署名。卡图原名与来源整理在 [资源来源说明](docs/README-CardBeautify-资源来源.md) 中；仓库主要保存代码和文档，大体积图片放在 Release 安装包里。

## 当前版本

推荐使用 [v0.5.7](https://github.com/Yummn/sts2-card-beautify/releases/tag/v0.5.7)。这一版重新处理了“冰之长枪”和“偏差认知”的构图：替换图会先按卡框比例裁切并缓存，再由百科、详情和局内卡牌共同使用，避免同一张图在不同界面被重复裁切。

近几个版本也修复了按钮离开百科后残留、返回筛选页后消失，以及详情页黑边等问题。完整变更和实机记录保留在 `docs/`、`tests/reports/` 与 [GitHub Releases](https://github.com/Yummn/sts2-card-beautify/releases) 中。

## 安装

下载与游戏版本对应的安装包：

- `手机-v103`：Android v0.103.2
- `手机-v110.1`：Android v0.110.1
- `电脑-v107.1`：PC v0.107.1

解压后，把完整的 `CardBeautify` 文件夹放进游戏的 `mods` 目录。不同平台的 DLL 不要混用。

## 从源码构建

准备好对应版本的游戏程序集后，在 `src` 目录运行：

```powershell
dotnet build CardBeautify.csproj -c Release
```

可通过 `Sts2DataDir`、`Sts2Path` 和 `ModsPath` 等 MSBuild 属性指定本机游戏路径。离线界面检查位于 `tests/verify_card_beautify_scope.py`。
