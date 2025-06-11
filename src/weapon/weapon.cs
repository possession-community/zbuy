using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using static Zbuy.Config;

namespace Zbuy;

public static class Weapon
{

    public static ushort DefIndex(string weaponName)
    {
        return weaponsList[weaponName];
    }

    public static string GetDesignerName(CBasePlayerWeapon weapon)
    {
        string weaponDesignerName = weapon.DesignerName;
        ushort weaponIndex = weapon.AttributeManager.Item.ItemDefinitionIndex;

        return (weaponDesignerName, weaponIndex) switch
        {
            var (name, _) when name.Contains("bayonet") => "weapon_knife",
            ("weapon_m4a1", 60) => "weapon_m4a1_silencer",
            ("weapon_hkp2000", 61) => "weapon_usp_silencer",
            ("weapon_mp7", 23) => "weapon_mp5sd",
            _ => weaponDesignerName
        };
    }

    private static readonly Dictionary<string, ushort> weaponsList = new(StringComparer.OrdinalIgnoreCase)
    {
        { "weapon_m4a1", (ushort)ItemDefinition.M4A4 },
        { "weapon_m4a1_silencer", (ushort)ItemDefinition.M4A1_S },
        { "weapon_famas", (ushort)ItemDefinition.FAMAS },
        { "weapon_aug", (ushort)ItemDefinition.AUG },
        { "weapon_ak47", (ushort)ItemDefinition.AK_47 },
        { "weapon_galilar", (ushort)ItemDefinition.GALIL_AR },
        { "weapon_sg556", (ushort)ItemDefinition.SG_553 },
        { "weapon_scar20", (ushort)ItemDefinition.SCAR_20 },
        { "weapon_awp", (ushort)ItemDefinition.AWP },
        { "weapon_ssg08", (ushort)ItemDefinition.SSG_08 },
        { "weapon_g3sg1", (ushort)ItemDefinition.G3SG1 },
        { "weapon_mp9", (ushort)ItemDefinition.MP9 },
        { "weapon_mp7", (ushort)ItemDefinition.MP7 },
        { "weapon_mp5sd", (ushort)ItemDefinition.MP5_SD },
        { "weapon_ump45", (ushort)ItemDefinition.UMP_45 },
        { "weapon_p90", (ushort)ItemDefinition.P90 },
        { "weapon_bizon", (ushort)ItemDefinition.PP_BIZON },
        { "weapon_mac10", (ushort)ItemDefinition.MAC_10 },
        { "weapon_usp_silencer", (ushort)ItemDefinition.USP_S },
        { "weapon_hkp2000", (ushort)ItemDefinition.P2000 },
        { "weapon_glock", (ushort)ItemDefinition.GLOCK_18 },
        { "weapon_elite", (ushort)ItemDefinition.DUAL_BERETTAS },
        { "weapon_p250", (ushort)ItemDefinition.P250 },
        { "weapon_fiveseven", (ushort)ItemDefinition.FIVE_SEVEN },
        { "weapon_cz75a", (ushort)ItemDefinition.CZ75_AUTO },
        { "weapon_tec9", (ushort)ItemDefinition.TEC_9 },
        { "weapon_revolver", (ushort)ItemDefinition.R8_REVOLVER },
        { "weapon_deagle", (ushort)ItemDefinition.DESERT_EAGLE },
        { "weapon_nova", (ushort)ItemDefinition.NOVA },
        { "weapon_xm1014", (ushort)ItemDefinition.XM1014 },
        { "weapon_mag7", (ushort)ItemDefinition.MAG_7 },
        { "weapon_sawedoff", (ushort)ItemDefinition.SAWED_OFF },
        { "weapon_m249", (ushort)ItemDefinition.M249 },
        { "weapon_negev", (ushort)ItemDefinition.NEGEV },
        { "weapon_taser", (ushort)ItemDefinition.ZEUS_X27 },
        { "weapon_hegrenade", (ushort)ItemDefinition.HIGH_EXPLOSIVE_GRENADE },
        { "weapon_molotov", (ushort)ItemDefinition.MOLOTOV },
        { "weapon_incgrenade", (ushort)ItemDefinition.INCENDIARY_GRENADE },
        { "weapon_smokegrenade", (ushort)ItemDefinition.SMOKE_GRENADE },
        { "weapon_flashbang", (ushort)ItemDefinition.FLASHBANG },
        { "weapon_decoy", (ushort)ItemDefinition.DECOY_GRENADE }
    };
}