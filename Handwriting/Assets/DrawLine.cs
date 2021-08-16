﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLine : MonoBehaviour
{
    // *** Variables *** //
    public GameObject linePrefab, canvas;
    public GameObject inputField, dialog, nameField, nextLetter, resultText;
    public GameObject namePanel, name, restartPanel;

    // Drawing
    List<GameObject> lineList = new List<GameObject>();
    LineRenderer lineRenderer;
    EdgeCollider2D col;
    List<Vector2> points = new List<Vector2>();
    
    // Samples
    List<List<Vector2>> sampledList = new List<List<Vector2>>();

    // Drawing sampled data
    List<GameObject> sampledLineList = new List<GameObject>();
    LineRenderer sampledLineRenderer;
    List<Vector2> sampledPoints = new List<Vector2>();
    EdgeCollider2D sampledCol;

    // 비율 맞추기 (디자인 시 보여지는 픽셀 크기(canvas)와 game play 시 보여지는 픽셀의 크기(Screen)가 다름)
    const float samplePixelSize = 300;
    float sampleWidth, sampleHeight;
    float xMin, yMin, xMax, yMax;
    float canvasWidth, canvasHeight;
    float ratioWidth, ratioHeight;      // Screen to Canvas ratio(screen/canvas)

    // 서버에 저장되는 데이터
    string savedPoints, linePoints;

    // 저장할 글자 제시
    string[] letter = new string[20] {"ㄱ","ㄴ","ㄷ","ㄹ","ㅁ","ㅂ","ㅅ","ㅇ","ㅈ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ","ㅏ","ㅓ","ㅗ","ㅜ","ㅡ","ㅣ"};
    int letterNum = 0;

    // PlayerPrefs.SetInt(“RewardPanel”, 1);
    // PlayerPrefs.GetInt(“RewardPanel”, 0);

    void Start()
    {
        // 이름 입력창
        // 이름 입력 받은 적 있으면 이름 입력창 제거
        string savedName = PlayerPrefs.GetString("Name","");  
        if(savedName != ""){
            namePanel.SetActive(false);
            // name.GetComponent<Text>().text = savedName;
            SetName(savedName);
        }

        // 스크린 가로 고정
        // Screen.orientation = ScreenOrientation.Landscape;

        // canvas 크기
        canvasWidth = canvas.transform.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.transform.GetComponent<RectTransform>().rect.height;

        // Screen.width / canvasWidth
        ratioWidth = Screen.width / canvasWidth;
        ratioHeight = Screen.height / canvasHeight;

        // 화면 비율에 맞게 sample size 조절
        sampleWidth = samplePixelSize * ratioWidth;
        sampleHeight = samplePixelSize * ratioHeight;

        // 제시어 세팅
        letterNum = PlayerPrefs.GetInt("letterNum", 0);
        if(letterNum >= 200) {
            nextLetter.GetComponent<Text>().text = "종료";
            resultText.GetComponent<Text>().text = "수고하셨습니다!!\n";
        } else {
            nextLetter.GetComponent<Text>().text = letter[letterNum/10];
        }
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
    }

    void DeleteSample() {
        foreach(GameObject line in sampledLineList) { 
            Destroy(line); 
        }
        sampledLineList.Clear();
    }

    public void Save(){
        // 초기화        
        linePoints = "";

        // 데이터 샘플링
        Sampling();

        // Sampled Area에 샘플된 데이터 그리기
        DrawSample();

        // 사용자가 입력한 음소 받아오기
        // string phoneme = inputField.GetComponent<InputField>().text;
        // GameObject.Find("Letter").GetComponent<Text>().text = phoneme;

        // 저장할 음소
        letterNum = PlayerPrefs.GetInt("letterNum", 0);
        string phoneme = letter[letterNum/10];

        //사용자 이름 받아오기
        string name = nameField.GetComponent<InputField>().text;

        // Dialog 메시지 띄우기
        // SetDialogMessage(phoneme,linePoints,name);
        SetResultText(linePoints,name);
        
        // 조건 만족하면 서버에 저장
        if(linePoints != "") { //if(name != "" && phoneme != "" && linePoints != "") {
            // 서버에 저장하기
            ServerManager manager = GameObject.Find("ServerManager").GetComponent<ServerManager>();
            manager.saveData(name, phoneme, linePoints);

            // 저장하면 linePoints 지우기
            DeleteAll();

            //10번째까지 저장하면 알림 메시지 띄우기
            if(letterNum % 10 == 9) {
                SetDialogMessage(phoneme,linePoints,name);
                DeleteSample();
            }

            // 다음 글자 제시하기
            letterNum++;

            if(letterNum >= 200) {
                nextLetter.GetComponent<Text>().text = "종료";
                resultText.GetComponent<Text>().text = "수고하셨습니다!!\n";
            } else {
                nextLetter.GetComponent<Text>().text = letter[letterNum/10];
                PlayerPrefs.SetInt("letterNum", letterNum);
            }
            
        }
    }

    public void ConfirmButton() {
        DeleteSample();
        resultText.GetComponent<Text>().text = "제시된 글자를 써주세요!";
        dialog.SetActive(false);
    }

    void SetDialogMessage(string phoneme, string linePoints, string name) {
        GameObject dialogText = dialog.transform.Find("Dialog/Text").gameObject;

        dialogText.GetComponent<Text>().text = "'" + phoneme + "'의 저장이 완료되었습니다.";

        // if(phoneme == "") {
        //     // 음소를 입력하지 않았을 때
        //     dialogText.GetComponent<Text>().text = "음소를\n입력해주세요.";
        // } else if(linePoints == "") {
        //     // 필기 데이터를 입력하지 않았을 때
        //     dialogText.GetComponent<Text>().text = "필기 데이터를\n입력해주세요.";
        // } else if(name == "") { 
        //     // 이름을 입력하지 않았을 때
        //     dialogText.GetComponent<Text>().text = "이름을\n입력해주세요.";
        // } else {
        //     // 저장 완료
        //     dialogText.GetComponent<Text>().text = "저장이\n완료되었습니다.";
        // }

        // 메시지 띄우기
        dialog.SetActive(true);
        // dialog.transform.SetAsLastSibling();
    }
    
    void SetResultText(string linePoints, string name) {

        if(linePoints == "") {
            // 필기 데이터를 입력하지 않았을 때
            resultText.GetComponent<Text>().text = "필기 데이터를\n입력해주세요.";
        // } else if(name == "") { 
        //     // 이름을 입력하지 않았을 때
        //     resultText.GetComponent<Text>().text = "이름을\n입력해주세요.";
        } else {
            // 저장 완료
            resultText.GetComponent<Text>().text = (letterNum%10+1) + "번째\n 저장되었습니다.";
        }
    }

    void Sampling() {
        // 변수 초기화
        sampledList.Clear();
        linePoints = "";

        // 오른쪽, 왼쪽, 위, 아래 가장 바깥점 찾기
        FindMaxMin();

        // 입력 데이터 크기 결정
        float xLength = xMax - xMin;
        float yLength = yMax - yMin;
        float inputSize;

        if(xLength > yLength) inputSize = xLength;
        else inputSize = yLength;

        // 크기 변환 후, 필기 데이터의 중앙점 찾기
        // float xCenter = (xMax + xMin)/2 * (sampleWidth  / inputSize);
        // float yCenter = (yMax + yMin)/2 * (sampleHeight / inputSize);

        // 필기 데이터 중앙점 찾기
        float xCenter = (xMax + xMin)/2;
        float yCenter = (yMax + yMin)/2;

        // 필기 데이터를 일정 크기로 맞추기
        foreach(GameObject line in lineList) {
            LineRenderer lr = line.GetComponent<LineRenderer>();
            List<Vector2> sampled = new List<Vector2>();
            
            for (int i = 0; i < lr.positionCount; i++){ 
                Vector2 pos = Camera.main.WorldToScreenPoint(lr.GetPosition(i));
                
                // 원점을 중심점으로 이동
                pos[0] -= xCenter;
                pos[1] -= yCenter;

                // 0~1 사이의 사이즈로 줄이기
                pos[0] /= inputSize;
                pos[1] /= inputSize;

                // // 필기 데이터 일정 사이즈로 조정 후, 중앙점을 원점으로 이동
                // pos[0] = pos[0] * sampleWidth/inputSize  - xCenter; // + Screen.width/2 + 560 * ratioWidth;
                // pos[1] = pos[1] * sampleHeight/inputSize - yCenter; // + Screen.height/2 - 115 * ratioHeight;
                
                // 노말라이즈 된 데이터를 배열에 저장
                sampled.Add(pos);
                
                // 서버에 저장할 데이터 string으로 이어붙이기
                linePoints += pos[0] + "," + pos[1] + ",";
            }
            sampledList.Add(sampled);
        }
    }
    
    void FindMaxMin() {
        //초기화
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

            }
        }
    }

    void DrawSample() {
        // 초기화
        DeleteSample();
        
        // Sample Area 중앙점
        float xSampleAreaCenter = Screen.width/2 + 560 * ratioWidth;
        float ySampleAreaCenter = Screen.height/2 -115 * ratioHeight;

        // 저장된 샘플 리스트 돌면서 모든 선 순회
        foreach(List<Vector2> line in sampledList) {
            GameObject sampledLine = Instantiate(linePrefab);
            sampledLineList.Add(sampledLine);
            sampledLine.transform.SetParent(this.transform.Find("SampledArea/SampledData"));
            sampledLineRenderer = sampledLine.GetComponent<LineRenderer>();
            sampledLineRenderer.positionCount = 0;
            
            // 각 선의 모든 점을 순회하면서 drawing
            for (int i = 0; i < line.Count; i++){
                // 샘플링 된 점을 sampling area로 옮긴 후
                Vector2 sampledPos = Vector2.zero;
                sampledPos[0] = line[i][0] * sampleWidth + xSampleAreaCenter;
                sampledPos[1] = line[i][1] * sampleHeight + ySampleAreaCenter;
                sampledPos = Camera.main.ScreenToWorldPoint(sampledPos);
                
                // line Drawing
                sampledPoints.Add(sampledPos);
                sampledLineRenderer.positionCount++;
                sampledLineRenderer.SetPosition(sampledLineRenderer.positionCount - 1, sampledPos);
            }
        }
        sampledPoints.Clear();
    }

    public void SetName(string userName) {
        name.GetComponent<Text>().text = userName;
    }

    public void Restart() {
        PlayerPrefs.DeleteAll();
        // 저장된 필기 데이터 지우기
        DeleteAll();
        DeleteSample();
        // 제시어 초기화
        nextLetter.GetComponent<Text>().text = letter[0];
        // 결과 안내문 초기화
        resultText.GetComponent<Text>().text = "제시된 글자를 써주세요!";
        // 재시작 패널 끄기
        restartPanel.SetActive(false);
        // 이름입력 패널 켜기
        namePanel.SetActive(true);
    }
}
