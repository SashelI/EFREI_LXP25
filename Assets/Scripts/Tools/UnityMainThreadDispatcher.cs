/*
Copyright 2015 Pim de Witte All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Tools
{
	/// Author: Pim de Witte (pimdewitte.com) and contributors, https://github.com/PimDeWitte/UnityMainThreadDispatcher
	/// <summary>
	/// A thread-safe class which holds a queue with actions to execute on the next Update() method. It can be used to make calls to the main thread for
	/// things such as UI Manipulation in Unity. It was developed for use in combination with the Firebase Unity plugin, which uses separate threads for event handling
	/// </summary>
	public class UnityMainThreadDispatcher : MonoBehaviour
	{
		private class QueueItem
        {
			public QueueItem()
            {
				ID = Guid.NewGuid();
				idString = ID.ToString();
			}

			public Action Action
            {
				get;
				set;
            }

			public Func<Task> Func
            {
				get;
				set;
            }

			public string Name
            {
				get
                {
					if(name == null)
                    {
						return idString;
                    }

					return name;
                }

				set
                {

					this.name = value;
                }
            }

			private string name;

			public Guid ID
			{
				get;
				private set;
			}

			private string idString;
        }

		private static readonly Queue<QueueItem> _executionQueue = new Queue<QueueItem>(50);

		private bool isExecutingQueue = false;

		public async void Update()
		{
			if(isExecutingQueue == true)
            {
				return;
            }

			isExecutingQueue = true;

			QueueItem queueItem = null;

			while (true)
            {
				lock (_executionQueue)
                {
					if(_executionQueue.Count <= 0)
                    {
						isExecutingQueue = false;
						return;
                    }
					else
                    {
						queueItem = _executionQueue.Dequeue();
                    }
                }

				if(queueItem.Func != null)
                {					
					await queueItem.Func.Invoke();
				}
				else
                {
					queueItem.Action.Invoke();
				}
			}
		}

		/// <summary>
		/// Execution d'une action dans le dispatcher. Le dispatcher attendra que l'action soit terminée pour dequeue la prochaine méthode a executer dans le Dispatcher
		/// </summary>
		/// <param name="action"></param>

		public void EnqueueAction(Action action, string name = null)
        {
			lock(_executionQueue)
            {
				_executionQueue.Enqueue(new QueueItem() { Action = action, Name = name });
            }
        }

		/// <summary>
		/// Execution d'une task dans le dispatcher. Le dispatcher attendra que la Task soit terminée pour dequeue la prochaine méthode a executer dans le Dispatcher
		/// </summary>
		/// <param name="func"></param>

		public void EnqueueTask(Func<Task> func, string name = null)
		{
			lock (_executionQueue)
			{
				_executionQueue.Enqueue(new QueueItem() { Func = func, Name = name });
			}
		}

		/// <summary>
		/// Execution d'une action dans le Dispatcher avec attente de fin d'execution
		/// ATTENTION : Pas de réentrance donc ne pas appeler un EnqueueActionAsync à l'intérieur d'un EnqueueTaskAsync/EnqueueActionAsync
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>

		public Task EnqueueActionAsync(Action action, string name = null)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			this.EnqueueAction(() =>
			{
				try
				{
					action.Invoke();
					tcs.TrySetResult(true);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			}, name);

			return tcs.Task;
		}

		/// <summary>
		/// Executer d'une Task dans le dispatcher avec attente de fin d'execution
		/// ATTENTION : Pas de réentrance donc ne pas appeler un EnqueueTaskAsync à l'intérieur d'un EnqueueTaskAsync/EnqueueActionAsync
		/// </summary>
		/// <param name="func"></param>
		/// <returns></returns>

		public Task EnqueueTaskAsync(Func<Task> func, string name = null)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			this.EnqueueTask(async () =>
			{
				try
				{
					await func.Invoke();
					tcs.TrySetResult(true);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			}, name);

			return tcs.Task;
		}

		private static UnityMainThreadDispatcher _instance = null;

		public static bool Exists()
		{
			return _instance != null;
		}

		public static UnityMainThreadDispatcher Instance()
		{
			if (!Exists())
			{
				throw new Exception("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
			}
			return _instance;
		}

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
		}

		private void OnDestroy()
		{
			_instance = null;
		}
	}
}
