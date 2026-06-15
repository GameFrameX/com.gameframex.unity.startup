# GameFrameX Startup

> Unity 범용 게임 시작 흐름 스캐폴드. URL 주-백업 failover, YooAsset 패치 흐름, 플러그 가능한 UI/핫픽스 백엔드를 갖춘, 앱 시작부터 핫픽스 로드까지의 전체 파이프라인을 캡슐화합니다.

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md)

> 자세한 내용은 [English README](README.md) 및 [简体中文 README](README.zh-CN.md) 를 참조하세요.

## 빠른 시작

```csharp
var result = await StartupRunner.Run(options, uiHandler, hotfixLauncher);
```

## 라이선스

MIT
