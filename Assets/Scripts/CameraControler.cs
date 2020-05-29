using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControler : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform camTransform;
    private Vector3 target;
    private Vector3 start;

    private float speed = 1;

    private float timeSinceLastCheck;
    private float checkInterval = 0.1f;

    public Transform ballsParent;

    private float defZ = -8;
    private float defX;

    private bool run= false;
    public void Initialze(Vector2 startingTarget)
    {
        defX = startingTarget.x;
        SnapToTarget(startingTarget);
        camTransform.position = target;

        timeSinceLastCheck = 0;

        run = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        if (run)
        {
            if (timeSinceLastCheck > checkInterval)
            {
                CheckForNewTarget();
                timeSinceLastCheck = 0;
            }

            camTransform.position = Vector3.Lerp(camTransform.position, target, speed * Time.deltaTime);

            timeSinceLastCheck += Time.deltaTime;
        }
    }

    void CheckForNewTarget()
    {
        for (int i = 0; i < ballsParent.childCount;i++)
        {
            Vector2 tempPos = ballsParent.GetChild(i).transform.position;
            if (tempPos.y < target.y) 
            SnapToTarget(tempPos);
        }
    }

    void SnapToTarget(Vector2 newTarget)
    {
        target = new Vector3(defX, newTarget.y,defZ);
    }


}
