using CounterStrikeSharp.API.Core;

namespace Zbuy;

public class Config : BasePluginConfig
{
    public Dictionary<string, WeaponData> WeaponDatas { get; set; } = [];
    
    public bool EnableBuyCommands { get; set; } = true;
    public bool EnableKnockback { get; set; } = false;

    public class WeaponData
    {
        public string Weapon { get; set; } = string.Empty;
        public int? Clip { get; set; }
        public int? Ammo { get; set; }
        public bool? BlockUsing { get; set; }
        public bool? IgnorePickUpFromBlockUsing { get; set; }
        public bool? ReloadAfterShoot { get; set; }
        public bool? UnlimitedAmmo { get; set; }
        public bool? UnlimitedClip { get; set; }
        public bool? OnlyHeadshot { get; set; }
        public string? ViewModel { get; set; }
        public string? WorldModel { get; set; }
        public List<string> AdminFlagsToIgnoreBlockUsing { get; set; } = [];
        public Dictionary<int, int> WeaponQuota { get; set; } = [];
        public string? Damage { get; set; }
        
        public int? Price { get; set; }
        public float? PriceScale { get; set; }
        public bool? PriceScaleMultiplicative { get; set; } = false;
        public bool? EnableBuyCommand { get; set; } = true;
        public List<string> BuyAliases { get; set; } = [];
        
        public float? KnockbackScale { get; set; } = 1.0f;
    }
}