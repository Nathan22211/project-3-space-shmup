using UnityEngine;

/// <summary>
/// Steers toward the hero like EnemySeeker but never fires; tune shield bite via Enemy.heroRamShieldDamage.
/// </summary>
public class EnemyRammer : EnemySeeker
{
    protected override void TryFire()
    {
    }
}
