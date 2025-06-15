using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using static Zbuy.Config;

namespace Zbuy;

public static class BuySystem
{
    private static readonly Dictionary<ulong, Dictionary<string, int>> PlayerPurchaseCount = new();

    public static void RegisterCommands()
    {
        Zbuy.Instance.AddCommand($"css_buy", "Buy weapon", OnBuyWithArgCommand);

        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            string weaponName = weaponData.Key;

            string cleanName = weaponName.Replace("weapon_", "");
            Zbuy.Instance.AddCommand($"css_{cleanName}", $"Buy {cleanName}", OnBuyCommand);

            foreach (string alias in weaponData.Value.BuyAliases)
            {
                if (alias == cleanName) continue;

                Zbuy.Instance.AddCommand($"css_{alias}", $"Buy {cleanName}", OnBuyCommand);
            }
        }
    }

    [CommandHelper(minArgs: 0, usage: "")]
    public static void OnBuyCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !player.IsValid)
            return;

        if (!WeaponRestrictionManager.IsZbuyEnabled())
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Buy commands are disabled"]}");
            return;
        }

        string command = commandInfo.GetCommandString;
        string weaponName = Utils.ExtractWeaponNameFromCommand(command);

        if (string.IsNullOrEmpty(weaponName))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Invalid weapon command"]}");
            return;
        }

        BuyWeapon(player, weaponName);
    }

    [CommandHelper(minArgs: 1, usage: "<weapon>")]
    public static void OnBuyWithArgCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !player.IsValid)
            return;

        if (!WeaponRestrictionManager.IsZbuyEnabled())
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Buy commands are disabled"]}");
            return;
        }

        string weaponArg = commandInfo.GetArg(1);
        string foundWeaponName = Utils.FindWeaponByAlias(weaponArg);

        if (string.IsNullOrEmpty(foundWeaponName))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Unknown weapon", weaponArg]}");
            return;
        }

        BuyWeapon(player, foundWeaponName);
    }

    public static void BuyWeapon(CCSPlayerController player, string weaponName)
    {
        if (!Zbuy.Instance.Config.WeaponDatas.TryGetValue(weaponName, out WeaponData? weaponData))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Weapon not configured", weaponName]}");
            return;
        }

        if (!WeaponRestrictionManager.IsWeaponBuyEnabled(weaponName))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["This weapon cannot be purchased"]}");
            return;
        }

        if (weaponData.EnableBuyCommand != true)
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["This weapon cannot be purchased"]}");
            return;
        }

        if (!Utils.IsPlayerAlive(player))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["You must be alive to buy weapons"]}");
            return;
        }

        if (!Zbuy.Instance.Config.GetAllowedCsTeams().Contains(player.Team))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Your team cannot buy weapons"]}");
            return;
        }

        if (Zbuy.Instance.Config.WeaponBuyZoneOnly && !Utils.IsClientInBuyZone(player))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["You must be in buy zone to purchase weapons"]}");
            return;
        }

        if (!WeaponRestrictionManager.CanPurchaseWeaponThisRound(player, weaponName))
        {
            int limit = WeaponRestrictionManager.GetWeaponRoundLimit(weaponName);
            int currentCount = WeaponRestrictionManager.GetPlayerRoundPurchaseCount(player, weaponName);
            string cleanName = Utils.CleanWeaponName(weaponName);
            player.PrintToChat($" {ChatColors.Red}[ZBuy] Round purchase limit reached for {cleanName} ({currentCount}/{limit})");
            return;
        }

        int price = CalculateWeaponPrice(player, weaponName, weaponData);

        if (player.InGameMoneyServices?.Account < price)
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Insufficient funds", price, player.InGameMoneyServices.Account]}");
            return;
        }

        var weaponToDrop = GetWeaponToDropBySlot(player, weaponName);

        Server.NextFrame(() =>
        {
            if (!string.IsNullOrEmpty(weaponToDrop))
            {
                Utils.DropWeaponByDesignName(player, weaponToDrop, false);
            }

            if (weaponName == "item_kevlar")
            {
                player.PlayerPawn.Value!.ArmorValue = 100;
                Utilities.SetStateChanged(player.PlayerPawn.Value, "CCSPlayerPawn", "m_ArmorValue");
            }
            else
            {
                player.GiveNamedItem(weaponName);
            }

            player.InGameMoneyServices!.Account -= price;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");

            UpdatePurchaseCount(player, weaponName);
            WeaponRestrictionManager.RecordWeaponPurchase(player, weaponName);

            string cleanName = Utils.CleanWeaponName(weaponName);
            player.PrintToChat($" {ChatColors.Green}[ZBuy] {Zbuy.Instance.Localizer["Purchased weapon", cleanName, price]}");
        });
    }

    private static int CalculateWeaponPrice(CCSPlayerController player, string weaponName, WeaponData weaponData)
    {
        int basePrice = weaponData.Price ?? WeaponMetadata.GetDefaultPrice(weaponName);
        int purchaseCount = GetPurchaseCount(player, weaponName);

        if (purchaseCount == 0 || !weaponData.PriceScale.HasValue)
            return basePrice;

        float scale = weaponData.PriceScale.Value;

        if (weaponData.PriceScaleMultiplicative == true)
        {
            // Multiplicative scaling: price * scale^count
            return (int)(basePrice * Math.Pow(scale, purchaseCount));
        }
        else
        {
            // Additive scaling: price + (price * scale * count)
            return (int)(basePrice + (basePrice * scale * purchaseCount));
        }
    }

    private static int GetPurchaseCount(CCSPlayerController player, string weaponName)
    {
        if (!PlayerPurchaseCount.TryGetValue(player.SteamID, out var weaponCounts))
            return 0;

        return weaponCounts.TryGetValue(weaponName, out int count) ? count : 0;
    }

    private static void UpdatePurchaseCount(CCSPlayerController player, string weaponName)
    {
        if (!PlayerPurchaseCount.TryGetValue(player.SteamID, out var weaponCounts))
        {
            weaponCounts = new Dictionary<string, int>();
            PlayerPurchaseCount[player.SteamID] = weaponCounts;
        }

        weaponCounts[weaponName] = weaponCounts.TryGetValue(weaponName, out int count) ? count + 1 : 1;
    }

    public static void ResetPurchaseCount(CCSPlayerController player)
    {
        PlayerPurchaseCount.Remove(player.SteamID);
    }

    public static void ResetAllPurchaseCounts()
    {
        PlayerPurchaseCount.Clear();
    }


    private static string GetWeaponToDropBySlot(CCSPlayerController player, string newWeaponName)
    {
        if (player.PlayerPawn.Value?.WeaponServices?.MyWeapons == null)
            return string.Empty;

        int newWeaponSlot = Utils.GetWeaponSlot(newWeaponName);
        if (newWeaponSlot == -1 || newWeaponSlot > 2)
            return string.Empty;

        foreach (var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
        {
            if (weapon.Value == null || !weapon.Value.IsValid)
                continue;

            int currentWeaponSlot = Utils.GetWeaponSlot(weapon.Value.DesignerName);

            if (currentWeaponSlot == newWeaponSlot)
            {
                return weapon.Value.DesignerName;
            }
        }

        return string.Empty;
    }
}