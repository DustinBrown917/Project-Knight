using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PerspectiveCameraSlider : MonoBehaviour {

    private Camera camCamera;

    [SerializeField]
    private FloatMinMax mm_VelocityModifier = new FloatMinMax(0.1f, 0.2f);
    private float modifiedVelocity;
    [SerializeField, Range(0, 1), Tooltip("Greater value means a quicker decay in velocity when released.")]
    private float velocityDecay = 0.9f;
    [SerializeField]
    private bool clampPosition = true;
    [SerializeField]
    private FloatMinMax mm_xMaxBounds = new FloatMinMax(-5, 5);
    [SerializeField]
    private FloatMinMax mm_yMaxBounds = new FloatMinMax(-5, 5);
    [SerializeField]
    private FloatMinMax mm_ZoomBounds = new FloatMinMax(1, 6);

    private float doubleTouchStartDistance = 0;
    private float doubleTouchNewDistance = 0;
    [SerializeField]
    private float zoomFactor = 0.01f;

    private void Start()
    {
        camCamera = gameObject.GetComponent<Camera>();
        modifiedVelocity = mm_VelocityModifier.Max;
    }

    private float zoomInterpolate;

    void Update () {

        if (Input.touchCount == 1)
        {
            if (SelectionHandler.IsPointerOverUIObject()) { return; }

            if(Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                cameraVelocity = -modifiedVelocity*((Input.GetTouch(0).deltaPosition) * ((Input.GetTouch(0).deltaTime) / 1));

                if(cr_DecelerateCamMovement == null)
                {
                    cr_DecelerateCamMovement = StartCoroutine(CamVelocityDecay());
                }
            }
        } else if (Input.touchCount == 2)
        {
            if(Input.GetTouch(1).phase == TouchPhase.Began)
            {
                doubleTouchStartDistance = DistanceBetweenTouches(0, 1);
            }
            if(Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                doubleTouchNewDistance = DistanceBetweenTouches(0, 1);
                camCamera.fieldOfView += (doubleTouchNewDistance - doubleTouchStartDistance) * -zoomFactor;
                doubleTouchStartDistance = doubleTouchNewDistance;

                zoomInterpolate = (camCamera.fieldOfView - mm_ZoomBounds.Min) / mm_ZoomBounds.Difference;
                modifiedVelocity = mm_VelocityModifier.Lerp(zoomInterpolate);

                if (camCamera.fieldOfView > mm_ZoomBounds.Max) { camCamera.fieldOfView = mm_ZoomBounds.Max; }
                else if(camCamera.fieldOfView < mm_ZoomBounds.Min) { camCamera.fieldOfView = mm_ZoomBounds.Min; }
            }
        }
    }


    private float DistanceBetweenTouches(int index1, int index2)
    {
        return Vector2.Distance(Input.GetTouch(index1).position, Input.GetTouch(index2).position);
    }

    private Vector3 cameraVelocity = Vector3.zero;
    private Vector3 stabilizedPosition = new Vector3();
    private bool stabilized = false;
    private float decelerationRate;
    private Coroutine cr_DecelerateCamMovement = null;
    private IEnumerator CamVelocityDecay()
    {
        while(Mathf.Abs(cameraVelocity.magnitude) > 0.001f)
        {
            if (clampPosition)
            {
                stabilizedPosition = gameObject.transform.position;
                if (gameObject.transform.position.x >= mm_xMaxBounds.Max && cameraVelocity.x > 0) {
                    cameraVelocity.x = 0;
                    stabilizedPosition.x = mm_xMaxBounds.Max;
                    stabilized = true;
                }
                else if (gameObject.transform.position.x <= mm_xMaxBounds.Min && cameraVelocity.x < 0) {
                    cameraVelocity.x = 0;
                    stabilizedPosition.x = mm_xMaxBounds.Min;
                    stabilized = true;
                }

                if (gameObject.transform.position.y >= mm_yMaxBounds.Max && cameraVelocity.y > 0) {
                    cameraVelocity.y = 0;
                    stabilizedPosition.y = mm_yMaxBounds.Max;
                    stabilized = true;
                }
                else if (gameObject.transform.position.y <= mm_yMaxBounds.Min && cameraVelocity.y < 0) {
                    cameraVelocity.y = 0;
                    stabilizedPosition.y = mm_yMaxBounds.Min;
                    stabilized = true;
                }
            }

            if (stabilized)
            {
                gameObject.transform.position = stabilizedPosition;
                stabilized = false;
            }

            gameObject.transform.Translate(cameraVelocity);
            yield return null;
            cameraVelocity *= velocityDecay;
        }

        cr_DecelerateCamMovement = null;
    }

    private enum CameraStates
    {
        STATIONARY,
        MOVING,
        ZOOMING
    }


}
