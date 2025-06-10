using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace Zbuy;

public static class ConVarManager
{
    public static FakeConVar<bool>? EnableBuyCommands { get; private set; }
    public static FakeConVar<bool>? EnableKnockback { get; private set; }
    
    private static readonly Dictionary<string, WeaponConVarSet> WeaponConVarSets = new();
    
    public class WeaponConVarSet
    {
        public FakeConVar<bool>? BuyEnabled { get; set; }
        public FakeConVar<int>? Price { get; set; }
        public FakeConVar<float>? PriceScale { get; set; }
        public FakeConVar<float>? KnockbackScale { get; set; }
        public FakeConVar<string>? Damage { get; set; }
        public FakeConVar<int>? Clip { get; set; }
        public FakeConVar<int>? Ammo { get; set; }
        public FakeConVar<bool>? BlockPickup { get; set; }
    }
    
    public static void Initialize()
    {
        EnableBuyCommands = new FakeConVar<bool>("zb_enable_buy_commands", "Enable/disable buy commands", false);
        EnableKnockback = new FakeConVar<bool>("zb_enable_knockback", "Enable/disable knockback system", false);
        
        EnableBuyCommands.ValueChanged += OnBuyCommandsChanged;
        EnableKnockback.ValueChanged += OnKnockbackChanged;
        
        foreach (var weaponData in Zbuy.Instance.Config.WeaponDatas)
        {
            RegisterWeaponConVars(weaponData.Key, weaponData.Value);
        }
    }
    
    private static void RegisterWeaponConVars(string weaponName, Config.WeaponData weaponData)
    {
        string cleanName = weaponName.Replace("weapon_", "");
        var convars = new WeaponConVarSet();
        
        convars.BuyEnabled = new FakeConVar<bool>($"zb_{cleanName}_buy_enabled", $"Enable/disable buying {cleanName}", false);
        
        if (weaponData.Price.HasValue)
        {
            convars.Price = new FakeConVar<int>($"zb_{cleanName}_price", $"Price for {cleanName}", weaponData.Price.Value);
        }
        
        if (weaponData.PriceScale.HasValue)
        {
            convars.PriceScale = new FakeConVar<float>($"zb_{cleanName}_price_scale", $"Price scale for {cleanName}", weaponData.PriceScale.Value);
        }
        
        if (weaponData.KnockbackScale.HasValue)
        {
            convars.KnockbackScale = new FakeConVar<float>($"zb_{cleanName}_knockback", $"Knockback scale for {cleanName}", weaponData.KnockbackScale.Value);
        }
        
        if (!string.IsNullOrEmpty(weaponData.Damage))
        {
            convars.Damage = new FakeConVar<string>($"zb_{cleanName}_damage", $"Damage modifier for {cleanName}", weaponData.Damage);
        }
        
        if (weaponData.Clip.HasValue)
        {
            convars.Clip = new FakeConVar<int>($"zb_{cleanName}_clip", $"Clip size for {cleanName}", weaponData.Clip.Value);
        }
        
        if (weaponData.Ammo.HasValue)
        {
            convars.Ammo = new FakeConVar<int>($"zb_{cleanName}_ammo", $"Ammo count for {cleanName}", weaponData.Ammo.Value);
        }
        
        if (weaponData.BlockPickup.HasValue)
        {
            convars.BlockPickup = new FakeConVar<bool>($"zb_{cleanName}_block_pickup", $"Block pickup and usage of {cleanName}", weaponData.BlockPickup.Value);
        }
        
        WeaponConVarSets[weaponName] = convars;
    }
    
    public static bool IsWeaponBuyEnabled(string weaponName)
    {
        if (WeaponConVarSets.TryGetValue(weaponName, out var convars) && convars.BuyEnabled != null)
        {
            return convars.BuyEnabled.Value;
        }
        return true;
    }
    
    public static int? GetWeaponPrice(string weaponName)
    {
        if (WeaponConVarSets.TryGetValue(weaponName, out var convars) && convars.Price != null)
        {
            return convars.Price.Value;
        }
        return null;
    }
    
    public static float? GetWeaponPriceScale(string weaponName)
    {
        if (WeaponConVarSets.TryGetValue(weaponName, out var convars) && convars.PriceScale != null)
        {
            return convars.PriceScale.Value;
        }
        return null;
    }
    
    public static float? GetWeaponKnockbackScale(string weaponName)
    {
        if (WeaponConVarSets.TryGetValue(weaponName, out var convars) && convars.KnockbackScale != null)
        {
            return convars.KnockbackScale.Value;
        }
        return null;
    }
    
    public static string? GetWeaponDamage(string weaponName)
    {
        if (WeaponConVarSets.TryGetValue(weaponName, out var convars) && convars.Damage != null)
        {
            return convars.Damage.Value;
        }
        return null;
    }
    
    public static bool IsWeaponPickupBlocked(string weaponName)
    {
        if (WeaponConVarSets.TryGetValue(weaponName, out var convars) && convars.BlockPickup != null)
        {
            return convars.BlockPickup.Value;
        }
        return false;
    }
    
    public static int? GetWeaponClip(string weaponName)
    {
        if (WeaponConVarSets.TryGetValue(weaponName, out var convars) && convars.Clip != null)
        {
            return convars.Clip.Value;
        }
        return null;
    }
    
    public static int? GetWeaponAmmo(string weaponName)
    {
        if (WeaponConVarSets.TryGetValue(weaponName, out var convars) && convars.Ammo != null)
        {
            return convars.Ammo.Value;
        }
        return null;
    }
    
    private static void OnBuyCommandsChanged(object? sender, bool newValue)
    {
        Zbuy.Instance.Config.EnableBuyCommands = newValue;
        
        string message = newValue ? Zbuy.Instance.Localizer["Buy commands enabled"] : Zbuy.Instance.Localizer["Buy commands disabled"];
        Server.PrintToChatAll($" {ChatColors.Green}[ZBuy] {message}");
    }
    
    private static void OnKnockbackChanged(object? sender, bool newValue)
    {
        Zbuy.Instance.Config.EnableKnockback = newValue;
        
        string message = newValue ? Zbuy.Instance.Localizer["Knockback system enabled"] : Zbuy.Instance.Localizer["Knockback system disabled"];
        Server.PrintToChatAll($" {ChatColors.Green}[ZBuy] {message}");
    }
    
    public static void Cleanup()
    {
        if (EnableBuyCommands != null)
            EnableBuyCommands.ValueChanged -= OnBuyCommandsChanged;
            
        if (EnableKnockback != null)
            EnableKnockback.ValueChanged -= OnKnockbackChanged;
    }
}