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
	private static string serverUrl = "http://127.0.0.1:8500"; // 로컬 서버
	private static string loginUrl = serverUrl + "/findAccountWithPW";
	private static string signupUrl = serverUrl + "/signup";
	private static string modeUrl = serverUrl + "/getServerMode";
	private static string tempPasswordUrl = serverUrl + "/makeTempPassword";
	private static string newPasswordUrl = serverUrl + "/makeNewPassword";
	private static string tokenUrl = serverUrl + "/mysql/addToken";
	private static string timeUrl = serverUrl + "/mysql/addTime";
	public LoginInput loginInput;
	public SignupInput signupInput;

	private static int serverMode = -1;
	public static DateTime today;

	public delegate void ServerDelegate (string id);

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

	public void GetToken(string email, string token) {
		StartCoroutine (GetTokenRoutine(email, token));
	}

	IEnumerator GetTokenRoutine(string email, string token) {

		WWWForm tokenForm = new WWWForm ();
		tokenForm.AddField("token", token);
		tokenForm.AddField("email", email);
		UnityWebRequest Token = UnityWebRequest.Post(ServerManager.tokenUrl, tokenForm); //post

		yield return Token.SendWebRequest();
		if(Token.error == null) {
			string serverResponseString = Token.downloadHandler.text;
			JObject serverResponse = JObject.Parse (serverResponseString);
		} else {
			Debug.Log("Received Registration Token: " + token);
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

    IEnumerator WaitForLoginRequest (string email, string password, bool autologin = false) {
		WWWForm loginForm = new WWWForm ();
		loginForm.AddField ("email", email);
		loginForm.AddField ("password", password);
		UnityWebRequest login = UnityWebRequest.Post(ServerManager.loginUrl, loginForm);
		yield return login.SendWebRequest();
		
		if (!login.isNetworkError) {
			string serverResponseString = login.downloadHandler.text;
			JObject serverResponse = JObject.Parse (serverResponseString);
			int serverCode = serverResponse ["code"].ToObject<int> ();
        }
	}

	void Start () {
        
	}

	void OnApplicationPause (bool pauseStatus)
	{
		// Check the pauseStatus to see if we are in the foreground
		// or background
		if (!pauseStatus) {
			//app resume			
		}
	}

    IEnumerator test1 (string email, string password, bool autologin = false) {
		Debug.Log("코루틴 시작");
		WWWForm testForm = new WWWForm();
		

		WWWForm loginForm = new WWWForm ();
		loginForm.AddField ("email", email);
		loginForm.AddField ("password", password);
		UnityWebRequest login = UnityWebRequest.Post(ServerManager.loginUrl, loginForm);
		yield return login.SendWebRequest();
		
		if (!login.isNetworkError) {
			// string serverResponseString = login.downloadHandler.text;
			// JObject serverResponse = JObject.Parse (serverResponseString);
			// int serverCode = serverResponse ["code"].ToObject<int> ();
        }
	}

	public void test() {
		Debug.Log("테스트 함수");
		StartCoroutine(test1("1","2",false));
	}

}