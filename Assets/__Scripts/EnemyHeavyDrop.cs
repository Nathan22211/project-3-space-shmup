using UnityEngine;

/// <summary>
/// Large enemy that drifts straight down from above; does not fire.
/// </summary>
public class EnemyHeavyDrop : Enemy
{
    protected override void Start()
    {
        // No weapon scheduling.
    }

    protected override void TryFire()
    {
    }
}
