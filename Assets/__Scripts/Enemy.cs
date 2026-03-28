using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class Enemy : MonoBehaviour
{
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

    private BoundsCheck bndCheck;
    private Vector3 formationVelocity;
    private bool formationMove;
    private float nextFireTime;
    private bool wasOnScreen;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
    }

    void Start()
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

    void ScheduleNextFire()
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

    void Update()
    {
        Move();
        if (bndCheck.isOnScreen) {
            wasOnScreen = true;
        }
        TryFire();
        if (IsOutOfPlay()) {
            Destroy(gameObject);
        }
    }

    bool IsOutOfPlay()
    {
        // BoundsCheck vertical labels are swapped vs world space: past bottom edge → offScreenTop, past top → offScreenBottom
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenTop)
            || bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenBottom)) {
            return true;
        }
        if (!wasOnScreen) {
            return false;
        }
        return bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenLeft)
            || bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenRight);
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

    void TryFire()
    {
        if (projectilePrefab == null) {
            return;
        }
        if (Time.time < nextFireTime) {
            return;
        }
        GameObject projGO = Instantiate(projectilePrefab);
        projGO.transform.position = transform.position;
        Rigidbody rb = projGO.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = Vector3.down * projectileSpeed;
        }
        ScheduleNextFire();
    }

    void OnCollisionEnter(Collision coll) {
        GameObject otherGO = coll.gameObject;
        if (otherGO.GetComponent<ProjecctileHero>() != null) {
            Destroy(otherGO);
            Destroy(gameObject);
        }
    }
}
