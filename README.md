# SteamCore

Co-op horror game framework for PC/Steam, built on **Unity 6** (URP).

Provides Steam integration, Mirror networking, and a bootstrap pipeline so you can focus on gameplay instead of infrastructure.

## Features

- **Steam Integration** (Steamworks.NET) — Auth, Lobby, Achievements, Cloud Save, Voice, Rich Presence
- **Mirror Networking** — Host mode (2-8 players), Steam P2P relay via FizzySteamworks
- **Bootstrap Pipeline** — Automated init sequence: Steam > Data > Network > Preload > Menu
- **Offline-First Architecture** — All systems compile and run without Steam/Mirror packages installed
- **Data Persistence** — DataModule bridge to Steam Cloud via `SteamCloudSyncAdapter`

---

## Dependencies

### Dependency Graph

```
com.gmx.top.steamcore (this package)
├── com.gmx.top.core          [REQUIRED]  Base framework
│   ├── com.unity.addressables             Asset management (auto-resolved)
│   └── com.unity.ugui                     Unity UI (auto-resolved)
├── com.cysharp.unitask        [REQUIRED]  Async/Await
├── com.rlabrecque.steamworks.net  [OPTIONAL]  Steamworks.NET
├── com.mirror-networking.mirror   [OPTIONAL]  Mirror Networking
└── com.mirror.steamworks.net      [OPTIONAL]  FizzySteamworks Transport
```

### com.gmx.top.core — Base Framework (REQUIRED)

| Package ID | `com.gmx.top.core` |
|---|---|
| Source | [unity-project-base](https://github.com/HaTrungVI/unity-project-base) |
| Install | Local path hoac git URL (xem Installation) |

`com.gmx.top.core` la base framework cung cap cac he thong nen tang ma SteamCore xay dung tren do. **KHONG duoc modify** package nay.

**Cac module SteamCore su dung tu com.gmx.top.core:**

| Module | Assembly | SteamCore su dung cho |
|---|---|---|
| **Common** | `ProjectBase.Common` | `MonoSingleton<T>` (SteamManager), `EventBus` (local events), `PoolManager` (object pooling), SO Architecture (`SOVariable`, `SOEventChannel`, `SOReference`), `StateMachine` |
| **DataSystem** | `ProjectBase.Data` | `DataModule<T>` (SO-based persistence), `DataRegistry` (module registry), `IDataSyncAdapter` (interface ma `SteamCloudSyncAdapter` implement de bridge Steam Cloud), `DataAutoSaver` (auto flush dirty modules) |
| **GameFlow** | `ProjectBase.GameFlow` | `GameBootstrapper` (Init scene entry point), `BaseSplashController` (splash screen base class ma `GameSplashController` ke thua), `IBootstrapTask` (interface cho bootstrap pipeline tasks), `GameFlowConfig` SO |
| **AssetSystem** | `ProjectBase.Asset` | `AssetLoader` (Addressables wrapper), `SceneLoader` (scene management) |
| **UISystem** | `ProjectBase.UI` | `UIManager` (screen/popup/overlay management), `BaseScreen`/`BasePopup`/`BaseOverlay` (UI base classes), MVP pattern (`[PresenterType]` attribute) |
| **TimeSystem** | `ProjectBase.Time` | `TimeManager`, `TimerService`, `ScheduleService`, `TimeServiceRef` SO (trusted time) |
| **AudioSystem** | `ProjectBase.Audio` | `AudioManager` (audio playback), `AudioConfig` SO |

**Asmdef references tu SteamCore:**

```
SteamCore.Steam.asmdef       → ProjectBase.Common, ProjectBase.Data, ProjectBase.GameFlow
SteamCore.Networking.asmdef  → ProjectBase.Common, ProjectBase.Asset, ProjectBase.GameFlow
SteamCore.GameFlow.asmdef    → ProjectBase.Common, ProjectBase.Data, ProjectBase.GameFlow, ProjectBase.Asset
SteamCore.Editor.asmdef      → ProjectBase.Common, ProjectBase.GameFlow, ProjectBase.Data
```

### UniTask — Async/Await (REQUIRED)

| Package ID | `com.cysharp.unitask` |
|---|---|
| Source | [github.com/Cysharp/UniTask](https://github.com/Cysharp/UniTask) |
| Git URL | `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask` |

Thay the Coroutine trong toan bo project. Tat ca async methods trong SteamCore deu return `UniTask` va nhan `CancellationToken`.

**Su dung trong SteamCore:**
- `SteamAuthService.GetAuthSessionTicketAsync(ct)` → `UniTask<AuthTicketResult>`
- `SteamLobbyService.CreateLobbyAsync(type, max, ct)` → `UniTask<ulong>`
- `SteamLobbyService.JoinLobbyAsync(lobbyId, ct)` → `UniTask<bool>`
- `SteamLobbyService.SearchLobbiesAsync(filter, ct)` → `UniTask<List<LobbyInfo>>`
- `SteamCloudSyncAdapter.LoadFromServerAsync(moduleId, ct)` / `SaveToServerAsync(moduleId, data, ct)`
- `NetworkSceneFlow.TransitionToGameplayAsync(ct)` / `ReturnToMenuAsync(ct)`
- Tat ca `IBootstrapTask.ExecuteAsync(ct)` trong bootstrap pipeline

### Steamworks.NET — Steam SDK (OPTIONAL)

| Package ID | `com.rlabrecque.steamworks.net` |
|---|---|
| Source | [github.com/rlabrecque/Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) |
| Git URL | `https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net` |
| Scripting Define | `STEAMWORKS_NET` (auto-detect qua `versionDefines` trong asmdef) |

1:1 C# wrapper cho Steamworks C++ API. Khi package duoc import, define `STEAMWORKS_NET` tu dong active va tat ca Steam code compile.

**Khi KHONG import:** Code fallback ve offline mode, `SteamManager.IsInitialized = false`, game chay binh thuong khong co Steam features.

**Su dung trong SteamCore:**
- `SteamManager` — `SteamAPI.Init()`, `SteamAPI.RunCallbacks()`, `SteamAPI.Shutdown()`
- `SteamAuthService` — `SteamUser.GetAuthSessionTicket()`, `SteamUser.BeginAuthSession()`
- `SteamLobbyService` — `SteamMatchmaking.CreateLobby()`, `SteamMatchmaking.JoinLobby()`
- `SteamFriendsService` — `SteamFriends.SetRichPresence()`, `SteamFriends.ActivateGameOverlay()`
- `SteamAchievementService` — `SteamUserStats.SetAchievement()`, `SteamUserStats.StoreStats()`
- `SteamCloudSyncAdapter` — `SteamRemoteStorage.FileRead()`, `SteamRemoteStorage.FileWrite()`
- `SteamVoiceService` — `SteamUser.StartVoiceRecording()`, `SteamUser.GetVoice()`

### Mirror — Networking Framework (OPTIONAL)

| Package ID | `com.mirror-networking.mirror` |
|---|---|
| Source | [github.com/MirrorNetworking/Mirror](https://github.com/MirrorNetworking/Mirror) |
| Scripting Define | `MIRROR` (auto-detect qua `versionDefines` trong asmdef) |

Client-server networking framework cho Unity. SteamCore dung Mirror cho host mode (1 player la server + client, 2-8 players total).

**Khi KHONG import:** Networking code bi skip (#if MIRROR), game chi chay offline mode.

**Su dung trong SteamCore:**
- `NetworkGameManager` extends `NetworkManager` — connection lifecycle, Steam auth validation
- `NetworkPlayerController` extends `NetworkBehaviour` — `[SyncVar]` steamId/name/ready, `[Command]`/`[ClientRpc]`
- `NetworkPlayerRegistry` — static registry, track tat ca `NetworkPlayerController` instances
- `NetworkObjectPool` — register `SpawnHandler`/`UnspawnHandler` voi Mirror
- `NetworkLatencyHelper` — `NetworkTime.time`, RTT calculation
- `NetworkSceneFlow` — `NetworkManager.ServerChangeScene()` cho online transitions
- `NetworkConnectionTracker` — map `NetworkConnection.connectionId` ↔ `CSteamID`
- Network messages: `AuthTicketMessage`, `ChatMessage`, `VoiceDataMessage`, `PlayerReadyMessage`, `KickPlayerMessage`

### FizzySteamworks — Steam P2P Transport (OPTIONAL)

| Package ID | `com.mirror.steamworks.net` |
|---|---|
| Source | [github.com/Chykary/FizzySteamworks](https://github.com/Chykary/FizzySteamworks) |
| Git URL | `https://github.com/Chykary/FizzySteamworks.git?path=/com.mirror.steamworks.net` |
| Scripting Define | `FIZZY_STEAMWORKS` (auto-detect qua `versionDefines` trong asmdef) |
| Requires | Steamworks.NET + Mirror |

Transport layer cho Mirror su dung Steam P2P relay. Players ket noi qua Steam network — **KHONG can port forwarding**.

**Khi KHONG import:** `NetworkGameManager` tu dong fallback ve `KcpTransport` (LAN/IP direct connect).

**Su dung trong SteamCore:**
- `SteamTransportSetup` — configure FizzySteamworks transport options
- `NetworkGameManager.Awake()` — auto-detect: co FizzySteamworks → dung Steam relay, khong co → dung KCP

### Addressables — Asset Management (REQUIRED, tu com.gmx.top.core)

| Package ID | `com.unity.addressables` |
|---|---|
| Version | 2.8.1+ |
| Installed by | Auto-resolved qua dependency cua `com.gmx.top.core` |

Unity Addressables system cho asset loading va scene management. **KHONG can install thu cong** — tu dong resolve khi install `com.gmx.top.core`.

**Su dung trong SteamCore:**
- `NetworkSceneFlow` — load scenes qua `SceneLoader` (Addressables wrapper tu base package)
- `GameSplashController` — `PreloadAssetsTask` load assets truoc khi vao Menu
- Asmdef reference: `SteamCore.Networking` va `SteamCore.GameFlow` reference `Unity.Addressables`

### DOTween Pro (OPTIONAL, khong reference trong asmdef)

| Source | [dotween.demigiant.com/pro.php](http://dotween.demigiant.com/pro.php) |
|---|---|
| Install | Import tu Asset Store hoac DLL |

SteamCore KHONG truc tiep reference DOTween trong asmdef. Nhung game project su dung SteamCore co the dung DOTween cho UI animations.

**Luu y:** Trong asmdef projects, chi dung `DOTween.To()` static method, **KHONG** dung extension methods (can `overrideReferences` + `precompiledReferences` trong asmdef).

### Scripting Defines Summary

| Define | Auto-detect | Package | Khi active |
|---|---|---|---|
| `STEAMWORKS_NET` | `versionDefines` trong asmdef | `com.rlabrecque.steamworks.net` | Steamworks.NET da import |
| `MIRROR` | `versionDefines` trong asmdef | `com.mirror-networking.mirror` | Mirror da import |
| `FIZZY_STEAMWORKS` | `versionDefines` trong asmdef | `com.mirror.steamworks.net` | FizzySteamworks da import |

Khi define **KHONG** active → code tu dong fallback ve offline mode, **KHONG** compile error.

---

## Installation

### 1. Install com.gmx.top.core (base framework)

**Option A — Local path** (recommended for development):

Clone `unity-project-base` cung cap voi project, sau do them vao `Packages/manifest.json`:

```json
"com.gmx.top.core": "file:../../unity-project-base"
```

**Option B — Git URL:**

```json
"com.gmx.top.core": "https://github.com/HaTrungVI/unity-project-base.git"
```

### 2. Install UniTask

Them vao `Packages/manifest.json`:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
```

### 3. Install SteamCore

**Option A — Via Unity Package Manager:**

1. Open **Window > Package Manager**
2. Click **+ > Add package from git URL...**
3. Enter: `https://github.com/HaTrungVI/unity-project-steam.git`

**Option B — Via manifest.json:**

```json
"com.gmx.top.steamcore": "https://github.com/HaTrungVI/unity-project-steam.git"
```

**Option C — Local development:**

```bash
cd YourUnityProject/Packages
git clone https://github.com/HaTrungVI/unity-project-steam.git com.gmx.top.steamcore
```

### 4. Install Optional Packages (for online features)

Them vao `Packages/manifest.json`:

```json
"com.rlabrecque.steamworks.net": "https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net",
"com.mirror.steamworks.net": "https://github.com/Chykary/FizzySteamworks.git?path=/com.mirror.steamworks.net"
```

Mirror install qua Asset Store hoac UPM (xem [Mirror docs](https://mirror-networking.gitbook.io/docs/)).

### Full manifest.json Example

```json
{
    "dependencies": {
        "com.gmx.top.core": "file:../../unity-project-base",
        "com.gmx.top.steamcore": "https://github.com/HaTrungVI/unity-project-steam.git",
        "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
        "com.rlabrecque.steamworks.net": "https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net",
        "com.mirror.steamworks.net": "https://github.com/Chykary/FizzySteamworks.git?path=/com.mirror.steamworks.net"
    }
}
```

---

## Architecture

### Offline > Online Pattern

Every gameplay system follows a 2-layer architecture:

```
Layer 1: Pure Gameplay Logic (no Mirror dependency)
  - MonoBehaviour / C# class
  - Uses EventBus, DataModule, PoolManager (from com.gmx.top.core)
  - Works entirely offline, testable standalone

Layer 2: Network Wrapper (adds sync when Mirror active)
  - NetworkBehaviour on same GameObject
  - SyncVar / Command / RPC delegate to Layer 1
  - Only active when Mirror is started
```

### Assembly Structure

```
Runtime/
  Steam/        SteamCore.Steam.asmdef        → ProjectBase.Common, ProjectBase.Data, ProjectBase.GameFlow, UniTask
  Networking/   SteamCore.Networking.asmdef    → ProjectBase.Common, ProjectBase.Asset, ProjectBase.GameFlow, SteamCore.Steam, UniTask, Mirror*, Steamworks.NET*, FizzySteamworks*
  GameFlow/     SteamCore.GameFlow.asmdef      → ProjectBase.Common/Data/GameFlow/Asset, SteamCore.Steam, SteamCore.Networking, UniTask
Editor/         SteamCore.Editor.asmdef        → SteamCore.Steam/Networking/GameFlow, ProjectBase.Common/GameFlow/Data, UniTask

(* = optional, auto-detected via versionDefines)
```

### Bootstrap Pipeline

```
Init Scene (GameBootstrapper from com.gmx.top.core)
  → Splash Scene (GameSplashController)
    1. InitializeServicesTask   → AssetLoader, PoolManager, SceneLoader (com.gmx.top.core)
    2. InitializeSteamTask      → SteamManager singleton init
    3. InitializeDataTask       → DataRegistry + SteamCloudSyncAdapter (bridge DataModule ↔ Steam Cloud)
    4. InitializeNetworkTask    → NetworkGameManager + NetworkObjectPool
    5. PreloadAssetsTask        → Addressable preload (via AssetLoader)
    6. LoadSceneTask            → Menu scene (via SceneLoader)
```

---

## Quick Start

### 1. Setup

After installing, open **SteamCore > Getting Started** from the Unity menu bar. The setup wizard checks your project configuration.

### 2. Create Config Assets

Create the required ScriptableObject assets (the setup wizard can do this for you):

- **SteamConfig** — `Create > SteamCore/Config/Steam Config`
- **NetworkConfig** — `Create > SteamCore/Config/Network Config`
- **GameFlowConfig** — `Create > ProjectBase/Game Flow Config` (from com.gmx.top.core)

### 3. Scene Setup

**Init Scene:**
- Add `GameBootstrapper` (from com.gmx.top.core)

**Splash Scene:**
- Add `GameSplashController` (refs: DataRegistry, NetworkGameManager, NetworkObjectPool)
- Add `SteamManager` (MonoSingleton, DontDestroyOnLoad)
- Add `NetworkGameManager` (transport auto-detected in Awake)
- Add `NetworkObjectPool` (register prefabs for Mirror spawn system)

### 4. steam_appid.txt

Create `steam_appid.txt` in your project root with your Steam App ID (use `480` for testing with Spacewar).

### 5. Connection Flow

```
Host Game:
  SteamLobbyService.CreateLobbyAsync() → NetworkGameManager.StartHosting()

Join Game:
  SteamLobbyService.JoinLobbyAsync(lobbyId) → NetworkGameManager.JoinGame(hostSteamId)

Play Offline:
  SceneLoader.Instance.LoadSceneAsync(gameplayScene)
```

---

## Steam Services

Steam services are plain C# classes (not Singletons). Create them after checking `SteamManager.Instance.IsInitialized`:

| Service | Interface | Purpose |
|---|---|---|
| `SteamAuthService` | `ISteamAuth` | Session tickets, auth validation |
| `SteamLobbyService` | `ISteamLobby`, `ISteamLobbySearch` | Create/Join/Leave/Search lobbies, invite overlay |
| `SteamFriendsService` | `ISteamFriends` | Rich presence, friends list, game overlay |
| `SteamAchievementService` | `ISteamAchievements` | Achievements + stats with batched StoreStats |
| `SteamCloudSyncAdapter` | `ISteamCloudService`, `IDataSyncAdapter` | DataModule ↔ Steam Cloud bridge |
| `SteamVoiceService` | `ISteamVoice` | Voice recording/decompression |

```csharp
if (SteamManager.Instance.IsInitialized)
{
    var auth = new SteamAuthService();
    var lobby = new SteamLobbyService(SteamManager.Instance.Config);
    var friends = new SteamFriendsService();
    var achievements = new SteamAchievementService();
}
```

---

## Networking

| Component | Purpose |
|---|---|
| `NetworkGameManager` | Custom NetworkManager with Steam auth |
| `NetworkPlayerController` | Base player: SyncVar steamId/name/ready |
| `NetworkPlayerRegistry` | Static registry for all players |
| `NetworkObjectPool` | PoolManager (com.gmx.top.core) ↔ Mirror spawn bridge |
| `NetworkLatencyHelper` | ServerTime, RTT, interpolation |
| `NetworkSceneFlow` | Mirror ServerChangeScene vs SceneLoader (com.gmx.top.core) transitions |
| `NetworkConnectionTracker` | SteamId ↔ ConnectionId mapping |

---

## Key Rules

- **ALWAYS** wrap Mirror code in `#if MIRROR`
- **ALWAYS** wrap Steamworks code in `#if STEAMWORKS_NET`
- **ALWAYS** use UniTask (not Coroutine), pass CancellationToken
- **NEVER** use EventBus for cross-network events (use Mirror RPC/SyncVar)
- **NEVER** use `FindObjectOfType` (use `NetworkPlayerRegistry`)
- **NEVER** modify SyncVar on client (only server sets them)
- **DO NOT** modify `com.gmx.top.core` base package
- Use `DOTween.To()` (not extension methods) in asmdef projects

---

## License

MIT License. See [LICENSE](LICENSE) for details.
