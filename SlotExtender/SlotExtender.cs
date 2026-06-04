using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using Common;
using SlotExtender.Configuration;
using BepInEx;
using BepInEx.Logging;
using System.IO;

namespace SlotExtender
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInProcess("Subnautica.exe")]
    internal class SlotExtender : BaseUnityPlugin
    {
        private const string GUID = "com.senna.slotextender";
        private const string MODNAME = "SlotExtender";
        private const string VERSION = "2.7.2";

        internal ManualLogSource BepinLogger;
        internal SlotExtender mInstance;
        internal Harmony hInstance;

        public static SEOptions Options => SEOptions.Instance;

        internal void Awake()
        {
            mInstance = this;
            BepinLogger = BepInEx.Logging.Logger.CreateLogSource(MODNAME);
            BepinLogger.LogInfo("Awake");

            SEInput.Register();
            SEConfig.Load();
            SlotHelper.InitSlotIDs();
            SlotHelper.ExpandSlotMapping();

            hInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);

            SNLogger.Debug($"Harmony Instance created, Name = [{hInstance.Id}]");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "XMenu")
            {
                // enabling game console
                PlatformUtils.SetDevToolsEnabled(true);

                // init config
                SEConfig.Init();

                // add an action if changed keybindings
                GameInput.OnBindingsChanged -= Main.GameInput_OnBindingsChanged;
                GameInput.OnBindingsChanged += Main.GameInput_OnBindingsChanged;

                SlotHelper.InitSessionAllSlots();
            }
            if (scene.name == "Main")
            {
                // creating a console input field listener to skip SlotExdender Update method key events conflict while console is active in game
                Main.ListenerInstance = InitializeListener();
            }
        }

        internal static InputFieldListener InitializeListener()
        {
            if (Main.ListenerInstance == null)
            {
                Main.ListenerInstance = UnityEngine.Object.FindObjectOfType(typeof(InputFieldListener)) as InputFieldListener;

                if (Main.ListenerInstance == null)
                {
                    GameObject inputFieldListener = new GameObject("InputFieldListener");
                    Main.ListenerInstance = inputFieldListener.AddComponent<InputFieldListener>();
                }
            }

            return Main.ListenerInstance;
        }
    }

    internal static class Main
    {
        internal static CommandRoot commandRoot = null;
        internal static InputFieldListener ListenerInstance;

        internal static bool isConsoleActive;
        internal static bool isKeyBindigsUpdate = false;

        internal static readonly string modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static Sprite atlasSpriteExosuitArm = null;

        internal static void GameInput_OnBindingsChanged()
        {
            // SlotExtender Update() method now disabled until all keybinding updates are complete
            isKeyBindigsUpdate = true;

            // updating slot key bindings
            SEConfig.SLOTKEYBINDINGS_Update();

            // updating ALLSLOTS dictionary
            SlotHelper.ALLSLOTS_Update();

            // updating SlotTextHandler
            if (uGUI_SlotTextHandler.Instance != null)
            {
                uGUI_SlotTextHandler.Instance.UpdateSlotText();
            }

            // SlotExtender Update() method now enabled
            isKeyBindigsUpdate = false;
        }
    }
}

