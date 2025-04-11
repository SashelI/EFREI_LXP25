using System.IO;
using System.Text;
using UnityEngine;

namespace Assets.Astrolabe.Libs.OBJImport.Samples
{
	public class ObjFromStream : MonoBehaviour
	{
		private void Start()
		{
			//make www
			var www = new WWW("https://people.sc.fsu.edu/~jburkardt/data/obj/lamp.obj");
			while (!www.isDone)
			{
				System.Threading.Thread.Sleep(1);
			}

			//create stream and load
			var textStream = new MemoryStream(Encoding.UTF8.GetBytes(www.text));
			var loadedObj = new OBJLoader().Load(textStream, global::Astrolabe.Twinkle.ShaderMode.Auto);
		}
	}
}