using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class NvrInteractiveObject : MonoBehaviour {
    //****************************************************************
    // Public Vars
    //****************************************************************
    // Is it possible to interact with this object or not. May be changed at runtime
    public bool interactionEnabled = true;
    // This is maximum interaction distance between the camera and this object
    [Range(0.0f, 100.0f)]
    public float distance = 5f;
    // Can you click on this object via gaze only?
    public bool clickWithGaze;
    // How long to gaze before click happens
    public float gazeClickTime = 3f;
    // Should this object show an outline?
    public bool useOutline;
    // This is the width of the border. This needs to be adjusted for each item until it looks good
    [Range(0.0f, 0.03f)]
    public float outlineWidth = 0.005f;
    // How fast the outline will grow to it's full width
    public float outlineGrowSpeed = 3f;
    // The Depth at which the outline is drawn    
    public float outlineDepthOffset = 0f;
    // Select a color for the outline
    public Color outlineColor = Color.white;

    //****************************************************************
    // Private Vars
    //****************************************************************
    // EventTrigger for PointerEnter, Exit, Click events
    private EventTrigger myTrigger;
    // Outline Gameobject
    private GameObject outlineObject;
    // Meshreneder on the Outline object
    private MeshRenderer outlineRenderer;
    // Outline Material loaded from resources folder
    private Material outlineMaterial;
    // Integer to track status of the outline object: 0 = Idle (no outline object); 1 = Gazed at (grow); 2 = Not gazed at (shrink)
    private int outlineStatus;
    // The outline Lerps between 0 and it's max width, this is the lerp value that goes between 0 and 1
    private float outlineLerp;
    // This is the current width of the outline during a lerp
    private float currentOutlineWidth;
    // Timer for gaze click
    private float timer = 0;
    // Is the player currently looking at this object?
    private bool isGazedAt = false;
    // bools to keep track if this object already has an Event Trigger with any of the following events
    private bool hasPointerEnter, hasPointerExit, hasPointerDown, hasPointerUp, hasPointerClick;

    //****************************************************************
    // Monobehaviour Functions
    //****************************************************************
    // Use this for initialization
    void Start(){
        // Load the Outline Material
        outlineMaterial = Resources.Load("Materials/matOutlineOnly") as Material;
        // Try to Find an EventTrigger Script on this GameObject
        myTrigger = gameObject.GetComponent<EventTrigger>();
        // If a the EventTrigger does not exist..
        if (myTrigger == null){
            // .. then create one.
            myTrigger = gameObject.AddComponent<EventTrigger>();
        }
        // Get a list of all existing event entries
        List<EventTrigger.Entry> entryList = myTrigger.triggers;
        // Add a callback to this script if an event already exists
        foreach (EventTrigger.Entry i in entryList) {
            if (i.eventID == EventTriggerType.PointerEnter) {
                i.callback.AddListener((eventData) => { OnPointerEnter(); });
                hasPointerEnter = true;
            }
            if (i.eventID == EventTriggerType.PointerExit) {
                i.callback.AddListener((eventData) => { OnPointerExit(); });
                hasPointerExit = true;
            }
            if (i.eventID == EventTriggerType.PointerDown) {
                i.callback.AddListener((eventData) => { OnPointerDown(); });
                hasPointerDown = true;
            }
            if (i.eventID == EventTriggerType.PointerUp) {
                i.callback.AddListener((eventData) => { OnPointerUp(); });
                hasPointerUp = true;
            }
            if (i.eventID == EventTriggerType.PointerClick) {
                i.callback.AddListener((eventData) => { OnPointerClick(); });
                hasPointerClick = true;
            }
        }
        // Register the Event for "Pointer Enter" (cursor goes Over button)
        if (hasPointerEnter == false) {
            EventTrigger.Entry entryOver = new EventTrigger.Entry();
            entryOver.eventID = EventTriggerType.PointerEnter;
            entryOver.callback.AddListener((eventData) => { OnPointerEnter(); });
            myTrigger.triggers.Add(entryOver);
        }
        // Register the Event for "Pointer Exit" (cursor goes Out of button)
        if (hasPointerExit == false) {
            EventTrigger.Entry entryOut = new EventTrigger.Entry();
            entryOut.eventID = EventTriggerType.PointerExit;
            entryOut.callback.AddListener((eventData) => { OnPointerExit(); });
            myTrigger.triggers.Add(entryOut);
        }
        // Register the Event for "Pointer Down" (physical button has been pushed down)
        if (hasPointerDown == false) {
            EventTrigger.Entry entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener((eventData) => { OnPointerDown(); });
            myTrigger.triggers.Add(entryDown);
        }
        // Register the Event for "Pointer Up" (physical button has been released)
        if (hasPointerUp == false) {
            EventTrigger.Entry entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener((eventData) => { OnPointerUp(); });
            myTrigger.triggers.Add(entryUp);
        }
        // Register the Event for "Pointer Click" (physical button has been pressed down and back up)
        if (hasPointerClick == false) {
            EventTrigger.Entry entryClick = new EventTrigger.Entry();
            entryClick.eventID = EventTriggerType.PointerClick;
            entryClick.callback.AddListener((eventData) => { OnPointerClick(); });
            myTrigger.triggers.Add(entryClick);
        }
    }

    // Update
    private void Update() {
        if (useOutline) {
            // Check the outline status: 1 = Gazed at (grow);
            if (outlineStatus == 1) {
                // If the lerp is not at max value
                if (outlineLerp < 1) {
                    // Increase the lerp value
                    outlineLerp += Time.deltaTime * outlineGrowSpeed;
                    // Calculate a new outline width  
                    currentOutlineWidth = Mathf.Lerp(0f, outlineWidth, outlineLerp);
                    // Set the outline width on the material
                    outlineRenderer.material.SetFloat("_Outline", currentOutlineWidth);
                }
            }
            // Check the outline status: 2 = Not gazed at (shrink)
            else if (outlineStatus == 2) {
                // If the lerp value is not at 0
                if (outlineLerp > 0f) {
                    // Decrease the lerp value
                    outlineLerp -= Time.deltaTime * outlineGrowSpeed;
                    // Calculate a new outline width
                    currentOutlineWidth = Mathf.Lerp(0f, outlineWidth, outlineLerp);
                    // Set the outline width on the material
                    outlineRenderer.material.SetFloat("_Outline", currentOutlineWidth);
                    // If the lerp value is 0 or less
                }
                else if (outlineLerp <= 0f) {
                    // Destroy the outline gameobject
                    GameObject.Destroy(outlineObject);
                    // Set the outline status: 0 = Idle (no outline object)
                    outlineStatus = 0;
                }
            }
        }
        
        // If interation is not enabled for this item, or we cannot click on it via gaze
        if (interactionEnabled == false || clickWithGaze == false) {
            // stop now
            return;
        } else if (isGazedAt) {
            // start timer
            timer += Time.deltaTime;
            // check if timer is larger than maxGazeTime
            if(timer > gazeClickTime) {
                // Reset timer
                timer = 0f;
                // Perform a 'click' (down, up, click)
                ExecuteEvents.Execute(gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            }
        }
        // check if timer doesn't equal zero when not gazed at
        else if(timer != 0f) {
            // timer backwards
            timer -= Time.deltaTime;
            // until less than 0
            if(timer < 0f)
            {
                // set timer 0
                timer = 0f;
            }
        }
    }

    //****************************************************************
    // Event Trigger Events
    //****************************************************************
    // Pointer Enter Event
    public void OnPointerEnter() {
        if (useOutline) {
            // Set the outline status to 1: Gazed at (Grow)
            outlineStatus = 1;
            // Spawn an outline gameobject
            SpawnOutlineChild();
        }
        // set isGazedAt to true
        isGazedAt = true;
    }

    // Pointer Exit Event
    public void OnPointerExit(){
        if (useOutline) {
            // Set the outline status to 2: NOT Gazed at (Shrink)
            outlineStatus = 2;
        }
        // set isGazedAt to false
        isGazedAt = false;
    }

    // Pointer Click Event
    public void OnPointerClick(){
    }

    // Pointer Up Event
    public void OnPointerUp(){
    }

    // Pointer Down Event
    public void OnPointerDown(){
        // Reset the gaze timer when a click occurs
        timer = 0;
    }

    //****************************************************************
    // Custom Functions
    //****************************************************************
    // Function to create the outline gameobject
    private void SpawnOutlineChild() {
        // If there is already an outline object..
        if (outlineObject != null) {
            // .. destroy it.
            GameObject.Destroy(outlineObject);
        }
        // Create an empty gameobject
        outlineObject = new GameObject(gameObject.name + "_Outline");
        // Make it my child
        outlineObject.transform.SetParent(transform);
        // Set position to my position
        outlineObject.transform.localPosition = Vector3.zero;
        // Set scale to parent's rotation
        outlineObject.transform.localRotation = Quaternion.identity;
        // Set rotation to parent's scale
        outlineObject.transform.localScale = Vector3.one;
        // Add a MeshRenderer to the new GameObject
        outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
        // Add a MeshFilter to the new GameObject
        MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
        // Set the mesh to the same as mine
        outlineFilter.mesh = (Mesh)gameObject.GetComponent<MeshFilter>().mesh;
        // Set the material to the outline material loaded from resources folder
        outlineRenderer.material = outlineMaterial;
        // Set outline width to 0
        outlineRenderer.material.SetFloat("_Outline", 0f);
        // Set outline depth offset
        outlineRenderer.material.SetFloat("_zDepthOffset", outlineDepthOffset);
        // Set outline color
        outlineRenderer.material.SetColor("_OutlineColor", outlineColor);
        // Set the Outline Lerp value to 0 so Update lerps back to 1
        outlineLerp = 0;
    }

    // Function to disable interaction on this object
    public void InteractionEnabled(bool status) {
        if (useOutline) {
            // Set the outline status to 2, shrink
            outlineStatus = 2;
        }
        interactionEnabled = status;
    }
}