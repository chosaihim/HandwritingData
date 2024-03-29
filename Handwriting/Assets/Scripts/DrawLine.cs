﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class DrawLine : MonoBehaviour
{
    public Camera mainCamera; 

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
    string savedPoints, baseLinePoint, linePoints, resizePoint;

    // 저장할 글자 제시
    string[] letter = new string[28] {"ㄱ","ㅏ","ㄷ","ㄹ","ㅁ","ㅂ","ㅅ","ㅇ","ㅈ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ","ㄴ","ㅓ","ㅗ","ㅜ","ㅡ","ㅣ","ㅑ","ㅕ","ㅛ","ㅠ","ㅐ","ㅒ","ㅔ","ㅖ"};
    int letterNum = 0;

    private int resWidth,resHeight;
    string path;


    void Start()
    {
        // 이름 입력창
        string savedName = PlayerPrefs.GetString("Name","");  
        if(savedName != ""){ // 이름 입력 받은 적 있으면 이름 입력창 제거
            namePanel.SetActive(false);
            SetName(savedName);
        }

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
        if(letterNum >= 700) {
            nextLetter.GetComponent<Text>().text = "종료";
            resultText.GetComponent<Text>().text = "수고하셨습니다!!\n";
        } else {
            nextLetter.GetComponent<Text>().text = letter[letterNum/25];
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
        baseLinePoint = ""; 
        resizePoint = "";

        // 데이터 샘플링
        Sampling();

        // Sampled Area에 샘플된 데이터 그리기
        DrawSample();

        // 저장할 음소
        letterNum = PlayerPrefs.GetInt("letterNum", 0);
        string phoneme = letter[letterNum/25];

        //사용자 이름 받아오기
        string name = this.name.GetComponent<Text>().text;

        // 결과 메시지 띄우기
        SetResultText(linePoints,name);
        
        // 조건 만족하면 서버에 저장
        if(linePoints != "") {
            // 서버에 저장하기
            ServerManager manager = GameObject.Find("ServerManager").GetComponent<ServerManager>();
            manager.saveData(name, phoneme, linePoints, baseLinePoint, resizePoint, "phoneme");

            // 저장하면 linePoints 지우기
            DeleteAll();

            //10번째까지 저장하면 Dialog 메시지 띄우기
            if(letterNum % 25 == 24) {
                SetDialogMessage(phoneme,linePoints,name);
                DeleteSample();
            }


            // 다음 글자 제시하기
            letterNum++;
            if(letterNum >= 700) {  // 마지막 글자 이후 
                nextLetter.GetComponent<Text>().text = "종료";
                resultText.GetComponent<Text>().text = "수고하셨습니다!!\n";
            } else {    // 마지막 글자 이전
                nextLetter.GetComponent<Text>().text = letter[letterNum/25];
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
        // 메시지 입력
        dialogText.GetComponent<Text>().text = "'" + phoneme + "'의 저장이 완료되었습니다.";
        // 메시지 띄우기
        dialog.SetActive(true);
    }
    
    void SetResultText(string linePoints, string name) {

        if(linePoints == "") {
            // 필기 데이터를 입력하지 않았을 때
            resultText.GetComponent<Text>().text = "필기 데이터를\n입력해주세요.";
        } else {
            // 저장 완료
            resultText.GetComponent<Text>().text = (letterNum%25+1) + "번째\n 저장되었습니다.";
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
        float xinputSize;
        float yinputSize;
        float inputSize;

        if(xLength > yLength) inputSize = xLength;
        else inputSize = yLength;

        xinputSize = xLength;
        yinputSize = yLength;

        // 필기 데이터 중앙점 찾기
        float xCenter = (xMax + xMin)/2;
        float yCenter = (yMax + yMin)/2;

        // 필기 데이터를 일정 크기로 맞추기
        foreach(GameObject line in lineList) {
            LineRenderer lr = line.GetComponent<LineRenderer>();
            List<Vector2> sampled = new List<Vector2>();
            
            for (int i = 0; i < lr.positionCount; i++){ 
                Vector2 basePos = Camera.main.WorldToScreenPoint(lr.GetPosition(i));
                Vector2 pos = Camera.main.WorldToScreenPoint(lr.GetPosition(i));
                Vector2 resizePos = Camera.main.WorldToScreenPoint(lr.GetPosition(i));
                
                // 원점을 중심점으로 이동
                pos[0] -= xCenter;
                pos[1] -= yCenter;
                resizePos[0] -= xCenter;
                resizePos[1] -= yCenter;

                // 0~1 사이의 사이즈로 줄이기
                pos[0] /= xinputSize;
                pos[1] /= yinputSize;
                resizePos[0] /= inputSize;
                resizePos[1] /= inputSize;

                // 노말라이즈 된 데이터를 배열에 저장
                sampled.Add(pos);
                
                // 서버에 저장할 데이터 string으로 이어붙이기
                linePoints += pos[0] + "," + pos[1] + ",";
                baseLinePoint += basePos[0] + "," + basePos[1] + "/"; 
                resizePoint += resizePos[0] + "," + resizePos[1] + ",";
                
            }
            sampledList.Add(sampled);
        }
        Debug.Log(linePoints);
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
        // 앱에 저장된 데이터 초기화
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
