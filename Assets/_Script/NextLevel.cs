using UnityEngine;
using System.Collections;

public class NextLevel : MonoBehaviour {
	public void Change_Level(){
		Application.LoadLevel(2);
	}
}
