using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileEnemy : MonoBehaviour
{
    private BoundsCheck bndCheck;
    Transform _shooterRoot;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
    }

    public void SetShooterRoot(Transform shooterRoot)
    {
        _shooterRoot = shooterRoot;
    }

    void Update()
    {
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenTop)
            || bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenBottom)
            || bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenLeft)
            || bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenRight)) {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_shooterRoot != null && other.transform.root == _shooterRoot) {
            return;
        }
        Enemy e = other.GetComponentInParent<Enemy>();
        if (e != null) {
            Destroy(e.gameObject);
            Destroy(gameObject);
        }
    }
}
