using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class VerifySceneManager : MonoBehaviour
{
    string[] letter = new string[20] {"ㄱ","ㄴ","ㄷ","ㄹ","ㅁ","ㅂ","ㅅ","ㅇ","ㅈ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ","ㅏ","ㅓ","ㅗ","ㅜ","ㅡ","ㅣ"};
    float[] centerX = new float[5] {-640, -420, -200, 20, 240};
    float[] centerY = new float[4] {330, 110, -110, -330};
    public GameObject linePrefab, loadText;
    int letterNum = 0;
    int pageNum = 0;
    int dataLength = 0;
    ServerManager manager;
    public int loadDataFlag = 0;
    public string loadDataString;
    JArray arrayData;


    // Drawing sampled data
    List<List<Vector2>> sampledList = new List<List<Vector2>>();
    List<GameObject> sampledLineList = new List<GameObject>();
    LineRenderer sampledLineRenderer;
    List<Vector2> sampledPoints = new List<Vector2>();
    EdgeCollider2D sampledCol;

    //비율 맞추기
    float canvasWidth, canvasHeight;
    float ratioWidth, ratioHeight;      // Screen to Canvas ratio(screen/canvas)
    const float samplePixelSize = 180;
    float sampleWidth, sampleHeight;

    

    // Start is called before the first frame update
    void Start()
    {
        // 서버 가져오기
        manager = GameObject.Find("ServerManager").GetComponent<ServerManager>();       

        // canvas 크기
        canvasWidth = this.GetComponent<RectTransform>().rect.width;
        canvasHeight = this.GetComponent<RectTransform>().rect.height;

        // Screen.width / canvasWidth
        ratioWidth = Screen.width / canvasWidth;
        ratioHeight = Screen.height / canvasHeight;

        // 화면 비율에 맞게 sample size 조절
        sampleWidth = samplePixelSize * ratioWidth;
        sampleHeight = samplePixelSize * ratioHeight;    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadDate() {
        pageNum = 0;
        dataLength = 0;

        if(letterNum < letter.Length) {
            StartCoroutine(getDataFromServer());
            loadText.GetComponent<Text>().text = letter[++letterNum];
        }
    }

    IEnumerator getDataFromServer() {
        Debug.Log(loadDataFlag);
        yield return StartCoroutine(manager.LoadData("이름", letter[letterNum], 0));

        if(loadDataString != null){
            JObject jsonData = JObject.Parse(loadDataString);
            dataLength = (int) jsonData["length"];
            Debug.Log(jsonData["length"]);
            arrayData = (JArray)jsonData["dataArray"];
            
            int loopLength = dataLength;

            if(dataLength >= 20)
                loopLength = 20;

            setSamples(0, loopLength-1);
        } else {
            Debug.Log("불러온 데이터가 없습니다.");
        }
    }


    public void setSamples(int first, int last) {
        DeleteSample();

        for(int i = 0; i < last - first +1; i++) {
            setASample(first + i);
        }

        DrawSample();
    }

    public void setASample(int index) {
        string pointString = arrayData[index]["data"].ToString().TrimEnd(',');
        string[] words = pointString.Split(',');

        List<Vector2> sampled = new List<Vector2>();
        for(int i=0; i < words.Length ; i += 2) {
            Vector2 pos = Vector2.zero;
            pos[0] = float.Parse(words[i]);
            pos[1] = float.Parse(words[i+1]);

            sampled.Add(pos);
        }
        sampledList.Add(sampled);
    }

    void DrawSample() {
        // 초기화
        // DeleteSample();
        
        // Sample Area 중앙점
        float xSampleAreaCenter = Screen.width/2  - 640 * ratioWidth;
        float ySampleAreaCenter = Screen.height/2 + 330 * ratioHeight;

        // 저장된 샘플 리스트 돌면서 모든 선 순회
        int index = 0;
        foreach(List<Vector2> line in sampledList) {
            Debug.Log("인덱스: " + index + " (x,y): (" + index/5 + "," + index/4 + ")");
            GameObject sampledLine = Instantiate(linePrefab);
            sampledLineList.Add(sampledLine);
            sampledLine.transform.SetParent(this.transform.Find("SampledArea/SampledData"));
            sampledLineRenderer = sampledLine.GetComponent<LineRenderer>();
            sampledLineRenderer.positionCount = 0;
            
            // 각 선의 모든 점을 순회하면서 drawing
            for (int i = 0; i < line.Count; i++){
                // 샘플링 된 점을 sampling area로 옮긴 후
                Vector2 sampledPos = Vector2.zero;
                sampledPos[0] = line[i][0] * sampleWidth + Screen.width/2 + centerX[index%5] * ratioWidth;
                sampledPos[1] = line[i][1] * sampleHeight + Screen.height/2 + centerY[index/5] * ratioWidth;
                sampledPos = Camera.main.ScreenToWorldPoint(sampledPos);
                
                // line Drawing
                sampledPoints.Add(sampledPos);
                sampledLineRenderer.positionCount++;
                sampledLineRenderer.SetPosition(sampledLineRenderer.positionCount - 1, sampledPos);
            }

            index++;
        }
        sampledPoints.Clear();
    }


    void DeleteSample() {
        foreach(GameObject line in sampledLineList) { 
            Destroy(line); 
        }
        sampledLineList.Clear();
        sampledList.Clear();
    }

    public void NextPage() {
        pageNum++;
        if(dataLength > pageNum * 20){
            int last = pageNum * 20 + 20;
            if(dataLength - pageNum * 20 < 20)
                last = dataLength;

            setSamples(pageNum*20,last-1);
        } else {
            pageNum--;
        }
    }

    public void PrevPage() {
        if(pageNum > 0){
            pageNum--;
            int last = pageNum * 20 + 20;
            if(dataLength - pageNum * 20 < 20)
                last = dataLength;

            setSamples(pageNum*20,last-1);
        }
    }

}
