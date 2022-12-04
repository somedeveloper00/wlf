using UnityEngine;

[ExecuteAlways]
public class PutEveryCellUnderTransform : MonoBehaviour
{
	public Transform parent;

	private void Update() {
		FindObjectsOfType<Cell>().ForEach( c => c.transform.parent = parent);
	}
}