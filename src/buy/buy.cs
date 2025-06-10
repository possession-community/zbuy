using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
            if (weaponData.Value.EnableBuyCommand != true)
                continue;

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
            
        if (!Zbuy.Instance.Config.EnableBuyCommands)
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Buy commands are disabled"]}");
            return;
        }
        
        string command = commandInfo.GetCommandString;
        string weaponName = ExtractWeaponNameFromCommand(command);
        
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
            
        if (!Zbuy.Instance.Config.EnableBuyCommands)
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Buy commands are disabled"]}");
            return;
        }
        
        string weaponArg = commandInfo.GetArg(1);
        string foundWeaponName = FindWeaponByAlias(weaponArg);
        
        if (string.IsNullOrEmpty(foundWeaponName))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Unknown weapon", weaponArg]}");
            return;
        }
        
        BuyWeapon(player, foundWeaponName);
    }
    
    private static void BuyWeapon(CCSPlayerController player, string weaponName)
    {
        if (!Zbuy.Instance.Config.WeaponDatas.TryGetValue(weaponName, out WeaponData? weaponData))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Weapon not configured", weaponName]}");
            return;
        }

        if (!ConVarManager.IsWeaponBuyEnabled(weaponName))
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["This weapon cannot be purchased"]}");
            return;
        }
        
        if (weaponData.EnableBuyCommand != true)
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["This weapon cannot be purchased"]}");
            return;
        }
        
        if (player.PlayerPawn.Value?.LifeState != (byte)LifeState_t.LIFE_ALIVE)
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["You must be alive to buy weapons"]}");
            return;
        }
        
        int price = CalculateWeaponPrice(player, weaponName, weaponData);
        
        if (player.InGameMoneyServices?.Account < price)
        {
            player.PrintToChat($" {ChatColors.Red}[ZBuy] {Zbuy.Instance.Localizer["Insufficient funds", price, player.InGameMoneyServices.Account]}");
            return;
        }
        
        player.GiveNamedItem(weaponName);
        
        player.InGameMoneyServices!.Account -= price;
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        
        UpdatePurchaseCount(player, weaponName);
        
        string cleanName = weaponName.Replace("weapon_", "");
        player.PrintToChat($" {ChatColors.Green}[ZBuy] {Zbuy.Instance.Localizer["Purchased weapon", cleanName, price]}");
    }
    
    private static int CalculateWeaponPrice(CCSPlayerController player, string weaponName, WeaponData weaponData)
    {
        int? convarPrice = ConVarManager.GetWeaponPrice(weaponName);
        if (convarPrice.HasValue)
        {
            int convarBasePrice = convarPrice.Value;
            int convarPurchaseCount = GetPurchaseCount(player, weaponName);
            
            if (convarPurchaseCount == 0)
                return convarBasePrice;
                
            float? convarPriceScale = ConVarManager.GetWeaponPriceScale(weaponName);
            float convarScale = convarPriceScale ?? weaponData.PriceScale ?? 1.0f;
            
            if (weaponData.PriceScaleMultiplicative == true)
            {
                return (int)(convarBasePrice * Math.Pow(convarScale, convarPurchaseCount));
            }
            else
            {
                return (int)(convarBasePrice + (convarBasePrice * convarScale * convarPurchaseCount));
            }
        }
        
        if (!weaponData.Price.HasValue)
            return 0;
            
        int basePrice = weaponData.Price.Value;
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
    
    private static string ExtractWeaponNameFromCommand(string command)
    {
        string cleanCommand = command.Replace("css_", "").ToLower();
        
        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            string weaponName = weaponData.Key;
            string cleanWeaponName = weaponName.Replace("weapon_", "");
            
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
    
    private static string FindWeaponByAlias(string alias)
    {
        alias = alias.ToLower();
        
        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            string weaponName = weaponData.Key;
            string cleanWeaponName = weaponName.Replace("weapon_", "");
            
            if (alias == cleanWeaponName.ToLower())
                return weaponName;
                
            foreach (string weaponAlias in weaponData.Value.BuyAliases)
            {
                if (alias == weaponAlias.ToLower())
                    return weaponName;
            }
        }
        
        return string.Empty;
    }
}