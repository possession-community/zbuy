using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Zbuy;

public static class Utils
{
    public static bool IsPlayerAlive(CCSPlayerController client)
    {
        if (client == null || !client.IsValid)
            return false;

        var clientPawn = client.PlayerPawn.Value;

        if (clientPawn == null || !clientPawn.IsValid)
            return false;

        return (LifeState_t)clientPawn.LifeState == LifeState_t.LIFE_ALIVE;
    }

    public static bool IsClientInBuyZone(CCSPlayerController client)
    {
        if (client == null)
            return false;

        return client.PlayerPawn.Value?.InBuyZone ?? false;
    }

    public static void DropWeapon(CCSPlayer_WeaponServices services, CBasePlayerWeapon weapon, bool remove = false)
    {
        Guard.IsValidEntity(weapon);
        VirtualFunction.CreateVoid<nint, CBasePlayerWeapon, Vector?, Vector?>(services.Handle, GameData.GetOffset("CCSPlayer_WeaponServices_DropWeapon"))(services.Handle, weapon, null, null);
        if (remove) weapon.Remove();
    }

    public static void DropWeaponByDesignName(CCSPlayerController client, string weaponName, bool remove = false)
    {
        if (client == null)
            return;

        var service = client.PlayerPawn.Value?.WeaponServices;
        var matchedWeapon = client.PlayerPawn.Value?.WeaponServices?.MyWeapons.Where(x => x.Value?.DesignerName == weaponName).FirstOrDefault();

        if (matchedWeapon != null && matchedWeapon.IsValid && service != null)
        {
            if (matchedWeapon.Value == null)
                return;

            DropWeapon(service.As<CCSPlayer_WeaponServices>(), matchedWeapon.Value, remove);
        }
    }

    public static int GetWeaponSlot(string weaponName)
    {
        if (weaponName.StartsWith("weapon_ak47") || weaponName.StartsWith("weapon_m4a1") ||
            weaponName.StartsWith("weapon_awp") || weaponName.StartsWith("weapon_aug") ||
            weaponName.StartsWith("weapon_famas") || weaponName.StartsWith("weapon_galilar") ||
            weaponName.StartsWith("weapon_scar20") || weaponName.StartsWith("weapon_g3sg1") ||
            weaponName.StartsWith("weapon_ssg08") || weaponName.StartsWith("weapon_sg556") ||
            weaponName.StartsWith("weapon_mp9") || weaponName.StartsWith("weapon_mp7") ||
            weaponName.StartsWith("weapon_mp5sd") || weaponName.StartsWith("weapon_ump45") ||
            weaponName.StartsWith("weapon_p90") || weaponName.StartsWith("weapon_bizon") ||
            weaponName.StartsWith("weapon_mac10") || weaponName.StartsWith("weapon_nova") ||
            weaponName.StartsWith("weapon_xm1014") || weaponName.StartsWith("weapon_mag7") ||
            weaponName.StartsWith("weapon_sawedoff") || weaponName.StartsWith("weapon_m249") ||
            weaponName.StartsWith("weapon_negev"))
        {
            return 0;
        }

        if (weaponName.StartsWith("weapon_glock") || weaponName.StartsWith("weapon_usp_silencer") ||
            weaponName.StartsWith("weapon_hkp2000") || weaponName.StartsWith("weapon_elite") ||
            weaponName.StartsWith("weapon_p250") || weaponName.StartsWith("weapon_fiveseven") ||
            weaponName.StartsWith("weapon_cz75a") || weaponName.StartsWith("weapon_tec9") ||
            weaponName.StartsWith("weapon_revolver") || weaponName.StartsWith("weapon_deagle"))
        {
            return 1;
        }

        if (weaponName.StartsWith("weapon_knife"))
        {
            return 2;
        }

        if (weaponName.StartsWith("weapon_hegrenade") || weaponName.StartsWith("weapon_flashbang") ||
            weaponName.StartsWith("weapon_smokegrenade") || weaponName.StartsWith("weapon_decoy") ||
            weaponName.StartsWith("weapon_molotov") || weaponName.StartsWith("weapon_incgrenade"))
        {
            return 3;
        }

        return -1;
    }

    public static string CleanWeaponName(string weaponName)
    {
        return weaponName.Replace("weapon_", "").Replace("item_", "");
    }

    public static string FindWeaponByAlias(string alias)
    {
        alias = alias.ToLower();

        if (alias.StartsWith("weapon_"))
        {
            if (Zbuy.Instance.Config.WeaponDatas.ContainsKey(alias))
                return alias;
        }
        else
        {
            string fullWeaponName = $"weapon_{alias}";
            if (Zbuy.Instance.Config.WeaponDatas.ContainsKey(fullWeaponName))
                return fullWeaponName;
        }

        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            string weaponName = weaponData.Key;
            string cleanWeaponName = CleanWeaponName(weaponName).ToLower();

            if (alias == cleanWeaponName)
                return weaponName;

            foreach (string weaponAlias in weaponData.Value.BuyAliases)
            {
                if (alias == weaponAlias.ToLower())
                    return weaponName;
            }
        }

        return string.Empty;
    }

    public static string ExtractWeaponNameFromCommand(string command)
    {
        string cleanCommand = command.Replace("css_", "").ToLower();

        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            string weaponName = weaponData.Key;
            string cleanWeaponName = CleanWeaponName(weaponName).ToLower();

            if (cleanCommand == cleanWeaponName)
                return weaponName;

            foreach (string alias in weaponData.Value.BuyAliases)
            {
                if (cleanCommand == alias.ToLower())
                    return weaponName;
            }
        }

        return string.Empty;
    }
}
