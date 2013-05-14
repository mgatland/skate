using UnityEngine;
using System.Collections;

public class funbox : MonoBehaviour {
	
	public Transform secretFunboxContainer;
	public Material[] openMaterials;
	
	Vector3 initialPosition;
	bool taken = false;

	// Use this for initialization
	void Start () {
		initialPosition = transform.position;
		if (secretFunboxContainer != null) {
			//I am the special unhidden funbox
			int maxScore = 1;
			foreach (Transform child in secretFunboxContainer.transform) {
				child.gameObject.SetActive(false);
				maxScore++;
			}	
			GameObject.Find("3rd Person Controller").GetComponent<Score>().setMaxScore(maxScore);
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(0f,20f* Time.deltaTime,0f);
		if (taken) {
			transform.Translate(0,0.7f * Time.deltaTime, 0f);
		} else {
			transform.position = initialPosition + Vector3.up * Mathf.PingPong(Time.time/10f, 0.2f);
		}
	}
	
	  // Destroy everything that enters the trigger
     void OnTriggerEnter(Collider other) {
		if (taken) {
			return;
		}
        taken = true;
		renderer.materials = openMaterials;
		Transform[] children = gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform t in children) {
			if (t != transform) { //every transform except my own
				ParticleSystem ps = t.GetComponent<ParticleSystem>();
				ps.Play();
			}
		}
		
		GameObject.Find("3rd Person Controller").GetComponent<Score>().increaseScore();
		if (secretFunboxContainer != null) {
			//when the first Funbox is found, enable all other Funboxes in the whole world.
			foreach (Transform child in secretFunboxContainer.transform) {
				child.gameObject.SetActive(true);
			}
		}
    }
}
