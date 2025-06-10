# Zbuy

zbuy is a weapon purchasing and management plugin based on [schwarper/cs2-advanced-weapon-system](https://github.com/schwarper/cs2-advanced-weapon-system).

All AWS features are retained, with new features such as purchase commands, pricing, knockbacks, and ConVar.

This is a plugin mainly for the zombie escape mode, but since the buy command and knockback function can be turned off, it can also be used as AWS as is.

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

# Configulation

see `zbuy-example.toml`

# Available Commands

## Player Commands
- **Buy Commands**: Players can purchase weapons using the following formats:
  - `css_<weapon>` (e.g., `css_ak47`, `css_glock`, `css_awp`)
  - `css_buy <weapon>` (e.g., `css_buy ak47`, `css_buy glock`)
  -  Player can buy the any weapon on chat, like a `!usp`, `!buy usp`
  - Weapon aliases also work (e.g., `css_kalash` for AK47, `css_pistol` for Glock)

## Server ConVars
- `zb_enable_buy_commands [0|1]` - Enable/disable buy command system
- `zb_enable_knockback [0|1]` - Enable/disable knockback system

### Weapon-specific ConVars
For each weapon, the following ConVars are available (replace `<weapon>` with weapon name without "weapon_" prefix):

**⚠️ Important: ConVar values take priority over config file settings for most weapon properties. However, the config `EnableBuyCommand` setting has higher priority than the weapon-specific ConVar.**

- `zb_<weapon>_buy_enabled [0|1]` - Enable/disable weapon purchasing (only applies if config `EnableBuyCommand` is true)
- `zb_<weapon>_price <amount>` - Set weapon price (overrides config `Price`)
- `zb_<weapon>_price_scale <value>` - Set price scaling factor (overrides config `PriceScale`)
- `zb_<weapon>_knockback <scale>` - Set knockback multiplier (overrides config `KnockbackScale`)
- `zb_<weapon>_damage <modifier>` - Set damage modifier (overrides config `Damage`)
- `zb_<weapon>_clip <amount>` - Set clip size (overrides config `Clip`)
- `zb_<weapon>_ammo <amount>` - Set ammo count (overrides config `Ammo`)
- `zb_<weapon>_block_pickup [0|1]` - Block weapon pickup and usage (overrides config `BlockPickup`)

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

**Grenades:**
- `hegrenade` - HE Grenade
- `molotov` - Molotov Cocktail
- `incgrenade` - Incendiary Grenade
- `smokegrenade` - Smoke Grenade
- `flashbang` - Flashbang
- `decoy` - Decoy Grenade

**Examples:**
- `zb_ak47_price 3000` - Set AK47 price to $3000 (overrides config setting)
- `zb_awp_knockback 2.5` - Set AWP knockback scale to 2.5 (overrides config setting)
- `zb_glock_buy_enabled 0` - Disable Glock purchases (overrides config setting)
- `zb_m4a1_block_pickup 1` - Block M4A4 pickup and usage (overrides config setting)

### ConVar Priority System
The plugin uses a priority system with different rules for different settings:

**For weapon purchasing:**
1. **Config `EnableBuyCommand`** (highest priority) - If false, weapon cannot be purchased regardless of ConVar
2. **ConVar `zb_<weapon>_buy_enabled`** (secondary) - Only applies if config allows purchasing
3. **Default value** (fallback) - Used if neither is set

**For other weapon properties (price, damage, knockback, etc.):**
1. **ConVar value** (highest priority) - If set, this value is used
2. **Config file value** (fallback) - Used only if ConVar is not set
3. **Default value** (lowest priority) - Used if neither ConVar nor config is set

# Special Thanks

- original plugin: [schwarper/cs2-advanced-weapon-system](https://github.com/schwarper/cs2-advanced-weapon-system)
