using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridEdit : MonoBehaviour
{
	public Transform placingPrefab;
	public Transform childPrefab;
	public string nameFormat = "ground $X $Y";

	public void Create(int w, int h, float d) {
		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				var obj = Instantiate( placingPrefab, new Vector3( x * d, y * d ), quaternion.identity, transform );
				obj.name = nameFormat.Replace( "$X", x.ToString() ).Replace( "$Y", y.ToString() );
				if ( childPrefab != null ) Instantiate( childPrefab, obj );
			}
		}
	}

	public void DeleteAll() {
		foreach (var trans in GetComponentsInChildren<Transform>()) {
			if ( ReferenceEquals( trans, transform ) || trans == null ) continue;
			if ( Application.isPlaying ) Destroy( trans.gameObject );
			else DestroyImmediate( trans.gameObject );
		}
	}

#if UNITY_EDITOR
	[CustomEditor( typeof(GridEdit) )]
	class editor : Editor
	{
		int w, h;
		float d = 1;

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.OnInspectorGUI();
			using (new GUILayout.VerticalScope( EditorStyles.helpBox )) {
				w = EditorGUILayout.IntSlider( "w", w, 0, 15 );
				h = EditorGUILayout.IntSlider( "h", h, 0, 15 );
				d = EditorGUILayout.Slider( "d", d, 0, 5 );
				if ( GUILayout.Button( "Create" ) ) {
					((GridEdit)serializedObject.targetObject).Create( w, h, d );
				}
				if ( GUILayout.Button( "Delete All" ) ) {
					((GridEdit)serializedObject.targetObject).DeleteAll();
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}