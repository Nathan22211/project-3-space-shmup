using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S {get; private set; }
    [Header("Inscribed")]

    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;

    [Header("Shield")]
    public int maxShieldLevel = 4;
    [Tooltip("Seconds until the first shield layer returns after taking damage.")]
    public float shieldRegenBaseDelay = 2.75f;
    [Tooltip("Each restored layer multiplies the next wait by this (>1). Keep close to 1 for a mild curve (e.g. 1.06–1.12).")]
    public float shieldRegenDelayExponent = 1.09f;

    [Header("Dynamic")]
    private float _shieldLevel = 1;
    [Tooltip("This field holds a reference to the last triggering object")]
    private GameObject lastTriggerObj = null;
    float _shieldRegenTimer;
    int _shieldRegenStep;

    void Awake()
    {
        if (S == null) {
            S = this;
        } else {
            Debug.LogError("Hero.Awake() - Attempted to assign Singleton Hero.S - failed!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.y += vAxis * speed * Time.deltaTime;
        transform.position = pos;

        transform.rotation = Quaternion.Euler(vAxis * pitchMult, hAxis * rollMult, 0);

        if (Input.GetKeyDown(KeyCode.Space)) {
            TempFire();
        }

        TickShieldRegen();
    }

    void TickShieldRegen()
    {
        if (_shieldLevel >= maxShieldLevel) {
            return;
        }
        if (shieldRegenBaseDelay <= 0f || shieldRegenDelayExponent < 1.0001f) {
            return;
        }
        _shieldRegenTimer += Time.deltaTime;
        float need = shieldRegenBaseDelay * Mathf.Pow(shieldRegenDelayExponent, _shieldRegenStep);
        if (_shieldRegenTimer < need) {
            return;
        }
        _shieldRegenTimer = 0f;
        _shieldRegenStep++;
        shieldLevel = Mathf.Min(_shieldLevel + 1f, maxShieldLevel);
    }

    void TempFire() {
        GameObject projGO = Instantiate<GameObject>(projectilePrefab);
        projGO.transform.position = transform.position;
        Rigidbody rb = projGO.GetComponent<Rigidbody>();
        rb.velocity = Vector3.up * projectileSpeed;
    }

    void OnTriggerEnter(Collider other) {
        ProjectileEnemy bullet = other.GetComponentInParent<ProjectileEnemy>();
        if (bullet != null) {
            shieldLevel--;
            Destroy(bullet.gameObject);
            return;
        }

        Transform rootT = other.gameObject.transform.root;
        GameObject rootGO = rootT.gameObject;

        if (rootGO == lastTriggerObj) {
            return;
        }
        lastTriggerObj = rootGO;

        Enemy enemy = rootGO.GetComponent<Enemy>();
        if (enemy != null) {
            shieldLevel--;
            Destroy(rootGO);
        } else {
            Debug.Log("Hero hit by non-Enemy: " + rootGO.name);
        }
    }

    public float shieldLevel {
        get {
            return _shieldLevel;
        }
        private set {
            float prev = _shieldLevel;
            _shieldLevel = Mathf.Min(value, maxShieldLevel);
            if (_shieldLevel < prev) {
                _shieldRegenTimer = 0f;
                _shieldRegenStep = 0;
            }
            if (value < 0) {
                Destroy(this.gameObject);
            }
        }
    }
}
