# Zbuy

zbuy is a weapon purchasing and management weapon's ammo/clip + some misc settings plugins.

This is a plugin mainly for the zombie escape mode.

# Installation
1. Download the plugin:
    * Download the plugin from https://github.com/possession-community/zbuy.
2. Install the plugin files:
    * Place the contents of the downloaded zip file in the **`addons/counterstrikesharp`** folder.
3. Configure the plugin settings:
    * For the first installation: You will need to change the names of the files in the **`addons/counterstrikesharp/configs/plugins/zbuy/`** folder. It should be called **`zbuy.toml`**. In the tomlyn file you can set the weapon datas.
4. Restart or install the plugin:
    * Restart your server or reload the plugin for the settings to take effect.
    * Send the command **`css_plugins load zbuy`** from the server ***(Load)***
    * Send the command **`css_plugins reload zbuy`** from the server ***(Reload)***

# Configuration

See `zbuy-example.toml` for configuration details.

## Configuration File Location
Place the plugin configuration file at **`addons/counterstrikesharp/configs/plugins/zbuy/zbuy.toml`**.

## Round Purchase Limits
You can set per-round purchase limits for weapons in the configuration file using the `MaxPurchasesPerRound` property:

```toml
[WeaponDatas.weapon_hegrenade]
EnableBuyCommand = true
BuyAliases = ["he", "hegrenade", "nade"]
MaxPurchasesPerRound = 3  # Players can buy max 3 HE grenades per round
```

This feature is useful for limiting grenade spam or controlling weapon economy in game modes like Zombie Escape.

# Buy Menu Integration

ZBuy automatically intercepts and overrides the default CS2 buy menu functionality. When players use the in-game buy menu (B key), the plugin:

1. **Blocks Default Purchases**: Prevents the default CS2 purchase system from processing transactions
2. **Automatic Refund**: Refunds any money deducted by the default system
3. **Weapon Removal**: Removes any weapons given by the default system
4. **Custom Processing**: Processes the purchase through ZBuy's custom system with all configured features

This ensures that all weapon purchases, whether through chat commands or the buy menu, use ZBuy's pricing, restrictions, and special features consistently.

**Note**: Players can still use both methods to purchase weapons:
- **Buy Menu (B key)**: Traditional in-game buy menu interface
- **Chat Commands**: `!ak47`, `!buy ak47`, etc.

Both methods will use ZBuy's configuration and pricing system.

# Available Commands

## Player Commands
- **Buy Commands**: Players can purchase weapons using the following formats:
  - `css_<weapon>` (e.g., `css_ak47`, `css_glock`, `css_awp`)
  - `css_buy <weapon>` (e.g., `css_buy ak47`, `css_buy glock`)
  -  Player can buy the any weapon on chat, like a `!usp`, `!buy usp`
  - Weapon aliases also work (e.g., `css_kalash` for AK47, `css_pistol` for Glock)

## Admin Commands
- `css_zb_enabled <0/1>` - Enable/disable the entire ZBuy system (requires admin permissions)
- `css_zb_restrict <weapon_name>` - Restrict purchasing of specified weapon (requires admin permissions)
- `css_zb_unrestrict <weapon_name>` - Remove purchase restriction from specified weapon (requires admin permissions)
- `css_zb_roundlimit <weapon_name> <limit>` - Set round purchase limit for specified weapon (0 = no limit) (requires admin permissions)
- `css_zb_resetround` - Reset all round purchase counts (requires admin permissions)

#### Available Weapons (without "weapon_" prefix):
**Assault Rifles:**
- `m4a1` - M4A4
- `m4a1_silencer` - M4A1-S
- `famas` - FAMAS
- `aug` - AUG
- `ak47` - AK-47
- `galilar` - Galil AR
- `sg556` - SG 553

**Sniper Rifles:**
- `scar20` - SCAR-20
- `awp` - AWP
- `ssg08` - SSG 08
- `g3sg1` - G3SG1

**SMGs:**
- `mp9` - MP9
- `mp7` - MP7
- `mp5sd` - MP5-SD
- `ump45` - UMP-45
- `p90` - P90
- `bizon` - PP-Bizon
- `mac10` - MAC-10

**Pistols:**
- `usp_silencer` - USP-S
- `hkp2000` - P2000
- `glock` - Glock-18
- `elite` - Dual Berettas
- `p250` - P250
- `fiveseven` - Five-SeveN
- `cz75a` - CZ75-Auto
- `tec9` - Tec-9
- `revolver` - R8 Revolver
- `deagle` - Desert Eagle

**Shotguns:**
- `nova` - Nova
- `xm1014` - XM1014
- `mag7` - MAG-7
- `sawedoff` - Sawed-Off

**Machine Guns:**
- `m249` - M249
- `negev` - Negev

**Equipment:**
- `taser` - Zeus x27
- `kevlar` - Kevlar

**Grenades:**
- `hegrenade` - HE Grenade
- `molotov` - Molotov Cocktail
- `incgrenade` - Incendiary Grenade
- `smokegrenade` - Smoke Grenade
- `flashbang` - Flashbang
- `decoy` - Decoy Grenade

# Special Thanks

- reference: [schwarper/cs2-advanced-weapon-system](https://github.com/schwarper/cs2-advanced-weapon-system)
