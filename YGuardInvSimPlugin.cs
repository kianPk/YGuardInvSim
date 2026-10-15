using System;
using System.IO;
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
    public override string ModuleVersion => "1.1.2";
    public override string ModuleAuthor => "YGuard";
    public override string ModuleDescription =>
        "Sets invsim_url / require_inventory / ws flags on every boot";

    private const string InventoryUrl = "https://inventory.yguard.ir";
    private bool _appliedThisMap;
    private bool _loggedKeyOk;

    public override void Load(bool hotReload)
    {
        Apply(log: true);
        // Key file is sometimes merged a moment after CSS starts â€” retry.
        foreach (var delay in new[] { 1.0f, 3.0f, 8.0f, 15.0f, 30.0f })
        {
            var d = delay;
            AddTimer(d, () => Apply(log: true));
        }
        RegisterListener<Listeners.OnMapStart>(_ =>
        {
            _appliedThisMap = false;
            AddTimer(2.0f, () => Apply(log: true));
            AddTimer(8.0f, () => Apply(log: false));
        });
        Logger.LogInformation("YGuardInvSim loaded â€” target invsim_url={Url}", InventoryUrl);
    }

    private void Apply(bool log)
    {
        try
        {
            Server.ExecuteCommand($"invsim_url \"{InventoryUrl}\"");
            Server.ExecuteCommand("invsim_require_inventory 1");
            Server.ExecuteCommand("invsim_ws_enabled 1");
            Server.ExecuteCommand("invsim_ws_immediately 1");
            Server.ExecuteCommand("invsim_spraychanger_enabled 1");

            var apiKey = ReadApiKey();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                Server.ExecuteCommand($"invsim_apikey \"{apiKey}\"");
                if (!_loggedKeyOk)
                {
                    _loggedKeyOk = true;
                    Logger.LogInformation(
                        "YGuardInvSim applied invsim_url={Url} apikey=true",
                        InventoryUrl
                    );
                }
            }
            else if (log && !_appliedThisMap)
            {
                Logger.LogWarning(
                    "YGuardInvSim applied invsim_url={Url} apikey=false (key file not found yet)",
                    InventoryUrl
                );
            }

            if (log)
            {
                _appliedThisMap = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "YGuardInvSim Apply failed");
        }
    }

    private string? ReadApiKey()
    {
        foreach (var path in new[]
        {
            Path.Combine(ModuleDirectory, "invsim_apikey"),
            "/opt/instance/game/csgo/addons/counterstrikesharp/plugins/YGuardInvSim/invsim_apikey",
            "/opt/5stack/custom-plugins/invsim_apikey",
        })
        {
            try
            {
                if (!File.Exists(path))
                {
                    continue;
                }
                var key = File.ReadAllText(path).Trim();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    return key;
                }
            }
            catch
            {
                // try next
            }
        }
        return null;
    }
}
