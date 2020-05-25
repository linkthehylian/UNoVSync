using GamepadInput;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UNO.GamePlay;
using UNO.Networking;
using UNO.Save;
using UNO.TRC;

namespace UNO
{
	public class TitleScreenManager : MonoBehaviour
	{
		private GameObject icon;

		public GameObject m_loadingAnimationObj;

		public GameObject m_btnContainer;

		private bool m_ButtonReady;

		private bool m_IsUbiserviceCheckComplete;

		private bool m_IsMainMenuLoadingComplete;

		private GameObject AcceptTutorial;

		private GameObject DeclineTutorial;

		private GameObject Tutorial;

		private bool IsActiveTutorial;

		private string LogTag = "TitleScreenManager";

		private AsyncOperation ao;

		private bool m_isActiveMainUserPressed;

		private float loginOvertime = 3.40282347E+38f;

		public static TitleScreenManager Instance
		{
			get;
			private set;
		}

		public bool IsRdvLoginProcessOK
		{
			get;
			set;
		}

		public bool IsLoading()
		{
			return !((Object)m_loadingAnimationObj != (Object)null) || m_loadingAnimationObj.activeSelf;
		}

		private void Awake()
		{
			m_IsMainMenuLoadingComplete = false;
			m_IsUbiserviceCheckComplete = false;
			IsRdvLoginProcessOK = false;
		}

		private void Start()
		{
			SimpleSingleton<InputHandler>.Instance.EnterInputCategory(Category.INPUT_MENU, $"TitleScreenManager.Start,Main:{m_IsMainMenuLoadingComplete}, Uplay:{m_IsUbiserviceCheckComplete}");
			Instance = this;
			//SimpleSingleton<AudioManager>.Instance.playEvent("Play_Sfx_Title_Screen_Start");
			//SimpleSingleton<AudioManager>.Instance.playEventInRadioGroup("Play_Mus_Main_Menu_LP", "Stop_Mus_Main_Menu_LP");
			SimpleSingleton<EventManager>.Instance.Subscribe(new OnLoadActionFinishedEvent(), OnLoadActionFinishedHandler);
			SimpleSingleton<EventManager>.Instance.Subscribe(new OnEnterMainMenuEvent(), OnEnterMainMenuEventHandler);
			SimpleSingleton<EventManager>.Instance.Subscribe(new OnFirstCheckUbiserviceCompleteEvent(), OnFirstCheckUbiserviceCompleteHandler);
			Debug.Log("Begin Title screen here");
			ShowButton();
			SimpleSingleton<MultiplayerManager>.Instance.SetIsNetworkNeeded(false);
		}

		private void FixedUpdate()
		{
			float num = 80f;
			if (m_loadingAnimationObj.activeSelf)
			{
				Transform transform = m_loadingAnimationObj.transform;
				transform.eulerAngles += new Vector3(0f, 0f, Time.fixedDeltaTime * num);
			}
		}

		private void Update()
		{
			if (Matchmaker_Rdv.Instance.IsLoadbyInvitation && m_ButtonReady && !m_isActiveMainUserPressed)
			{
				SimpleSingleton<InputHandler>.Instance.FireEvent(InputEventID.START_MATCH, GamePad.Index.One);
			}
			if (Application.platform == RuntimePlatform.PS4 && Application.platform == RuntimePlatform.XboxOne)
			{
				return;
			}
			if (Input.anyKey && !m_isActiveMainUserPressed)
			{
				OnMainUserActive(GamePad.Index.One, string.Empty);
				SimpleSingleton<UserManagerAdapter>.Instance.ActivePressX(null);
			}
			PrepareEnterMainmenu();
		}

		private void OnMainUserActive(GamePad.Index _index, string _name)
		{
			Singleton<LogManager>.Instance.LogInfo("OnMainUserActive", _name + ", " + _index.ToString());
			RootScript.Attach();
			TRCManager.Instance.RegisterOnBackToTitleScreenEvent();
			GlobalPopupController.Instance.RegisterOnBackToTitleScreenEvent();
			MenuManager.startupMenuNode = "mainmenu";
			m_isActiveMainUserPressed = true;
			m_loadingAnimationObj.SetActive(true);
			m_btnContainer.SetActive(false);
			CursorController.Instance.UpdateCustomCursor(CursorController.CursorType.E_ArrowTurnCycle);
			OnPressButtonInTitleScreenEvent onPressButtonInTitleScreenEvent = new OnPressButtonInTitleScreenEvent();
			onPressButtonInTitleScreenEvent.GamePadIndex = (int)SimpleSingleton<UserManagerAdapter>.Instance.MainUserGamepadIndex();
			onPressButtonInTitleScreenEvent.Emit();
			loginOvertime = Time.realtimeSinceStartup + 20f;
			base.StartCoroutine(waitForLoadingCompleted());
		}

		private void FinishScreenAdjust()
		{
			SimpleSingleton<EventManager>.Instance.SendEvent(new OnCompleteTitleScreenEvent());
		}

		private void OnEnterMainMenuEventHandler(EventBase _data)
		{
			if (SimpleSingleton<SaveManagerGlobal>.Instance.IsFirstTimePlayUno)
			{
				ScreenAdjustManager.OnComfirmAjust -= FinishScreenAdjust;
			}
			if (ao != null)
			{
				ao.allowSceneActivation = true;
			}
		}

		private void ShowButton()
		{
			m_loadingAnimationObj.SetActive(false);
			m_ButtonReady = true;
		}

		private IEnumerator waitForLoadingCompleted()
		{
			yield return (object)new WaitForSeconds(0.1f);
			ao = SceneManager.LoadSceneAsync("MainMenu");
			ao.allowSceneActivation = false;
			ao.priority = 4;
			while (ao.progress < 0.9f)
			{
				yield return (object)new WaitForEndOfFrame();
			}
			m_IsMainMenuLoadingComplete = true;
			Singleton<LogManager>.Instance.LogInfo("TitleScreenManager", "Load Main menu Async Completed");
			PrepareEnterMainmenu();
		}

		private void OnFirstCheckUbiserviceCompleteHandler(EventBase _data)
		{
			Singleton<LogManager>.Instance.LogInfo("OnFirstCheckUbiserviceCompleteHandler", "First Login Ubiservice Completed");
			m_IsUbiserviceCheckComplete = true;
			PrepareEnterMainmenu();
		}

		private void OnLoadActionFinishedHandler(EventBase _data)
		{
			Singleton<LogManager>.Instance.LogInfo("TitleScreenManager", $"OnLoadActionFinishedHandler,Main:{m_IsMainMenuLoadingComplete}, Uplay:{m_IsUbiserviceCheckComplete}");
			if (SimpleSingleton<ProductManager>.Instance.IsTrialVersion)
			{
				ThemeCardsTeaserManager.Instance.InitHurryUpThemeCardAsTeaserForTrail();
			}
			m_IsUbiserviceCheckComplete = true;
			PrepareEnterMainmenu();
		}

		private void OnDestroy()
		{
			Instance = null;
			SimpleSingleton<EventManager>.Instance.Unsubscribe(new OnLoadActionFinishedEvent(), OnLoadActionFinishedHandler);
			SimpleSingleton<EventManager>.Instance.Unsubscribe(new OnEnterMainMenuEvent(), OnEnterMainMenuEventHandler);
			SimpleSingleton<EventManager>.Instance.Unsubscribe(new OnFirstCheckUbiserviceCompleteEvent(), OnFirstCheckUbiserviceCompleteHandler);
		}

		private void PrepareEnterMainmenu()
		{
			if (m_IsMainMenuLoadingComplete)
			{
				if (!IsRdvLoginProcessOK && !(Time.realtimeSinceStartup > loginOvertime))
				{
					return;
				}
				IsRdvLoginProcessOK = false;
				FinishScreenAdjust();
				if (!UplayPCNative.NativeMethods.IsUplayConnected() || UplayPCNative.NativeMethods.IsInOfflineMode())
				{
					UplayNotAvailableYetMessage message = new UplayNotAvailableYetMessage();
					TRCManager.Instance.AddMessage(message);
				}
				Matchmaker_Rdv.Instance.IsTitleScreenProcessComplete = true;
			}
		}
	}
}
