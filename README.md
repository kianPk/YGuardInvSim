# YGuard InvSim

CounterStrikeSharp helper for YGuard dedicated servers.

On every boot (and map start) it sets:

```
invsim_url "https://inventory.yguard.ir"
invsim_require_inventory 1
invsim_ws_enabled 1
invsim_ws_immediately 1
invsim_spraychanger_enabled 1
```

If `/opt/5stack/custom-plugins/invsim_apikey` exists (one line), it also sets
`invsim_apikey`.

Needed because `/opt/instance` cfg is wiped on pod restart; `custom-plugins` is not.

## Install (game node)

```bash
cd /tmp
rm -rf yguardinvsim YGuardInvSim-1.1.0.zip
wget -O YGuardInvSim-1.1.0.zip \
  "https://github.com/kianPk/YGuardInvSim/releases/download/v1.1.0/YGuardInvSim-1.1.0.zip"
unzip -o YGuardInvSim-1.1.0.zip -d yguardinvsim

mkdir -p /opt/5stack/custom-plugins/addons/counterstrikesharp/plugins/YGuardInvSim
cp -f yguardinvsim/YGuardInvSim/YGuardInvSim.dll \
  /opt/5stack/custom-plugins/addons/counterstrikesharp/plugins/YGuardInvSim/

# Required for equipped skins to paint at spawn:
# (Inventory Admin â†’ Server key â†’ inv_â€¦)
echo 'inv_YOUR_KEY_HERE' > /opt/5stack/custom-plugins/invsim_apikey
chmod 600 /opt/5stack/custom-plugins/invsim_apikey
```

Restart Public/Custom dedicated servers. In RCON after boot:

```
invsim_url
invsim_require_inventory
```

Should print `https://inventory.yguard.ir` and `1`.
