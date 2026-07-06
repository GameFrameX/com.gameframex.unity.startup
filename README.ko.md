# GameFrameX Startup

> Unity 범용 게임 시작 흐름 스캐폴드. URL 주-백업 failover, YooAsset 패치 흐름, 플러그 가능한 UI/핫픽스 백엔드를 갖춘, 앱 시작부터 핫픽스 로드까지의 전체 파이프라인을 캡슐화합니다.

[**한국어**](README.ko.md) | [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md)

## 기능 특성

- **一行 엔트리**：`await StartupRunner.Run(options, uiHandler, hotfixLauncher)`
- **설정 주도**：`StartupOptions` ScriptableObject, 15개 필드 (리소스 모드, URL 목록, 스탠드얼론 토글, 핫픽스 엔트리, HTTP 파라미터, UI 리소스)
- **주-백업 failover**：`GlobalInfoUrls[]` 배열 + `MaxAttemptsPerUrl` 재시도 정책
- **UI 백엔드 무관**：`IStartupUIHandler` 인터페이스, FairyGUI / UGUI / 커스텀 UI 호환
- **핫픽스方案 무관**：`IHotfixLauncher` 인터페이스, HybridCLR / 다른 핫픽스方案 호환
- **이중 트랙 완료 알림**：`UniTask<StartupResult>` await + `StartupCompleted/FailedEventArgs` 이벤트 (두 경로 모두 트리거)
- **PlayMode 인식**：`EditorSimulateMode` / `OfflinePlayMode` / `HostPlayMode` / `WebPlayMode` 분기 처리
- **스탠드얼론 지원**: `SkipRemoteStartupRequests` 옵션은 모든 원격 시작 요청(글로벌 정보 / App 버전 / 에셋 패키지 버전)을 건너뜁니다. WebGL 스탠드얼론 또는 백엔드 없는 배포에 적합.
- **YooAsset 통합**：표준 패치 흐름 (초기화 → 정적 버전 → 매니페스트 → 다운로드 → 완료)
- **채널 SDK 호출 없음**：Channel/SubChannel 필드는 순수 데이터, SDK 의존 없음

## 설치

`Packages/manifest.json`에 추가：

```json
{
  "dependencies": {
    "com.gameframex.unity.startup": "1.1.0"
  },
  "scopedRegistries": [
    {
      "name": "GameFrameX",
      "url": "https://gameframex.upm.alianblank.uk",
      "scopes": ["com.gameframex"]
    }
  ]
}
```

`scopes`는 이 레지스트리를 통해 어떤 패키지를 해석할지 제어합니다. `com.gameframex`로 시작하는 패키지만 이 레지스트리에서 가져옵니다.

## 빠른 시작

### 1. 설정 에셋 생성

Unity Editor에서：`Create > GameFrameX > Startup Options`。설정：

- `GlobalInfoUrls`：글로벌 정보 API의 주-백업 URL 목록
- `GamePlayMode`：리소스 실행 모드. 시작 플로우가 리소스 로드 전에 Asset 컴포넌트로 동기화합니다
- `HotfixAssemblyName` / `HotfixEntryTypeName` / `HotfixEntryMethodName`：핫픽스 엔트리 포인트
- `PackageName` / `Channel` / `SubChannel`：HTTP 공용 파라미터
- `LauncherUIResName`：실행 UI 리소스 경로 (기본 `UI/UILauncher`)
- `SkipRemoteStartupRequests`: 모든 원격 시작 요청(글로벌 정보 / App 버전 / 에셋 패키지 버전)을 건너뜁니다. WebGL 스탠드얼론 또는 백엔드 없는 배포 시 활성화.

### 2. UI 핸들러 구현

```csharp
public class GameStartupUIHandler : IStartupUIHandler
{
    public UniTask StartAsync(string uiResName) { /* UI 로드 */ }
    public void SetTipText(string text) { /* 힌트 텍스트 업데이트 */ }
    public void SetProgress(float progress, string sizeInfo) { /* 프로그레스바 업데이트 */ }
    public void SetProgressUpdateFinish() { /* 완료 표시 */ }
    public void Dispose() { /* UI 닫기, 구독 해제 */ }
}
```

### 3. 핫픽스 런처 구현

```csharp
public class HybridClrHotfixLauncher : IHotfixLauncher
{
    public async UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
    {
        // options.HotfixAssemblyName으로 핫픽스 어셈블리 로드
        // options.HotfixEntryTypeName.options.HotfixEntryMethodName 호출
        return HotfixLaunchResult.Succeed();
    }
}
```

### 4. 실행

```csharp
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private StartupOptions _options;

    private async void Start()
    {
        var uiHandler = new GameStartupUIHandler();
        var hotfixLauncher = new HybridClrHotfixLauncher();

        var result = await StartupRunner.Run(_options, uiHandler, hotfixLauncher);

        if (result.Success)
        {
            // 실행 완료, 게임 준비됨
        }
        else
        {
            Debug.LogError($"실행 실패 at {result.FailedProcedureName}: {result.ErrorMessage}");
        }
    }
}
```

이벤트 구독을 통해 알림을 분리할 수도 있습니다：

```csharp
GameApp.Event.Subscribe(StartupCompletedEventArgs.EventId, OnStartupCompleted);
GameApp.Event.Subscribe(StartupFailedEventArgs.EventId, OnStartupFailed);
```

## API 레퍼런스

### 코어 타입

| 타입 | 설명 |
|-----|------|
| `StartupOptions` | ScriptableObject 설정 에셋 (15개 필드) |
| `StartupResult` | 반환값, `Success` / `FailedProcedureName` / `FailedUrl` / `ErrorMessage` 포함 |
| `HotfixLaunchResult` | 핫픽스 전용 결과, `Success` / `ErrorMessage` 포함 |
| `IStartupUIHandler` | UI 작업 인터페이스 (5개 메서드) |
| `IHotfixLauncher` | 핫픽스 실행 인터페이스 (1개 비동기 메서드) |
| `StartupCompletedEventArgs` | 성공 알림 이벤트 |
| `StartupFailedEventArgs` | 실패 알림 이벤트, 진단 필드 포함 |
| `StartupRunner` | 정적 엔트리 — `Run(options, uiHandler, hotfixLauncher)` |
| `UrlFailoverRunner` | URL 주-백업 순차 시도 헬퍼, 유계 재시도 지원 |
| `UrlAttemptResult` / `UrlFailoverResult` | URL 단일 시도 결과와 최종 failover 결과 값 구조체 |
| `StartupHttpParams` | HTTP 공용 파라미터 컨테이너, JSON 직렬화 지원 |

### FSM BlackBoard 키

패키지는 procedure FSM에 3개의 고정 키를 주입하여 크로스 상태 데이터 공유를 실현：

| 키 | 타입 | 내용 |
|-----|------|------|
| `__startup_options__` | `VarObject` | `StartupOptions` 인스턴스 |
| `__startup_ui_handler__` | `VarObject` | `IStartupUIHandler` 인스턴스 |
| `__startup_hotfix_launcher__` | `VarObject` | `IHotfixLauncher` 인스턴스 |

상수는 `GameFrameX.Startup.Runtime.Constants.BlackBoardKeys`에 배치。

## 의존

- `com.gameframex.unity`（GameApp 퍼사드, Utility, ReferencePool）
- `com.gameframex.unity.procedure`（ProcedureBase, IProcedureManager）
- `com.gameframex.unity.fsm`（IFsm<T>, IFsmManager）
- `com.gameframex.unity.event`（GameEventArgs, EventComponent.Fire）
- `com.gameframex.unity.cysharp.unitask`（UniTask, UniTaskCompletionSource）
- Unity 2019.4+

## 문서

- [전체 spec](https://gameframex.doc.alianblank.com)
- [변경 기록](CHANGELOG.md)
- [라이선스](LICENSE.md)

## 라이선스

세부 사항은 [LICENSE.md](LICENSE.md)를 참조하세요.
