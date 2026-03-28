using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjecctileHero : MonoBehaviour
{
    [Header("Dynamic")]
    private BoundsCheck bndCheck;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
    }

    void Update()
    {
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offScreenBottom)) {
            Destroy(this.gameObject);
        }
    }
}
