using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsCheck : MonoBehaviour
{
    public enum eType {center, inset, outset};
    [Header("Inscribed")]
    public eType boundsType = eType.center;
    public float radius = 1f;
    public bool keepOnScreen = true;

    [Header("Dynamic")]
    public eScreenLocs screenLocs = eScreenLocs.onScreen;
    public float camWidth;
    public float camHeight;

    [System.Flags]
    public enum eScreenLocs {
        onScreen = 0,
        offScreenLeft = 1,
        offScreenRight = 2,
        offScreenBottom = 4,
        offScreenTop = 8
    }
    void Awake()
    {
        camHeight = Camera.main.orthographicSize;
        camWidth = camHeight * Camera.main.aspect;
    }

    void LateUpdate()
    {
        float checkRadius = 0;
        if (boundsType == eType.inset || boundsType == eType.center) {
            checkRadius = -radius;
        }
        if (boundsType == eType.outset || boundsType == eType.center) {
            checkRadius = radius;
        }

        Vector3 pos = transform.position;
        screenLocs = eScreenLocs.onScreen;
        if (pos.x > camWidth + checkRadius) {
            pos.x = -camWidth - checkRadius;
            screenLocs = eScreenLocs.offScreenLeft;
        }
        if (pos.x < -camWidth - checkRadius) {
            pos.x = camWidth + checkRadius;
            screenLocs = eScreenLocs.offScreenRight;
        }
        if (pos.y > camHeight + checkRadius) {
            pos.y = -camHeight - checkRadius;
            screenLocs = eScreenLocs.offScreenBottom;
        }
        if (pos.y < -camHeight - checkRadius) {
            pos.y = camHeight + checkRadius;
            screenLocs = eScreenLocs.offScreenTop;
        }

        if (keepOnScreen && screenLocs != eScreenLocs.onScreen) {
            transform.position = pos;
            screenLocs = eScreenLocs.onScreen;
        }
    }

    public bool isOnScreen {
        get {
            return (screenLocs == eScreenLocs.onScreen);
        }
    }

    public bool LocIs(eScreenLocs checkLoc) {
        if (checkLoc == eScreenLocs.onScreen) {
            return isOnScreen;
        }
        return ((screenLocs & checkLoc) == checkLoc);
    }
}
