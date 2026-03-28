using UnityEngine;

/// <summary>
/// Single ship that steers toward the hero with a capped turn rate (cannot snap into a full U-turn).
/// </summary>
public class EnemySeeker : Enemy
{
    [Header("Seeker")]
    [Tooltip("Max degrees the heading can rotate per second toward the player.")]
    public float maxTurnRateDegrees = 95f;
    [Tooltip("Extra Z rotation (degrees) after aligning transform.up to velocity — tune if the mesh nose is wrong.")]
    public float headingFacingOffset = 0f;

    Vector2 _heading;

    protected override void Awake()
    {
        base.Awake();
        _heading = InitialHeading();
    }

    protected override void Start()
    {
        base.Start();
    }

    Vector2 InitialHeading()
    {
        Vector2 d = FlatDeltaToHero(transform.position);
        if (d.sqrMagnitude > 1e-6f) {
            return d.normalized;
        }
        return Vector2.down;
    }

    static Vector2 FlatDeltaToHero(Vector3 fromWorld)
    {
        if (Hero.S == null) {
            return Vector2.zero;
        }
        Vector3 d = Hero.S.transform.position - fromWorld;
        d.z = 0f;
        return new Vector2(d.x, d.y);
    }

    public override void Move()
    {
        Vector2 delta = FlatDeltaToHero(transform.position);
        Vector2 desired = delta.sqrMagnitude > 1e-6f ? delta.normalized : _heading;

        float curDeg = Mathf.Atan2(_heading.y, _heading.x) * Mathf.Rad2Deg;
        float tgtDeg = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
        float turn = Mathf.DeltaAngle(curDeg, tgtDeg);
        float maxStep = maxTurnRateDegrees * Time.deltaTime;
        turn = Mathf.Clamp(turn, -maxStep, maxStep);
        float newDeg = curDeg + turn;
        float rad = newDeg * Mathf.Deg2Rad;
        _heading = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        pos += (Vector3)(_heading * speed * Time.deltaTime);
        ApplyFacing();
    }

    protected override void TryFire()
    {
        if (projectilePrefab == null) {
            return;
        }
        if (Time.time < nextFireTime) {
            return;
        }
        Vector3 dir = Vector3.down;
        if (Hero.S != null) {
            Vector3 d = Hero.S.transform.position - transform.position;
            d.z = 0f;
            if (d.sqrMagnitude > 1e-6f) {
                dir = d.normalized;
            }
        }
        LaunchProjectile(dir);
        ScheduleNextFire();
    }

    void ApplyFacing()
    {
        if (_heading.sqrMagnitude < 1e-6f) {
            return;
        }
        Vector3 h = new Vector3(_heading.x, _heading.y, 0f).normalized;
        transform.up = h;
        if (Mathf.Abs(headingFacingOffset) > 0.01f) {
            transform.Rotate(0f, 0f, headingFacingOffset, Space.Self);
        }
    }
}
