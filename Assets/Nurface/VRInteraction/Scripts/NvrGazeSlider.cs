using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NvrGazeSlider : MonoBehaviour {
    // How long it takes to fill the slider
    public float fillTime = 2f;
    // Private vars
    private Slider mySlider;
    private float timer;
    private bool gazedAt;
    private Coroutine fillBarRoutine;
    private Coroutine unfillBarRoutine;

    // Use this for initialization
    void Start () {
        mySlider = GetComponent<Slider>();
        if (mySlider == null) Debug.Log("Please add a Slider Component to this GameObject!");
	}
    
    // PointerEnter
    public void PointerEnter() {
        gazedAt = true;
        if (unfillBarRoutine != null) {
            StopCoroutine(unfillBarRoutine);
        }
        fillBarRoutine = StartCoroutine(FillBar());
    }
    
    // PointerExit
    public void PointerExit() {
        gazedAt = false;
        if (fillBarRoutine != null) {
            StopCoroutine(fillBarRoutine);
        }
        unfillBarRoutine = StartCoroutine(UnfillBar());
        //timer = 0f;
        //mySlider.value = 0f;
    }
    
    // Fill the Bar
    private IEnumerator FillBar() {
        // When the bar starts to fill, reset the timer.
        //timer = 0f;
        // Until the timer is greater than the fill time...
        while (timer < fillTime) {
            // ... add to the timer the difference between frames.
            timer += Time.deltaTime;
            // Set the value of the slider 
            mySlider.value = timer / fillTime;
            // Wait until next frame.
            yield return null;
            if (gazedAt) continue;
            timer = 0f;
            mySlider.value = 0f;
            yield break;
        }
        // The bar has been filled
        OnBarFilled();
    }

    private void OnBarFilled() {
        Debug.Log("Do Something Amazing!!!");
    }

    private IEnumerator UnfillBar() {
        while (timer > 0) {
            timer -= Time.deltaTime;
            mySlider.value = timer / fillTime;
            yield return null;
        }
    }
}
