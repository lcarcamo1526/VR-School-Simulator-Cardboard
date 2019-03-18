// The MIT License (MIT)
//
// Copyright (c) 2015, Unity Technologies & Google, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR;
/// This script provides an implemention of Unity's `BaseInputModule` class, so
/// that Canvas-based (_uGUI_) UI elements can be selected by looking at them and
/// pulling the viewer's trigger or touching the screen.
/// This uses the player's gaze and the trigger as a raycast generator.
///
/// To use, attach to the scene's **EventSystem** object.  Be sure to move it above the
/// other modules, such as _TouchInputModule_ and _StandaloneInputModule_, in order
/// for the user's gaze to take priority in the event system.
///
/// Next, set the **Canvas** object's _Render Mode_ to **World Space**, and set its _Event Camera_
/// to a (mono) camera that is controlled by a NvrHead.  If you'd like gaze to work
/// with 3D scene objects, add a _PhysicsRaycaster_ to the gazing camera, and add a
/// component that implements one of the _Event_ interfaces (_EventTrigger_ will work nicely).
/// The objects must have colliders too.
///
/// GazeInputModule emits the following events: _Enter_, _Exit_, _Down_, _Up_, _Click_, _Select_,
/// _Deselect_, and _UpdateSelected_.  Scroll, move, and submit/cancel events are not emitted.
public class NvrGazeInputModule : BaseInputModule {    
    // The INvrGazePointer which will be responding to gaze events.
    public static NvrGazePointer gazePointer;
    //[HideInInspector]
    //public bool unityVRModeEnabled;
    // Does the gameobject at gaze have an Interactive object script, and if so is it within the 'distance' var from camera
    [HideInInspector]
    public bool interactiveObjectWithinDistance;

    // Center of screen point. This is different for GoogleVR and Native Unity VR modes
    private Vector2 centerOfScreen;
    // Current Pointer Data
    private PointerEventData pointerData;
    // The previous headpose
    private Vector2 lastHeadPose;    
    // Time in seconds between the pointer down and up events sent by a trigger.
    // Allows time for the UI elements to make their state transitions.
    private const float clickTime = 0.1f;

    protected override void Start() {
        // Set the CenterOfScreen var
        SetCenterOfScreen();
    }

    public void SetCenterOfScreen() {
#if UNITY_5_5_2
        centerOfScreen = new Vector2(Screen.width / 2, Screen.height / 2);
#else
        // GearVR, or native Daydream/cardboard (pre 5.5.2)
        if (UnityEngine.XR.XRSettings.enabled == true) {
            // Center of screen is center of eye texture width
            centerOfScreen = new Vector2((float)UnityEngine.XR.XRSettings.eyeTextureWidth / 2, (float)UnityEngine.XR.XRSettings.eyeTextureHeight / 2);
        }
        // VR support is not on (Using Cardboard SDK)
        else {
            // Get center of screen the normal way
            centerOfScreen = new Vector2(Screen.width / 2, Screen.height / 2);
        }
#endif
    }

    public override void DeactivateModule() {
        DisableGazePointer();
        base.DeactivateModule();
        if (pointerData != null) {
            HandlePendingClick();
            HandlePointerExitAndEnter(pointerData, null);
            pointerData = null;
        }
        eventSystem.SetSelectedGameObject(null, GetBaseEventData());
    }

    public override bool IsPointerOverGameObject(int pointerId) {
        return pointerData != null && pointerData.pointerEnter != null;
    }

    public override void Process() {
        // Save the previous Game Object
        GameObject gazeObjectPrevious = GetCurrentGameObject();
        // Cast the ray from gaze
        CastRayFromGaze();
        // Find out if there is a gameobject at the gaze position
        GameObject gameObjectAtGaze = pointerData.pointerCurrentRaycast.gameObject;
        // Set the 'interactive object within distance' var to false until we check distance
        interactiveObjectWithinDistance = false;
        // If there is a gameobject at the gaze position
        if (gameObjectAtGaze) {
            // Check to see if it has an InteractObject script
            NvrInteractiveObject interactiveObjectScript = gameObjectAtGaze.GetComponent<NvrInteractiveObject>();
            // If it has the InteractiveObject script
            if (interactiveObjectScript) {
                // Check the distance to see if player is close enough to interact, and make sure interaction is enabled on the gameobject
                if (Vector3.Distance(gameObjectAtGaze.transform.position, Camera.main.transform.position) <= interactiveObjectScript.distance && interactiveObjectScript.interactionEnabled) {
                    // Now we can set the 'interactive object within distance' var to true
                    interactiveObjectWithinDistance = true;
                    // And update the current object in the Event System
                    UpdateCurrentObject();
                    // And update the Reticle because it's over an interactive object
                    UpdateReticle(gazeObjectPrevious);
                }
                // Interacte object is too far away, or interaction is disabled
                else {
                    // Make the pointer data null, so the event system thinks there is nothing at the pointer
                    HandlePointerExitAndEnter(pointerData, null);
                    pointerData = new PointerEventData(eventSystem);
                    UpdateReticle(gazeObjectPrevious);
                }
            }
            // If the gameobject is on layer 5 (should be the 'UI' Layer)
            else if (gameObjectAtGaze.layer == 5) {
                // Try to find an 'InteractiveUI' script
                NvrInteractiveUI interactiveUIScript = gameObjectAtGaze.GetComponent<NvrInteractiveUI>();
                // Object has UI script
                if (interactiveUIScript) {
                    // And is within distance
                    if (Vector3.Distance(gameObjectAtGaze.transform.position, Camera.main.transform.position) <= interactiveUIScript.distance) {
                        // Now we can set the 'interactive object within distance' var to true
                        interactiveObjectWithinDistance = true;
                        UpdateCurrentObject();
                        UpdateReticle(gazeObjectPrevious);
                    }
                    // Object is too far away, nothing at pointer
                    else {
                        HandlePointerExitAndEnter(pointerData, null);
                        pointerData = new PointerEventData(eventSystem);
                        UpdateReticle(gazeObjectPrevious);
                    }
                }
                // UI Object does not have a VR UI script, nothing at pointer
                else {
                    HandlePointerExitAndEnter(pointerData, null);
                    pointerData = new PointerEventData(eventSystem);
                    UpdateReticle(gazeObjectPrevious);
                }
            // Not a UI object & not an Interactive object, perhaps it just has a collider and event trigger- handle normally??
            } else {
                UpdateCurrentObject();
                UpdateReticle(gazeObjectPrevious);
            }
        }
        // There is no gameobject at gaze, update normally
        else {
            UpdateCurrentObject();
            UpdateReticle(gazeObjectPrevious);
        }

        bool isNvrTriggered = Input.GetMouseButtonDown(0);
        bool handlePendingClickRequired = !Input.GetMouseButton(0);

        /* Native GoogleVR Only
        #if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
        handlePendingClickRequired &= !GvrController.ClickButton;
        isGvrTriggered |= GvrController.ClickButtonDown;
        #endif
        */

        // Handle input
        if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0)) {
            HandleDrag();
        } else if (Time.unscaledTime - pointerData.clickTime < clickTime) {
            // Delay new events until clickTime has passed.
        } else if (!pointerData.eligibleForClick &&
            (isNvrTriggered || Input.GetMouseButtonDown(0))) {
            // New trigger action.
            HandleTrigger();
        } else if (handlePendingClickRequired) {
            // Check if there is a pending click to handle.
            HandlePendingClick();
        }
    }
    
    private void CastRayFromGaze() {
        Quaternion headOrientation;
        /* Native GoogleVR Only
        #if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
        headOrientation = InputTracking.GetLocalRotation(VRNode.Head);
        #else
        */
        headOrientation = Camera.main.transform.rotation;
        //#endif

        Vector2 headPose = NormalizedCartesianToSpherical(headOrientation * Vector3.forward);

        if (pointerData == null) {
            pointerData = new PointerEventData(eventSystem);
            lastHeadPose = headPose;
        }

        // Cast a ray into the scene
        pointerData.Reset();
        pointerData.position = centerOfScreen;
        eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
        pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        if (pointerData.pointerCurrentRaycast.gameObject != null) {
            if (pointerData.pointerCurrentRaycast.gameObject.name == "VRMain") {
                if (m_RaycastResultCache.Count > 1) {
                    pointerData.pointerCurrentRaycast = m_RaycastResultCache[1];
                }
                else {
                    pointerData.Reset();
                }
            }
        }
        m_RaycastResultCache.Clear();
        pointerData.delta = headPose - lastHeadPose;
        lastHeadPose = headPose;
        //Debug.DrawLine(Camera.main.transform.position, pointerData.pointerCurrentRaycast.worldPosition, Color.red, 10f);
    }

    private void UpdateCurrentObject() {
        // Send enter events and update the highlight.
        var go = pointerData.pointerCurrentRaycast.gameObject;
        HandlePointerExitAndEnter(pointerData, go);
        // Update the current selection, or clear if it is no longer the current object.
        var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
        if (selected == eventSystem.currentSelectedGameObject) {
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
        }
        else {
            eventSystem.SetSelectedGameObject(null, pointerData);
        }
    }

    void UpdateReticle(GameObject previousGazedObject) {
        if (gazePointer == null) {
            return;
        }
        Camera camera = pointerData.enterEventCamera; // Get the camera
        GameObject gazeObject = GetCurrentGameObject(); // Get the gaze target
        Vector3 intersectionPosition = GetIntersectionPosition();
        bool isInteractive = pointerData.pointerPress != null || ExecuteEvents.GetEventHandler<IPointerClickHandler>(gazeObject) != null;
        if (gazeObject == previousGazedObject) {
            if (gazeObject != null) {
                gazePointer.OnGazeStay(camera, gazeObject, intersectionPosition, isInteractive);
            }
        } else {
            if (previousGazedObject != null) {
                gazePointer.OnGazeExit(camera, previousGazedObject);
            }
            if (gazeObject != null) {
                gazePointer.OnGazeStart(camera, gazeObject, intersectionPosition, isInteractive);
            }
        }
    }

    private void HandleDrag() {
        bool moving = pointerData.IsPointerMoving();
        if (moving && pointerData.pointerDrag != null && !pointerData.dragging) {
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
                ExecuteEvents.beginDragHandler);
            pointerData.dragging = true;
        }
        // Drag notification
        if (pointerData.dragging && moving && pointerData.pointerDrag != null) {
            // Before doing drag we should cancel any pointer down state
            // And clear selection!
            if (pointerData.pointerPress != pointerData.pointerDrag) {
                ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);

                pointerData.eligibleForClick = false;
                pointerData.pointerPress = null;
                pointerData.rawPointerPress = null;
            }
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
        }
    }

    private void HandlePendingClick() {
        if (!pointerData.eligibleForClick && !pointerData.dragging) {
            return;
        }
        if (gazePointer != null) {
            Camera camera = pointerData.enterEventCamera;
            gazePointer.OnGazeTriggerEnd(camera);
        }
        var go = pointerData.pointerCurrentRaycast.gameObject;
        // Send pointer up and click events.
        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
        if (pointerData.eligibleForClick) {
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
        } else if (pointerData.dragging) {
            ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
        }
        // Clear the click state.
        pointerData.pointerPress = null;
        pointerData.rawPointerPress = null;
        pointerData.eligibleForClick = false;
        pointerData.clickCount = 0;
        pointerData.clickTime = 0;
        pointerData.pointerDrag = null;
        pointerData.dragging = false;
    }

    private void HandleTrigger() {
        var go = pointerData.pointerCurrentRaycast.gameObject;
        // Send pointer down event.
        pointerData.pressPosition = pointerData.position;
        pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
        pointerData.pointerPress =
            ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
            ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);
        // Save the drag handler as well
        pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
        if (pointerData.pointerDrag != null) {
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
        }
        // Save the pending click state.
        pointerData.rawPointerPress = go;
        pointerData.eligibleForClick = true;
        pointerData.delta = Vector2.zero;
        pointerData.dragging = false;
        pointerData.useDragThreshold = true;
        pointerData.clickCount = 1;
        pointerData.clickTime = Time.unscaledTime;
        if (gazePointer != null) {
            gazePointer.OnGazeTriggerStart(pointerData.enterEventCamera);
        }
    }

    private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords) {
        cartCoords.Normalize();
        if (cartCoords.x == 0)
            cartCoords.x = Mathf.Epsilon;
        float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
        if (cartCoords.x < 0)
            outPolar += Mathf.PI;
        float outElevation = Mathf.Asin(cartCoords.y);
        return new Vector2(outPolar, outElevation);
    }

    GameObject GetCurrentGameObject() {
        if (pointerData != null && pointerData.enterEventCamera != null) {
            return pointerData.pointerCurrentRaycast.gameObject;
        }
        return null;
    }

    Vector3 GetIntersectionPosition() {
        // Check for camera
        Camera cam = pointerData.enterEventCamera;
        if (cam == null) {
            return Vector3.zero;
        }
        float intersectionDistance = pointerData.pointerCurrentRaycast.distance + cam.nearClipPlane;
        Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;
        return intersectionPosition;
    }

    void DisableGazePointer() {
        if (gazePointer == null) {
            return;
        }
        GameObject currentGameObject = GetCurrentGameObject();
        if (currentGameObject) {
            Camera camera = pointerData.enterEventCamera;
            gazePointer.OnGazeExit(camera, currentGameObject);
        }
        gazePointer.OnGazeDisabled();
    }
}

