using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class DrawLine : MonoBehaviour
{
    public GameObject linePrefab, canvas;
    public GameObject inputField, dialog;

    // Drawing
    List<GameObject> lineList = new List<GameObject>();
    LineRenderer lineRenderer;
    EdgeCollider2D col;
    List<Vector2> points = new List<Vector2>();
    
    // Samples
    // List<Vector2> sampled = new List<Vector2>();
    List<List<Vector2>> sampledList = new List<List<Vector2>>();

    // Drawing sampled data
    List<GameObject> sampledLineList = new List<GameObject>();
    LineRenderer sampledLineRenderer;
    List<Vector2> sampledPoints = new List<Vector2>();
    EdgeCollider2D sampledCol;

    const float samplePixelSize = 300;
    float sampleWidth, sampleHeight;
    float xMin, yMin, xMax, yMax;

    // GameObject canvas;
    float canvasWidth, canvasHeight, CanavsTo;

    // Screen to Canvas pixel ratio(screen/canvas): 실제 스크린 사이즈와 scanvas 사이즈의 비율 
    float ratioWidth, ratioHeight;
    string savedPoints, linePoints; 
    

    void Start()
    {
        // canvas 크기
        canvasWidth = canvas.transform.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.transform.GetComponent<RectTransform>().rect.height;

        // Screen.width / canvasWidth
        ratioWidth = Screen.width / canvasWidth;
        ratioHeight = Screen.height / canvasHeight;

        // 화면 비율에 맞게 sample size 조절
        sampleWidth = samplePixelSize * ratioWidth;
        sampleHeight = samplePixelSize * ratioHeight;
    }

    void Update()
    {
        Draw();
    }

    void Draw() {
        if (Input.GetMouseButtonDown(0)) {
            // 마우스 버튼이 처음으로 눌러졌을 때,
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(worldPosition, Vector2.zero);

            // Debug.Log("world position: " + worldPosition);
            // Debug.Log("screen position: " + Input.mousePosition);

            if(VerifyPosition(hit)){
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

        } else if (Input.GetMouseButton(0)) {
            // 마우스 버튼이 연속해서 눌러져 있는 상태
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(pos, Vector2.zero);
            
            if(points.Count > 0 && VerifyPosition(hit)) {
                // 선 그리기가 이전에 시작되었고, 박스안에 있으면
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

        // DeleteSample();
    }

    void DeleteSample() {
        foreach(GameObject line in sampledLineList) { 
            Destroy(line); 
        }
        sampledLineList.Clear();
    }

    public void Save(){
        
        // string linePoints = "";
        // linePoints += lr.GetPosition(i)[0] + "," + lr.GetPosition(i)[1] + ",";
        linePoints = "";

        Sampling();

        DrawSample();

        // 사용자가 입력하려는 음소
        string phoneme = inputField.GetComponent<InputField>().text;
        GameObject.Find("Letter").GetComponent<Text>().text = phoneme;

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

        if(phoneme == "") {
            // 음소를 입력하지 않았을 때
            dialogText.GetComponent<Text>().text = "음소를\n입력해주세요.";
        } else if(linePoints == "") {
            // 필기 데이터를 입력하지 않았을 때
            dialogText.GetComponent<Text>().text = "필기 데이터를\n입력해주세요.";
        } else {
            // 저장 완료
            dialogText.GetComponent<Text>().text = "저장이\n완료되었습니다.";
        }

        // 메시지 띄우기
        dialog.SetActive(true);
        dialog.transform.SetAsLastSibling();
    }
    void Sampling() {
        // 변수 초기화
        sampledList.Clear();

        // 입력 값의 각 크기와 중앙점 찾기
        FindMaxMin();
        float xLength = xMax - xMin;
        float yLength = yMax - yMin;
        float inputSize;

        if(xLength > yLength) inputSize = xLength;
        else inputSize = yLength;

        float xCenter = (xMax + xMin)/2 * sampleWidth / inputSize;
        float yCenter = (yMax + yMin)/2 * sampleHeight / inputSize;

        // 입력값을 일정 크기로 맞추기
        foreach(GameObject line in lineList) {
            LineRenderer lr = line.GetComponent<LineRenderer>();
            List<Vector2> sampled = new List<Vector2>();
            
            for (int i = 0; i < lr.positionCount; i++){ 
                Vector2 pos = Camera.main.WorldToScreenPoint(lr.GetPosition(i));
                
                // 원점을 중앙으로 한 sampling data 저장하기
                pos[0] = pos[0] * sampleWidth/inputSize  - xCenter; // + Screen.width/2 + 560 * ratioWidth;
                pos[1] = pos[1] * sampleHeight/inputSize - yCenter; // + Screen.height/2 - 115 * ratioHeight;
                
                sampled.Add(pos);
            }
            sampledList.Add(sampled);
        }
    }
    
    void FindMaxMin() {
        xMin = Screen.width;
        xMax = 0;
        yMin = Screen.height;
        yMax = 0;

        // 각 라인의 모든 포인트 순회하며 최대, 최소값 구하기
        foreach(GameObject line in lineList) {
            LineRenderer lr = line.GetComponent<LineRenderer>();
            for (int i = 0; i < lr.positionCount; i++){
                // 다시 월드 -> 스크린 포인트로 변환
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
    }

    void DrawSample() {

        DeleteSample();
        
        float xSampleAreaCenter = Screen.width/2 + 560 * ratioWidth;
        float ySampleAreaCenter = Screen.height/2 -115 * ratioHeight;

        // 저장된 샘플 리스트 돌면서 모든 포인트를 저장
        foreach(List<Vector2> line in sampledList) {
            GameObject sampledLine = Instantiate(linePrefab);
            sampledLineList.Add(sampledLine);
            sampledLine.transform.SetParent(this.transform.Find("SampledArea/SampledData"));
            sampledLineRenderer = sampledLine.GetComponent<LineRenderer>();
            sampledLineRenderer.positionCount = 0;
            
            for (int i = 0; i < line.Count; i++){
                Vector2 sampledPos = Vector2.zero;
                sampledPos[0] = line[i][0] + xSampleAreaCenter;  //Screen.width/2  + 560 * ratioWidth;
                sampledPos[1] = line[i][1] + ySampleAreaCenter;  //Screen.height/2 - 115 * ratioHeight;
                sampledPos = Camera.main.ScreenToWorldPoint(sampledPos);

                sampledPoints.Add(sampledPos);
                sampledLineRenderer.positionCount++;
                sampledLineRenderer.SetPosition(sampledLineRenderer.positionCount - 1, sampledPos);
            }
        }
        sampledPoints.Clear();
    }

}
