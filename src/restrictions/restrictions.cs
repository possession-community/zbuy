using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace Zbuy;

public static class WeaponRestrictionManager
{
    private static readonly Dictionary<string, bool> WeaponRestrictions = new(StringComparer.OrdinalIgnoreCase);
    private static bool _zbuyEnabled = true;

    public static void Initialize()
    {
        _zbuyEnabled = Zbuy.Instance.Config.EnableBuyCommands;

        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            WeaponRestrictions[weaponData.Key] = true;
        }
    }

    public static bool IsZbuyEnabled()
    {
        return _zbuyEnabled;
    }

    public static void SetZbuyEnabled(bool enabled)
    {
        _zbuyEnabled = enabled;
    }

    public static bool IsWeaponBuyEnabled(string weaponName)
    {
        if (WeaponRestrictions.TryGetValue(weaponName, out bool enabled))
        {
            return enabled;
        }
        return true;
    }

    public static void SetWeaponRestriction(string weaponName, bool enabled)
    {
        WeaponRestrictions[weaponName] = enabled;
    }

    public static void RegisterAdminCommands()
    {
        Zbuy.Instance.AddCommand("css_zb_enabled", "Enable/disable zbuy system", OnZbuyEnabledCommand);
        Zbuy.Instance.AddCommand("css_zb_restrict", "Restrict weapon buying", OnRestrictWeaponCommand);
        Zbuy.Instance.AddCommand("css_zb_unrestrict", "Unrestrict weapon buying", OnUnrestrictWeaponCommand);
    }

    [CommandHelper(minArgs: 1, usage: "<0/1>")]
    [RequiresPermissions("@css/generic")]
    public static void OnZbuyEnabledCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/generic"))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] You don't have permission to use this command.");
            return;
        }

        string arg = commandInfo.GetArg(1);
        if (!int.TryParse(arg, out int value) || (value != 0 && value != 1))
        {
            string message = "Usage: css_zb_enabled <0/1>";
            if (player != null)
                player.PrintToChat($" {ChatColors.Red}[ZBuy] {message}");
            else
                Server.PrintToConsole($"[ZBuy] {message}");
            return;
        }

        bool enabled = value == 1;
        SetZbuyEnabled(enabled);

        string statusMessage = enabled ? "ZBuy system enabled" : "ZBuy system disabled";
        string chatMessage = $" {ChatColors.Green}[ZBuy] {statusMessage}";

        if (player != null)
        {
            player.PrintToChat(chatMessage);
            Server.PrintToChatAll(chatMessage);
        }
        else
        {
            Server.PrintToConsole($"[ZBuy] {statusMessage}");
            Server.PrintToChatAll(chatMessage);
        }
    }

    [CommandHelper(minArgs: 1, usage: "<weapon_name>")]
    [RequiresPermissions("@css/generic")]
    public static void OnRestrictWeaponCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/generic"))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] You don't have permission to use this command.");
            return;
        }

        string weaponArg = commandInfo.GetArg(1);
        string weaponName = FindWeaponName(weaponArg);

        if (string.IsNullOrEmpty(weaponName))
        {
            string message = $"Unknown weapon: {weaponArg}";
            if (player != null)
                player.PrintToChat($" {ChatColors.Red}[ZBuy] {message}");
            else
                Server.PrintToConsole($"[ZBuy] {message}");
            return;
        }

        SetWeaponRestriction(weaponName, false);

        string cleanName = weaponName.Replace("weapon_", "");
        string statusMessage = $"Weapon '{cleanName}' has been restricted";
        string chatMessage = $" {ChatColors.Yellow}[ZBuy] {statusMessage}";

        if (player != null)
        {
            player.PrintToChat(chatMessage);
            Server.PrintToChatAll(chatMessage);
        }
        else
        {
            Server.PrintToConsole($"[ZBuy] {statusMessage}");
            Server.PrintToChatAll(chatMessage);
        }
    }

    [CommandHelper(minArgs: 1, usage: "<weapon_name>")]
    [RequiresPermissions("@css/generic")]
    public static void OnUnrestrictWeaponCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/generic"))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] You don't have permission to use this command.");
            return;
        }

        string weaponArg = commandInfo.GetArg(1);
        string weaponName = FindWeaponName(weaponArg);

        if (string.IsNullOrEmpty(weaponName))
        {
            string message = $"Unknown weapon: {weaponArg}";
            if (player != null)
                player.PrintToChat($" {ChatColors.Red}[ZBuy] {message}");
            else
                Server.PrintToConsole($"[ZBuy] {message}");
            return;
        }

        SetWeaponRestriction(weaponName, true);

        string cleanName = weaponName.Replace("weapon_", "");
        string statusMessage = $"Weapon '{cleanName}' has been unrestricted";
        string chatMessage = $" {ChatColors.Green}[ZBuy] {statusMessage}";

        if (player != null)
        {
            player.PrintToChat(chatMessage);
            Server.PrintToChatAll(chatMessage);
        }
        else
        {
            Server.PrintToConsole($"[ZBuy] {statusMessage}");
            Server.PrintToChatAll(chatMessage);
        }
    }

    private static string FindWeaponName(string weaponArg)
    {
        weaponArg = weaponArg.ToLower();

        if (weaponArg.StartsWith("weapon_"))
        {
            if (Zbuy.Instance.Config.WeaponDatas.ContainsKey(weaponArg))
                return weaponArg;
        }
        else
        {
            string fullWeaponName = $"weapon_{weaponArg}";
            if (Zbuy.Instance.Config.WeaponDatas.ContainsKey(fullWeaponName))
                return fullWeaponName;
        }

        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            string weaponName = weaponData.Key;
            string cleanWeaponName = weaponName.Replace("weapon_", "").ToLower();

            if (weaponArg == cleanWeaponName)
                return weaponName;

            foreach (string alias in weaponData.Value.BuyAliases)
            {
                if (weaponArg == alias.ToLower())
                    return weaponName;
            }
        }

        return string.Empty;
    }
}
