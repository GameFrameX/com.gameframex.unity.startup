# GameFrameX Startup

> Unity 通用啟動流程腳手架。封裝從遊戲啟動到熱更加載的完整管線，支援 URL 主備 failover、YooAsset 補丁流程、可插拔 UI/熱更後端。

[English](README.md) | [简体中文](README.zh-CN.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

> 完整說明請參考 [English README](README.md) 與 [简体中文 README](README.zh-CN.md)。

## 快速開始

```csharp
var result = await StartupRunner.Run(options, uiHandler, hotfixLauncher);
```

## 授權

MIT
