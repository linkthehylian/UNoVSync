using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UNO.GamePlay;
using UNO.Save;

namespace UNO
{
	public class RootScript : Singleton<RootScript>
	{
		private static bool runInBackground;

		private SingletonInstanceDict InstanceDictionary
		{
			get;
			set;
		}

		private SingletonInstanceDict PendingAttachedObject
		{
			get;
			set;
		}

		private List<string> PendingDettachedObject
		{
			get;
			set;
		}

		private bool IsStarted
		{
			get;
			set;
		}

		private bool IsUpdating
		{
			get;
			set;
		}

		public static bool Attached
		{
			get;
			private set;
		}

		public RootScript()
		{
			Attached = false;
			IsStarted = false;
			InstanceDictionary = new SingletonInstanceDict();
			PendingAttachedObject = new SingletonInstanceDict();
			PendingDettachedObject = new List<string>();
		}

		public static void RunInBackground(bool _value, string _desc)
		{
			runInBackground = _value;
			Debug.LogWarning("RunToBackground:" + _value + "," + _desc);
		}

		public static bool IsRunInBackground()
		{
			return runInBackground;
		}

		public static void Attach()
		{
			if (Attached)
			{
				ReInit();
			}
			else
			{
				Attached = true;
				SimpleSingleton<ProductManager>.Instance.DetectVersionOnActiveMainUser();
				VideoAndAudioBlockManager.Instance.InitBlocklistAsync();
				SimpleSingleton<EventManager>.Instance.Attach();
				SimpleSingleton<LobbyMenuGlobalConfig>.Instance.Attach();
				SimpleSingleton<SaveManagerGlobal>.Instance.Attach();
				SimpleSingleton<MatchSetupGlobal>.Instance.Attach();
				SimpleSingleton<HUDFrameDisplay>.Instance.Attach();
				SimpleSingleton<UbiservicesPortal>.Instance.Attach();
				SimpleSingleton<UbiservicePopupManager>.Instance.Attach();
				SimpleSingleton<DNATracking>.Instance.Attach();
				SimpleSingleton<RdvPortal>.Instance.Attach();
				SimpleSingleton<LeaderboardAdapter>.Instance.Attach();
				SimpleSingleton<TitleXPSystem>.Instance.Attach();
				SimpleSingleton<StatsManager>.Instance.Attach();
				SimpleSingleton<PlayerCardStats>.Instance.Attach();
				SimpleSingleton<LeaderboardAvatarCache>.Instance.Attach();
				SimpleSingleton<PresenceManager>.Instance.Attach();
				SimpleSingleton<AFKManager>.Instance.Attach();
			}
		}

		private static void ReInit()
		{
			SimpleSingleton<DNATracking>.Instance.Reset();
		}

		protected override void OnAwake()
		{
			base.gameObject.isStatic = false;
		}

		protected override void PreDestroy()
		{
			int count = InstanceDictionary.Count;
			for (int num = InstanceDictionary.Count - 1; num >= 0; num--)
			{
				KeyValuePair<string, RootObject> keyValuePair = ((List<KeyValuePair<string, RootObject>>)InstanceDictionary)[num];
				if (keyValuePair.Value != null)
				{
					keyValuePair.Value.Dispose();
					Debug.Log($"LEHDEBUG RootScript: InstanceDictionary count:{count} i:{num} class: {keyValuePair.Key} ");
				}
			}
			InstanceDictionary.Clear();
		}

		private void Start()
		{
			IsStarted = true;
			IsUpdating = true;
			StartAll();
			IsUpdating = false;
			PostRegisterProcess();
		}

		private void Update()
		{
			try
			{
				IsUpdating = true;
				UpdateAll();
				IsUpdating = false;
				PostRegisterProcess();
			}
			catch (Exception ex)
			{
				Singleton<LogManager>.Instance.LogInfo("RootScript", "Exception " + ex.Message + ", " + ex.StackTrace);
			}
		}

		private void FixedUpdate()
		{
			IsUpdating = true;
			FixedUpdateAll();
			IsUpdating = false;
			PostRegisterProcess();
		}

		private void OnGUI()
		{
            if (SplashScreen.onlyUplay)
            {
                GUILayout.BeginArea(GUIHelpers.AlignRect(250, 50, GUIHelpers.Alignment.CENTER, 0, 0), "", GUI.skin.box);
                GUILayout.Label("<size=20><b>Attaching Uplay service.</b></size>");
                GUILayout.EndArea();
            }
            if (SplashScreen.readyForMain)
            {
                GUILayout.BeginArea(GUIHelpers.AlignRect(250, 50, GUIHelpers.Alignment.CENTER, 0, 0), "", GUI.skin.box);
                GUILayout.Label("<size=20><b>Loading title screen.</b></size>");
                GUILayout.EndArea();
            }
            IsUpdating = true;
			foreach (KeyValuePair<string, RootObject> item in InstanceDictionary)
			{
				if (item.Value.Enabled)
				{
					item.Value.OnGUI();
				}
			}
			IsUpdating = false;
			PostRegisterProcess();
		}

		public RootObject Get<T>()
		{
			RootObject result = null;
			foreach (KeyValuePair<string, RootObject> item in (List<KeyValuePair<string, RootObject>>)InstanceDictionary)
			{
				if (item.Value is T)
				{
					return item.Value;
				}
			}
			return result;
		}

		public RootObject Get(string _typeName)
		{
			RootObject result = null;
			foreach (KeyValuePair<string, RootObject> item in InstanceDictionary)
			{
				if (item.Key == _typeName)
				{
					return item.Value;
				}
			}
			return result;
		}

		public void Register(string _typeName, RootObject _object)
		{
			if (!InstanceDictionary.ContainsKey(_typeName) && _object != null)
			{
				_object.Parent = base.gameObject;
				_object.Behaviour = this;
				if (IsStarted)
				{
					_object.Start();
				}
				if (!IsUpdating)
				{
					InstanceDictionary.Add(_typeName, _object);
				}
				else
				{
					PendingAttachedObject.Add(_typeName, _object);
				}
			}
		}

		public void UnRegister(string _typeName)
		{
			if (InstanceDictionary.ContainsKey(_typeName))
			{
				RootObject rootObject = Get(_typeName);
				rootObject?.Dispose();
				if (!IsUpdating)
				{
					InstanceDictionary.Remove(_typeName);
				}
				else
				{
					PendingDettachedObject.Add(_typeName);
				}
			}
		}

		public void UnRegister(RootObject _object)
		{
			foreach (KeyValuePair<string, RootObject> item in InstanceDictionary)
			{
				if (item.Value == _object)
				{
					if (!IsUpdating)
					{
						InstanceDictionary.Remove(item.Key);
					}
					else
					{
						PendingDettachedObject.Add(item.Key);
					}
					break;
				}
			}
		}

		private void StartAll()
		{
			foreach (KeyValuePair<string, RootObject> item in InstanceDictionary)
			{
				if (item.Value.Enabled)
				{
					item.Value.Start();
				}
			}
		}

		private void UpdateAll()
		{
			foreach (KeyValuePair<string, RootObject> item in InstanceDictionary)
			{
				if (item.Value.Enabled)
				{
					item.Value.Update();
				}
			}
		}

		private void FixedUpdateAll()
		{
			foreach (KeyValuePair<string, RootObject> item in InstanceDictionary)
			{
				if (item.Value.Enabled)
				{
					item.Value.FixedUpdate();
				}
			}
		}

		private void PostRegisterProcess()
		{
			foreach (KeyValuePair<string, RootObject> item in PendingAttachedObject)
			{
				InstanceDictionary.Add(item.Key, item.Value);
			}
			PendingAttachedObject.Clear();
			for (int i = 0; i < PendingDettachedObject.Count; i++)
			{
				UnRegister(PendingDettachedObject[i]);
			}
			PendingDettachedObject.Clear();
		}
	}
}
