using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class WriteDrawLine : MonoBehaviour
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
    // SubSamples
    List<List<Vector2>> sampledList1 = new List<List<Vector2>>();
    List<List<Vector2>> sampledList2 = new List<List<Vector2>>();
    List<List<Vector2>> sampledList3 = new List<List<Vector2>>();
    List<List<Vector2>> sampledList4 = new List<List<Vector2>>();
    List<List<Vector2>> sampledList5 = new List<List<Vector2>>();
    List<List<Vector2>> sampledList6 = new List<List<Vector2>>();
    List<List<Vector2>> sampledList7 = new List<List<Vector2>>();
    List<List<Vector2>> sampledList8 = new List<List<Vector2>>();

    // Drawing sampled data
    LineRenderer sampledLineRenderer;
    List<Vector2> sampledPoints = new List<Vector2>();

        // Drawing sampled data1
   // List<GameObject> subSampledLineList = new List<GameObject>();
    LineRenderer subSampledLineRenderer;
    List<Vector2> subSampledPoints = new List<Vector2>();


    // 비율 맞추기 (디자인 시 보여지는 픽셀 크기(canvas)와 game play 시 보여지는 픽셀의 크기(Screen)가 다름)
    const float samplePixelSize = 300;
    const float smallSamplePixelSize = 150;
    float sampleWidth, sampleHeight;
    float xMin, yMin, xMax, yMax;
    float canvasWidth, canvasHeight;
    float ratioWidth, ratioHeight;      // Screen to Canvas ratio(screen/canvas)

    float smallSampleWidth, smallSampleHeight;
    // 서브 박스 좌표
    List<float> xSubMin = new List<float>();
    List<float> ySubMin = new List<float>();
    List<float> xSubMax = new List<float>();
    List<float> ySubMax = new List<float>();


    // 서버에 저장되는 데이터
    string baseLinePoint, linePoints, resizePoint;
    List<string> subBaseLinePointArray = new List<string>();
    List<string> subLinePointsArray = new List<string>();
    List<string> subReLinePointsArray = new List<string>();

    // 저장할 글자 제시
    //string[] letter = new string[26] {"가","너","도","루","므","비","샤","여","죠","츄","캐","턔","페","혜","꽊","뙜","뾗","쒅","쮆","읽","잚","밟","걼","톭","놇","빖"};
    //string[] letter = new string[5] {"ㅣ","이","기","비","니"};
    string[] letter = new string[5] {"혜","토","토","궟","늵"};
    
    Dictionary<string, int> wordStroke = new Dictionary<string, int>();

    int letterNum = 0;
    int gameCnt = 0;

    private int resWidth,resHeight;
    string path;

    // 제시어 음소 음절 분리
    List<string> elementArray = new List<string>();
    // 각 음소 획순 정보
    List<int> strokeArray = new List<int>();



    void Start()
    {
        // 자음모음 획순 정보 입력
        wordInit();

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

        // 화면 비율에 맞게 sample size 조절
        smallSampleWidth = smallSamplePixelSize * ratioWidth;
        smallSampleHeight = smallSamplePixelSize * ratioHeight;

        // 제시어 세팅
        letterNum = 0;
        nextLetter.GetComponent<Text>().text = letter[letterNum];

        char[] inputword = letter[letterNum].ToCharArray();

        // 제시어 자음 모음 분리
        elementArray = DivideHangul(inputword[0]);
        // 분리된 자음 모음 획순
        for(int i=0; i < elementArray.Count; i++) {
            Debug.Log(elementArray[i]);
            Debug.Log(wordStroke[elementArray[i]]);
            strokeArray.Add(wordStroke[elementArray[i]]);
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
        foreach(GameObject line in GameObject.FindGameObjectsWithTag("Lines")) { 
            Destroy(line); 
        }
    }

    public void Save(){
        // 초기화        
        linePoints = "";

        // 데이터 샘플링
        Sampling();

        // Sampled Area에 샘플된 데이터 그리기
        DrawSample();

        // 저장할 음소
       // letterNum = PlayerPrefs.GetInt("letterNum", 0);
        //string phoneme = letter[letterNum/10];

        //사용자 이름 받아오기
        string name = this.name.GetComponent<Text>().text;

        // 결과 메시지 띄우기
        SetResultText(linePoints,name);
        
        // 조건 만족하면 서버에 저장
        if(linePoints != "") {
            // 서버에 저장하기
            ServerManager manager = GameObject.Find("ServerManager").GetComponent<ServerManager>();
            //manager.saveData(name, phoneme, linePoints);

            // 저장하면 linePoints 지우기
            DeleteAll();

            // //10번째까지 저장하면 Dialog 메시지 띄우기
            // if(letterNum % 10 == 9) {
            //     SetDialogMessage(phoneme,linePoints,name);
            //     DeleteSample();
            // }

            StartCoroutine(ScreenShot());
        }
    }

    IEnumerator ScreenShot() {
        yield return new WaitForEndOfFrame();

        path = "/Users/isangho/HandwritingData/Handwriting/Assets/Temp/";
        Transform target = GameObject.Find("Canvas/SampledArea0/SampledData").GetComponent<Transform> ();
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

        DirectoryInfo dir = new DirectoryInfo(path);
        if (!dir.Exists)
        {
            Directory.CreateDirectory(path);
        }
        string filename;
        filename = path + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

        float basewidth = 1280.0f;
        float baseBox = 240.0f;

        float inputBox = (Screen.width / basewidth) * baseBox;

        Texture2D tex = new Texture2D((int)inputBox, (int)inputBox, TextureFormat.RGB24, true);

        // 1280 * 720 = 240
        float lastx = screenPos.x - (int)inputBox / 2;
        float lasty = screenPos.y - (int)inputBox / 2;

  
        tex.ReadPixels(new Rect((int)lastx, (int)lasty, (int)inputBox, (int)inputBox), 0, 0, true);
        tex.Apply();

        StartCoroutine(Uploads(tex.EncodeToPNG()));
       // byte[] bytes = tex.EncodeToPNG();
       // File.WriteAllBytes(filename, bytes);

        yield return new WaitForSeconds(3.2f);
    }


    IEnumerator Uploads(byte[] bytes) {
        WWWForm form = new WWWForm();
        form.AddBinaryData("files", bytes);
        form.AddField("problemWord", letter[letterNum]);
        form.AddField("wordCount", elementArray.Count);


        string fullBaseParam= "";
        string fullLineParam= "";
        string fullReLineParam= "";

                // 자음 모음 서버에 저장하기
        Debug.Log(elementArray.Count);
        int startNum = 0;
        int endNum = 0;
        for(int i = 0; i < elementArray.Count; i++) {
            
            if(i == 0) {
                startNum = i;
                endNum = strokeArray[i];
            } else {
                startNum = startNum + strokeArray[i-1];
                endNum = endNum + strokeArray[i];
            }

            Debug.Log("i:"+i+"/"+elementArray[i]+":"+strokeArray[i]+"/startNum:"+startNum+"/endNum:"+endNum);


            int k = 0;
            for(int j = startNum; j < endNum; j++) {
                if(k==0){
                    fullBaseParam = fullBaseParam + elementArray[i] +":";
                    fullLineParam = fullLineParam + elementArray[i] +":";
                    fullReLineParam = fullReLineParam + elementArray[i] +":";
                }
                fullBaseParam = fullBaseParam += subBaseLinePointArray[j] + "/";
                fullLineParam = fullLineParam += subLinePointsArray[j] + "/";
                fullReLineParam = fullReLineParam += subReLinePointsArray[j] + "/";
                k++;
            }
            fullBaseParam = fullBaseParam.Substring(0, fullBaseParam.Length - 1) + "_";
            fullLineParam = fullLineParam.Substring(0, fullLineParam.Length - 1) + "_";
            fullReLineParam = fullReLineParam.Substring(0, fullReLineParam.Length - 1) + "_";
        }

        fullBaseParam = fullBaseParam.Substring(0, fullBaseParam.Length - 1) ;
        fullLineParam = fullLineParam.Substring(0, fullLineParam.Length - 1) ;
        fullReLineParam = fullReLineParam.Substring(0, fullReLineParam.Length - 1);

        Debug.Log(fullLineParam);

        form.AddField("paramData", fullLineParam);
        form.AddField("paramReData", fullReLineParam);
        

        UnityWebRequest www = UnityWebRequest.Post("http://192.168.0.27/deep/uploadfiles", form);
        yield return www.SendWebRequest();
 
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            Debug.Log("Upload complete!");
        }

        // 제시어 세팅
        elementArray.Clear();
        strokeArray.Clear();

        gameCnt++;
        if(gameCnt % 2 == 0 ){
            letterNum ++;
        } 
        nextLetter.GetComponent<Text>().text = letter[letterNum];

        char[] inputword = letter[letterNum].ToCharArray();


        // 제시어 자음 모음 분리
        elementArray = DivideHangul(inputword[0]);

        Debug.Log(elementArray.Count);
        // 분리된 자음 모음 획순
        for(int i=0; i < elementArray.Count; i++) {
            Debug.Log(elementArray[i]);
            Debug.Log(wordStroke[elementArray[i]]);
            strokeArray.Add(wordStroke[elementArray[i]]);
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
            resultText.GetComponent<Text>().text = (letterNum%10+1) + "번째\n 저장되었습니다.";
        }
    }

    void Sampling() {
        // 변수 초기화
        sampledList.Clear();
        sampledList1.Clear();
        sampledList2.Clear();
        sampledList3.Clear();
        sampledList4.Clear();
        sampledList5.Clear();
        sampledList6.Clear();
        sampledList7.Clear();
        sampledList8.Clear();

        linePoints = "";
        baseLinePoint = "";
        resizePoint = "";
        subBaseLinePointArray.Clear();
        subLinePointsArray.Clear();
        subReLinePointsArray.Clear();

        xSubMin.Clear();
        ySubMin.Clear();
        xSubMax.Clear();
        ySubMax.Clear();

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
                pos[0] /= inputSize;
                pos[1] /= inputSize;
                resizePos[0] /= xinputSize;
                resizePos[1] /= yinputSize;

                // 노말라이즈 된 데이터를 배열에 저장
                sampled.Add(pos);
                
                // 서버에 저장할 데이터 string으로 이어붙이기
                linePoints += pos[0] + "," + pos[1] + ",";
                baseLinePoint += basePos[0] + "," + basePos[1] + ","; 
                resizePoint += resizePos[0] + "," + resizePos[1] + ",";
            }
            sampledList.Add(sampled);

        }
        linePoints = linePoints.Substring(0, linePoints.Length - 1);
        baseLinePoint = baseLinePoint.Substring(0, baseLinePoint.Length - 1);
        resizePoint = resizePoint.Substring(0, resizePoint.Length - 1);
        ////////////sub 자/모음 처리
        int startFor = 0;
        int endFor = 0;


        // 분리된 자음 모음 좌표 처리
        for(int i = 0; i < elementArray.Count; i++)
        {
            string subLinePoints = "";
            string subBaseLinePoint = "";
            string subResizePoint = "";
            
            float xLength2 = xSubMax[i] - xSubMin[i];
            float yLength2 = ySubMax[i] - ySubMin[i];

            float xinputSize2 = xLength2;
            float yinputSize2 = yLength2;
            float inputSize2 ;

            if(xLength2 > yLength2) inputSize2 = xLength2;
            else inputSize2 = yLength2;

            // 필기 데이터 중앙점 두번째 찾기
            float  xCenter2 = (xSubMax[i] + xSubMin[i])/2;
            float  yCenter2 = (ySubMax[i] + ySubMin[i])/2;

            // 전체 라인 배열에서 각 자음과 모음의 가져올 배열의 시작과 끝의 위치 정보
            if(i == 0) {
                endFor = strokeArray[0];
            }
            else if(i > 0) {
                startFor = endFor;
                endFor = endFor + strokeArray[i];
            }

            int t = 0;
            Debug.Log("획순 순서 : " + i); 
            // 필기 데이터를 일정 크기로 맞추기
            foreach(GameObject line in lineList) {
                if(t >= startFor && t < endFor) {
                    LineRenderer lr = line.GetComponent<LineRenderer>();
                    List<Vector2> sampled2 = new List<Vector2>();
                    List<Vector2> reSampled2 = new List<Vector2>();

                    subBaseLinePoint = "";
                    subLinePoints = "";
                    subResizePoint = "";

                    for (int k = 0; k < lr.positionCount; k++) { 
                        Vector2 basePos = Camera.main.WorldToScreenPoint(lr.GetPosition(k));
                        Vector2 pos = Camera.main.WorldToScreenPoint(lr.GetPosition(k));
                        Vector2 rePos = Camera.main.WorldToScreenPoint(lr.GetPosition(k));
                        
                        // 원점을 중심점으로 이동
                        pos[0] -= xCenter2;
                        pos[1] -= yCenter2;
                        rePos[0] -= xCenter2;
                        rePos[1] -= yCenter2;

                        // 0~1 사이의 사이즈로 줄이기
                        pos[0] /= inputSize2;
                        pos[1] /= inputSize2;
                        rePos[0] /= xinputSize2;
                        rePos[1] /= yinputSize2;

                        // 노말라이즈 된 데이터를 배열에 저장
                        sampled2.Add(pos);
    
                        // 서버에 저장할 데이터 string으로 이어붙이기
                        subBaseLinePoint += basePos[0] + "," + basePos[1] + ",";
                        subLinePoints += pos[0] + "," + pos[1] + ",";
                        subResizePoint += rePos[0] + "," + rePos[1] + ",";
                    }

                    subBaseLinePoint = subBaseLinePoint.Substring(0, subBaseLinePoint.Length - 1);
                    subLinePoints = subLinePoints.Substring(0, subLinePoints.Length - 1);
                    subResizePoint = subResizePoint.Substring(0, subResizePoint.Length - 1);

                    subBaseLinePointArray.Add(subBaseLinePoint);
                    subReLinePointsArray.Add(subResizePoint);
                    subLinePointsArray.Add(subLinePoints);

                    Debug.Log(subLinePoints);
                    if(i==0) {sampledList1.Add(sampled2);}
                    if(i==1) {sampledList2.Add(sampled2);}
                    if(i==2) {sampledList3.Add(sampled2);}
                    if(i==3) {sampledList4.Add(sampled2);}
                    if(i==4) {sampledList5.Add(sampled2);}
                    if(i==5) {sampledList6.Add(sampled2);}
                    if(i==6) {sampledList7.Add(sampled2);}
                    if(i==7) {sampledList8.Add(sampled2);}
                }
                t++;
            }
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

        int startFor = 0;
        int endFor = 0;

        for(int k = 0; k < elementArray.Count; k++) {
            xSubMin.Add(Screen.width);
            xSubMax.Add(0);
            ySubMin.Add(Screen.height);
            ySubMax.Add(0);

            if(k == 0) {
                endFor = strokeArray[0];
            }
            else if(k > 0) {
                startFor = endFor;
                endFor = endFor + strokeArray[k];
            }

            int t = 0;
             // 각 라인의 모든 포인트 순회하며 음소 최대, 최소값 구하기
            foreach(GameObject line in lineList) {
                if(t >= startFor && t < endFor) {
                    LineRenderer lr = line.GetComponent<LineRenderer>();
                    for (int i = 0; i < lr.positionCount; i++){
                        // 다시 월드 -> 스크린 포인트로 변환
                        Vector2 screenPosition = Camera.main.WorldToScreenPoint(lr.GetPosition(i));
                        float x = screenPosition[0];
                        float y = screenPosition[1];

                        if(xSubMin[k] > x) xSubMin[k] = x;
                        if(xSubMax[k] < x) xSubMax[k] = x;

                        if(ySubMin[k] > y) ySubMin[k] = y;
                        if(ySubMax[k] < y) ySubMax[k] = y;
                    }
                } 
                t++;
            }
        }
    }

    void DrawSample() {
        // 초기화
        DeleteSample();
        
        // Sample Area 중앙점
        float xSampleAreaCenter = Screen.width/2 + transform.Find("SampledArea0").GetComponent<RectTransform>().anchoredPosition.x  * ratioWidth;
        float ySampleAreaCenter = Screen.height/2 + transform.Find("SampledArea0").GetComponent<RectTransform>().anchoredPosition.y * ratioHeight;

        // 저장된 샘플 리스트 돌면서 모든 선 순회
        foreach(List<Vector2> line in sampledList) {
            GameObject sampledLine = Instantiate(linePrefab);
           //sampledLineList.Add(sampledLine);
            sampledLine.transform.SetParent(this.transform.Find("SampledArea0/SampledData"));
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

        for(int k = 0; k < elementArray.Count; k++) {
            string objectName = "SampledArea" + (k + 1);

            if(k == 0) {DrawCanvas(sampledList1,objectName);}
            else if(k == 1) {DrawCanvas(sampledList2,objectName);}
            else if(k == 2) {DrawCanvas(sampledList3,objectName);}
            else if(k == 3) {DrawCanvas(sampledList4,objectName);}
            else if(k == 4) {DrawCanvas(sampledList5,objectName);}
            else if(k == 5) {DrawCanvas(sampledList6,objectName);}
            else if(k == 6) {DrawCanvas(sampledList7,objectName);}
            else if(k == 7) {DrawCanvas(sampledList8,objectName);}
        }
    }

    void DrawCanvas (List<List<Vector2>> sampledSubList,string objectName) {
        float xSampleAreaCenterSub = 0.0f;
        float ySampleAreaCenterSub = 0.0f;

        xSampleAreaCenterSub = Screen.width/2 + transform.Find(objectName).GetComponent<RectTransform>().anchoredPosition.x  * ratioWidth;
        ySampleAreaCenterSub = Screen.height/2 + transform.Find(objectName).GetComponent<RectTransform>().anchoredPosition.y * ratioHeight;

        int LineCount = 0;
        // 저장된 샘플 리스트 돌면서 모든 선 순회
        foreach(List<Vector2> line in sampledSubList) {
            Color c1 = new Color(1, 0, 0, 1);
            Color c2 = Color.black;
            GameObject subSampledLine = Instantiate(linePrefab);

            //subSampledLineList.Add(subSampledLine);
            subSampledLine.transform.SetParent(this.transform.Find(objectName+"/SampledData"));
            
            subSampledLineRenderer = subSampledLine.GetComponent<LineRenderer>();
            subSampledLineRenderer.positionCount = 0;

            // 각 선의 모든 점을 순회하면서 drawing
            for (int i = 0; i < line.Count; i++) {
                if(LineCount == 0){ c1 = new Color(1, 0, 0, 1);} 
                else if(LineCount == 1){c1 = new Color(1, 0, 0, 1);}
                else if(LineCount == 2){c1 = new Color(0, 0, 1, 1);}
                else if(LineCount == 3){c1 = new Color(0, 1, 0, 1);}
                else if(LineCount == 4){c1 = new Color(1, 1, 0, 1);}
                else if(LineCount == 5){c1 = new Color(0, 1, 1, 1);}
                else if(LineCount == 6){c1 = new Color(1, 0, 1, 1);}
                
                subSampledLineRenderer.SetColors(c1, c2);
                // 샘플링 된 점을 sampling area로 옮긴 후
                Vector2 sampledPos = Vector2.zero;
                sampledPos[0] = line[i][0] * smallSampleWidth + xSampleAreaCenterSub;
                sampledPos[1] = line[i][1] * smallSampleHeight + ySampleAreaCenterSub;
                sampledPos = Camera.main.ScreenToWorldPoint(sampledPos);
                
                // line Drawing
                subSampledPoints.Add(sampledPos);
                subSampledLineRenderer.positionCount++;
                subSampledLineRenderer.SetPosition(subSampledLineRenderer.positionCount - 1, sampledPos);
            }
            LineCount++;
        }
        subSampledPoints.Clear();
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


    public static List<string> DivideHangul(char source)
    {
        string mOnsetTbl = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        string mNucleusTbl = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        string mCodaTbl = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
        ushort mUniCodeBase = 0xAC00;
        ushort mUniCodeLast = 0xD79F;
        string mOnset=" ";
        string mNucleus=" ";
        string mCoda=" ";

        List<string>fullString = new List<string>();
        int iOnsetIdx, iNucleusIdx, iCodaIdx; // Onset,Nucleus,Coda의 인덱스
        ushort uTempCode = 0x0000;       // 임시 코드용
        //Char을 16비트 부호없는 정수형 형태로 변환 - Unicode
        uTempCode = System.Convert.ToUInt16(source);
        // 캐릭터가 한글이 아닐 경우 처리
        if ((uTempCode < mUniCodeBase) || (uTempCode > mUniCodeLast))
        {
            mOnset = " "; mNucleus = " "; mCoda = " ";
        }
        // iUniCode에 한글코드에 대한 유니코드 위치를 담고 이를 이용해 인덱스 계산.
        int iUniCode = uTempCode - mUniCodeBase;
        iOnsetIdx = iUniCode / (21 * 28);
        iUniCode = iUniCode % (21 * 28);
        iNucleusIdx = iUniCode / 28;
        iUniCode = iUniCode % 28;
        iCodaIdx = iUniCode;
        string consonant = System.Convert.ToString(source);


        if(iUniCode < 0) {
            mNucleus = consonant;
        } else {
            mOnset = new string(mOnsetTbl[iOnsetIdx], 1);
            mNucleus = new string(mNucleusTbl[iNucleusIdx], 1);
            mCoda = new string(mCodaTbl[iCodaIdx], 1);
        }

       

        if(!" ".Equals(mOnset)){
            if("ㄲ".Equals(mOnset)) {fullString.Add("ㄱ");fullString.Add("ㄱ");}
            else if("ㄸ".Equals(mOnset)) {fullString.Add("ㄷ");fullString.Add("ㄷ");}
            else if("ㅃ".Equals(mOnset)) {fullString.Add("ㅂ");fullString.Add("ㅂ");}
            else if("ㅆ".Equals(mOnset)) {fullString.Add("ㅅ");fullString.Add("ㅅ");}
            else if("ㅉ".Equals(mOnset)) {fullString.Add("ㅈ");fullString.Add("ㅈ");}
            else {fullString.Add(mOnset);}
        }

        if(!" ".Equals(mNucleus)){
            if("ㅘ".Equals(mNucleus)) {fullString.Add("ㅗ");fullString.Add("ㅏ");}
            else if("ㅙ".Equals(mNucleus)) {fullString.Add("ㅗ");fullString.Add("ㅐ");}
            else if("ㅚ".Equals(mNucleus)) {fullString.Add("ㅗ");fullString.Add("ㅣ");}
            else if("ㅝ".Equals(mNucleus)) {fullString.Add("ㅜ");fullString.Add("ㅓ");}
            else if("ㅞ".Equals(mNucleus)) {fullString.Add("ㅜ");fullString.Add("ㅔ");}
            else if("ㅟ".Equals(mNucleus)) {fullString.Add("ㅜ");fullString.Add("ㅣ");}
            else if("ㅢ".Equals(mNucleus)) {fullString.Add("ㅡ");fullString.Add("ㅣ");}
            else fullString.Add(mNucleus);
        }

        if(!" ".Equals(mCoda)) {
            if("ㄲ".Equals(mCoda)) {fullString.Add("ㄱ");fullString.Add("ㄱ");}
            else if("ㄸ".Equals(mCoda)) {fullString.Add("ㄷ");fullString.Add("ㄷ");}
            else if("ㅃ".Equals(mCoda)) {fullString.Add("ㅂ");fullString.Add("ㅂ");}
            else if("ㅆ".Equals(mCoda)) {fullString.Add("ㅅ");fullString.Add("ㅅ");}
            else if("ㅉ".Equals(mCoda)) {fullString.Add("ㅈ");fullString.Add("ㅈ");}
            else if("ㄳ".Equals(mCoda)) {fullString.Add("ㄱ");fullString.Add("ㅅ");}
            else if("ㄵ".Equals(mCoda)) {fullString.Add("ㄴ");fullString.Add("ㅈ");}
            else if("ㄶ".Equals(mCoda)) {fullString.Add("ㄴ");fullString.Add("ㅎ");}
            else if("ㄺ".Equals(mCoda)) {fullString.Add("ㄹ");fullString.Add("ㄱ");}
            else if("ㄻ".Equals(mCoda)) {fullString.Add("ㄹ");fullString.Add("ㅁ");}
            else if("ㄼ".Equals(mCoda)) {fullString.Add("ㄹ");fullString.Add("ㅂ");}
            else if("ㄽ".Equals(mCoda)) {fullString.Add("ㄹ");fullString.Add("ㅅ");}
            else if("ㄾ".Equals(mCoda)) {fullString.Add("ㄹ");fullString.Add("ㅌ");}
            else if("ㅀ".Equals(mCoda)) {fullString.Add("ㄹ");fullString.Add("ㅎ");}
            else if("ㅄ".Equals(mCoda)) {fullString.Add("ㅂ");fullString.Add("ㅅ");}
            else {fullString.Add(mCoda);}
        } 


        string debugString = ""; 
        for(int i=0; i < fullString.Count; i++) {
            debugString = debugString + fullString[i];
        }
        Debug.Log("/"+debugString+"/");
        return fullString;
    }


    void wordInit() {
        wordStroke.Add("ㄱ",1);
        wordStroke.Add("ㄴ",1);
        wordStroke.Add("ㄷ",2);
        wordStroke.Add("ㄹ",3);
        wordStroke.Add("ㅁ",3);
        wordStroke.Add("ㅂ",4);
        wordStroke.Add("ㅅ",2);
        wordStroke.Add("ㅇ",1);
        wordStroke.Add("ㅈ",2);
        wordStroke.Add("ㅊ",3);
        wordStroke.Add("ㅋ",2);
        wordStroke.Add("ㅌ",3);
        wordStroke.Add("ㅍ",4);
        wordStroke.Add("ㅎ",3);
        wordStroke.Add("ㅏ",2);
        wordStroke.Add("ㅓ",2);
        wordStroke.Add("ㅗ",2);
        wordStroke.Add("ㅜ",2);
        wordStroke.Add("ㅡ",1);
        wordStroke.Add("ㅣ",1);
        wordStroke.Add("ㅑ",3);
        wordStroke.Add("ㅕ",3);
        wordStroke.Add("ㅛ",3);
        wordStroke.Add("ㅠ",3);
        wordStroke.Add("ㅐ",3);
        wordStroke.Add("ㅒ",4);
        wordStroke.Add("ㅔ",3);
        wordStroke.Add("ㅖ",4);
        wordStroke.Add("ㅘ",4);
        wordStroke.Add("ㅙ",5);
        wordStroke.Add("ㅚ",3);
        wordStroke.Add("ㅝ",4);
        wordStroke.Add("ㅞ",5);
        wordStroke.Add("ㅟ",3);
        wordStroke.Add("ㅢ",2);
    }
}
