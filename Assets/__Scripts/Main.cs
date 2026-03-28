using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    static private Main S;

    [Header("Inscribed")]
    public GameObject[] prefabEnemies;
    public float enemySpawnPerSecond = 0.5f;
    public float enemyDefaultPadding = 1.5f;
    public float gameRestartDelay = 2f;

    private BoundsCheck bndCheck;

    void Awake()
    {
        S = this;
        bndCheck = GetComponent<BoundsCheck>();

        Invoke( nameof(SpawnEnemy), 1f/enemySpawnPerSecond);
    }

    public void SpawnEnemy() {

        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);

        float enemyInset = enemyDefaultPadding;
        if (go.GetComponent<BoundsCheck>() != null) {
            enemyInset = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        }

        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-bndCheck.camWidth + enemyInset, bndCheck.camWidth - enemyInset);
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;

        Invoke( nameof(SpawnEnemy), 1f/enemySpawnPerSecond);
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
