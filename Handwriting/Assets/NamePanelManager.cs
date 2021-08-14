using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NamePanelManager : MonoBehaviour
{
    public GameObject nameText;
    public GameObject confirmButton;
	public DrawLine drawLine;

    public void IsNameIn() {
        string name = nameText.GetComponent<Text>().text;

        // 이름이 입력되었다면 버튼 활성화
        if(name != "") {
            confirmButton.GetComponent<Button>().interactable = true;
        } else {    // 입력된 것이 없으면 버튼 비활성화
            confirmButton.GetComponent<Button>().interactable = false;
        }
    }

    public void SaveName() {
        string name = nameText.GetComponent<Text>().text;
        PlayerPrefs.SetString("Name", name);
        this.gameObject.SetActive(false);
        drawLine.SetName(name);
    }
}
