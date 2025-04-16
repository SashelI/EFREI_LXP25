using Astrolabe.Diagnostics;
using Harbor.Platforms.Meta_Quest;
using Meta.XR.MRUtilityKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using Random = UnityEngine.Random;

public class CubeManager : MonoBehaviour
{
	//Objet parent de tout nos cubes
	public Transform parentObject;

	//Prefab du cube que l'on va spawn
	public GameObject cubePrefab;

	private MRUK _sceneManager;
	private bool _isSceneLoaded;

	//Liste qui contient tous nos cubes instanciés.
	//Stocker des Transform est moins couteux que de stocker un GameObject (tous les gameobjects ont des transforms)
	private List<Transform> _cubeInstances=new();

	private bool _isRaveOn = false;

	/// <summary>
	/// Instancie un cube à des coordonnées aléatoires dans un rayon de 1m devant l'utilisateur
	/// </summary>
	public void SpawnCube()
	{
		//On choisit une position dans le rayon voulu
		var position = Camera.main.transform.position //Position du user
		               + new Vector3(Random.Range(-0.5f, 0.5f), 1.0f, Random.Range(1.0f, 1.5f)); //offset aléatoire en x et z, un peu de hauteur pour le style
		
		//On instancie le prefab et on le stocke dans la liste, dans la même ligne
		_cubeInstances.Add(Instantiate(cubePrefab, position, Quaternion.identity, parentObject).transform);
	}

	/// <summary>
	/// On supprime le dernier cube instancié
	/// </summary>
	public void DeleteLastCube()
	{
		if (_cubeInstances.Count > 0) //Si j'ai des cubes dans ma liste
		{
			if (_cubeInstances.Last() != null) //Si le dernier cube listé existe toujours
			{
				Destroy(_cubeInstances.Last().gameObject); //On le détruit
			}

			_cubeInstances.RemoveAt(_cubeInstances.Count - 1); //On supprime la référence vers le dernier cube de la liste
		}
	}

	public void SetAllCubesRed()
	{
		_isRaveOn = false;
		foreach (var cubeInstance in _cubeInstances) //Pour chaque cube de ma liste
		{
			SetCubeColor(cubeInstance, Color.red);
		}
	}

	public void SetAllCubesBlue()
	{
		_isRaveOn = false;
		foreach (var cubeInstance in _cubeInstances) //Pour chaque cube de ma liste
		{
			SetCubeColor(cubeInstance, Color.blue);
		}
	}

	private void SetCubeColor(Transform cube, Color color)
	{
		cube.GetComponent<MeshRenderer>().material.color = color;
	}

	public void ToggleRaveParty(bool enable)
	{
		_isRaveOn = enable;
	}

	private Color GetRandomColor()
	{
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
	}

	private void FixedUpdate()
	{
		if (_isRaveOn)
		{
			foreach (var cubeInstance in _cubeInstances)
			{
				SetCubeColor(cubeInstance, GetRandomColor());
			}
		}
	}
}
