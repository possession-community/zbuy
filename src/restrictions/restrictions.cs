using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace Zbuy;

public static class WeaponRestrictionManager
{
    private static readonly Dictionary<string, bool> WeaponRestrictions = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, int> WeaponRoundLimits = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<CCSPlayerController, Dictionary<string, int>> PlayerRoundPurchases = new();
    private static bool _zbuyEnabled = true;

    public static void Initialize()
    {
        _zbuyEnabled = Zbuy.Instance.Config.EnableBuyCommands;

        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            WeaponRestrictions[weaponData.Key] = true;

            if (weaponData.Value.MaxPurchasesPerRound.HasValue)
            {
                WeaponRoundLimits[weaponData.Key] = weaponData.Value.MaxPurchasesPerRound.Value;
            }
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

    public static bool CanPurchaseWeaponThisRound(CCSPlayerController player, string weaponName)
    {
        if (!WeaponRoundLimits.TryGetValue(weaponName, out int limit))
            return true;

        if (!PlayerRoundPurchases.TryGetValue(player, out var weaponCounts))
            return true;

        int currentCount = weaponCounts.TryGetValue(weaponName, out int count) ? count : 0;
        return currentCount < limit;
    }

    public static void RecordWeaponPurchase(CCSPlayerController player, string weaponName)
    {
        if (!PlayerRoundPurchases.TryGetValue(player, out var weaponCounts))
        {
            weaponCounts = new Dictionary<string, int>();
            PlayerRoundPurchases[player] = weaponCounts;
        }

        weaponCounts[weaponName] = weaponCounts.TryGetValue(weaponName, out int count) ? count + 1 : 1;
    }

    public static void ResetRoundPurchases()
    {
        PlayerRoundPurchases.Clear();
    }

    public static int GetWeaponRoundLimit(string weaponName)
    {
        return WeaponRoundLimits.TryGetValue(weaponName, out int limit) ? limit : -1;
    }

    public static void SetWeaponRoundLimit(string weaponName, int limit)
    {
        if (limit <= 0)
        {
            WeaponRoundLimits.Remove(weaponName);
        }
        else
        {
            WeaponRoundLimits[weaponName] = limit;
        }
    }

    public static int GetPlayerRoundPurchaseCount(CCSPlayerController player, string weaponName)
    {
        if (!PlayerRoundPurchases.TryGetValue(player, out var weaponCounts))
            return 0;

        return weaponCounts.TryGetValue(weaponName, out int count) ? count : 0;
    }

    public static void RegisterAdminCommands()
    {
        Zbuy.Instance.AddCommand("css_zb_enabled", "Enable/disable zbuy system", OnZbuyEnabledCommand);
        Zbuy.Instance.AddCommand("css_zb_restrict", "Restrict weapon buying", OnRestrictWeaponCommand);
        Zbuy.Instance.AddCommand("css_zb_unrestrict", "Unrestrict weapon buying", OnUnrestrictWeaponCommand);
        Zbuy.Instance.AddCommand("css_zb_roundlimit", "Set weapon round purchase limit", OnSetRoundLimitCommand);
        Zbuy.Instance.AddCommand("css_zb_resetround", "Reset round purchase counts", OnResetRoundCommand);
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
        string weaponName = Utils.FindWeaponByAlias(weaponArg);

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

        string cleanName = Utils.CleanWeaponName(weaponName);
        string statusMessage = $"Weapon '{cleanName}' has been restricted";
        string chatMessage = $" {ChatColors.Yellow}[ZBuy] {statusMessage}";

        if (player != null)
        {
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
        string weaponName = Utils.FindWeaponByAlias(weaponArg);

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

        string cleanName = Utils.CleanWeaponName(weaponName);
        string statusMessage = $"Weapon '{cleanName}' has been unrestricted";
        string chatMessage = $" {ChatColors.Green}[ZBuy] {statusMessage}";

        if (player != null)
        {
            Server.PrintToChatAll(chatMessage);
        }
        else
        {
            Server.PrintToConsole($"[ZBuy] {statusMessage}");
            Server.PrintToChatAll(chatMessage);
        }
    }

    [CommandHelper(minArgs: 2, usage: "<weapon_name> <limit>")]
    [RequiresPermissions("@css/generic")]
    public static void OnSetRoundLimitCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/generic"))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] You don't have permission to use this command.");
            return;
        }

        string weaponArg = commandInfo.GetArg(1);
        string limitArg = commandInfo.GetArg(2);

        string weaponName = Utils.FindWeaponByAlias(weaponArg);
        if (string.IsNullOrEmpty(weaponName))
        {
            string message = $"Unknown weapon: {weaponArg}";
            if (player != null)
                player.PrintToChat($" {ChatColors.Red}[ZBuy] {message}");
            else
                Server.PrintToConsole($"[ZBuy] {message}");
            return;
        }

        if (!int.TryParse(limitArg, out int limit) || limit < 0)
        {
            string message = "Limit must be a non-negative integer (0 = no limit)";
            if (player != null)
                player.PrintToChat($" {ChatColors.Red}[ZBuy] {message}");
            else
                Server.PrintToConsole($"[ZBuy] {message}");
            return;
        }

        SetWeaponRoundLimit(weaponName, limit);

        string cleanName = Utils.CleanWeaponName(weaponName);
        string statusMessage = limit == 0
            ? $"Round limit removed for '{cleanName}'"
            : $"Round limit set to {limit} for '{cleanName}'";
        string chatMessage = $" {ChatColors.Green}[ZBuy] {statusMessage}";

        if (player != null)
        {
            Server.PrintToChatAll(chatMessage);
        }
        else
        {
            Server.PrintToConsole($"[ZBuy] {statusMessage}");
            Server.PrintToChatAll(chatMessage);
        }
    }

    [CommandHelper(minArgs: 0, usage: "")]
    [RequiresPermissions("@css/generic")]
    public static void OnResetRoundCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/generic"))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] You don't have permission to use this command.");
            return;
        }

        ResetRoundPurchases();

        string statusMessage = "Round purchase counts have been reset";
        string chatMessage = $" {ChatColors.Green}[ZBuy] {statusMessage}";

        if (player != null)
        {
            Server.PrintToChatAll(chatMessage);
        }
        else
        {
            Server.PrintToConsole($"[ZBuy] {statusMessage}");
            Server.PrintToChatAll(chatMessage);
        }
    }
}
