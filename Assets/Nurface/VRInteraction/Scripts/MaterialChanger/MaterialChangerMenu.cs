using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChangerMenu : MonoBehaviour {
    public MaterialChangerButton buttonPrefab;
    public bool autoSpaceButtons;
    public int numberOfButtons = 6;
    public float buttonSize = 50f;
    public float distanceFromCenter = 50f;

    private RectTransform newButtonRect;

    private void Start() {
        transform.GetChild(0).gameObject.GetComponent<RectTransform>().localScale = Vector3.zero;
        iTween.ScaleTo(transform.GetChild(0).gameObject, Vector3.one, 1f);
    }

    public void SpawnButtons(MaterialChanger obj) {
        for (int i = 0; i < obj.materials.Length; i++) {
            MaterialChangerButton newButton = Instantiate(buttonPrefab) as MaterialChangerButton;
            newButton.transform.SetParent(transform.GetChild(0));
            newButton.transform.localScale = Vector3.one;
            newButton.transform.rotation = transform.rotation;
            newButtonRect = newButton.gameObject.GetComponent<RectTransform>();
            newButtonRect.sizeDelta = new Vector2(buttonSize, buttonSize);
            newButton.title = obj.materials[i].title;
            newButton.material = obj.materials[i].material;
            newButton.obj = obj;

            float theta = 0;
            if (autoSpaceButtons == true) {
                theta = (2 * Mathf.PI / obj.materials.Length) * i;
            }
            else {
                theta = (2 * Mathf.PI / numberOfButtons) * i;
            }
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);
            newButton.transform.localPosition = new Vector3(xPos, yPos, 0f) * distanceFromCenter;

        }
    }
}
