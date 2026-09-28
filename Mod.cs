using FFT_enhanced.UI.unique_icons.Configuration;
using FFT_enhanced.UI.unique_icons.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using FF16Framework.Interfaces.Nex;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using System.Runtime.CompilerServices;
using FF16Framework.Interfaces.Nex.Structures;

#if DEBUG

#endif

namespace FFT_enhanced.UI.unique_icons;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    /// <summary>
    /// job command table handling
    /// </summary>
    private INexTable? jobCommandTable;
    private INextExcelDBApiManagedV2? nextExcelDBApi;

    private void NextExcelDBApi_OnNexLoaded()
    {
        if (nextExcelDBApi.Initialized)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Configuring job command NEX table");
            jobCommandTable = nextExcelDBApi.GetTable(0x29);
        }
        
    }
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;
        _logger.WriteLine($"[{_modConfig.ModId}] Loading... Author Talcall");


#if DEBUG
        // Attaches debugger in debug mode; ignored in release.
        //Debugger.Launch();
#endif

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.

        // TODO: Implement some mod logic
        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out var startupScanner))
        {
            return;
        }
        var _nexApi = _modLoader.GetController<INextExcelDBApiManagedV2>();
        if (!_nexApi.TryGetTarget(out nextExcelDBApi))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Could not get INextExcelDBApi.");
            return;
        }
        nextExcelDBApi.OnNexLoaded += NextExcelDBApi_OnNexLoaded;


        //hook scan
        startupScanner.AddMainModuleScan("40 53 48 83 EC ?? 48 8B D9 8B CA E8 ?? ?? ?? ?? 48 85 C0 74 ?? 8B 50 ?? 44 8B C2 41 83 E8 ?? 74 ?? 41 83 F8 ?? 74 ?? 8D 42 ?? 83 F8 ?? 76 ?? 33 D2 41 B8 ?? ?? ?? ?? 48 8B CB 48 83 C4 ?? 5B E9 ?? ?? ?? ?? BA ?? ?? ?? ?? 41 B8 ?? ?? ?? ?? 48 8B CB 48 83 C4 ?? 5B E9", result => // 0x1400FBF9C in Original release of FFTIC
        {
            if (!result.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] sub_1400FAE34 could not be found");
                return;
            }
            _logger.WriteLine($"[{_modConfig.ModId}] sub_1400FAE34 found");
            var sub_1400FAE34_address = Process.GetCurrentProcess().MainModule.BaseAddress + result.Offset;

            sub_1400FAE34_Hook = _hooks!.CreateHook<sub_1400FAE34>(sub_1400FAE34_Replacement, sub_1400FAE34_address).Activate();

            if (!sub_1400FAE34_Hook.IsHookEnabled)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] sub_1400FAE34 could not be hooked");
                return;
            }
            _logger.WriteLine($"[{_modConfig.ModId}] sub_1400FAE34 hooked successfully");
        });

        // j_fGetJobCommandIconPath scan
        startupScanner.AddMainModuleScan("40 53 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 49 63 F8 8B DA 48 8B F1 48 8D 54 24 ?? 41 B0 ?? B9 10 ?? ?? ?? 41 B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B D7 4C 8D 44 24 ?? 44 8B CB 48 8B CE E8 ?? ?? ?? ?? 48 8B 8C 24 ?? ?? ?? ?? 48 33 CC E8 ?? ?? ?? ?? 48 81 C4 ?? ?? ?? ?? 5F 5E 5B C3", result => // j_fGetJobCommandIconPath in Original release of FFTIC
        {
            if (!result.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] j_fGetJobCommandIconPath could not be found");
                return;
            }
            var j_fGetJobCommandIconPath_address = Process.GetCurrentProcess().MainModule.BaseAddress + result.Offset;
            j_fGetJobCommandIconPath_func = _hooks!.CreateWrapper<j_fGetJobCommandIconPath>(j_fGetJobCommandIconPath_address, out var _);
            _logger.WriteLine($"[{_modConfig.ModId}] j_fGetJobCommandIconPath found");
        });

    }


    [Function(CallingConventions.MicrosoftThiscall)]
    private delegate nint sub_1400FAE34(nint a1, uint a2);

    [Function(CallingConventions.MicrosoftThiscall)]
    private delegate nint j_fGetJobCommandIconPath(nint a1, nint a2, nint a3);
    private j_fGetJobCommandIconPath j_fGetJobCommandIconPath_func;

    private static IHook<sub_1400FAE34> sub_1400FAE34_Hook;
    //private static IFunction<j_fNex_GetJobCommand> j_fNex_GetJobCommand_func;
    //private static IFunction<j_fGetJobCommandIconPath> j_fGetJobCommandIconPath_func;

    private unsafe nint sub_1400FAE34_Replacement(nint a1, uint a2)
    {
        //_logger.WriteLine($"[{_modConfig.ModId}] sub_1400FAE34()", Color.LightBlue);
        int icon_id;
        INexRow? row = jobCommandTable.GetRow(a2);
        if (row == null)
        {
            icon_id = 0;
        }
        else
        {
            _logger.WriteLine($"getting icon from NXD: [{row}]", Color.LightBlue);
            icon_id = (row.GetInt32(4));
        }
        if (40 >= icon_id && icon_id >= 28)
        {
            _logger.WriteLine($"Getting modded Icon [{icon_id}]", Color.LightBlue);
            return j_fGetJobCommandIconPath_func(a1, icon_id, 256);
        }
        _logger.WriteLine($"Getting vanilla Icon [{icon_id}] from [{sub_1400FAE34_Hook.OriginalFunction}]", Color.LightBlue);
        return sub_1400FAE34_Hook.OriginalFunction(a1, a2);
    }



    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
};