using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Tools.Helpers
{

	/// <summary>
	/// Classe statique de gestion des GameObject
	/// </summary>
	public static class GameObjectHelper
	{
		private static int _indexGlobal = 0;

		/// <summary>
		/// Essayer de récupérer le component d'un GO en assignant à la variable en paramètre seulement si pas déjà assignée
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gameObject"></param>
		/// <param name="variableToAssign"></param>
		/// <param name="instantiateIfNull"></param>
		public static void TryGetComponentAndAssign<T>(this GameObject gameObject, ref T variableToAssign,
			bool instantiateIfNull = false) where T : Component
		{
			if (gameObject == null)
			{
				return;
			}

			if (variableToAssign == null)
			{
				gameObject.TryGetComponent(out variableToAssign);
			}

			// On instancie le component sur l'objet si demandé
			if (variableToAssign == null && instantiateIfNull)
			{
				variableToAssign = gameObject.AddComponent<T>();
			}
		}

		/// <summary>
		/// Lancer une action liée à un component récupéré sur le GameObject
		/// Si le component est null, l'action n'est pas appelée
		/// Le component est assigné à la variable passée en paramètre
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gameObject"></param>
		/// <param name="variableToAssign"></param>
		/// <param name="actionWithComponent">Action à laquelle s'abonner pour utiliser le component</param>
		/// <param name="instantiateIfNull">Instancier le component sur l'objet si null ?</param>
		public static void WithComponent<T>(this GameObject gameObject, ref T variableToAssign,
			Action<T> actionWithComponent,
			bool instantiateIfNull = false) where T : Component
		{
			TryGetComponentAndAssign(gameObject, ref variableToAssign, instantiateIfNull);

			if (actionWithComponent != null && variableToAssign != null)
			{
				actionWithComponent.Invoke(variableToAssign);
			}
		}

		/// <summary>
		/// Lancer une action liée à un component récupéré sur le GameObject
		/// Si le component est null, l'action n'est pas appelée
		/// Le component est récupéré dynamiquement avant de lancer l'action sans assignation à une variable extérieure
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gameObject"></param>
		/// <param name="actionWithComponent"></param>
		/// <param name="instantiateIfNull"></param>
		public static void WithComponent<T>(this GameObject gameObject,
			Action<T> actionWithComponent,
			bool instantiateIfNull = false) where T : Component
		{
			T variableToAssign = null;

			TryGetComponentAndAssign(gameObject, ref variableToAssign, instantiateIfNull);

			if (actionWithComponent != null && variableToAssign != null)
			{
				actionWithComponent.Invoke(variableToAssign);
			}
		}

		/// <summary>
		/// Récupérer un component du même type chez le parent, puis chez lui même et enfin chez ses enfants
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gameObject"></param>
		/// <param name="variableToAssign"></param>
		/// <param name="includeInactive"></param>
		public static void GetComponentInFamily<T>(this GameObject gameObject, ref T variableToAssign, bool includeInactive) where T : Component
		{
			T component;
			if ((component = gameObject.GetComponentInParent<T>(includeInactive)) != null)
			{
				variableToAssign = component;
				return;
			}
			if ((component = gameObject.GetComponent<T>()) != null)
			{
				variableToAssign = component;
				return;
			}
			variableToAssign = gameObject.GetComponentInChildren<T>(includeInactive);
		}

		/// <summary>
		/// Obtenir un GameObject depuis une structure root en se basant sur un index représentant la totalité de la hiérarchie (pas seulement les enfants directs)
		/// </summary>
		/// <param name="root"></param>
		/// <param name="searchedIndexInHierarchy"></param>
		/// <param name="currentIndexInHierarchy"></param>
		/// <returns></returns>
		public static GameObjectHelperContainer GetGameObjectByIndexInTotalHierarchy(this GameObject root, int searchedIndexInHierarchy, int maxDepth, int currentIndexInHierarchy = 0, int currentDepth = 0)
		{
			if (currentIndexInHierarchy == 0)
			{
				_indexGlobal = 0;
			}
			if (root != null)
			{
				// Si on est au tout premier niveau (le "vrai" root), on va regarder si l'index 0 est recherché, on le renvoie alors lui
				if (currentDepth == 0 && searchedIndexInHierarchy == 0)
				{
					return new GameObjectHelperContainer(root, GameObjectHelperContainerTypeEnum.None, currentDepth);
				}

				// Sinon, on parcourt les enfants
				// Si on n'a pas encore atteint la profondeur max autorisée
				int nextDepth = currentDepth + 1;

				if (root.transform.childCount > 0 && nextDepth <= maxDepth)
				{
					for (int i = 0; i < root.transform.childCount; i++)
					{
						// L'index de l'enfant direct + l'index en cours global correspond à l'index recherché ?
						_indexGlobal++;

						if (_indexGlobal == searchedIndexInHierarchy)
						{
							return new GameObjectHelperContainer(root.transform.GetChild(i).gameObject, GameObjectHelperContainerTypeEnum.None, nextDepth);
						}

						// Sinon on va chercher "deeper"
						var nextObject = GetGameObjectByIndexInTotalHierarchy(
							root.transform.GetChild(i).gameObject, searchedIndexInHierarchy, maxDepth, _indexGlobal, nextDepth);

						if (nextObject != null)
						{
							return nextObject;
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Obtenir le prochain gameobject selon qu'il soit enfant ou frère du gameobject passé en paramètre
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="indexToReach"></param>
		/// <param name="previousObject"></param>
		/// <param name="currentIndex"></param>
		/// <returns>Object conteneur avec le GO cible sa profondeur et son type (parent ou enfant)</returns>
		public static GameObjectHelperContainerResult GetNextGameObject(this GameObject current, int currentDepth, bool bypassChildren = false,
			int minDepth = 0, int maxDepth = 2,
			params Type[] requiredComponents)
		{
			GameObject nextGameObject = null;
			GameObjectHelperContainerTypeEnum objType = GameObjectHelperContainerTypeEnum.None;

			if (current != null)
			{
				// On regarde si l'objet a des enfants
				// et qu'on n'a pas explicitement demandé à ne pas les récupérer
				// et qu'on n'a pas encore atteint la profondeur maximale autorisée

				bool foundNextChild = false;

				if (current.transform.childCount > 0 && !bypassChildren && currentDepth < maxDepth)
				{
					// On parcourt les enfants pour trouver le prochain qui respecte la condition
					// (Si composants listés, au moins un des composants trouvés, sinon, le 1er enfant tout court)
					var nextChild = GetNextChildGameObjectWithRequiredComponents(current, currentDepth + 1, maxDepth, requiredComponents);

					if (nextChild != null)
					{
						return nextChild;
					}
				}

				// Pas d'enfant trouvé
				if (!foundNextChild)
				{
					// On prend le prochain frère suivant s'il existe
					var nextSibling = GetNextSibling(current.transform, null, requiredComponents);

					if (nextSibling != null)
					{
						nextGameObject = nextSibling;
					}
					else
					{
						if (currentDepth - 1 < 0)
						{
							return new GameObjectHelperContainerResult(null, GameObjectHelperContainerResultTypeEnum.MinDepthReached);
						}

						// Pas de frère
						// On essaie de récupérer le prochain parent s'il existe
						// On se limite forcément au niveau minimal indiqué
						GameObjectHelperContainerResult nextParent = null;

						while (current.transform.parent != null && (currentDepth - 1) >= 0 && nextParent == null)
						{
							nextParent = current.transform.parent.gameObject.GetNextGameObject(currentDepth - 1, true, minDepth, maxDepth, requiredComponents);
						}

						return nextParent;
					}
				}
			}

			if (nextGameObject != null)
			{
				return new GameObjectHelperContainerResult(
					new GameObjectHelperContainer(nextGameObject,
					objType,
					objType == GameObjectHelperContainerTypeEnum.Child ? currentDepth + 1 : currentDepth), GameObjectHelperContainerResultTypeEnum.Ok);
			}

			return null;

		}

		/// <summary>
		/// Obtenir le prochain enfant du parent passé en paramètre qui respecte le fait des composants demandés en paramètre
		/// Si aucun composant passé en paramètre, on retourne le premier enfant directement
		/// </summary>
		/// <param name="currentParent"></param>
		/// <param name="currentDepth"></param>
		/// <param name="maxDepth"></param>
		/// <param name="requiredComponents"></param>
		/// <returns></returns>
		private static GameObjectHelperContainerResult GetNextChildGameObjectWithRequiredComponents(GameObject currentParent,
			int currentDepth,
			int maxDepth = 2,
			params Type[] requiredComponents)
		{
			if (currentDepth <= maxDepth)
			{
				// Parcours des enfants du parent
				for (int i = 0; i < currentParent.transform.childCount; i++)
				{
					var nextGameObject = currentParent.transform.GetChild(i).gameObject;

					if (requiredComponents != null && requiredComponents.Any())
					{
						bool foundAny = false;

						// Parcours des composants en paramètre
						foreach (var componentType in requiredComponents)
						{
							if (nextGameObject.TryGetComponent(componentType, out Component foundComponent))
							{
								foundAny = true;
								break;
							}
						}

						if (foundAny)
						{
							// On s'arrête, on a trouvé un des composants demandés
							return new GameObjectHelperContainerResult(new GameObjectHelperContainer(nextGameObject, GameObjectHelperContainerTypeEnum.Child, currentDepth),
								GameObjectHelperContainerResultTypeEnum.Ok);
						}
						else if (requiredComponents != null && requiredComponents.Any() && nextGameObject.transform.childCount > 0)
						{
							// On va essayer d'aller plus profondément pour chercher si cet enfant n'a pas un enfant avec un des composants demandés
							return GetNextChildGameObjectWithRequiredComponents(
								nextGameObject.transform.GetChild(i).gameObject, currentDepth + 1, maxDepth, requiredComponents);
						}
					}
					else
					{
						// Pas de composant, on retourne direct le GameObject
						return new GameObjectHelperContainerResult(new GameObjectHelperContainer(nextGameObject, GameObjectHelperContainerTypeEnum.Child, currentDepth), GameObjectHelperContainerResultTypeEnum.Ok);
					}
				}
			}

			return null;
		}


		/// <summary>
		/// Executer une action sur chaque composant demandé dans toute la hiérarchie d'un GO (limité par la maxdepth)
		/// </summary>
		/// <param name="currentItem"></param>
		/// <param name="currentDepth"></param>
		/// <param name="maxDepth"></param>
		/// <param name="actionToDo"></param>
		/// <param name="requiredComponents"></param>
		public static void DoActionWithRequiredComponentsUntilEnds(this GameObject currentItem, int currentDepth, int maxDepth,
			Action<Component> actionToDo,
			Type[] requiredComponents)
		{
			if (requiredComponents == null || !requiredComponents.Any())
			{
				return;
			}

			if (currentItem != null)
			{
				// Parcours des composants en paramètre
				foreach (var componentType in requiredComponents)
				{
					if (currentItem.TryGetComponent(componentType, out Component foundComponent))
					{
						if (actionToDo != null)
						{
							actionToDo(foundComponent);
						}
					}
				}

				var nextObject = currentItem.GetNextGameObject(currentDepth);

				if (nextObject != null
					&& nextObject.ResultType == GameObjectHelperContainerResultTypeEnum.Ok
					&& nextObject?.Container.Object != null)
				{
					DoActionWithRequiredComponentsUntilEnds(nextObject.Container.Object, currentDepth, maxDepth, actionToDo, requiredComponents);
				}
			}
		}


		/// <summary>
		/// Obtenir le précédent gameobject selon qu'il soit [frère, parent, enfant du parent] du gameobject passé en paramètre
		/// </summary>
		/// <param name="currentItem"></param>
		/// <param name="currentDepth"></param>
		/// <param name="maxDepth"></param>
		/// <param name="preferSiblingOverChild">Indique si l'on prefère récupérer un frère plutôt qu'un enfant en précédent (s'il existe)</param>
		/// <returns></returns>
		public static GameObjectHelperContainer GetPreviousGameObject(this GameObject currentItem,
			int currentDepth,
			int maxDepth,
			bool preferSiblingOverChild = true)
		{
			GameObject previousGameObject = null;
			GameObjectHelperContainerTypeEnum objType = GameObjectHelperContainerTypeEnum.None;
			int newDepth = currentDepth;

			if (currentItem != null)
			{
				// On va vers l'arrière, on regarde d'abord si on a pas un frère précédent l'item
				previousGameObject = GetPreviousSibling(currentItem.transform);

				if (previousGameObject == null)
				{
					// Sinon, on va récupérer le parent de l'item en cours
					if (currentItem.transform.parent != null)
					{
						previousGameObject = currentItem.transform.parent.gameObject;
						objType = GameObjectHelperContainerTypeEnum.Parent;
						newDepth = currentDepth - 1;
					}
				}

				// Si on veut récupérer l'enfant précédent, on regarde un niveau en dessous
				// en essayant de voir si le précédent frère/parent a des enfants
				// En contrôlant la profondeur autorisée
				if (!preferSiblingOverChild
					&& previousGameObject != null
					&& objType != GameObjectHelperContainerTypeEnum.Parent
					&& previousGameObject.transform.childCount > 0
					&& newDepth + 1 <= maxDepth)
				{
					// On essaie de récupérer le dernier enfant du frère récupéré
					var lastChildOfPreviousGameObject = previousGameObject.transform.GetChild(previousGameObject.transform.childCount - 1);

					if (lastChildOfPreviousGameObject != null)
					{
						previousGameObject = lastChildOfPreviousGameObject.gameObject;
						newDepth++;
					}
				}

			}

			return new GameObjectHelperContainer(
				previousGameObject,
				objType,
				newDepth);
		}

		/// <summary>
		/// On n'utilise pas le GetSiblingIndex car buggé en build (Renvoie 0. Mais fonctionne en éditeur)
		/// Retourne le prochain GameObject frère de celui passé en paramètre
		/// Possibilité d'avoir une condition sur les composants requis
		/// </summary>
		/// <param name="current"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static GameObject GetNextSibling(this Transform current, Transform parent = null, params Type[] requiredComponents)
		{
			if (current != null)
			{
				int currentIndex = current.GetBuildSaveSiblingIndex();

				int nextIndex = currentIndex + 1;

				var currentParent = parent ?? current.parent;

				if (currentParent != null)
				{
					try
					{
						for (int i = nextIndex; i < currentParent.transform.childCount; i++)
						{
							var nextSibling = currentParent.GetChild(nextIndex).gameObject;

							if (requiredComponents != null && requiredComponents.Any())
							{
								foreach (var componentType in requiredComponents)
								{
									if (nextSibling.TryGetComponent(componentType, out Component foundComponent))
									{
										return nextSibling;
									}
								}
							}
							else
							{
								return nextSibling;
							}
						}
					}
					catch (Exception)
					{
						return null;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Retourne le précédent GameObject frère de celui passé en paramètre
		/// </summary>
		/// <param name="current"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static GameObject GetPreviousSibling(this Transform current, Transform parent = null)
		{
			if (current != null)
			{
				int currentIndex = current.GetBuildSaveSiblingIndex();

				int previousIndex = currentIndex - 1;

				if (previousIndex >= 0)
				{
					var currentParent = parent ?? current.parent;

					if (currentParent != null)
					{
						try
						{
							return currentParent.GetChild(previousIndex).gameObject;
						}
						catch (Exception)
						{
							return null;
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// En mode build, le sibling index d'un objet root est toujours 0
		/// Cette méthode permet de savoir si un Transform est un objet root et, si c'est le cas, retourne son bon index grâce à IndexOf()
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static int GetBuildSaveSiblingIndex(this Transform transform)
		{
			// Pas un objet root, on retourne le SiblingIndex
			if (transform.parent != null)
			{
				return transform.GetSiblingIndex();
			}

			var rootGameObjectTransforms = SceneManager.GetActiveScene()
				.GetRootGameObjects()
				.Select(go => go.transform)
				.ToArray();

			return Array.IndexOf(rootGameObjectTransforms, transform);
		}

		/// <summary>
		/// Calculer le nombre de GameObjects présents sur toute sa hiérarchie
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="currentDepth"></param>
		/// <param name="maxDepth"></param>
		/// <param name="milestoneDepthWhenNotFoundComponent">Profondeur marquée à partir de laquelle on n'a pas trouvé un des composants requis</param>
		/// <param name="requiredComponents"></param>
		/// <returns></returns>
		public static int GetTotalGameObjectInHierarchy(this GameObject obj, int currentDepth, int maxDepth = 2,
			int milestoneDepthWhenNotFoundComponent = 0,
			params Type[] requiredComponents)
		{
			if (obj == null)
			{
				return 0;
			}

			int totalItems = 0;

			int nextDepth = currentDepth + 1;

			if (nextDepth <= maxDepth)
			{
				foreach (Transform child in obj.transform)
				{
					if (child == null)
					{
						continue;
					}

					bool foundAnyComponent = false;

					if (requiredComponents != null && requiredComponents.Any())
					{
						foreach (var componentType in requiredComponents)
						{
							if (child.TryGetComponent(componentType, out Component foundComponent))
							{
								foundAnyComponent = true;
								break;
							}
						}
					}
					else
					{
						foundAnyComponent = true;
					}

					if (!foundAnyComponent)
					{
						// On va essayer de voir si un enfant a les composants demandés
						var childItemsCount =
							child.gameObject.GetTotalGameObjectInHierarchy(nextDepth, maxDepth,
							milestoneDepthWhenNotFoundComponent > 0 ? milestoneDepthWhenNotFoundComponent : nextDepth,
							requiredComponents);

						if (childItemsCount > 0)
						{
							// On va calculer le nombre d'items en fonction de la depth initiale sur laquelle on n'a pas trouvé le composant
							totalItems += currentDepth - milestoneDepthWhenNotFoundComponent + 1;
						}
					}
					else
					{
						// L'enfant en cours
						totalItems += 1;

						// Les potentiels enfants de l'enfant en cours
						totalItems += child.gameObject.GetTotalGameObjectInHierarchy(nextDepth, maxDepth, 0, requiredComponents);
					}
				}
			}

			return totalItems;
		}
	}

	/// <summary>
	/// Type du GameObject renvoyé par le helper
	/// </summary>
	public enum GameObjectHelperContainerTypeEnum
	{
		None = 0,
		Child,
		Parent
	}

	/// <summary>
	/// Container dédié aux méthodes statiques du GameObjectHelper
	/// </summary>
	public class GameObjectHelperContainer
	{
		/// <summary>
		/// GameObject du container
		/// </summary>
		public GameObject Object { get; private set; }

		/// <summary>
		/// Type d'objet renvoyé par le helper (enfant ou parent ?)
		/// </summary>
		public GameObjectHelperContainerTypeEnum ObjectType { get; private set; }

		/// <summary>
		/// Profondeur dans la hiérarchie du GameObject
		/// </summary>
		public int Depth { get; private set; }

		/// <summary>
		/// Constructeur
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="objType"></param>
		/// <param name="depth"></param>
		public GameObjectHelperContainer(GameObject obj, GameObjectHelperContainerTypeEnum objType, int depth)
		{
			Object = obj;
			ObjectType = objType;
			Depth = depth;
		}
	}

	/// <summary>
	/// Type de réponse d'un GameObjectHelperContainer
	/// </summary>
	public enum GameObjectHelperContainerResultTypeEnum
	{
		Ok,
		/// <summary>
		/// Profondeur minimale atteinte (généralement, 0)
		/// </summary>
		MinDepthReached
	}

	/// <summary>
	/// Résultat de la récupération d'un GameObjectContainer
	/// </summary>
	public class GameObjectHelperContainerResult
	{
		/// <summary>
		/// Container
		/// </summary>
		public GameObjectHelperContainer Container { get; private set; }

		/// <summary>
		/// Type de résultat sur le container
		/// </summary>
		public GameObjectHelperContainerResultTypeEnum ResultType { get; private set; }

		/// <summary>
		/// Constructeur
		/// </summary>
		/// <param name="container"></param>
		/// <param name="resultType"></param>
		public GameObjectHelperContainerResult(GameObjectHelperContainer container, GameObjectHelperContainerResultTypeEnum resultType)
		{
			Container = container;
			ResultType = resultType;
		}
	}
}
