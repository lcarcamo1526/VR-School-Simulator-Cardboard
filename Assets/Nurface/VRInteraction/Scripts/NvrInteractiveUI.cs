using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class NvrInteractiveUI : MonoBehaviour {
    //****************************************************************
    // Public Vars
    //****************************************************************
    //[Tooltip("This is maximum interaction distance between the camera and this object")]
    [Range(0.0f, 100.0f)]
    public float distance = 5f;
    // Can you click on this object via gaze only?
    public bool clickWithGaze;
    // How long to gaze before click happens
    public float gazeClickTime = 3f;

    //****************************************************************
    // Private Vars
    //****************************************************************
    // EventTrigger for PointerEnter, Exit, Click events
    private EventTrigger myTrigger;
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
    void Start() {
        if (clickWithGaze) {
            // Try to Find an EventTrigger Script on this GameObject
            myTrigger = gameObject.GetComponent<EventTrigger>();
            // If a the EventTrigger does not exist..
            if (myTrigger == null) {
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
    }

    // Update
    private void Update() {
        // If you can click on this UI element via gaze
        if (clickWithGaze) {
            // If player is gazing at this object
            if (isGazedAt) {
                // Increase timer
                timer += Time.deltaTime;
                // check if timer is larger than maxGazeTime
                if (timer > gazeClickTime) {
                    // Reset timer
                    timer = 0f;
                    // Perform a 'click' (down, up, click)
                    ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
                }
            }
            // If timer is not at zero when not gazed at (player looked away)
            else if (timer != 0f){
                // timer backwards
                timer -= Time.deltaTime;
                // until less than 0
                if (timer < 0f) {
                    // set timer 0
                    timer = 0f;
                }
            }
        }
    }

    //****************************************************************
    // Event Trigger Events
    //****************************************************************
    // Pointer Enter Event
    public void OnPointerEnter() {
        // set isGazedAt to true
        isGazedAt = true;
    }

    // Pointer Exit Event
    public void OnPointerExit() {
        // set isGazedAt to false
        isGazedAt = false;
    }

    // Pointer Click Event
    public void OnPointerClick() {
    }

    // Pointer Up Event
    public void OnPointerUp() {
    }

    // Pointer Down Event
    public void OnPointerDown() {
        // Reset the gaze timer when a click occurs
        timer = 0f;
    }

}
