using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Astrolabe.Scripts
{
	public class VoidObject
	{
		// reference sur un GameObject qui contient les elements non relié à l'application (par exemple GameObject de VideoPlayer ou element que l'on ne relie pas pour le moment)
		private GameObject _voidGameObject;

		// reference sur un IDisposable associé ua GameObject du void par exemple VisualMediaPlayerController
		private readonly Dictionary<IDisposable, GameObject> _dictionaryVoidDisposableObject = new();

		public VoidObject()
		{
		}

		public void Initialize()
		{
			// création du GaemObject void qui sera chargé de stocké les visualElements sans logicalElement associé ou que l'on ne veut pas stocké pour le moment. Ils sont disposé automatiquement
			_voidGameObject = new GameObject() { name = "Void" };
			// on le relie au GameObject de Astrolabe
			_voidGameObject.transform.SetParent(AstrolabeManager.Instance.transform);
		}

		public void AddToVoid(GameObject gameObject)
		{
			if (gameObject == null)
			{
				return;
			}

			gameObject.transform.SetParent(_voidGameObject.transform);
		}

		public bool AddToVoid(IDisposable disposableObject, GameObject gameObjectAssociated)
		{
			if (disposableObject == null)
			{
				return false;
			}

			if (_dictionaryVoidDisposableObject.ContainsKey(disposableObject))
			{
				return false;
			}

			AddToVoid(gameObjectAssociated);
			_dictionaryVoidDisposableObject.Add(disposableObject, gameObjectAssociated);

			return true;
		}

		/// <summary>
		/// Dispose les objets qui pourrait être oublié
		/// </summary>
		/// <param name="disposableObject"></param>
		/// <returns></returns>
		public bool DisposeFromVoid(IDisposable disposableObject)
		{
			if (disposableObject == null)
			{
				return false;
			}

			if (_dictionaryVoidDisposableObject.ContainsKey(disposableObject) == false)
			{
				return false;
			}

			var go = _dictionaryVoidDisposableObject[disposableObject];

			_dictionaryVoidDisposableObject.Remove(disposableObject);

			// normalement détruit le GameObject
			disposableObject.Dispose();
			// mais par sécurité on va l'enlever
			UnityEngine.Object.Destroy(go);

			return true;
		}

		/// <summary>
		/// Disposé automatique en mode design lorsque l'on change la chaine de l'application
		/// </summary>
		public void DisposeAllFromVoid()
		{
			var disposableObjects = _dictionaryVoidDisposableObject.Keys.ToList();

			foreach (var logicalObjet in disposableObjects)
			{
				DisposeFromVoid(logicalObjet);
			}
		}
	}
}