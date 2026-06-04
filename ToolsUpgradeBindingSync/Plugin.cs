using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ToolsUpgradeBindingSync
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Subnautica.exe")]
    internal sealed class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.toolsupgradebindingsync";
        private const string PluginName = "Tools Upgrade Binding Sync";
        private const string PluginVersion = "1.0.0";

        private const string DefaultButtonIds =
            "OpenLCUpgrades;OpenRTUpgrades;OpenABUpgrades;OpenSeaglideUpgrades";

        private static readonly char[] ButtonSeparators = { ';', ',', '\n', '\r' };

        private ManualLogSource log;
        private ConfigEntry<string> buttonIdsConfig;
        private ConfigEntry<string> canonicalButtonIdConfig;
        private ConfigEntry<string> primaryKeyboardBindingConfig;
        private ConfigEntry<string> secondaryKeyboardBindingConfig;
        private ConfigEntry<float> retryDurationConfig;

        private Coroutine syncCoroutine;
        private bool isSynchronizing;
        private string lastPrimaryBinding;
        private string lastSecondaryBinding;

        private void Awake()
        {
            log = Logger;

            buttonIdsConfig = Config.Bind(
                "Bindings",
                "ButtonIds",
                DefaultButtonIds,
                "ToolsUpgradesLIB GameInput button IDs to keep synchronized. Separate values with semicolons.");

            canonicalButtonIdConfig = Config.Bind(
                "Bindings",
                "CanonicalButtonId",
                "OpenLCUpgrades",
                "Button ID used as the initial source binding when all configured buttons are present.");

            primaryKeyboardBindingConfig = Config.Bind(
                "Bindings",
                "PrimaryKeyboardBindingOverride",
                string.Empty,
                "Optional explicit primary keyboard binding string, for example <Keyboard>/u. Leave empty to copy/adopt from the synced buttons.");

            secondaryKeyboardBindingConfig = Config.Bind(
                "Bindings",
                "SecondaryKeyboardBindingOverride",
                string.Empty,
                "Optional explicit secondary keyboard binding string. Leave empty to copy/adopt from the synced buttons.");

            retryDurationConfig = Config.Bind(
                "Startup",
                "RetryDurationSeconds",
                30f,
                "How long to wait at startup for other mods to register their GameInput button IDs.");

            SceneManager.sceneLoaded += OnSceneLoaded;
            GameInput.OnBindingsChanged += OnBindingsChanged;

            log.LogInfo("Loaded");
        }

        private void Start()
        {
            QueueSync("startup");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GameInput.OnBindingsChanged -= OnBindingsChanged;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            QueueSync("scene loaded: " + scene.name);
        }

        private void OnBindingsChanged()
        {
            if (isSynchronizing)
            {
                return;
            }

            QueueSync("bindings changed");
        }

        private void QueueSync(string reason)
        {
            if (syncCoroutine != null)
            {
                StopCoroutine(syncCoroutine);
            }

            syncCoroutine = StartCoroutine(SyncWhenReady(reason));
        }

        private IEnumerator SyncWhenReady(string reason)
        {
            float retryDuration = Math.Max(0f, retryDurationConfig.Value);
            float deadline = Time.realtimeSinceStartup + retryDuration;
            SyncResult result;

            do
            {
                result = TrySynchronize(reason, false);

                if (result.HasSynchronized && result.MissingButtonIds.Count == 0)
                {
                    syncCoroutine = null;
                    yield break;
                }

                yield return new WaitForSecondsRealtime(0.5f);
            }
            while (Time.realtimeSinceStartup < deadline);

            result = TrySynchronize(reason, true);

            if (!result.HasSynchronized)
            {
                log.LogWarning("No configured ToolsUpgradesLIB buttons were available to synchronize.");
            }

            syncCoroutine = null;
        }

        private SyncResult TrySynchronize(string reason, bool logMissingButtons)
        {
            string[] buttonIds = ParseButtonIds(buttonIdsConfig.Value);
            SyncResult result = new SyncResult();

            if (buttonIds.Length == 0)
            {
                log.LogWarning("No button IDs are configured.");
                result.HasSynchronized = true;
                return result;
            }

            List<ResolvedButton> buttons = ResolveButtons(buttonIds, result.MissingButtonIds);

            if (buttons.Count == 0)
            {
                return result;
            }

            BindingPair desired = GetDesiredBindings(buttons);

            isSynchronizing = true;

            try
            {
                int changedCount = 0;

                foreach (ResolvedButton button in buttons)
                {
                    changedCount += SetBindingIfDifferent(button.Id, button.Button, GameInput.BindingSet.Primary, desired.Primary);
                    changedCount += SetBindingIfDifferent(button.Id, button.Button, GameInput.BindingSet.Secondary, desired.Secondary);
                }

                if (changedCount > 0)
                {
                    GameInput.SetBindingsChanged();
                    log.LogInfo(
                        "Synchronized " + buttons.Count + " ToolsUpgradesLIB buttons from " + reason +
                        " using primary '" + desired.Primary + "' and secondary '" + desired.Secondary + "'.");
                }
            }
            finally
            {
                isSynchronizing = false;
            }

            lastPrimaryBinding = desired.Primary;
            lastSecondaryBinding = desired.Secondary;
            result.HasSynchronized = true;

            if (logMissingButtons && result.MissingButtonIds.Count > 0)
            {
                log.LogWarning("Some configured button IDs were not registered: " + string.Join(", ", result.MissingButtonIds.ToArray()));
            }

            return result;
        }

        private BindingPair GetDesiredBindings(List<ResolvedButton> buttons)
        {
            string primaryOverride = NormalizeBinding(primaryKeyboardBindingConfig.Value);
            string secondaryOverride = NormalizeBinding(secondaryKeyboardBindingConfig.Value);

            return new BindingPair
            {
                Primary = string.IsNullOrEmpty(primaryOverride)
                    ? ChooseBinding(buttons, GameInput.BindingSet.Primary, lastPrimaryBinding)
                    : primaryOverride,
                Secondary = string.IsNullOrEmpty(secondaryOverride)
                    ? ChooseBinding(buttons, GameInput.BindingSet.Secondary, lastSecondaryBinding)
                    : secondaryOverride
            };
        }

        private string ChooseBinding(List<ResolvedButton> buttons, GameInput.BindingSet bindingSet, string previousBinding)
        {
            if (previousBinding != null)
            {
                foreach (ResolvedButton button in buttons)
                {
                    string current = GetKeyboardBinding(button.Button, bindingSet);

                    if (!StringComparer.Ordinal.Equals(current, previousBinding))
                    {
                        return current;
                    }
                }

                return previousBinding;
            }

            GameInput.Button canonicalButton;

            if (TryResolveButton(canonicalButtonIdConfig.Value, out canonicalButton))
            {
                return GetKeyboardBinding(canonicalButton, bindingSet);
            }

            return GetKeyboardBinding(buttons[0].Button, bindingSet);
        }

        private int SetBindingIfDifferent(
            string buttonId,
            GameInput.Button button,
            GameInput.BindingSet bindingSet,
            string desiredBinding)
        {
            string currentBinding = GetKeyboardBinding(button, bindingSet);

            if (StringComparer.Ordinal.Equals(currentBinding, desiredBinding))
            {
                return 0;
            }

            GameInput.SetBinding(GameInput.Device.Keyboard, button, bindingSet, desiredBinding ?? string.Empty);
            log.LogDebug("Set " + buttonId + " " + bindingSet + " keyboard binding to '" + desiredBinding + "'.");
            return 1;
        }

        private static List<ResolvedButton> ResolveButtons(string[] buttonIds, List<string> missingButtonIds)
        {
            List<ResolvedButton> buttons = new List<ResolvedButton>();

            foreach (string buttonId in buttonIds)
            {
                GameInput.Button button;

                if (TryResolveButton(buttonId, out button))
                {
                    buttons.Add(new ResolvedButton(buttonId, button));
                }
                else
                {
                    missingButtonIds.Add(buttonId);
                }
            }

            return buttons;
        }

        private static bool TryResolveButton(string buttonId, out GameInput.Button button)
        {
            button = GameInput.Button.None;

            if (string.IsNullOrWhiteSpace(buttonId))
            {
                return false;
            }

            try
            {
                button = (GameInput.Button)Enum.Parse(typeof(GameInput.Button), buttonId.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetKeyboardBinding(GameInput.Button button, GameInput.BindingSet bindingSet)
        {
            return GameInput.GetBinding(GameInput.Device.Keyboard, button, bindingSet) ?? string.Empty;
        }

        private static string[] ParseButtonIds(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new string[0];
            }

            string[] rawIds = value.Split(ButtonSeparators, StringSplitOptions.RemoveEmptyEntries);
            List<string> buttonIds = new List<string>(rawIds.Length);
            HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (string rawId in rawIds)
            {
                string buttonId = rawId.Trim();

                if (buttonId.Length == 0 || !seenIds.Add(buttonId))
                {
                    continue;
                }

                buttonIds.Add(buttonId);
            }

            return buttonIds.ToArray();
        }

        private static string NormalizeBinding(string binding)
        {
            return string.IsNullOrWhiteSpace(binding) ? string.Empty : binding.Trim();
        }

        private struct BindingPair
        {
            internal string Primary;
            internal string Secondary;
        }

        private struct ResolvedButton
        {
            internal readonly string Id;
            internal readonly GameInput.Button Button;

            internal ResolvedButton(string id, GameInput.Button button)
            {
                Id = id;
                Button = button;
            }
        }

        private sealed class SyncResult
        {
            internal bool HasSynchronized;
            internal readonly List<string> MissingButtonIds = new List<string>();
        }
    }
}
