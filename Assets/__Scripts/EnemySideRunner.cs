using UnityEngine;

/// <summary>
/// Flies in a straight horizontal line from one side of the screen to the other, facing travel direction.
/// </summary>
public class EnemySideRunner : Enemy
{
    [Tooltip("Extra Z degrees after aligning transform.up to flight direction (mesh nose tweak).")]
    public float headingFacingOffset = 0f;

    Vector3 _cruise;

    public void SetStraightRunFromLeft(float runSpeed)
    {
        _cruise = new Vector3(Mathf.Abs(runSpeed), 0f, 0f);
    }

    public void SetStraightRunFromRight(float runSpeed)
    {
        _cruise = new Vector3(-Mathf.Abs(runSpeed), 0f, 0f);
    }

    public override void Move()
    {
        pos += _cruise * Time.deltaTime;
        ApplyFacing();
    }

    void ApplyFacing()
    {
        if (_cruise.sqrMagnitude < 1e-6f) {
            return;
        }
        Vector3 h = new Vector3(_cruise.x, _cruise.y, 0f).normalized;
        transform.up = h;
        if (Mathf.Abs(headingFacingOffset) > 0.01f) {
            transform.Rotate(0f, 0f, headingFacingOffset, Space.Self);
        }
    }

    void OnDestroy()
    {
        Debug.Log(
            $"[SideRunner] Destroyed frame={Time.frameCount} " +
            $"pos=({transform.position.x:F2}, {transform.position.y:F2}) instanceID={GetInstanceID()}");
    }
}
