using System;
using System.Linq;
using AnimFlex.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
	public Main input;
	
	public Cell[] cells = Array.Empty<Cell>();

	[SerializeField] string nextSceneName;
	[SerializeField] float cellDistance = 10;
	[SerializeField] float duration = 1;
	[SerializeField] float delay = 0.2f;
	[SerializeField] Ease ease = Ease.InOutCirc;

	private int _inMovings = 0;


	void Awake() {
		cells = FindObjectsOfType<Cell>();
		input = new();
		input.player.up.performed += onMoveUpInput;
		input.player.down.performed += onMoveDownInput;
		input.player.right.performed += onMoveRightInput;
		input.player.left.performed += onMoveLeftInput;
	}

	void OnEnable() => input.player.Enable();
	void OnDisable() => input.player.Disable();

#region cell change commands
	public void SetBabaIsYou( bool value ) => cells.SetYou<Baba>( value );
	public void SetWallIsYou( bool value ) => cells.SetYou<Wall>( value );
#endregion

	void onMoveUpInput(InputAction.CallbackContext _) => moveYous( Vector2Int.up );
	void onMoveDownInput(InputAction.CallbackContext _) => moveYous( Vector2Int.down );
	void onMoveRightInput(InputAction.CallbackContext _) => moveYous( Vector2Int.right );
	void onMoveLeftInput(InputAction.CallbackContext _) => moveYous( Vector2Int.left );

	void moveYous(Vector2Int dir) {
		if(_inMovings > 0) return;
		foreach (var movable in cells.Where( c => c.IsYou )) 
			moveCell( dir, movable );
	}

	private void moveCell(Vector2Int dir, Cell movable) {
		Debug.Log( $"moving cell {movable.transform.name} to {dir}" );
		var dest = (Vector2)movable.transform.up * dir.y + (Vector2)movable.transform.right * dir.x;
		dest += (Vector2)movable.transform.position;
		
		// check if destination is valid
		if ( cells.Any( c => (Vector2)c.transform.position == dest && c is Wall ) ) {
			return;
		}
		
		_inMovings++;
		var anim = movable.transform.AnimPositionTo( dest * cellDistance, ease, duration, delay );
		anim.onComplete += () => _inMovings--;
	}
}