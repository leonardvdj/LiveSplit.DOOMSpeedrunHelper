#define DEBUG

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;

namespace LiveSplit.DOOMSpeedrunHelper
{
    public class DOOMSpeedrunHelperComponent : LogicComponent
    {
        public override string ComponentName => "DOOM Speedrun Helper";
        private readonly DOOMSpeedrunHelperSettings settings = new DOOMSpeedrunHelperSettings();

        private int searchProcessCounter = int.MaxValue - 1;
        private Process gameProcess;
        private SignatureScanner scanner;
        private IntPtr moduleBaseAddress, timescaleAddress;
        private bool wasCutscenePlaying;

        public override void Dispose() { }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return settings.GetSettings(document);
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return settings;
        }

        public override void SetSettings(XmlNode node)
        {
            settings.SetSettings(node);
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            try
            {
                if (!settings.TimescaleEnabled) return;

                if (searchProcessCounter++ > 15)
                {
                    searchProcessCounter = 0;
                    var process = Process.GetProcessesByName("DOOMx64vk").FirstOrDefault();
                    if (process != null && process.Id != gameProcess?.Id)
                    {
                        var module = process.Modules.Cast<ProcessModule>().FirstOrDefault(m => m.ModuleName == "DOOMx64vk.exe");
                        moduleBaseAddress = module.BaseAddress;
                        scanner = new SignatureScanner(process, moduleBaseAddress, module.ModuleMemorySize);

                        timescaleAddress = scanner.Scan(new SigScanTarget(0, 0x01, 0x51, 0x0C, 0x48, 0x8B, 0xD9));
                        timescaleAddress += 9;
                        timescaleAddress += process.ReadValue<int>(timescaleAddress) + 4;

                        gameProcess = process;

                        Debug.WriteLine("[DOOM Speedrun Helper] Found DOOM process");
                        Debug.WriteLine($"[DOOM Speedrun Helper] Module base address: 0x{moduleBaseAddress.ToInt64():X}");
                        Debug.WriteLine($"[DOOM Speedrun Helper] PID: {gameProcess.Id}");
                        Debug.WriteLine($"[DOOM Speedrun Helper] Timescale address: 0x{timescaleAddress.ToInt64():X}");
                    }
                }

                if (gameProcess != null)
                {
                    var map = gameProcess.ReadString(moduleBaseAddress + 0x66E13B5, 19);

                    if (map != "game/sp/intro/intro") return;

                    var position = gameProcess.ReadValue<Vector3f>(moduleBaseAddress + 0x5BB6058);
                    var camX = gameProcess.ReadValue<float>(moduleBaseAddress + 0x360F6B0);

                    var isCutscenePlaying = Math.Abs(position.X - -17904.0) < 5.0
                        && Math.Abs(position.Y - -2805.0) < 5.0
                        && Math.Abs(position.Z - 3089.9287) < 5.0
                        && camX > -17950.0;

                    var timeAddr = gameProcess.ReadValue<IntPtr>(timescaleAddress) + 0x60;
                    var timescale = gameProcess.ReadValue<float>(timeAddr);

                    if (isCutscenePlaying)
                    {
                        if (timescale != settings.TimescaleSpeed)
                        {
                            Debug.WriteLine($"[DOOM Speedrun Helper] Setting timescale to {settings.TimescaleSpeed}");
                            gameProcess.WriteValue<float>(timeAddr, settings.TimescaleSpeed);
                        }
                    }
                    else
                    {
                        if (wasCutscenePlaying && timescale != 1.0)
                        {
                            Debug.WriteLine("[DOOM Speedrun Helper] Returning timescale to 1");
                            gameProcess.WriteValue(timeAddr, 1.0f);
                        }
                    }
                    wasCutscenePlaying = isCutscenePlaying;
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException) return;
                Debug.WriteLine($"[DOOM Speedrun Helper] Error: {ex.Message}");
            }
        }
    }
}
