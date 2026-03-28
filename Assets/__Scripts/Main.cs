using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    static private Main S;

    [Header("Inscribed")]
    public GameObject[] prefabEnemies;
    public GameObject enemyProjectilePrefab;
    public float enemyProjectileSpeed = 24f;

    [Header("Side formation (3 ships)")]
    public float sideFormationWaveInterval = 5f;
    [Tooltip("Distance along X between ships (abreast, parallel to the screen top/bottom).")]
    public float sideFormationAlongEdgeSpacing = 5f;
    public float sideFormationLateralSpeed = 12f;
    public float sideFormationSpawnPadding = 3f;
    [Tooltip("Vertical position in view: 0 = bottom edge, 1 = top edge (uses main camera).")]
    [Range(0.05f, 0.95f)] public float formationSpawnYMinFrac = 0.62f;
    [Tooltip("Vertical position in view: 0 = bottom edge, 1 = top edge.")]
    [Range(0.05f, 0.95f)] public float formationSpawnYMaxFrac = 0.92f;

    [Header("Seeker (single homing ship)")]
    public GameObject seekerPrefab;
    public float seekerWaveInterval = 7f;
    public float seekerSpawnTopPadding = 4f;

    [Header("Side runner (#3 — straight across, faces travel)")]
    public GameObject sideRunnerPrefab;
    public float sideRunnerWaveInterval = 6f;
    public float sideRunnerSpeed = 16f;
    public float sideRunnerSpawnPadding = 3f;
    [Tooltip("Vertical band in view: 0 = bottom, 1 = top.")]
    [Range(0.05f, 0.95f)] public float sideRunnerSpawnYMinFrac = 0.25f;
    [Range(0.05f, 0.95f)] public float sideRunnerSpawnYMaxFrac = 0.85f;

    [Header("Heavy drop (#4 — larger, slow, no shots)")]
    public GameObject heavyDropPrefab;
    public float heavyDropWaveInterval = 9f;
    public float heavyDropSpawnTopPadding = 6f;
    [Tooltip("Random X as a fraction of camera half-width from center.")]
    [Range(0.1f, 0.48f)] public float heavyDropSpawnXSpread = 0.36f;

    public float enemyDefaultPadding = 1.5f;
    public float gameRestartDelay = 2f;

    private BoundsCheck bndCheck;
    private bool _warnedSideRunnerPrefabMissing;

    void Awake()
    {
        S = this;
        bndCheck = GetComponent<BoundsCheck>();
    }

    void Start()
    {
        StartCoroutine(SideFormationWaves());
        StartCoroutine(SeekerWaves());
        StartCoroutine(SideRunnerWaves());
        StartCoroutine(HeavyDropWaves());
    }

    IEnumerator SideRunnerWaves()
    {
        Debug.Log(
            $"[SideRunner] Coroutine SideRunnerWaves started (prefab={(sideRunnerPrefab != null ? sideRunnerPrefab.name : "NULL — assign on Main")}).");
        yield return new WaitForSeconds(1.25f);
        while (true) {
            if (sideRunnerPrefab != null) {
                SpawnSideRunner();
            } else if (!_warnedSideRunnerPrefabMissing) {
                _warnedSideRunnerPrefabMissing = true;
                Debug.LogWarning(
                    "[SideRunner] sideRunnerPrefab is not assigned on Main — no runners will spawn. Drag EnemySideRunner prefab into the field.");
            }
            yield return new WaitForSeconds(sideRunnerWaveInterval);
        }
    }

    void SpawnSideRunner()
    {
        Camera cam = Camera.main;
        if (cam == null) {
            Debug.LogWarning("[SideRunner] SpawnSideRunner aborted — Camera.main is null (need a camera tagged MainCamera).");
            return;
        }
        bool fromLeft = Random.value < 0.5f;
        float y = RandomSideRunnerY();
        float x = FormationBaseXForPrefab(fromLeft, sideRunnerSpawnPadding, sideRunnerPrefab);
        GameObject go = Instantiate(sideRunnerPrefab);
        go.transform.position = new Vector3(x, y, 0f);
        EnemySideRunner runner = go.GetComponent<EnemySideRunner>();
        if (runner != null) {
            if (fromLeft) {
                runner.SetStraightRunFromLeft(sideRunnerSpeed);
            } else {
                runner.SetStraightRunFromRight(sideRunnerSpeed);
            }
        }
        Enemy e = go.GetComponent<Enemy>();
        if (e != null && enemyProjectilePrefab != null) {
            e.AssignProjectile(enemyProjectilePrefab, enemyProjectileSpeed);
        }
        Debug.Log(
            $"[SideRunner] Spawn fromLeft={fromLeft} pos=({x:F2}, {y:F2}) " +
            $"hasRunnerScript={runner != null} instanceID={go.GetInstanceID()}");
    }

    float RandomSideRunnerY()
    {
        Camera cam = Camera.main;
        if (cam == null) {
            return 0f;
        }
        float ch = cam.orthographicSize;
        float cy = cam.transform.position.y;
        float t0 = Mathf.Min(sideRunnerSpawnYMinFrac, sideRunnerSpawnYMaxFrac);
        float t1 = Mathf.Max(sideRunnerSpawnYMinFrac, sideRunnerSpawnYMaxFrac);
        float yBottom = cy - ch;
        return Random.Range(yBottom + t0 * 2f * ch, yBottom + t1 * 2f * ch);
    }

    float FormationBaseXForPrefab(bool fromLeft, float padding, GameObject prefabForInset)
    {
        Camera cam = Camera.main;
        if (cam == null) {
            return 0f;
        }
        float cx = cam.transform.position.x;
        float inset = enemyDefaultPadding;
        if (prefabForInset != null) {
            BoundsCheck eb = prefabForInset.GetComponent<BoundsCheck>();
            if (eb != null) {
                inset = Mathf.Max(inset, Mathf.Abs(eb.radius));
            }
        }
        float halfW = (bndCheck != null && bndCheck.camWidth > 1e-4f)
            ? bndCheck.camWidth
            : cam.orthographicSize * cam.aspect;
        return fromLeft
            ? (cx - halfW - inset - padding)
            : (cx + halfW + inset + padding);
    }

    IEnumerator SeekerWaves()
    {
        yield return new WaitForSeconds(2.5f);
        while (true) {
            if (seekerPrefab != null) {
                SpawnSeeker();
            }
            yield return new WaitForSeconds(seekerWaveInterval);
        }
    }

    IEnumerator HeavyDropWaves()
    {
        yield return new WaitForSeconds(4f);
        while (true) {
            if (heavyDropPrefab != null) {
                SpawnHeavyDrop();
            }
            yield return new WaitForSeconds(heavyDropWaveInterval);
        }
    }

    void SpawnHeavyDrop()
    {
        Camera cam = Camera.main;
        if (cam == null) {
            return;
        }
        float cx = cam.transform.position.x;
        float cy = cam.transform.position.y;
        float ch = cam.orthographicSize;
        float halfW = (bndCheck != null && bndCheck.camWidth > 1e-4f)
            ? bndCheck.camWidth
            : ch * cam.aspect;
        float y = cy + ch + heavyDropSpawnTopPadding;
        float x = cx + Random.Range(-halfW * heavyDropSpawnXSpread, halfW * heavyDropSpawnXSpread);
        GameObject go = Instantiate(heavyDropPrefab);
        go.transform.position = new Vector3(x, y, 0f);
    }

    void SpawnSeeker()
    {
        Camera cam = Camera.main;
        if (cam == null) {
            return;
        }
        float cx = cam.transform.position.x;
        float cy = cam.transform.position.y;
        float ch = cam.orthographicSize;
        float y = cy + ch + seekerSpawnTopPadding;
        float x = cx + Random.Range(-bndCheck.camWidth * 0.45f, bndCheck.camWidth * 0.45f);
        GameObject go = Instantiate(seekerPrefab);
        go.transform.position = new Vector3(x, y, 0f);
        Enemy e = go.GetComponent<Enemy>();
        if (e != null && enemyProjectilePrefab != null) {
            e.AssignProjectile(enemyProjectilePrefab, enemyProjectileSpeed);
        }
    }

    IEnumerator SideFormationWaves()
    {
        yield return new WaitForSeconds(0.75f);
        while (true) {
            if (prefabEnemies != null && prefabEnemies.Length > 0) {
                SpawnSideFormationTrio();
            }
            yield return new WaitForSeconds(sideFormationWaveInterval);
        }
    }

    void SpawnSideFormationTrio()
    {
        bool fromLeft = Random.value < 0.5f;
        float yMid = RandomFormationY();
        float baseX = FormationBaseX(fromLeft);
        for (int i = -1; i <= 1; i++) {
            float x = baseX + i * sideFormationAlongEdgeSpacing;
            SpawnFormationShip(fromLeft, yMid, x);
        }
    }

    float RandomFormationY()
    {
        Camera cam = Camera.main;
        if (cam == null) {
            return 0f;
        }
        float ch = cam.orthographicSize;
        float cy = cam.transform.position.y;
        float t0 = Mathf.Min(formationSpawnYMinFrac, formationSpawnYMaxFrac);
        float t1 = Mathf.Max(formationSpawnYMinFrac, formationSpawnYMaxFrac);
        float yBottom = cy - ch;
        return Random.Range(yBottom + t0 * 2f * ch, yBottom + t1 * 2f * ch);
    }

    float FormationBaseX(bool fromLeft)
    {
        GameObject insetSrc = (prefabEnemies != null && prefabEnemies.Length > 0)
            ? prefabEnemies[0]
            : null;
        return FormationBaseXForPrefab(fromLeft, sideFormationSpawnPadding, insetSrc);
    }

    void SpawnFormationShip(bool fromLeft, float worldY, float worldX)
    {
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate(prefabEnemies[ndx]);
        go.transform.position = new Vector3(worldX, worldY, 0f);

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null) {
            if (fromLeft) {
                enemy.SetSideEntryFromLeft(sideFormationLateralSpeed);
            } else {
                enemy.SetSideEntryFromRight(sideFormationLateralSpeed);
            }
            if (enemyProjectilePrefab != null) {
                enemy.AssignProjectile(enemyProjectilePrefab, enemyProjectileSpeed);
            }
        }
    }

    void DelayedRestart() {
        Invoke( nameof(Restart), gameRestartDelay);
    }

    void Restart() {
        SceneManager.LoadScene("_Scene_0");
    }

    static public void END_GAME() {
        S.DelayedRestart();
    }
}
