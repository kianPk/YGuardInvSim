# YGuard InvSim

CounterStrikeSharp helper for YGuard dedicated servers.

On every boot (and map start) it sets:

```
invsim_url "https://inventory.yguard.ir"
invsim_ws_enabled 1
invsim_ws_immediately 1
```

Needed because `/opt/instance` cfg is wiped on pod restart; `custom-plugins` is not.

## Install (game node)

```bash
cd /tmp
rm -rf yguardinvsim YGuardInvSim-1.0.0.zip
wget -O YGuardInvSim-1.0.0.zip \
  "https://github.com/kianPk/YGuardInvSim/releases/download/v1.0.0/YGuardInvSim-1.0.0.zip"
unzip -o YGuardInvSim-1.0.0.zip -d yguardinvsim

mkdir -p /opt/5stack/custom-plugins/addons/counterstrikesharp/plugins/YGuardInvSim
cp -f yguardinvsim/YGuardInvSim/YGuardInvSim.dll \
  /opt/5stack/custom-plugins/addons/counterstrikesharp/plugins/YGuardInvSim/
```

Restart Public/Custom dedicated servers. In RCON after boot (no manual set needed):

```
invsim_url
```

Should print `https://inventory.yguard.ir`.
