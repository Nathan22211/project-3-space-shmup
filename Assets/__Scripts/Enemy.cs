using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class Enemy : MonoBehaviour
{
    static readonly Collider[] OverlapScratch = new Collider[16];
    static int s_enemyLayerMask = -1;

    [Header("Inscribed")]
    public float speed = 10f;
    public float fireRate = 0.3f;
    public float health = 10;
    public int score = 100;

    [Header("Weapon")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 22f;
    public float fireIntervalMin = 1.1f;
    public float fireIntervalMax = 2.4f;
    [Tooltip("Push spawn along fire direction so the bolt clears the hull.")]
    public float enemyProjectileSpawnPush = 1.2f;

    [Header("Enemy vs enemy")]
    [Tooltip("If > 0, overlapping another enemy within this radius destroys both (kinematic ships often skip OnCollision).")]
    public float mutualDestroyOverlapRadius = 1.35f;

    private BoundsCheck bndCheck;
    private Vector3 formationVelocity;
    private bool formationMove;
    protected float nextFireTime;
    private bool wasOnScreen;

    protected virtual void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        if (s_enemyLayerMask < 0) {
            int l = LayerMask.NameToLayer("Enemy");
            s_enemyLayerMask = l >= 0 ? (1 << l) : 0;
        }
    }

    protected virtual void Start()
    {
        ScheduleNextFire();
    }

    public void AssignProjectile(GameObject prefab, float projSpeed)
    {
        projectilePrefab = prefab;
        projectileSpeed = projSpeed;
        ScheduleNextFire();
    }

    public void SetSideEntryFromLeft(float lateralSpeed)
    {
        formationMove = true;
        formationVelocity = new Vector3(lateralSpeed, -speed, 0f);
    }

    public void SetSideEntryFromRight(float lateralSpeed)
    {
        formationMove = true;
        formationVelocity = new Vector3(-lateralSpeed, -speed, 0f);
    }

    public void ClearFormationMove()
    {
        formationMove = false;
        formationVelocity = Vector3.zero;
    }

    protected void ScheduleNextFire()
    {
        nextFireTime = Time.time + Random.Range(fireIntervalMin, fireIntervalMax);
    }

    public Vector3 pos {
        get {
            return (this.transform.position);
        }
        set {
            this.transform.position = value;
        }
    }

    protected virtual void Update()
    {
        Move();
        if (bndCheck.isOnScreen) {
            wasOnScreen = true;
        }
        TryFire();
        CheckMutualDestroyWithNearbyEnemy();
        if (IsOutOfPlay()) {
            Destroy(gameObject);
        }
    }

    bool IsOutOfPlay()
    {
        if (!wasOnScreen) {
            return false;
        }
        return !bndCheck.isOnScreen;
    }

    public virtual void Move()
    {
        Vector3 tempPos = pos;
        if (formationMove) {
            tempPos += formationVelocity * Time.deltaTime;
        } else {
            tempPos.y -= speed * Time.deltaTime;
        }
        pos = tempPos;
    }

    protected virtual void TryFire()
    {
        if (projectilePrefab == null) {
            return;
        }
        if (Time.time < nextFireTime) {
            return;
        }
        LaunchProjectile(Vector3.down);
        ScheduleNextFire();
    }

    protected void LaunchProjectile(Vector3 worldDir)
    {
        if (projectilePrefab == null) {
            return;
        }
        worldDir.z = 0f;
        if (worldDir.sqrMagnitude < 1e-6f) {
            worldDir = Vector3.down;
        }
        worldDir.Normalize();

        Vector3 spawnPos = transform.position + worldDir * enemyProjectileSpawnPush;
        GameObject projGO = Instantiate(projectilePrefab);
        projGO.transform.position = spawnPos;
        Rigidbody rb = projGO.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = worldDir * projectileSpeed;
        }
        ProjectileEnemy pe = projGO.GetComponent<ProjectileEnemy>();
        if (pe != null) {
            pe.SetShooterRoot(transform.root);
        }
    }

    void CheckMutualDestroyWithNearbyEnemy()
    {
        if (mutualDestroyOverlapRadius <= 0f || s_enemyLayerMask == 0) {
            return;
        }
        int n = Physics.OverlapSphereNonAlloc(
            transform.position,
            mutualDestroyOverlapRadius,
            OverlapScratch,
            s_enemyLayerMask,
            QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++) {
            Collider c = OverlapScratch[i];
            Enemy o = c.GetComponentInParent<Enemy>();
            if (o == null || o == this) {
                continue;
            }
            if (GetInstanceID() < o.GetInstanceID()) {
                Destroy(o.gameObject);
                Destroy(gameObject);
                return;
            }
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;
        if (otherGO.GetComponent<ProjecctileHero>() != null) {
            Destroy(otherGO);
            Destroy(gameObject);
            return;
        }
        Enemy otherEnemy = otherGO.GetComponentInParent<Enemy>();
        if (otherEnemy != null && otherEnemy != this) {
            if (GetInstanceID() < otherEnemy.GetInstanceID()) {
                Destroy(otherEnemy.gameObject);
                Destroy(gameObject);
            }
        }
    }
}
