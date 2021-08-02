using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLine : MonoBehaviour
{
    public GameObject linePrefab;
    public GameObject inputField, dialog;
    int saveFlag = 0;
    List<GameObject> lineList = new List<GameObject>();

    LineRenderer lineRenderer;
    EdgeCollider2D col;
    List<Vector2> points = new List<Vector2>();
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))    // 마우스 버튼이 처음으로 눌러졌을 때,
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(worldPosition, Vector2.zero);
            
            if(VerifyPosition(hit)){
                Debug.Log("박스 안에 있음");

                GameObject line = Instantiate(linePrefab);
                lineList.Add(line);

                line.transform.SetParent(this.transform.Find("WritingArea"));
                lineRenderer = line.GetComponent<LineRenderer>();
                col = line.GetComponent<EdgeCollider2D>();
                
                points.Add(worldPosition);
                lineRenderer.positionCount = 1;
                lineRenderer.SetPosition(0, points[0]);
            }

        }
        else if (Input.GetMouseButton(0))   //마우스 버튼이 눌러져있는 상태
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(pos, Vector2.zero);
            
            if(points.Count > 0 && VerifyPosition(hit))    // 선그리기가 시작되었고, 박스안에 있으면
            {

                if (Vector2.Distance(points[points.Count - 1], pos) > 0.1f)
                {
                    points.Add(pos);
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, pos);
                    col.points = points.ToArray();
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            points.Clear();
        }
    }

    protected bool VerifyPosition(RaycastHit2D[] hit)
    {
        if(hit.Length == 1){
            return true;
        }

        return false;

    }

    public void DeleteAll()
    {
        foreach(GameObject line in lineList) { 
            Destroy(line); 
        }

        lineList.Clear();
    }

    public void Save(){
        string linePoints = "";
        
        foreach(GameObject line in lineList) {
            LineRenderer lr = line.GetComponent<LineRenderer>();
            for (int i = 0; i < lr.positionCount; i++){
                linePoints += lr.GetPosition(i)[0] + "," + lr.GetPosition(i)[1] + ",";
            }
        }

        string phoneme = inputField.GetComponent<InputField>().text;

        // Debug for now
        GameObject.Find("DebugText").GetComponent<Text>().text = "음소: " + phoneme + " 좌표: " + linePoints; //"음소: " + phoneme + 

        // Dialog 메시지 띄우기
        SetDialogMessage(phoneme,linePoints);
        
        // 조건 만족하면
        if(phoneme != "" && linePoints != "") {
            Debug.Log("저장하고 지우기");
            // 서버에 저장하기

            // 저장하면 linePoints 지우기
            DeleteAll();
        }

    }

    public void ConfirmButton() {
        dialog.SetActive(false);
    }

    void SetDialogMessage(string phoneme, string linePoints) {
        GameObject dialogText = dialog.transform.Find("Text").gameObject;
        Debug.Log("음소: " + phoneme + " 데이터: -" + linePoints + "--");

        // 음소를 입력하지 않았을 때
        if(phoneme == "")
        {
            Debug.Log("1");
            dialogText.GetComponent<Text>().text = "음소를\n입력해주세요.";
        }
        // 필기 데이터를 입력하지 않았을 때
        else if(linePoints == "") {
            Debug.Log("2");
            dialogText.GetComponent<Text>().text = "필기 데이터를\n입력해주세요.";
        }
        // 저장 완료 메시지 띄우기
        else {
            Debug.Log("3");
            dialogText.GetComponent<Text>().text = "저장이\n완료되었습니다.";
        }

        // 에러 메시지 띄우기
        dialog.SetActive(true);
        dialog.transform.SetAsLastSibling();
    }
}
