using System;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class LevelName : MonoBehaviour
{
	public bool nextLevel;
	private void OnEnable() {
		GetComponentInChildren<TMP_Text>().text = nextLevel ? FindObjectOfType<GameManager>().nextSceneName : gameObject.scene.name;
	}
}