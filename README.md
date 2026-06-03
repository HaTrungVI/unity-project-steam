# SteamCore

Co-op horror game framework for PC/Steam, built on **Unity 6** (URP).

Provides Steam integration, Mirror networking, and a bootstrap pipeline so you can focus on gameplay instead of infrastructure.

## Features

- **Steam Integration** (Steamworks.NET) — Auth, Lobby, Achievements, Cloud Save, Voice, Rich Presence
- **Mirror Networking** — Host mode (2-8 players), Steam P2P relay via FizzySteamworks
- **Bootstrap Pipeline** — Automated init sequence: Steam > Data > Network > Preload > Menu
- **Offline-First Architecture** — All systems compile and run without Steam/Mirror packages installed
- **Data Persistence** — DataModule bridge to Steam Cloud via `SteamCloudSyncAdapter`

## Requirements

| Dependency | Required | Notes |
|---|---|---|
| [com.gmx.top.core](https://github.com/user/unity-project-base) | Yes | Base framework (UI, Data, GameFlow, Asset, Common) |
| [UniTask](https://github.com/Cysharp/UniTask) | Yes | Async/Await (replaces Coroutines) |
| [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) | Optional | Auto-detected via `STEAMWORKS_NET` define |
| [Mirror](https://github.com/MirrorNetworking/Mirror) | Optional | Auto-detected via `MIRROR` define |
| [FizzySteamworks](https://github.com/Chykary/FizzySteamworks) | Optional | Steam P2P transport for Mirror |
| [DOTween Pro](http://dotween.demigiant.com/pro.php) | Optional | Animation/tweening |
| [Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest) | Yes | Asset management |

> **Note:** SteamCore compiles without Mirror, Steamworks.NET, or FizzySteamworks. Code is guarded with `#if STEAMWORKS_NET`, `#if MIRROR`, and `#if FIZZY_STEAMWORKS` defines that activate automatically via `versionDefines` in assembly definitions.

## Installation

### Via Unity Package Manager (Git URL)

1. Open **Window > Package Manager**
2. Click **+ > Add package from git URL...**
3. Enter:
   ```
   https://github.com/user/unity-project-steam.git
   ```

### Via manifest.json

Add to `Packages/manifest.json`:

```json
{
    "dependencies": {
        "com.gmx.top.steamcore": "https://github.com/user/unity-project-steam.git"
    }
}
```

### Local Development

Clone to your `Packages/` folder:

```bash
cd YourUnityProject/Packages
git clone https://github.com/user/unity-project-steam.git com.gmx.top.steamcore
```

## Architecture

### Offline > Online Pattern

Every gameplay system follows a 2-layer architecture:

```
Layer 1: Pure Gameplay Logic (no Mirror dependency)
  - MonoBehaviour / C# class
  - Uses EventBus, DataModule, PoolManager
  - Works entirely offline, testable standalone

Layer 2: Network Wrapper (adds sync when Mirror active)
  - NetworkBehaviour on same GameObject
  - SyncVar / Command / RPC delegate to Layer 1
  - Only active when Mirror is started
```

### Assembly Structure

```
Runtime/
  Steam/        SteamCore.Steam.asmdef        - Steamworks.NET integration
  Networking/   SteamCore.Networking.asmdef    - Mirror networking
  GameFlow/     SteamCore.GameFlow.asmdef      - Bootstrap pipeline
Editor/         SteamCore.Editor.asmdef        - Editor tools & setup wizard
```

### Bootstrap Pipeline

```
1. InitializeServicesTask   (AssetLoader, PoolManager, SceneLoader)
2. InitializeSteamTask      (SteamManager singleton init)
3. InitializeDataTask       (DataRegistry + Steam Cloud adapter)
4. InitializeNetworkTask    (NetworkGameManager + ObjectPool)
5. PreloadAssetsTask        (Addressable preload assets)
6. LoadSceneTask            (Menu scene)
```

## Quick Start

### 1. Setup

After installing the package, open **SteamCore > Getting Started** from the Unity menu bar. The setup wizard checks your project configuration.

### 2. Create Config Assets

Create the required ScriptableObject assets (the setup wizard can do this for you):

- **SteamConfig** — `Create > SteamCore/Config/Steam Config`
- **NetworkConfig** — `Create > SteamCore/Config/Network Config`

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
  SteamLobbyService.CreateLobbyAsync() > NetworkGameManager.StartHosting()

Join Game:
  SteamLobbyService.JoinLobbyAsync(lobbyId) > NetworkGameManager.JoinGame(hostSteamId)

Play Offline:
  SceneLoader.Instance.LoadSceneAsync(gameplayScene)
```

## Steam Services

Steam services are plain C# classes (not Singletons). Create them after checking `SteamManager.Instance.IsInitialized`:

| Service | Interface | Purpose |
|---|---|---|
| `SteamAuthService` | `ISteamAuth` | Session tickets, auth validation |
| `SteamLobbyService` | `ISteamLobby` | Create/Join/Leave/Search lobbies |
| `SteamFriendsService` | `ISteamFriends` | Rich presence, friends list |
| `SteamAchievementService` | `ISteamAchievements` | Achievements + stats (batched) |
| `SteamCloudSyncAdapter` | `ISteamCloudService` | DataModule <> Steam Cloud bridge |
| `SteamVoiceService` | `ISteamVoice` | Voice recording/decompression |

## Networking

| Component | Purpose |
|---|---|
| `NetworkGameManager` | Custom NetworkManager with Steam auth |
| `NetworkPlayerController` | Base player: SyncVar steamId/name/ready |
| `NetworkPlayerRegistry` | Static registry for all players |
| `NetworkObjectPool` | PoolManager <> Mirror spawn bridge |
| `NetworkLatencyHelper` | ServerTime, RTT, interpolation |
| `NetworkSceneFlow` | Mirror vs SceneLoader transitions |
| `NetworkConnectionTracker` | SteamId <> ConnectionId mapping |

## Key Rules

- **ALWAYS** wrap Mirror code in `#if MIRROR`
- **ALWAYS** wrap Steamworks code in `#if STEAMWORKS_NET`
- **ALWAYS** use UniTask (not Coroutine), pass CancellationToken
- **NEVER** use EventBus for cross-network events (use Mirror RPC/SyncVar)
- **NEVER** use `FindObjectOfType` (use `NetworkPlayerRegistry`)
- **NEVER** modify SyncVar on client (only server sets them)
- Use `DOTween.To()` (not extension methods) in asmdef projects

## License

MIT License. See [LICENSE](LICENSE) for details.
