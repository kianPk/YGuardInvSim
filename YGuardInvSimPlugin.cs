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
    public override string ModuleVersion => "1.1.1";
    public override string ModuleAuthor => "YGuard";
    public override string ModuleDescription =>
        "Sets invsim_url / require_inventory / ws flags on every boot";

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
        Logger.LogInformation("YGuardInvSim loaded â€” target invsim_url={Url}", InventoryUrl);
    }

    private void Apply(bool log)
    {
        try
        {
            Server.ExecuteCommand($"invsim_url \"{InventoryUrl}\"");
            // Required so GiveNamedItem detour paints weapons at spawn time.
            Server.ExecuteCommand("invsim_require_inventory 1");
            Server.ExecuteCommand("invsim_ws_enabled 1");
            Server.ExecuteCommand("invsim_ws_immediately 1");
            Server.ExecuteCommand("invsim_spraychanger_enabled 1");

            var apiKey = ReadApiKey();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                Server.ExecuteCommand($"invsim_apikey \"{apiKey}\"");
            }

            if (log && !_appliedThisMap)
            {
                _appliedThisMap = true;
                Logger.LogInformation(
                    "YGuardInvSim applied invsim_url={Url} apikey={HasKey}",
                    InventoryUrl,
                    !string.IsNullOrWhiteSpace(apiKey)
                );
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "YGuardInvSim Apply failed");
        }
    }

    /// <summary>
    /// Dedicated pods merge custom-plugins/addons into /opt/instance, but do NOT
    /// expose /opt/5stack/custom-plugins/invsim_apikey. Prefer the key next to
    /// this DLL (gets copied into the instance), then fall back to the host path.
    /// </summary>
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
