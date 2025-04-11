using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Astrolabe.Scripts
{
	public class TwinklePrefabFactory : MonoBehaviour
	{
		public static TwinklePrefabFactory Instance { get; set; }

		public void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			// Lancement de AstrolabeManager
			AstrolabeManager.InitializeApplication();
		}

		[FormerlySerializedAs("PrefabRectangle")] public Transform prefabRectangle;
		[FormerlySerializedAs("PrefabButtonBase")] public Transform prefabButtonBase;
		[FormerlySerializedAs("PrefabSwitchToggle")] public Transform prefabSwitchToggle;
		[FormerlySerializedAs("PrefabTextMeshPro")] public Transform prefabTextMeshPro;

		[FormerlySerializedAs("PrefabCube")] public Transform prefabCube;
		[FormerlySerializedAs("PrefabSphere")] public Transform prefabSphere;
		[FormerlySerializedAs("PrefabCylinder")] public Transform prefabCylinder;
		[FormerlySerializedAs("PrefabCapsule")] public Transform prefabCapsule;
		[FormerlySerializedAs("PrefabQuad")] public Transform prefabQuad;
		[FormerlySerializedAs("PrefabPlane")] public Transform prefabPlane;

		[FormerlySerializedAs("PrefabModel3D")] public Transform prefabModel3D;
		[FormerlySerializedAs("PrefabApplication")] public Transform prefabApplication;
		[FormerlySerializedAs("PrefabTextBox")] public Transform prefabTextBox;
		[FormerlySerializedAs("PrefabSimpleTextBox")] public Transform prefabSimpleTextBox;

		[FormerlySerializedAs("PrefabScrollList")] public Transform prefabScrollList;
	}
}