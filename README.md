<div align="center">

<img width="740" height="310" alt="image" src="https://user-images.githubusercontent.com/6818236/115637633-a0d9cd80-a2de-11eb-89f8-48373c34d740.png" />

<i>安装并自定义你最喜欢的《军团要塞2》自定义 HUD。</i>

[![发布](https://img.shields.io/github/v/release/CriticalFlaw/TF2HUD.Editor)](https://github.com/CriticalFlaw/TF2HUD.Editor/releases/latest)
[![构建](https://github.com/CriticalFlaw/TF2HUD.Editor/actions/workflows/build.yml/badge.svg)](https://github.com/CriticalFlaw/TF2HUD.Editor/actions/workflows/build.yml)
[![文档](https://img.shields.io/badge/-文档-512BD4?logo=readme&logoColor=white)](https://criticalflaw.ca/TF2HUD.Editor)
[![Discord](https://img.shields.io/badge/-Discord-5865F2?logo=discord&logoColor=white)](https://discord.gg/hTdtK9vBhE)
[![GameBanana](https://img.shields.io/badge/-GameBanana-F1E133)](https://gamebanana.com/mods/7219)

</div>

---

## 安装说明

1. 下载并解压[最新版本](https://github.com/CriticalFlaw/TF2HUD.Editor/releases/latest)。
2. 以管理员身份启动 **HUDEditor.exe**。

需要帮助、报告 Bug 或提出问题？
* [阅读项目文档](https://criticalflaw.ca/TF2HUD.Editor/)
* [在问题跟踪器上提交工单](https://github.com/CriticalFlaw/TF2HUD.Editor/issues)
* [加入 Discord 服务器](https://discord.gg/hTdtK9vBhE)

---

## 开发说明

**应用（从仓库根目录运行）：**
```bash
dotnet build src/HUDEditor/HUDEditor.csproj
dotnet publish -c Release -r <rid> --self-contained true -p:PublishSingleFile=true
dotnet watch run --project src/HUDEditor/HUDEditor.csproj
```

**文档（从仓库根目录运行）：**
```bash
pnpm install
pnpm build-docs
pnpm start-docs
```

---

## 项目结构

- `src/HUDEditor/` — Avalonia .NET 应用（目标框架：net10.0）
- 根目录 `package.json` / `docusaurus.config.js` — 仅用于文档站

---

## CI/CD

- `build.yml` — 构建 Debug 版本（win-x64 和 linux-x64）
- `package.yml` — 发布 Release 单文件可执行程序
- `docs.yml` — 构建并部署 Docusaurus 文档站到 GitHub Pages

---

## 致谢

* [CriticalFlaw](https://github.com/CriticalFlaw) 和 [Revan](https://github.com/cooolbros) — 代码、设计、文档
* [mastercoms](https://github.com/mastercoms) — 透明视图模型扩展
* [Zeesastrous](https://github.com/Zeesastrous) — 项目横幅
* [hypnotize](https://github.com/Hypnootize) — 准星和图标
* [StrataSource](https://github.com/StrataSource) — vtex2 工具

#### 本地化贡献

* [Blueberryy](https://github.com/Blueberryy) — 俄语
* [tacokete](https://github.com/tacokete) — 法语
* [KayaDLX](https://github.com/KayaDLX) — 法语
* [Tiagonix](https://github.com/Tiagonix) — 巴西葡萄牙语
* [thejaviertc](https://github.com/thejaviertc) — 西班牙语
* [SignorUpB](https://github.com/SignorUpB) — 意大利语
* [HotoCocoaco](https://github.com/HotoCocoaco) — 简体中文

<a href="https://github.com/criticalflaw/TF2HUD.Editor/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=criticalflaw/TF2HUD.Editor" />
</a>
