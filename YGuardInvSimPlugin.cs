using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace YGuardInvSim;

/// <summary>
/// Persists InventorySimulator API settings across dedicated-server restarts.
/// /opt/instance cfg is ephemeral; custom-plugins (this DLL) is not.
/// </summary>
public class YGuardInvSimPlugin : BasePlugin
{
    public override string ModuleName => "YGuard InvSim";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "YGuard";
    public override string ModuleDescription =>
        "Sets invsim_url / ws flags on every boot";

    private const string InventoryUrl = "https://inventory.yguard.ir";
    private bool _appliedThisMap;

    public override void Load(bool hotReload)
    {
        Apply(log: true);
        AddTimer(3.0f, () => Apply(log: true));
        RegisterListener<Listeners.OnMapStart>(_ =>
        {
            _appliedThisMap = false;
            AddTimer(2.0f, () => Apply(log: true));
        });
        Logger.LogInformation("YGuardInvSim loaded — target invsim_url={Url}", InventoryUrl);
    }

    private void Apply(bool log)
    {
        try
        {
            Server.ExecuteCommand($"invsim_url \"{InventoryUrl}\"");
            Server.ExecuteCommand("invsim_ws_enabled 1");
            Server.ExecuteCommand("invsim_ws_immediately 1");
            if (log && !_appliedThisMap)
            {
                _appliedThisMap = true;
                Logger.LogInformation("YGuardInvSim applied invsim_url={Url}", InventoryUrl);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "YGuardInvSim Apply failed");
        }
    }
}
