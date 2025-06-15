using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Zbuy;

public class Config : BasePluginConfig
{
    public Dictionary<string, WeaponData> WeaponDatas { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool EnableBuyCommands { get; set; } = true;
    public bool WeaponBuyZoneOnly { get; set; } = false;
    public bool EnableDamageMoneyReward { get; set; } = true;

    public List<string> AllowedTeamsToBuy { get; set; } = ["T", "CT"];

    public List<CsTeam> GetAllowedCsTeams()
    {
        var teams = new List<CsTeam>();
        foreach (var teamString in AllowedTeamsToBuy)
        {
            switch (teamString.ToUpper())
            {
                case "T":
                case "TERRORIST":
                    teams.Add(CsTeam.Terrorist);
                    break;
                case "CT":
                case "COUNTERTERRORIST":
                case "COUNTER-TERRORIST":
                    teams.Add(CsTeam.CounterTerrorist);
                    break;
            }
        }
        return teams;
    }

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

        public int? MaxPurchasesPerRound { get; set; }
    }
}