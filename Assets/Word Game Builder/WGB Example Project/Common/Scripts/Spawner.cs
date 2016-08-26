using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
	void Start() 
	{
		spawn ();
	}

	// Groups


	public GameObject[] groups;
	public Vector3[] spawns;


	void spawn()
	{
		
			InvokeRepeating("spawnNext", 1, 1);
	
	
	}

	public void spawnNext() 
	{
		// Random Index
		int i = Random.Range(0, groups.Length);
		int j = Random.Range (0, spawns.Length);

		// Spawn Group at current Position
		Instantiate(groups[i],
			spawns[j],
			Quaternion.identity);
	}
}