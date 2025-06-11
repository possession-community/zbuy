using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using static Zbuy.Config;
using static Zbuy.Weapon;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Zbuy;

public class Zbuy : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Zbuy";
    public override string ModuleVersion => "2.0";
    public override string ModuleAuthor => "uru, schwarper";

    public Config Config { get; set; } = new Config();
    public static Zbuy Instance { get; private set; } = new();

    public override void Load(bool hotReload)
    {
        Instance = this;

        WeaponRestrictionManager.Initialize();
        WeaponRestrictionManager.RegisterAdminCommands();
        
        BuySystem.RegisterCommands();
    }

    public override void Unload(bool hotReload)
    {
    }

    public void OnConfigParsed(Config config)
    {
        Config = config;
    }

    [GameEventHandler]
    public HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        if (@event.Userid is not { } player || player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value is not { } activeWeapon)
            return HookResult.Continue;

        if (!Config.WeaponDatas.TryGetValue(GetDesignerName(activeWeapon), out WeaponData? weaponData))
            return HookResult.Continue;

        if (weaponData.UnlimitedClip == true)
            activeWeapon.Clip1 += 1;

        if (weaponData.UnlimitedAmmo == true)
            activeWeapon.ReserveAmmo[0] += 1;


        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        BuySystem.ResetAllPurchaseCounts();
        return HookResult.Continue;
    }

    [ListenerHandler<OnEntitySpawned>]
    public void OnEntitySpawned(CEntityInstance entity)
    {
        if (!entity.DesignerName.StartsWith("weapon_"))
            return;

        if (!Config.WeaponDatas.TryGetValue(GetDesignerName(entity.As<CBasePlayerWeapon>()), out WeaponData? weaponData))
            return;

        if (entity.As<CCSWeaponBase>().VData is not CCSWeaponBaseVData weaponVData)
            return;

        if (weaponData.Clip.HasValue)
            weaponVData.MaxClip1 = weaponData.Clip.Value;

        if (weaponData.Ammo.HasValue)
            weaponVData.PrimaryReserveAmmoMax = weaponData.Ammo.Value;
    }
}
