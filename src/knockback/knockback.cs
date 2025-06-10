using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static Zbuy.Config;

namespace Zbuy;

public static class KnockbackSystem
{
    public static void ApplyKnockback(CCSPlayerController victim, CCSPlayerController attacker, string weaponName, float damage)
    {
        if (!Zbuy.Instance.Config.EnableKnockback)
            return;
            
        if (victim == null || attacker == null || victim == attacker)
            return;
            
        if (victim.PlayerPawn.Value == null || attacker.PlayerPawn.Value == null)
            return;

        if (victim.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;
            
        if (!Zbuy.Instance.Config.WeaponDatas.TryGetValue(weaponName, out WeaponData? weaponData))
            return;
            
        float? convarKnockbackScale = ConVarManager.GetWeaponKnockbackScale(weaponName);
        float knockbackScale = convarKnockbackScale ?? weaponData.KnockbackScale ?? 0f;
        
        if (knockbackScale <= 0)
            return;
            
        Vector? victimPos = victim.PlayerPawn.Value.AbsOrigin;
        Vector? attackerPos = attacker.PlayerPawn.Value.AbsOrigin;
        
        if (victimPos == null || attackerPos == null)
            return;
            
        Vector direction = victimPos - attackerPos;
        Vector normalizedDirection = NormalizeVector(direction);
        
        float knockbackForce = damage * knockbackScale;
        
        Vector knockbackVelocity = new Vector(
            normalizedDirection.X * knockbackForce,
            normalizedDirection.Y * knockbackForce,
            Math.Max(0, normalizedDirection.Z * knockbackForce * 0.5f)
        );
        
        if (victim.PlayerPawn.Value.AbsVelocity != null)
        {
            victim.PlayerPawn.Value.AbsVelocity.X += knockbackVelocity.X;
            victim.PlayerPawn.Value.AbsVelocity.Y += knockbackVelocity.Y;
            victim.PlayerPawn.Value.AbsVelocity.Z += knockbackVelocity.Z;
        }
    }
    
    private static Vector NormalizeVector(Vector vector)
    {
        float x = vector.X;
        float y = vector.Y;
        float z = vector.Z;
        
        float magnitude = MathF.Sqrt(x * x + y * y + z * z);
        
        if (magnitude > 0.0f)
        {
            x /= magnitude;
            y /= magnitude;
            z /= magnitude;
        }
        
        return new Vector(x, y, z);
    }
}