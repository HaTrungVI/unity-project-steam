using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SteamCore.Editor
{
    public class SteamCoreIntroWindow : EditorWindow
    {
        private const string Version = "1.0.0";
        private const string PrefKeyTab = "SteamCore_IntroTab";
        private const string SessionKeyShown = "SteamCore_IntroShown";

        private int _selectedTab;
        private Vector2 _scrollPos;

        private static readonly string[] TabNames = { "Overview", "Setup Checklist", "How to Use" };

        private static readonly Color HeaderColor = new(0.18f, 0.53f, 0.87f);
        private static readonly Color SectionBg = new(0.22f, 0.22f, 0.22f);

        private GUIStyle _headerStyle;
        private GUIStyle _sectionTitleStyle;
        private GUIStyle _codeStyle;
        private GUIStyle _richLabel;
        private bool _stylesInitialized;

        [MenuItem("SteamCore/Getting Started", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<SteamCoreIntroWindow>("SteamCore");
            window.minSize = new Vector2(580, 500);
        }

        [InitializeOnLoadMethod]
        private static void AutoOpenOnFirstSession()
        {
            if (SessionState.GetBool(SessionKeyShown, false)) return;
            SessionState.SetBool(SessionKeyShown, true);
            EditorApplication.delayCall += ShowWindow;
        }

        private void OnEnable()
        {
            _selectedTab = EditorPrefs.GetInt(PrefKeyTab, 0);
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = HeaderColor },
                margin = new RectOffset(0, 0, 10, 5)
            };

            _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.9f, 0.75f, 0.3f) },
                margin = new RectOffset(0, 0, 10, 4)
            };

            _codeStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 11,
                richText = true,
                padding = new RectOffset(8, 8, 6, 6),
                wordWrap = true
            };

            _richLabel = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                wordWrap = true
            };

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            GUILayout.Label("SteamCore Framework", _headerStyle);
            GUILayout.Label($"v{Version}  |  Co-op Horror Base for Steam", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(4);

            EditorGUI.BeginChangeCheck();
            _selectedTab = GUILayout.Toolbar(_selectedTab, TabNames, GUILayout.Height(28));
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetInt(PrefKeyTab, _selectedTab);

            GUILayout.Space(6);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_selectedTab)
            {
                case 0: DrawOverviewTab(); break;
                case 1: DrawSetupTab(); break;
                case 2: DrawHowToUseTab(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        // ───────────────────────── TAB 0: OVERVIEW ─────────────────────────

        private void DrawOverviewTab()
        {
            Section("About", () =>
            {
                Label("SteamCore is a <b>co-op horror game framework</b> for PC/Steam, built on Unity 6.\n" +
                      "It provides Steam integration, Mirror networking, and a bootstrap pipeline\n" +
                      "so you can focus on gameplay instead of infrastructure.");
            });

            Section("Tech Stack", () =>
            {
                StackRow("Mirror", "Client-server networking (host mode, 2-8 players)");
                StackRow("Steamworks.NET", "Steam SDK: auth, lobby, achievements, cloud, voice");
                StackRow("FizzySteamworks", "Mirror transport via Steam P2P relay");
                StackRow("UniTask", "Async/Await (replaces Coroutines)");
                StackRow("DOTween Pro", "Animation/tweening");
                StackRow("Addressables", "Asset loading & scene management");
            });

            Section("Architecture: Offline -> Online", () =>
            {
                Code(
                    "Layer 1: Pure Gameplay Logic (no Mirror dependency)\n" +
                    "  MonoBehaviour / C# class\n" +
                    "  Uses EventBus, DataModule, PoolManager\n" +
                    "  Works entirely offline, testable standalone\n\n" +
                    "Layer 2: Network Wrapper (adds sync when Mirror active)\n" +
                    "  NetworkBehaviour on same GameObject\n" +
                    "  SyncVar / Command / RPC delegate to Layer 1\n" +
                    "  Only active when Mirror is started");
            });

            Section("Assembly Structure", () =>
            {
                Code(
                    "_SteamCore/\n" +
                    "  Runtime/\n" +
                    "    Steam/        SteamCore.Steam.asmdef\n" +
                    "    Networking/   SteamCore.Networking.asmdef\n" +
                    "    GameFlow/     SteamCore.GameFlow.asmdef\n" +
                    "  Editor/         SteamCore.Editor.asmdef");
            });

            Section("Bootstrap Pipeline", () =>
            {
                Code(
                    "1. InitializeServicesTask   (AssetLoader, PoolManager, SceneLoader)\n" +
                    "2. InitializeSteamTask       (SteamManager singleton init)\n" +
                    "3. InitializeDataTask        (DataRegistry + Steam Cloud adapter)\n" +
                    "4. InitializeNetworkTask     (NetworkGameManager + ObjectPool)\n" +
                    "5. PreloadAssetsTask         (Addressable preload assets)\n" +
                    "6. LoadSceneTask             (Menu scene, additive)");
            });
        }

        // ───────────────────────── TAB 1: SETUP CHECKLIST ─────────────────────────

        private void DrawSetupTab()
        {
            EditorGUILayout.HelpBox(
                "This checklist verifies your project is correctly set up.\nGreen = OK, Red = action needed.",
                MessageType.Info);
            GUILayout.Space(4);

            Section("1. Required Packages", () =>
            {
                CheckItem("Steamworks.NET", IsTypeAvailable("Steamworks.SteamAPI, com.rlabrecque.steamworks.net"));
                CheckItem("Mirror", IsTypeAvailable("Mirror.NetworkManager, Mirror"));
                CheckItem("FizzySteamworks", IsTypeAvailable("Mirror.FizzySteam.FizzySteamworks, FizzySteamworks"));
                CheckItem("UniTask", IsTypeAvailable("Cysharp.Threading.Tasks.UniTask, UniTask"));
                CheckItem("Addressables", IsTypeAvailable("UnityEngine.AddressableAssets.Addressables, Unity.Addressables"));
                CheckItem("DOTween", IsTypeAvailable("DG.Tweening.DOTween, DOTween"));
            });

            Section("2. Project Files", () =>
            {
                var appIdExists = File.Exists(Path.Combine(Application.dataPath, "..", "steam_appid.txt"));
                CheckItem("steam_appid.txt in project root", appIdExists);
                if (!appIdExists)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(24);
                    if (GUILayout.Button("Create steam_appid.txt (AppId 480)", GUILayout.Width(280)))
                    {
                        File.WriteAllText(
                            Path.Combine(Application.dataPath, "..", "steam_appid.txt"), "480");
                        AssetDatabase.Refresh();
                    }
                    GUILayout.EndHorizontal();
                }
            });

            Section("3. ScriptableObject Assets", () =>
            {
                SOCheckItem<SteamCore.Steam.SteamConfig>("SteamConfig", "SteamCore/Config/Steam Config");
                SOCheckItem<SteamCore.Networking.NetworkConfig>("NetworkConfig", "SteamCore/Config/Network Config");
                SOCheckItem<ProjectBase.GameFlow.GameFlowConfig>("GameFlowConfig", "ProjectBase/Game Flow Config");
            });

            Section("4. Scene Setup", () =>
            {
                Label("<b>Init Scene</b> should contain:");
                BulletList(new[]
                {
                    "GameBootstrapper (from base package)"
                });

                GUILayout.Space(4);
                Label("<b>Splash Scene</b> should contain:");
                BulletList(new[]
                {
                    "GameSplashController (refs: DataRegistry, NetworkGameManager, NetworkObjectPool)",
                    "SteamManager (MonoSingleton — DontDestroyOnLoad)",
                    "NetworkGameManager (transport auto-added in Awake)",
                    "SteamTransportSetup (optional, assign to NetworkGameManager for custom config)",
                    "NetworkObjectPool (register prefabs for Mirror spawn system)"
                });

                GUILayout.Space(4);
                EditorGUILayout.HelpBox(
                    "Transport: NetworkGameManager auto-detects transport in Awake().\n" +
                    "- If FizzySteamworks is imported -> uses Steam P2P relay\n" +
                    "- Otherwise -> falls back to KcpTransport (LAN/IP)\n" +
                    "You can also manually add the transport component on the same GameObject.",
                    MessageType.Info);
            });

            Section("5. DOTween", () =>
            {
                Label("Run <b>Tools > Demigiant > DOTween Utility Panel > Create ASMDEF</b>\n" +
                      "to generate DOTween assembly definitions for asmdef-based projects.");
            });
        }

        // ───────────────────────── TAB 2: HOW TO USE ─────────────────────────

        private void DrawHowToUseTab()
        {
            Section("A. Connection Flow", () =>
            {
                Code(
                    "App Start\n" +
                    "  -> Init scene (GameBootstrapper)\n" +
                    "  -> Splash scene (SteamManager + NetworkGameManager + GameSplashController)\n" +
                    "  -> Bootstrap tasks run (Steam, Data, Network, Preload)\n" +
                    "  -> Menu scene loaded\n\n" +
                    "Host Game:\n" +
                    "  SteamLobbyService.CreateLobbyAsync()\n" +
                    "  -> NetworkGameManager.StartHosting()\n\n" +
                    "Join Game:\n" +
                    "  SteamLobbyService.JoinLobbyAsync(lobbyId)\n" +
                    "  -> NetworkGameManager.JoinGame(hostSteamId)\n\n" +
                    "Play Offline:\n" +
                    "  SceneLoader.Instance.LoadSceneAsync(gameplayScene)");
            });

            Section("B. Steam Services", () =>
            {
                Label("Steam services are <b>plain C# classes</b> (not Singletons).\n" +
                      "They are created by <b>InitializeNetworkTask</b> during bootstrap\n" +
                      "and injected into <b>NetworkGameManager</b>.\n");

                Code(
                    "// Check Steam is available before using services\n" +
                    "if (SteamManager.Instance.IsInitialized)\n" +
                    "{\n" +
                    "    var auth = new SteamAuthService();\n" +
                    "    var lobby = new SteamLobbyService(SteamManager.Instance.Config);\n" +
                    "    var friends = new SteamFriendsService();\n" +
                    "    var achievements = new SteamAchievementService();\n" +
                    "}\n\n" +
                    "// Lobby example\n" +
                    "ulong lobbyId = await lobby.CreateLobbyAsync(2, 4, ct);\n" +
                    "lobby.SetLobbyData(LobbyDataKeys.MapName, \"asylum\");\n" +
                    "lobby.OpenInviteOverlay();\n\n" +
                    "// Achievements\n" +
                    "achievements.UnlockAchievement(SteamAchievementIds.FirstSurvival);\n" +
                    "achievements.SetStat(SteamStatIds.GamesPlayed, 1);");
            });

            Section("C. Networking (Offline -> Online Pattern)", () =>
            {
                Label("Every gameplay system follows the <b>2-layer pattern</b>:");

                Code(
                    "// LAYER 1 — SteamCore.Gameplay (NO Mirror dependency)\n" +
                    "public class HealthSystem : MonoBehaviour\n" +
                    "{\n" +
                    "    private int _currentHealth;\n" +
                    "    public event Action<int, int> OnHealthChanged;\n\n" +
                    "    public void TakeDamage(int amount)\n" +
                    "    {\n" +
                    "        var old = _currentHealth;\n" +
                    "        _currentHealth = Mathf.Max(0, _currentHealth - amount);\n" +
                    "        OnHealthChanged?.Invoke(old, _currentHealth);\n" +
                    "    }\n\n" +
                    "    public void SetHealthDirect(int value) { ... }\n" +
                    "}\n\n" +
                    "// LAYER 2 — SteamCore.Networking (#if MIRROR)\n" +
                    "#if MIRROR\n" +
                    "public class NetworkHealthSync : NetworkBehaviour\n" +
                    "{\n" +
                    "    [SerializeField] private HealthSystem _health;\n\n" +
                    "    [SyncVar(hook = nameof(OnNetHealthChanged))]\n" +
                    "    private int _syncedHealth;\n\n" +
                    "    [Server]\n" +
                    "    void OnLocalHealthChanged(int old, int val)\n" +
                    "        => _syncedHealth = val;\n\n" +
                    "    void OnNetHealthChanged(int old, int val)\n" +
                    "    {\n" +
                    "        if (!isServer) _health.SetHealthDirect(val);\n" +
                    "    }\n\n" +
                    "    [Command]\n" +
                    "    public void CmdRequestDamage(int amount)\n" +
                    "        => _health.TakeDamage(amount);\n" +
                    "}\n" +
                    "#endif");
            });

            Section("D. Player & Registry", () =>
            {
                Code(
                    "// Access players via NetworkPlayerRegistry (NEVER FindObjectOfType)\n" +
                    "var local = NetworkPlayerRegistry.LocalPlayer;\n\n" +
                    "var players = new List<NetworkPlayerController>();\n" +
                    "NetworkPlayerRegistry.GetAll(players);\n\n" +
                    "if (NetworkPlayerRegistry.TryGet(netId, out var player))\n" +
                    "    Debug.Log(player.DisplayName);");
            });

            Section("E. Data Persistence", () =>
            {
                Code(
                    "// Create a DataModule SO for your save data\n" +
                    "[CreateAssetMenu(menuName = \"SteamCore/Data/Player Progress\")]\n" +
                    "public class PlayerProgressModule : DataModule<PlayerProgressData>\n" +
                    "{\n" +
                    "    public int Level => Data.level;\n\n" +
                    "    public void AddExperience(int amount)\n" +
                    "    {\n" +
                    "        Data.experience += amount;\n" +
                    "        MarkDirty(); // auto-saved by DataAutoSaver\n" +
                    "    }\n" +
                    "}\n\n" +
                    "// Steam Cloud is auto-configured in bootstrap\n" +
                    "// via SteamCloudSyncAdapter (IDataSyncAdapter).\n" +
                    "// Just use MarkDirty() — saving + cloud sync is handled.");
            });

            Section("F. Events (Local Only)", () =>
            {
                Code(
                    "// EventBus for LOCAL cross-system communication\n" +
                    "// (NOT for cross-network — use Mirror RPC/SyncVar for that)\n\n" +
                    "// Subscribe\n" +
                    "EventBus.Subscribe<LobbyCreatedEvent>(OnLobbyCreated);\n\n" +
                    "// Publish\n" +
                    "EventBus.Publish(new LobbyCreatedEvent { LobbyId = id });\n\n" +
                    "// ALWAYS unsubscribe in OnDestroy\n" +
                    "EventBus.Unsubscribe<LobbyCreatedEvent>(OnLobbyCreated);");
            });

            Section("G. Key Rules", () =>
            {
                BulletList(new[]
                {
                    "ALWAYS wrap Mirror code in <b>#if MIRROR</b>",
                    "ALWAYS wrap Steamworks code in <b>#if STEAMWORKS_NET</b>",
                    "ALWAYS use <b>UniTask</b> (not Coroutine), pass CancellationToken",
                    "ALWAYS use <b>[SerializeField] private</b> (not public fields)",
                    "NEVER use EventBus for cross-network events",
                    "NEVER use FindObjectOfType — use <b>NetworkPlayerRegistry</b>",
                    "NEVER modify SyncVar on client — only server sets them",
                    "NEVER use DateTime.UtcNow — use <b>TimeServiceRef.TrustedUtcNow</b>",
                    "DOTween: use <b>DOTween.To()</b> (not extension methods)"
                });
            });
        }

        // ───────────────────────── HELPERS ─────────────────────────

        private void Section(string title, Action content)
        {
            GUILayout.Label(title, _sectionTitleStyle);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            content();
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private void Label(string richText)
        {
            GUILayout.Label(richText, _richLabel);
        }

        private void Code(string code)
        {
            GUILayout.Label(code, _codeStyle);
        }

        private void StackRow(string name, string description)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{name}</b>", _richLabel, GUILayout.Width(140));
            GUILayout.Label(description, _richLabel);
            GUILayout.EndHorizontal();
        }

        private void CheckItem(string label, bool ok)
        {
            GUILayout.BeginHorizontal();
            var icon = ok ? "✓" : "✗";
            var color = ok ? "green" : "red";
            GUILayout.Label($"<color={color}><b>{icon}</b></color>  {label}", _richLabel);
            GUILayout.EndHorizontal();
        }

        private void BulletList(string[] items)
        {
            foreach (var item in items)
                GUILayout.Label($"  • {item}", _richLabel);
        }

        private void SOCheckItem<T>(string label, string menuPath) where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var found = guids.Length > 0;
            CheckItem(label, found);

            if (!found)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(24);
                if (GUILayout.Button($"Create {label}", GUILayout.Width(200)))
                {
                    var asset = CreateInstance<T>();
                    var dir = "Assets/_Config/SteamCore";
                    if (!AssetDatabase.IsValidFolder(dir))
                    {
                        if (!AssetDatabase.IsValidFolder("Assets/_Config"))
                            AssetDatabase.CreateFolder("Assets", "_Config");
                        AssetDatabase.CreateFolder("Assets/_Config", "SteamCore");
                    }
                    AssetDatabase.CreateAsset(asset, $"{dir}/{label}.asset");
                    AssetDatabase.SaveAssets();
                    EditorGUIUtility.PingObject(asset);
                }
                GUILayout.EndHorizontal();
            }
        }

        private static bool IsTypeAvailable(string assemblyQualifiedName)
        {
            return Type.GetType(assemblyQualifiedName) != null;
        }
    }
}
