using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileEnemy : MonoBehaviour
{
    private BoundsCheck bndCheck;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
    }

    void Update()
    {
        // Downward shots leave through the bottom → BoundsCheck sets offScreenTop (see BoundsCheck.LateUpdate).
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenTop)
            || bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenBottom)
            || bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenLeft)
            || bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenRight)) {
            Destroy(gameObject);
        }
    }
}
