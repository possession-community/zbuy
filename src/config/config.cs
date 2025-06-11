using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Zbuy;

public class Config : BasePluginConfig
{
    public Dictionary<string, WeaponData> WeaponDatas { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
    public bool EnableBuyCommands { get; set; } = true;
    public bool WeaponBuyZoneOnly { get; set; } = false;
    public List<CsTeam> AllowedTeamsToBuy { get; set; } = [CsTeam.CounterTerrorist, CsTeam.Terrorist];

    public class WeaponData
    {
        public string Weapon { get; set; } = string.Empty;
        public int? Clip { get; set; }
        public int? Ammo { get; set; }
        public bool? UnlimitedAmmo { get; set; }
        public bool? UnlimitedClip { get; set; }
        
        public int? Price { get; set; }
        public float? PriceScale { get; set; }
        public bool? PriceScaleMultiplicative { get; set; } = false;
        public bool? EnableBuyCommand { get; set; } = true;
        public List<string> BuyAliases { get; set; } = [];
        
    }
}