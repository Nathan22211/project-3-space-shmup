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

    public float enemyDefaultPadding = 1.5f;
    public float gameRestartDelay = 2f;

    private BoundsCheck bndCheck;

    void Awake()
    {
        S = this;
        bndCheck = GetComponent<BoundsCheck>();
    }

    void Start()
    {
        StartCoroutine(SideFormationWaves());
        StartCoroutine(SeekerWaves());
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
        Camera cam = Camera.main;
        if (cam == null) {
            return 0f;
        }
        float cx = cam.transform.position.x;
        float inset = enemyDefaultPadding;
        if (prefabEnemies != null && prefabEnemies.Length > 0) {
            BoundsCheck eb = prefabEnemies[0].GetComponent<BoundsCheck>();
            if (eb != null) {
                inset = Mathf.Max(inset, Mathf.Abs(eb.radius));
            }
        }
        return fromLeft
            ? (cx - bndCheck.camWidth - inset - sideFormationSpawnPadding)
            : (cx + bndCheck.camWidth + inset + sideFormationSpawnPadding);
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
