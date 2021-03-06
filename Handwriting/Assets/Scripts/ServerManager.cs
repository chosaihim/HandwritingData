using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class LoginInput {
	public InputField emailInput;
	public InputField passwordInput;
	public Button loginButton;
	public int nextSceneIndex = -1;
}

[System.Serializable]
public class SignupInput {
	public InputField emailInput;
	public InputField passwordInput;
	public InputField phoneInput;
	public Button signupButton;
	public int nextSceneIndex = -1;
}

public class ServerManager : MonoBehaviour {
	static ServerManager instance;

	public VerifySceneManager verifySceneManager;

	// private static string serverUrl = "http://api.sojunghangeul.com"; // 운영 서버
	// private static string serverUrl = "http://ec2-3-35-166-183.ap-northeast-2.compute.amazonaws.com:8600"; // 개발 서버
	//private static string serverUrl = "http://127.0.0.1:8500"; // 로컬 서버
	private static string serverUrl = "http://192.168.0.27:8500"; // 딥러닝 서버 주소
	private static string modeUrl = serverUrl + "/getServerMode";
	private static string timeUrl = serverUrl + "/mysql/addTime";
	private static string saveDataUrl = serverUrl + "/saveData";
	private static string loadDataUrl = serverUrl + "/loadData";
	private static string deleteDataUrl = serverUrl + "/deleteData";

	private static int serverMode = -1;
	public static DateTime today;

	void Awake () {
		if(instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);
			StartCoroutine (GetDate ());
		} else {
			Destroy(gameObject);
		}
	}

	public static string GetNow(string format) {
		DateTime utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
		DateTime date = utc.AddHours(9);

		return date.ToString(format);
	}

	IEnumerator GetDate () {
		string serverUrl = ServerManager.GetServerUrl();
		UnityWebRequest www = UnityWebRequest.Get(serverUrl + "/getDate");
		yield return www.SendWebRequest();

		if(www.error == null) {
			string serverResponseString = www.downloadHandler.text;
			JObject serverResponse = JObject.Parse (serverResponseString);
			string date = serverResponse ["date"].ToString();		
			ServerManager.today = System.DateTime.Parse(date);
		}
	}

	public void GetTime(string email) {
		StartCoroutine (GetTimeRoutine(email));
	}
	
	IEnumerator GetTimeRoutine(string email) {
		string time = System.DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");

		WWWForm timeForm = new WWWForm ();
		timeForm.AddField("time", time);
		timeForm.AddField("email", email);
		UnityWebRequest Time = UnityWebRequest.Post(ServerManager.timeUrl, timeForm); //post

		yield return Time.SendWebRequest();
		if(Time.error == null) {
			string serverResponseString = Time.downloadHandler.text;
			JObject serverResponse = JObject.Parse (serverResponseString);
		} else {
			Debug.Log("Received Registration Time: " + time);
		}
	}

	IEnumerator GetServerModeRoutine () {
		WWWForm form = new WWWForm ();
		UnityWebRequest mode = UnityWebRequest.Post(ServerManager.modeUrl, form);

		yield return mode.SendWebRequest();
		if (mode.error == null) {
			string serverResponseString = mode.downloadHandler.text;
			JObject serverResponse = JObject.Parse (serverResponseString);
			ServerManager.serverMode = serverResponse ["serverMode"].ToObject<int> ();

		} else {
			Debug.Log ("GetServerMode: " + mode.error);
		}
	}

	public static int GetServerMode () {
		return serverMode;
	}

	public static string GetServerUrl() {
		return serverUrl;
	}

	void Start () {
        
	}

	public void saveData (string name, string phoneme, string data, string baseLinePoint, string resizePoint, string wordType) {
		StartCoroutine(SaveData(name, phoneme, data, baseLinePoint, resizePoint, wordType));
	}

	IEnumerator SaveData (string name, string phoneme, string data, string baseLinePoint, string resizePoint, string wordType) {
		WWWForm saveDataForm = new WWWForm();
		saveDataForm.AddField ("name", name);
		saveDataForm.AddField ("data", data);
		saveDataForm.AddField ("resizeData", resizePoint);
		saveDataForm.AddField ("baseData", baseLinePoint);
		saveDataForm.AddField ("phoneme", phoneme);
		saveDataForm.AddField ("wordType", wordType);
		UnityWebRequest saveDataReturn = UnityWebRequest.Post(saveDataUrl, saveDataForm);
		yield return saveDataReturn.SendWebRequest();

		if(!saveDataReturn.isNetworkError){
			Debug.Log(saveDataReturn);
		} else {
			Debug.Log("서버 저장 실패!");
		}
	}

	public void GetLoadData (string name, string phoneme, int index) {
		StartCoroutine(LoadData(name, phoneme, index));
	}

	public IEnumerator LoadData (string name, string phoneme, int index) {
		
		WWWForm loadDataForm = new WWWForm();
		loadDataForm.AddField ("name", name);
		loadDataForm.AddField ("phoneme", phoneme);
		loadDataForm.AddField ("index", index);

		UnityWebRequest loadDataReturn = UnityWebRequest.Post(loadDataUrl,loadDataForm);
		yield return loadDataReturn.SendWebRequest();

		if(!loadDataReturn.isNetworkError){
						
			string loadDataReturnString = loadDataReturn.downloadHandler.text;
			// JObject jsonData = JObject.Parse(loadDataReturnString);
			// Debug.Log(jsonData["length"]);
			// JArray arrayData = (JArray)jsonData["dataArray"];

			// for(int i=0; i < (int)jsonData["length"]; i++){
			// 	Debug.Log(arrayData[i]["data"]);
			// }
			verifySceneManager.loadDataFlag = 1;
			verifySceneManager.loadDataString = loadDataReturnString;
		} else {
			Debug.Log("데이터 로드 실패!");
		}
	}

	public void DeleteData (string id) {
		StartCoroutine(IEDeleteData(id));
	}

	IEnumerator IEDeleteData (string id) {
		
		WWWForm deleteDataForm = new WWWForm();
		deleteDataForm.AddField ("id", id);

		UnityWebRequest deleteDataReturn = UnityWebRequest.Post(deleteDataUrl,deleteDataForm);
		yield return deleteDataReturn.SendWebRequest();

		if(!deleteDataReturn.isNetworkError){

			Debug.Log("데이터 삭제 성공!");
		} else {
			Debug.Log("데이터 삭제 실패!");
		}
	}


}