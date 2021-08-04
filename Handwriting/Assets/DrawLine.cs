using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class DrawLine : MonoBehaviour
{
    public GameObject linePrefab;
    public GameObject inputField, dialog;
    float xMin, yMin, xMax, yMax;


    List<GameObject> lineList = new List<GameObject>();
    LineRenderer lineRenderer;
    EdgeCollider2D col;
    List<Vector2> points = new List<Vector2>();
    
    // Sampled Data 
    List<GameObject> sampledLineList = new List<GameObject>();
    LineRenderer sampledLineRenderer;
    List<Vector2> sampledPoints = new List<Vector2>();
    EdgeCollider2D sampledCol;

    const float samplePixelSize = 360;

    GameObject canvas;
    float canvasWidth, canvasHeight, CanavsTo;
    

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.Find("Canvas");
        canvasWidth = canvas.transform.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.transform.GetComponent<RectTransform>().rect.height;
        
    }

    // Update is called once per frame
    void Update()
    {
        Draw();
    }

    void Draw() {
        if (Input.GetMouseButtonDown(0)) { // 마우스 버튼이 처음으로 눌러졌을 때,
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(worldPosition, Vector2.zero);

            Debug.Log("world position: " + worldPosition);
            Debug.Log("screen position: " + Input.mousePosition);

            if(VerifyPosition(hit)){
                Debug.Log("박스 안에 있음");
                //첫번째 점이면, line 프리팹 만들고 선 그리기 시작
                GameObject line = Instantiate(linePrefab);
                lineList.Add(line);
                line.transform.SetParent(this.transform.Find("WritingArea"));
                lineRenderer = line.GetComponent<LineRenderer>();
                col = line.GetComponent<EdgeCollider2D>();
                
                points.Add(worldPosition);
                lineRenderer.positionCount = 1;
                lineRenderer.SetPosition(0, points[0]);
            }

        } else if (Input.GetMouseButton(0)) {//마우스 버튼이 눌러져있는 상태
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
        } else if (Input.GetMouseButtonUp(0)) {
            points.Clear();
        }
    }

    protected bool VerifyPosition(RaycastHit2D[] hit) {
        if(hit.Length == 1){
            return true;
        }

        return false;

    }

    public void DeleteAll() {
        foreach(GameObject line in lineList) { 
            Destroy(line); 
        }
        lineList.Clear();
        
        foreach(GameObject line in sampledLineList) { 
            Destroy(line); 
        }
        sampledLineList.Clear();
    }

    public void Save(){
        string linePoints = "";

        xMax = 0;
        xMin = 1024;
        yMax = 0;
        yMin = 1024;

        Debug.Log("Actual width: " + Screen.width);
        Debug.Log("Actual height: " + Screen.height);


        // 모든 라인 순회하며 포인트 수집
        foreach(GameObject line in lineList) {
            LineRenderer lr = line.GetComponent<LineRenderer>();
            
            for (int i = 0; i < lr.positionCount; i++){
                // float x = lr.GetPosition(i)[0];
                // float y = lr.GetPosition(i)[1];
                
                Vector2 screenPosition = Camera.main.WorldToScreenPoint(lr.GetPosition(i));
                float x = screenPosition[0];
                float y = screenPosition[1];

                if(xMin > x) xMin = x;
                if(xMax < x) xMax = x;

                if(yMin > y) yMin = y;
                if(yMax < y) yMax = y;


                linePoints += lr.GetPosition(i)[0] + "," + lr.GetPosition(i)[1] + ",";
            }
        }

        Debug.Log("(Xmax, Xmin, Ymax, Ymin): (" + xMax + "," + xMin + "," + yMax + "," + yMin + ")");
        Debug.Log("SIZE: " + (xMax - xMin) + ", " + (yMax - yMin));

        float xLength = xMax - xMin;
        float yLength = yMax - yMin;
        float xCenter = (xMax + xMin)/2;
        float yCenter = (yMax + yMin)/2;
        float sampledLength;

        if(xLength > yLength) sampledLength = xLength;
        else sampledLength = yLength;

        float xNewCenter = (xMax + xMin)/2 * (300* Screen.width / canvasWidth) / sampledLength;
        float yNewCenter = (yMax + yMin)/2 * (300* Screen.height / canvasHeight) / sampledLength;
        Debug.Log("Center: (" + xCenter + ", " + yCenter + ")");

        // Sampled Data 보여주기
        foreach(GameObject line in lineList) {
            LineRenderer lr = line.GetComponent<LineRenderer>();

            GameObject sampledLine = Instantiate(linePrefab);
            sampledLineList.Add(sampledLine);
            sampledLine.transform.SetParent(this.transform.Find("SampledArea/SampledData"));
            sampledLineRenderer = sampledLine.GetComponent<LineRenderer>();
            col = sampledLine.GetComponent<EdgeCollider2D>();
            sampledLineRenderer.positionCount = 0;
            
            for (int i = 0; i < lr.positionCount; i++){ 

                Vector2 sampledPos = Camera.main.WorldToScreenPoint(lr.GetPosition(i));
                sampledPos[0] = sampledPos[0] * (300* Screen.width / canvasWidth)   / sampledLength - xNewCenter + Screen.width/2 + 560 * Screen.width / canvasWidth;
                sampledPos[1] = sampledPos[1] * (300* Screen.height / canvasHeight) / sampledLength - yNewCenter + Screen.height/2 - 115 * Screen.height / canvasHeight;
                sampledPos = Camera.main.ScreenToWorldPoint(sampledPos);

                sampledPoints.Add(sampledPos);
                sampledLineRenderer.positionCount++;
                sampledLineRenderer.SetPosition(sampledLineRenderer.positionCount - 1, sampledPos);
            }
        }


        // 사용자가 입력하려는 음소
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
        GameObject dialogText = dialog.transform.Find("Dialog/Text").gameObject;
        Debug.Log("음소: " + phoneme + " 데이터: ++" + linePoints + "++");

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

        // 메시지 띄우기
        dialog.SetActive(true);
        dialog.transform.SetAsLastSibling();
    }

    void Sampling() {

    }
}
