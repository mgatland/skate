using UnityEngine;
using System.Collections;

public class Level : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Transform[] children = gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform t in children) {
			if (t != transform) { //every transform except my own
				t.gameObject.AddComponent<MeshCollider>();
				t.gameObject.AddComponent("GridUV");
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
