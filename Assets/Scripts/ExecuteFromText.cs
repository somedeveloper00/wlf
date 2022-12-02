using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ExecuteFromText : MonoBehaviour
{
	public TMP_InputField text;

	public void Execute() {
		FindObjectOfType<GameManager>().ExecuteCommand( text.text.TrimEnd() );
	}
}