using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour {
    public MaterialChangerMenu menuPrefab;
    public Renderer objectToChange;

    [HideInInspector]
    public bool pointerOverButton, pointerOverMenu;

    [System.Serializable]
    public class MaterialInfo {
        public string title;
        public Material material;
    }

    public MaterialInfo[] materials;

    private MaterialChangerMenu currentMenu;
    private Transform childCollider;

	// Use this for initialization
	void Start () {
        childCollider = transform.GetChild(0);
	}

    public void OnPointerEnter() {
        pointerOverMenu = true;
        // Spawn a menu
        if (currentMenu == null) {
            currentMenu = Instantiate(menuPrefab) as MaterialChangerMenu;
            currentMenu.transform.position = transform.position;
            currentMenu.transform.rotation = transform.rotation;
            childCollider.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            // Spawn Buttons 
            currentMenu.SpawnButtons(this);
        }
    }

    public void OnPointerExit() {
        pointerOverMenu = false;
        StartCoroutine(TryDestroyMenu());
    }

    public IEnumerator TryDestroyMenu() {
        yield return new WaitForEndOfFrame();
        if (pointerOverButton == false && pointerOverMenu == false) {
            childCollider.gameObject.SetActive(false);
            iTween.ScaleTo(currentMenu.transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.zero, "time", 1f, "oncomplete", "DestroyMenu", "oncompletetarget", gameObject));
        }
    }

    private void DestroyMenu() {
        Destroy(currentMenu.gameObject);
        childCollider.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        childCollider.gameObject.SetActive(true);
    }
}