using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChangerButton : MonoBehaviour {

    [HideInInspector]
    public string title;
    [HideInInspector]
    public Material material;
    [HideInInspector]
    public MaterialChanger obj;

    private MeshRenderer childRenderer;
    private bool buttonEnabled;

    // Use this for initialization
    void Start () {
        childRenderer = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        childRenderer.material = material;
        StartCoroutine(EnableButton());
	}

    private IEnumerator EnableButton() {
        yield return new WaitForSeconds(0.3f);
        buttonEnabled = true;
    }

    public void OnPointerEnter() {
        obj.pointerOverButton = true;
        if (buttonEnabled == true) {
            obj.objectToChange.material = material;
        }
    }

    public void OnPointerExit() {
        obj.pointerOverButton = false;
        if (buttonEnabled == true) {
            StartCoroutine(obj.TryDestroyMenu());
        }
    }

    public void OnPointerDown() {
        obj.objectToChange.material = material;
    }
}
