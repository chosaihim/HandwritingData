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

	// private static string serverUrl = "http://api.sojunghangeul.com"; // 운영 서버
	// private static string serverUrl = "http://ec2-3-35-166-183.ap-northeast-2.compute.amazonaws.com:8600"; // 개발 서버
	// private static string serverUrl = "http://127.0.0.1:8500"; // 로컬 서버
	private static string serverUrl = "http://183.107.25.42:8500"; // 딥러닝 서버 주소
	private static string modeUrl = serverUrl + "/getServerMode";
	private static string timeUrl = serverUrl + "/mysql/addTime";
	private static string saveDataUrl = serverUrl + "/saveData";

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

	public void saveData (string name, string phoneme, string data) {
		StartCoroutine(SaveData(name, phoneme, data));
	}

	IEnumerator SaveData (string name, string phoneme, string data) {
		WWWForm saveDataForm = new WWWForm();
		saveDataForm.AddField ("name", name);
		saveDataForm.AddField ("data", data);
		saveDataForm.AddField ("phoneme", phoneme);
		UnityWebRequest saveDataReturn = UnityWebRequest.Post(saveDataUrl, saveDataForm);
		yield return saveDataReturn.SendWebRequest();

		if(!saveDataReturn.isNetworkError){
			Debug.Log(saveDataReturn);
		} else {
			Debug.Log("서버 저장 실패!");
		}
	}

}