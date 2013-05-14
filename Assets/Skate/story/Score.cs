using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {
	
	public GUIStyle myButtonStyle;
	
	private int score;
	private int maxScore;
	
	// Use this for initialization
	void Start () {
	}
	
	public void increaseScore() {
		this.score++;
		if (score == maxScore || true) {
			
			GameObject.Find("3rd Person Controller").GetComponent<SkateController>().Win();	
		}
	}
	
	public void setMaxScore(int maxScore) {
		this.maxScore = maxScore;
	}
	
	// Update is called once per frame
	void OnGUI () {
		if (score > 0) {
				GUI.Label (new Rect (0,40,Screen.width,200), score + " of " + maxScore, myButtonStyle);
		}
		if (Time.timeSinceLevelLoad < 5f) {
			GUI.Label (new Rect (0,Screen.height / 2,Screen.width,Screen.height / 2), "Arrow keys, spacebar", myButtonStyle);
		}
	}
	
	void Update() {
	 if(Input.GetKeyDown(KeyCode.Escape) == true)
		 {
		  Application.Quit();
		 }	
	}
}
