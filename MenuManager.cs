using GamepadInput;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using SupremoLocalization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UNO.GamePlay;
using UNO.Save;

namespace UNO
{
	public class MenuManager : MonoBehaviour
	{
		public enum EOpenFrom
		{
			MainMenu,
			PauseMenu
		}

		[Serializable]
		public class UNOMenuItemEditable
		{
			public string childMenuID;

			public GameObject gameObj;

			public short Order;

			public string ItemText;

			public string ItemDescriptionText;

			public bool isDisabled;

			public bool openScene;
		}

		[Serializable]
		public class UNOMenuEditable
		{
			public string menuID;

			public UNOMenuItemEditable[] menuItems;

			public string menuDescription;

			public bool isCustomMenu;

			public bool isCustomAnimation;

			public bool topBarAvailable;

			public string BottonsContent;
		}

		public enum EUNOMenuDirection
		{
			DIR_UP,
			DIR_DOWN,
			DIR_LEFT,
			DIR_RIGHT
		}

		public delegate void menuHiddenCallBackDelegate();

		public delegate void beforeMenuHiddenCallBackDelegate();

		public delegate void PressMenuItem();

		public delegate void BackToPauseMenuDelegate();

		public static MenuManager _instance;

		public GameObject m_menuObj;

		public UNOMenuEditable[] editableMenuList;

		public static string startupMenuNode = "mainmenu";

		private bool hyperJumpBack;

		public Dictionary<string, UNOMenu> menuInstancesMap = new Dictionary<string, UNOMenu>();

		public UNOMenuItem selectedMenuItem;

		public UNOMenu currentMenu;

		public static Action OnShowMenuComplete;

		private UNOMenu mLastMenuBeforeBack;

		private GameObject _desktop;

		private GameObject cachedDLCMenuItemObj;

		private bool HyperJump;

		public bool editMode;

		private bool DefaultButtonContent = true;

		public int spawnLayer;

		public bool ignoreTimeScale;

		private string m_lastMainMenuBtnContent = "A:UI_MAIN_MENU_TIP_SELECT;B:UI_PC_MAIN_MENU_QUIT_GAME_BUTTON";

		private PressMenuItem OnPressMenuItem;

		public static MenuManager Instance => _instance;

		public static EOpenFrom OpenFrom
		{
			get;
			set;
		}

		private bool HyperJumpBack
		{
			get
			{
				bool result = hyperJumpBack;
				hyperJumpBack = false;
				return result;
			}
			set
			{
				hyperJumpBack = value;
			}
		}

		public GameObject desktop
		{
			get
			{
				if ((UnityEngine.Object)_desktop == (UnityEngine.Object)null)
				{
					_desktop = GameObject.Find("Desktop");
				}
				return _desktop;
			}
		}

		private Transform trans_menuItemStart
		{
			get
			{
				GameObject gameObject = GameObject.Find("PositionPlaceHolder");
				return gameObject.transform.Find("MenuItemStart");
			}
		}

		private Transform trans_menuItemInter
		{
			get
			{
				GameObject gameObject = GameObject.Find("PositionPlaceHolder");
				return gameObject.transform.Find("MenuItemInter");
			}
		}

		private Transform trans_menuItemCenter
		{
			get
			{
				GameObject gameObject = GameObject.Find("PositionPlaceHolder");
				return gameObject.transform.Find("MenuItemCenter");
			}
		}

		private Transform trans_menuItemEnd
		{
			get
			{
				GameObject gameObject = GameObject.Find("PositionPlaceHolder");
				return gameObject.transform.Find("MenuItemEnd");
			}
		}

		private Transform trans_medalMenuItemLeft1
		{
			get
			{
				GameObject gameObject = GameObject.Find("MedalSystemPositionPlaceHolder");
				return gameObject.transform.Find("MenuItemLeft1");
			}
		}

		private Transform trans_medalMenuItemLeft2
		{
			get
			{
				GameObject gameObject = GameObject.Find("MedalSystemPositionPlaceHolder");
				return gameObject.transform.Find("MenuItemLeft2");
			}
		}

		private Transform trans_medalMenuItemLeft3
		{
			get
			{
				GameObject gameObject = GameObject.Find("MedalSystemPositionPlaceHolder");
				return gameObject.transform.Find("MenuItemLeft3");
			}
		}

		private Transform trans_medalMenuItemHolder
		{
			get
			{
				GameObject gameObject = GameObject.Find("MedalSystemPositionPlaceHolder");
				return gameObject.transform.Find("MenuItemContainer");
			}
		}

		public event menuHiddenCallBackDelegate menuHiddenCallBack;

		public event beforeMenuHiddenCallBackDelegate beforeMenuHiddenByBackCallBack;

		public event beforeMenuHiddenCallBackDelegate beforeMenuHiddenByValidateCallBack;

		public event BackToPauseMenuDelegate OnBackToPauseMenu;

		public static void OnQuotaFinishedPopup()
		{
			string term = "UI_ONLINE_POPUP_GO_TO_ONLINE";
			string empty = string.Empty;
			empty = ((UplayPortalClient.sGlobalFlipConfig == null || UplayPortalClient.sGlobalFlipConfig.daysUntilReset > 3) ? Singleton<LocalizationManager>.Instance.GetTermTranslation("UI_ONLINE_POPUP_QUOTA_LIMIT_WEEKLY", false, null, string.Empty) : Singleton<LocalizationManager>.Instance.GetTermTranslation("UI_ONLINE_POPUP_QUOTA_LIMIT_DAILY", false, null, string.Empty));
			empty = string.Format(empty, SerializeManager.Instance.MSaveData.MaxPlayableFlipGames);
			empty += "\n";
			empty += Singleton<LocalizationManager>.Instance.GetTermTranslation(term, false, null, string.Empty);
			GlobalPopupController.Instance.ShowNoticePopup("ERRORMSG_ONLINE_ALL_UBI_TITLE", empty, "UI_MAIN_MENU_POPUP_KICK_PLAYER_BUTTON_OK", null);
		}

		public static void OnFirstFlipJoined()
		{
			if (!SerializeManager.Instance.MSaveData.FirstFreeFlipMatchPlayed)
			{
				SerializeManager.Instance.MSaveData.FirstFreeFlipMatchPlayed = true;
				SimpleSingleton<SaveManagerGlobal>.Instance.SaveGameSettings(true);
				GameObject prefab = Singleton<PreloadSystem>.Instance.GetPrefab("Prefabs/FLIP_Popup");
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
				gameObject.SetActive(true);
				string empty = string.Empty;
				empty = ((UplayPortalClient.sGlobalFlipConfig == null || UplayPortalClient.sGlobalFlipConfig.daysUntilReset > 3) ? Singleton<LocalizationManager>.Instance.GetTermTranslation("UI_ONLINE_POPUP_FLIP_JOIN_WEEKLY", false, null, string.Empty) : Singleton<LocalizationManager>.Instance.GetTermTranslation("UI_ONLINE_POPUP_FLIP_JOIN_DAILY", false, null, string.Empty));
				empty = string.Format(empty, SerializeManager.Instance.MSaveData.MaxPlayableFlipGames);
				GlobalPopupBase component = gameObject.GetComponent<GlobalPopupBase>();
				component.SetTitle(Singleton<LocalizationManager>.Instance.GetTermTranslation("UI_TITLE_CAT01_TITLE06_NAME", false, null, string.Empty));
				component.SetConfirmButtonLabel(Singleton<LocalizationManager>.Instance.GetTermTranslation("UI_OPTION_OK", false, null, string.Empty));
				component.SetContent(empty);
				component.onConfirm += delegate
				{
					GlobalPopupController.Instance.CloseCustomPopup();
				};
				GlobalPopupController.Instance.ShowCustomizePopup(gameObject, null, "FLIP_FIRST_JOINED");
			}
		}

		public void ToggleHyperJump(bool _flag)
		{
			HyperJump = _flag;
		}

		public void ToggleDefaultButtonContent(bool _flag)
		{
			DefaultButtonContent = _flag;
		}

		private void Awake()
		{
			CursorController.Instance.UpdateCustomCursor(CursorController.CursorType.E_EndArrowTurnCycle);
			_instance = this;
			OpenFrom = EOpenFrom.MainMenu;
			Singleton<PreloadSystem>.Instance.RegisterWithFullUrl("Theme/Common/GFX/Card_Shadow_Menu", null);
			Singleton<PreloadSystem>.Instance.RegisterWithFullUrl("Menu/3D_Assets/MedalSystem/TitleItem", null);
			Singleton<PreloadSystem>.Instance.RegisterWithFullUrl("Menu/3D_Assets/MedalSystem/MissionItem", null);
		}

		private void Start()
		{
            if (!GameObject.Find("MichiManager"))
                new GameObject("MichiManager").AddComponent<CardSkinManager>();
            onStart();
			HyperJumpBack = (UNOMenu.lastSelMenuId == startupMenuNode);
			injectCommonEditableMenuLists();
			ChangeOnlineMenu();
			m_menuObj = GameObject.Find("SpawnManager");
			if ((UnityEngine.Object)m_menuObj != (UnityEngine.Object)null)
			{
				spawnLayer = m_menuObj.layer;
			}
			SimpleSingleton<AudioManager>.Instance.registerSoundBank();
			buildMenuTree();
			SimpleSingleton<InputHandler>.Instance.EnterInputCategory(Category.INPUT_MENU, "MenuManager");
			UNOMenu uNOMenu = menuInstancesMap[startupMenuNode];
			if (OpenFrom == EOpenFrom.PauseMenu)
			{
				int num = 0;
				while (num < uNOMenu.menuItems.Count)
				{
					if (!(uNOMenu.menuItems[num].relatedMenu.MenuID == "helpoption_credits"))
					{
						num++;
						continue;
					}
					uNOMenu.menuItems.RemoveAt(num);
					break;
				}
			}
			if (uNOMenu != null && !editMode)
			{
				base.StartCoroutine(DelayToShowMenu(uNOMenu));
			}
			if ((UnityEngine.Object)GameObject.Find("TopBar") != (UnityEngine.Object)null)
			{
				GameObject.Find("TopBar").gameObject.SetActive(false);
			}
		}

		public bool StartByPauseMenu()
		{
			base.gameObject.SetActive(true);
			OpenFrom = EOpenFrom.PauseMenu;
			UNOMenu menu = menuInstancesMap[startupMenuNode];
			base.StartCoroutine(DelayToShowMenu(menu));
			return true;
		}

		private void onStart()
		{
			if (SimpleSingleton<MultiplayerManager>.Instance != null)
			{
				SimpleSingleton<MultiplayerManager>.Instance.OnInvitationAccept += acceptInviteHandler;
			}
		}

		private IEnumerator DelayToShowMenu(UNOMenu _menu)
		{
			if (!ignoreTimeScale)
			{
				yield return (object)new WaitForSeconds(1f);
			}
			else
			{
				base.StartCoroutine(WaitForRealSeconds(1f));
			}
			SimpleSingleton<EventManager>.Instance.SendEvent(new MenuReadyEvent());
			base.StartCoroutine(showMenu(_menu));
			Singleton<PreloadSystem>.Instance.RegisterWithFullUrl("Prefabs/PlayerCard", null);
			Singleton<PreloadSystem>.Instance.RegisterWithFullUrl("Prefabs/NewsPopup", null);
		}

		private void OnDestroy()
		{
			SimpleSingleton<EventManager>.Instance.Unsubscribe(new OnSystemInternetConnectionChangeEvent(), uplinkErrorHandler);
			SimpleSingleton<MultiplayerManager>.Instance.OnInvitationAccept -= acceptInviteHandler;
			EnableInputListener(false);
			_instance = null;
		}

		private void acceptInviteHandler()
		{
			SimpleSingleton<InputHandler>.Instance.EnterInputCategory(Category.INPUT_MENU, "MenuManager->AcceptInvitation");
			TweenBlur.Off(Camera.main);
			SimpleSingleton<AudioManager>.Instance.StopAllSounds();
		}

		public void injectCommonEditableMenuLists()
		{
			MenuItemMeta menuItemMeta = Resources.Load<MenuItemMeta>("Meta/Menu/MenuItemMeta");
			if ((UnityEngine.Object)menuItemMeta != (UnityEngine.Object)null && menuItemMeta.data.Length > 0)
			{
				int num = editableMenuList.Length;
				int num2 = menuItemMeta.data.Length;
				int num3 = num + num2;
				UNOMenuEditable[] array = new UNOMenuEditable[num3];
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						array[i] = editableMenuList[i];
					}
				}
				for (int j = num; j < num3; j++)
				{
					array[j] = menuItemMeta.data[j - num];
				}
				editableMenuList = array;
			}
		}

		public void ChangeOnlineMenu()
		{
			if (!SimpleSingleton<ProductManager>.Instance.IsAddtionalThemeLicensed(Theme_Enum.Flip))
			{
				UNOMenuEditable[] array = editableMenuList;
				foreach (UNOMenuEditable uNOMenuEditable in array)
				{
					if (uNOMenuEditable.menuID == "classicaction_multi" || uNOMenuEditable.menuID == "2v2_classicaction_multi")
					{
						uNOMenuEditable.menuItems = new UNOMenuItemEditable[2]
						{
							uNOMenuEditable.menuItems[0],
							uNOMenuEditable.menuItems[1]
						};
					}
				}
			}
		}

		protected void uplinkErrorHandler(EventBase _evt)
		{
			SimpleSingleton<EventManager>.Instance.Unsubscribe(new OnSystemInternetConnectionChangeEvent(), uplinkErrorHandler);
			SimpleSingleton<MultiplayerManager>.Instance.OnUplinkDisconnect();
		}

		public void SubscribeUplinkDisconnect()
		{
			SimpleSingleton<EventManager>.Instance.Unsubscribe(new OnSystemInternetConnectionChangeEvent(), uplinkErrorHandler);
			SimpleSingleton<EventManager>.Instance.Subscribe(new OnSystemInternetConnectionChangeEvent(), uplinkErrorHandler);
		}

		private void onMenuShows()
		{
			GameObject gameObject = GameObject.Find("Desktop/MenuDesManagerObj");
			if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
			{
				MeneDescScriptionManager componentInChildren = gameObject.GetComponentInChildren<MeneDescScriptionManager>();
				componentInChildren.showMenuDesLv1(currentMenu.menuDescription, 0.5f);
			}
			if (currentMenu.MenuID == "mainmenu")
			{
				bool isAddonContentAvailable = SimpleSingleton<ProductManager>.Instance.IsAddonContentAvailable;
				List<UNOMenuItem> menuItems = currentMenu.menuItems;
				if (!isAddonContentAvailable)
				{
					GameObject prefab = Resources.Load("Menu/3D_Assets/Menu_Item/MI_Main_DLC_comingSoon") as GameObject;
					cachedDLCMenuItemObj = menuItems[menuItems.Count - 1].prefab;
					menuItems[menuItems.Count - 1].prefab = prefab;
				}
				else if ((UnityEngine.Object)cachedDLCMenuItemObj != (UnityEngine.Object)null)
				{
					menuItems[menuItems.Count - 1].prefab = cachedDLCMenuItemObj;
				}
			}
			if (OpenFrom == EOpenFrom.MainMenu)
			{
				if (currentMenu.MenuID == "mainmenu")
				{
					SimpleSingleton<MultiplayerManager>.Instance.SetIsNetworkNeeded(false);
					SimpleSingleton<EventManager>.Instance.Unsubscribe(new OnSystemInternetConnectionChangeEvent(), uplinkErrorHandler);
					SimpleSingleton<AudioManager>.Instance.playEventInRadioGroup("Play_Mus_Main_Menu_LP", "Stop_Mus_Main_Menu_LP");
				}
				if (currentMenu.MenuID == "multiplayer")
				{
					SimpleSingleton<EventManager>.Instance.Subscribe(new OnSystemInternetConnectionChangeEvent(), uplinkErrorHandler);
					SimpleSingleton<AudioManager>.Instance.playEventInRadioGroup("Play_Mus_Multiplayer_Lobby_LP", "Stop_Mus_Multiplayer_Lobby_LP");
				}
			}
		}

		public void onMenuShown()
		{
			SimpleSingleton<UbiservicesPortal>.Instance.CheckUplayPrivillage();
			if (Instance.currentMenu.topBarAvailable)
			{
				if ((UnityEngine.Object)TopBarManager.Instance != (UnityEngine.Object)null)
				{
					TopBarManager.Instance.ShowTopBar();
				}
			}
			else if ((UnityEngine.Object)TopBarManager.Instance != (UnityEngine.Object)null)
			{
				TopBarManager.Instance.RemoveTopBar();
			}
			if (SimpleSingleton<ProductManager>.Instance.IsTrialVersion && OpenFrom == EOpenFrom.MainMenu)
			{
				SimpleSingleton<ProductManager>.Instance.InvalidateProduct();
			}
			GameObject gameObject = GameObject.Find("Desktop/BtnPanel");
			if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null && DefaultButtonContent)
			{
				gameObject.GetComponent<ButtonsManager_v2>().ShowButtonsByMenuConfig(Instance.currentMenu.BottonsContent, ButtonsManager_v2.Pivot.MIDDLE, false, false);
			}
			else if (OpenFrom == EOpenFrom.PauseMenu)
			{
				onMenuItemSelected(selectedMenuItem);
				if (currentMenu.MenuID == "helpoption" || currentMenu.MenuID == "helpoption_tutorial")
				{
					PauseMenuController.Instance.ButtonPanel.GetComponent<ButtonsManager_v2>().ShowButtonsByMenuConfig("A:UI_MAIN_MENU_TIP_SELECT;B:UI_MAIN_MENU_TIP_BACK", ButtonsManager_v2.Pivot.MIDDLE, false, false);
				}
			}
			if (!currentMenu.isCustomMenu)
			{
				int count = currentMenu.menuItems.Count;
				for (int i = 0; i < count; i++)
				{
					if (!((UnityEngine.Object)currentMenu.menuItems[i].instance == (UnityEngine.Object)null))
					{
						Transform transform = currentMenu.menuItems[i].instance.transform;
						GameObject prefab = Singleton<PreloadSystem>.Instance.GetPrefab("Theme/Common/GFX/Card_Shadow_Menu");
						GameObject gameObject2 = UnityEngine.Object.Instantiate(prefab);
						Util.SetLayerRecursively(gameObject2, spawnLayer);
						gameObject2.name = "Card_Shadow";
						Transform transform2 = transform.Find("BoneRoot");
						if ((UnityEngine.Object)transform2 != (UnityEngine.Object)null)
						{
							gameObject2.transform.SetParent(transform2, false);
						}
						TweenAlpha.Begin(gameObject2, 0f, 0f);
						TweenAlpha.Begin(gameObject2, 0.5f, 1f);
					}
				}
			}
			foreach (UNOMenuItem menuItem in currentMenu.menuItems)
			{
				object[] value = new object[3]
				{
					currentMenu.MenuID,
					null,
					null
				};
				menuItem.instance.SendMessage("onMenuShown", value, SendMessageOptions.DontRequireReceiver);
			}
			if (currentMenu.menuItems.Count > 0)
			{
				if (HyperJumpBack)
				{
					currentMenu.selectedIdx = UNOMenu.lastSelIdx;
					if (currentMenu.selectedIdx >= currentMenu.menuItems.Count)
					{
						currentMenu.selectedIdx = 0;
					}
				}
				selectedMenuItem = currentMenu.menuItems[currentMenu.selectedIdx];
				if (!currentMenu.isCustomMenu)
				{
					onMenuItemSelected(selectedMenuItem);
					EnableInputListener(true);
				}
			}
			if (!currentMenu.isCustomMenu)
			{
				addMouseTriggerForCurMenus();
			}
			if (OnShowMenuComplete != null)
			{
				OnShowMenuComplete();
				OnShowMenuComplete = null;
			}
			if (IsQuitPlayTogether())
			{
				Debug.Log("Quit Play Together");
			}
		}

		private bool IsQuitPlayTogether()
		{
			bool result = false;
			if (currentMenu != null && currentMenu.MenuID == "mainmenu" && IsBackToMainmenu())
			{
				result = true;
			}
			return result;
		}

		private bool IsBackToMainmenu()
		{
			bool result = false;
			foreach (UNOMenuItem menuItem in currentMenu.menuItems)
			{
				if (mLastMenuBeforeBack != null && menuItem.relatedMenu != null && mLastMenuBeforeBack.MenuID == menuItem.relatedMenu.MenuID)
				{
					return true;
				}
			}
			return result;
		}

		public void SubscribePressMenuItemEvent(PressMenuItem _callback, string _desc)
		{
			Singleton<LogManager>.Instance.LogInfo("MenuManager", "SubscribePressMenuItemEvent:" + _desc);
			OnPressMenuItem = (PressMenuItem)Delegate.Combine(OnPressMenuItem, _callback);
		}

		public void UnsubscribePressMenuItemEvent(PressMenuItem _callback, string _desc)
		{
			Singleton<LogManager>.Instance.LogInfo("MenuManager", "UnsubscribePressMenuItemEvent:" + _desc);
			OnPressMenuItem = (PressMenuItem)Delegate.Remove(OnPressMenuItem, _callback);
		}

		private void onMenuHides()
		{
			if (OnPressMenuItem != null)
			{
				OnPressMenuItem();
			}
			if ((UnityEngine.Object)TopBarManager.Instance != (UnityEngine.Object)null)
			{
				TopBarManager.Instance.RemoveTopBar();
			}
			if (!currentMenu.isCustomMenu)
			{
				EnableInputListener(false);
				removeMouseTriggerForCurMenus();
				onMenuItemDeselected(selectedMenuItem);
			}
			GameObject gameObject = GameObject.Find("Desktop/BtnPanel");
			if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null && DefaultButtonContent)
			{
				gameObject.GetComponent<ButtonsManager_v2>().destroyButtons();
			}
			if (!currentMenu.isCustomMenu)
			{
				int count = currentMenu.menuItems.Count;
				for (int i = 0; i < count; i++)
				{
					Transform transform = currentMenu.menuItems[i].instance.transform;
					ParticleSystem[] componentsInChildren = ((Component)transform).GetComponentsInChildren<ParticleSystem>();
					ParticleSystem[] array = componentsInChildren;
					foreach (ParticleSystem particleSystem in array)
					{
						UnityEngine.Object.Destroy(particleSystem.gameObject);
					}
					Transform x = transform.Find("BoneRoot/Card_Shadow");
					if ((UnityEngine.Object)x != (UnityEngine.Object)null)
					{
						UnityEngine.Object.Destroy(transform.Find("BoneRoot/Card_Shadow").gameObject);
					}
				}
			}
			foreach (UNOMenuItem menuItem in currentMenu.menuItems)
			{
				object[] value = new object[3]
				{
					currentMenu.MenuID,
					null,
					null
				};
				menuItem.instance.SendMessage("onMenuHides", value, SendMessageOptions.DontRequireReceiver);
			}
		}

		public void onMenuHidden(TweenEvent _data = null)
		{
			m_menuObj.transform.rotation = Quaternion.identity;
			if (_data != null && !(bool)_data.parms[0] && OpenFrom == EOpenFrom.PauseMenu && currentMenu.MenuID == "helpoption")
			{
				destroyMenu(currentMenu);
				BackToPauseMenu();
			}
			else
			{
				UNOMenu menu = currentMenu;
				foreach (UNOMenuItem menuItem in currentMenu.menuItems)
				{
					object[] value = new object[3]
					{
						currentMenu.MenuID,
						null,
						null
					};
					menuItem.instance.SendMessage("onMenuHidden", value, SendMessageOptions.DontRequireReceiver);
				}
				if (this.menuHiddenCallBack != null)
				{
					this.menuHiddenCallBack();
					this.menuHiddenCallBack = null;
				}
				destroyMenu(menu);
			}
		}

		private void onMedalMenuHidden()
		{
			if (this.menuHiddenCallBack != null)
			{
				this.menuHiddenCallBack();
				this.menuHiddenCallBack = null;
			}
		}

		public void destroyMedalMenu(bool _isAnimated = false)
		{
			if (_isAnimated)
			{
				if ((UnityEngine.Object)trans_medalMenuItemHolder != (UnityEngine.Object)null)
				{
					((Component)trans_medalMenuItemHolder).GetComponent<TweenAlpha>().tweenFactor = 0f;
					EventDelegate item = new EventDelegate(destroyMedalMenuImmediate);
					if (!((Component)trans_medalMenuItemHolder).GetComponent<TweenAlpha>().onFinished.Contains(item))
					{
						((Component)trans_medalMenuItemHolder).GetComponent<TweenAlpha>().onFinished.Add(item);
					}
					((Component)trans_medalMenuItemHolder).GetComponent<TweenPosition>().tweenFactor = 0f;
					((Component)trans_medalMenuItemHolder).GetComponent<TweenPosition>().Play(true);
					((Component)trans_medalMenuItemHolder).GetComponent<TweenAlpha>().Play(true);
				}
			}
			else
			{
				destroyMedalMenuImmediate();
			}
		}

		private void destroyMedalMenuImmediate()
		{
			if ((UnityEngine.Object)trans_medalMenuItemHolder != (UnityEngine.Object)null)
			{
				((Component)trans_medalMenuItemHolder).GetComponent<TweenAlpha>().tweenFactor = 0f;
				((Component)trans_medalMenuItemHolder).GetComponent<TweenAlpha>().Play(false);
				trans_medalMenuItemHolder.localPosition = new Vector3(0f, 0f, 0f);
				while (trans_medalMenuItemHolder.childCount > 0)
				{
					UnityEngine.Object.DestroyImmediate(trans_medalMenuItemHolder.GetChild(0).gameObject);
				}
			}
		}

		private void AddShadowToMedalMenu(UNOMenuItem _menuItem)
		{
			Transform transform = _menuItem.instance.transform;
			GameObject prefab = Singleton<PreloadSystem>.Instance.GetPrefab("Theme/Common/GFX/Card_Shadow_Menu");
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			Util.SetLayerRecursively(gameObject, spawnLayer);
			gameObject.name = "Card_Shadow";
			Transform transform2 = transform.Find("BoneRoot");
			if ((UnityEngine.Object)transform2 != (UnityEngine.Object)null)
			{
				gameObject.transform.SetParent(transform2, false);
			}
			TweenAlpha.Begin(gameObject, 0f, 0f);
			TweenAlpha.Begin(gameObject, 0.5f, 1f);
		}

		private void onMouseSelected(UNOMenuItem _item)
		{
			if (_item != selectedMenuItem && selectedMenuItem.isCurrentSelected)
			{
				onMenuItemDeselected(selectedMenuItem);
			}
			selectedMenuItem = _item;
			onMenuItemSelected(selectedMenuItem);
			SimpleSingleton<AudioManager>.Instance.playEvent("Play_UI_Menu_Action_Card_Select");
		}

		private void onMouseDeselected(UNOMenuItem _item)
		{
			if (_item.itemName == selectedMenuItem.itemName && selectedMenuItem.isCurrentSelected)
			{
				ToggleOnFxOfSelectedItem(false);
				int count = currentMenu.menuItems.Count;
				List<TransformMetaData> list = MenuManagerUtil.generateMenuItemOriginalInfo(count);
				for (int i = 0; i < count; i++)
				{
					Tweener tweener = HOTween.To(currentMenu.menuItems[i].instance.transform, 0.5f, new TweenParms().Prop("localPosition", list[i].localPosition).UpdateType(UpdateType.TimeScaleIndependentUpdate).Prop("localRotation", Quaternion.Euler(list[i].localEulerAngles))
						.Prop("localScale", list[i].localScale)
						.Ease(EaseType.EaseOutQuad));
				}
			}
		}

		private void onMouseClicked(UNOMenuItem _item)
		{
			base.StartCoroutine(onValidate(currentMenu, _item));
		}

		private void onMenuItemSelected(UNOMenuItem _item)
		{
			_item.isCurrentSelected = true;
			object[] value = new object[3]
			{
				currentMenu.MenuID,
				_item.itemName,
				_item.prefab
			};
			int count = currentMenu.menuItems.Count;
			int num = currentMenu.menuItems.IndexOf(selectedMenuItem);
			currentMenu.selectedIdx = num;
			float num2 = (float)(count - 1) / 2f;
			float num3 = ((float)num - num2) * 5f;
			if (num3 < 0f)
			{
				num3 *= 2f;
			}
			Quaternion quaternion = Quaternion.Euler(0f, num3, 0f);
			GameObject menuObj = m_menuObj;
			if ((UnityEngine.Object)menuObj != (UnityEngine.Object)null)
			{
				HOTween.To(menuObj.transform, 0.5f, new TweenParms().Prop("localRotation", quaternion).UpdateType(UpdateType.TimeScaleIndependentUpdate).Ease(EaseType.EaseOutQuad));
			}
			List<TransformMetaData> list = MenuManagerUtil.generateMenuItemInfo(count, num);
			for (int i = 0; i < count; i++)
			{
				HOTween.To(currentMenu.menuItems[i].instance.transform, 0.5f, new TweenParms().Prop("localPosition", list[i].localPosition).Prop("localRotation", Quaternion.Euler(list[i].localEulerAngles)).Prop("localScale", list[i].localScale)
					.UpdateType(UpdateType.TimeScaleIndependentUpdate)
					.Ease(EaseType.EaseOutQuad));
			}
			ToggleOnFxOfSelectedItem(true);
			ShowMenuItemDescription(_item);
			if ((UnityEngine.Object)MenuBackgroundManager.Instance != (UnityEngine.Object)null)
			{
				MenuBackgroundManager.Instance.onSelected(currentMenu.selectedIdx);
			}
			_item.instance.SendMessage("OnItemSelected", value, SendMessageOptions.DontRequireReceiver);
		}

		private void onMenuItemDeselected(UNOMenuItem _item)
		{
			if (_item != null)
			{
				_item.isCurrentSelected = false;
				object[] value = new object[3]
				{
					currentMenu.MenuID,
					_item.itemName,
					_item.prefab
				};
				ToggleOnFxOfSelectedItem(false);
				_item.instance.SendMessage("OnItemDeselected", value, SendMessageOptions.DontRequireReceiver);
			}
		}

		public IEnumerator showMenu(UNOMenu _menu)
		{
			if (currentMenu != null)
			{
				TrackMenuClickEvent menuClickEvent = new TrackMenuClickEvent
				{
					Attribute = 
					{
						Page = currentMenu.MenuID,
						Target = _menu.MenuID
					}
				};
				SimpleSingleton<EventManager>.Instance.SendEvent(menuClickEvent);
			}
			bool bEmptyInstanceMenu = true;
			foreach (UNOMenuItem menuItem in _menu.menuItems)
			{
				if ((UnityEngine.Object)menuItem.instance != (UnityEngine.Object)null)
				{
					bEmptyInstanceMenu = false;
				}
			}
			if (bEmptyInstanceMenu)
			{
				currentMenu = _menu;
				onMenuShows();
				if (_menu.isCustomMenu)
				{
					if (_menu.menuItems.Count == 1)
					{
						GameObject spawnedItemObject2 = UnityEngine.Object.Instantiate(_menu.menuItems[0].prefab);
						spawnedItemObject2.transform.SetParent(m_menuObj.transform);
						_menu.menuItems[0].instance = spawnedItemObject2;
						base.StartCoroutine(animationOnOpen(_menu));
					}
					yield return (object)null;
				}
				else
				{
					SimpleSingleton<AudioManager>.Instance.playEvent("Play_UI_Menu_Action_Card_Out");
					int menuItemCount = _menu.menuItems.Count;
					float singleAnimTime = 0.5f / (float)menuItemCount;
					List<TransformMetaData> itemsList = MenuManagerUtil.generateMenuItemOriginalInfo(menuItemCount);
					for (int i = 0; i < menuItemCount; i++)
					{
						GameObject spawnedItemObject = UnityEngine.Object.Instantiate(_menu.menuItems[i].prefab);
						Transform spTrans = spawnedItemObject.transform;
						spTrans.position = trans_menuItemStart.position;
						spTrans.localScale = trans_menuItemStart.localScale;
						spTrans.SetParent(m_menuObj.transform);
						Util.SetLayerRecursively(spawnedItemObject, spawnLayer);
						_menu.menuItems[i].instance = spawnedItemObject;
						bool isLastItem = i == menuItemCount - 1;
						Sequence sequence = new Sequence(new SequenceParms().Loops(1));
						TweenParms parm3 = new TweenParms().Prop("position", trans_menuItemInter.position).Prop("rotation", Quaternion.identity).Prop("localScale", trans_menuItemInter.localScale)
							.Ease(EaseType.EaseOutQuad);
						sequence.Append(HOTween.To(spawnedItemObject.transform, 0.1f, parm3));
						TweenParms parm2 = new TweenParms().Prop("localPosition", itemsList[i].localPosition).Prop("localRotation", Quaternion.Euler(itemsList[i].localEulerAngles)).Prop("localScale", itemsList[i].localScale)
							.Ease(EaseType.EaseOutQuad);
						if (isLastItem)
						{
							parm2.OnComplete(onMenuShown);
						}
						sequence.Append(HOTween.To(spawnedItemObject.transform, 0.4f, parm2));
						sequence.Play();
						if (!ignoreTimeScale)
						{
							yield return (object)new WaitForSeconds(singleAnimTime);
						}
						else
						{
							base.StartCoroutine(WaitForRealSeconds(singleAnimTime));
						}
					}
					foreach (UNOMenuItem menuItem2 in currentMenu.menuItems)
					{
						object[] parameters = new object[3]
						{
							currentMenu.MenuID,
							null,
							null
						};
						menuItem2.instance.SendMessage("onMenuShowsReady", parameters, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}

		private IEnumerator WaitForRealSeconds(float delay)
		{
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + delay)
			{
				yield return (object)null;
			}
		}

		private void addMouseTriggerForCurMenus()
		{
			foreach (UNOMenuItem menuItem in currentMenu.menuItems)
			{
				addMouseTriggerForMenu(menuItem.instance, menuItem);
			}
		}

		private void removeMouseTriggerForCurMenus()
		{
			foreach (UNOMenuItem menuItem in currentMenu.menuItems)
			{
				menuItem.instance.GetComponent<SimpleEventTrigger>().enabled = false;
			}
		}

		private void addMouseTriggerForMenu(GameObject menuGO, UNOMenuItem menuItem)
		{
			Util.AddEventTriggerToGameObject(menuGO, EventTriggerType.PointerEnter, delegate
			{
				onMouseSelected(menuItem);
			});
			Util.AddEventTriggerToGameObject(menuGO, EventTriggerType.PointerClick, delegate
			{
				MenuInputListener(0, InputEventID.RELEASEA);
			});
			Util.AddEventTriggerToGameObject(menuGO, EventTriggerType.PointerExit, delegate
			{
				onMouseDeselected(menuItem);
			});
			Util.AddEventTriggerToGameObject(menuGO, EventTriggerType.PointerEnter, delegate
			{
				menuGO.GetComponentInChildren<BoxCollider>().transform.localScale = new Vector3(1f, 1f, 1.2f);
			});
			Util.AddEventTriggerToGameObject(menuGO, EventTriggerType.PointerExit, delegate
			{
				menuGO.GetComponentInChildren<BoxCollider>().transform.localScale = new Vector3(1f, 1f, 1f);
			});
			Collider componentInChildren = menuGO.GetComponentInChildren<Collider>();
			if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null)
			{
				componentInChildren.enabled = true;
			}
		}

		public void buildMenuTree()
		{
			for (int i = 0; i < editableMenuList.Length; i++)
			{
				UNOMenu uNOMenu = new UNOMenu();
				string text = uNOMenu.menuDescription = editableMenuList[i].menuDescription;
				menuInstancesMap.Add(editableMenuList[i].menuID, uNOMenu);
				uNOMenu.isCustomMenu = editableMenuList[i].isCustomMenu;
				uNOMenu.topBarAvailable = editableMenuList[i].topBarAvailable;
				uNOMenu.isCustomAnimation = editableMenuList[i].isCustomAnimation;
				uNOMenu.BottonsContent = editableMenuList[i].BottonsContent;
				uNOMenu.MenuID = editableMenuList[i].menuID;
			}
			for (int j = 0; j < editableMenuList.Length; j++)
			{
				UNOMenu uNOMenu2 = menuInstancesMap[editableMenuList[j].menuID];
				for (int k = 0; k < editableMenuList[j].menuItems.Length; k++)
				{
					UNOMenuItemEditable uNOMenuItemEditable = editableMenuList[j].menuItems[k];
					UNOMenu uNOMenu3 = (!string.IsNullOrEmpty(uNOMenuItemEditable.childMenuID)) ? menuInstancesMap[uNOMenuItemEditable.childMenuID] : null;
					if (uNOMenu3 != null)
					{
						uNOMenu3.parentMenu = uNOMenu2;
					}
					string itemText = uNOMenuItemEditable.ItemText;
					string itemDescriptionText = uNOMenuItemEditable.ItemDescriptionText;
					UNOMenuItem uNOMenuItem = new UNOMenuItem(itemText, uNOMenuItemEditable.gameObj, uNOMenu3, itemDescriptionText, uNOMenuItemEditable.Order, uNOMenuItemEditable.isDisabled, uNOMenuItemEditable.openScene);
					if ((UnityEngine.Object)uNOMenuItem.prefab.GetComponent<MenuItemBase>() == (UnityEngine.Object)null || uNOMenuItem.prefab.GetComponent<MenuItemBase>().willShow())
					{
						uNOMenu2.menuItems.Add(uNOMenuItem);
					}
				}
			}
		}

		private void destroyMenu(UNOMenu _menu)
		{
			foreach (UNOMenuItem menuItem in _menu.menuItems)
			{
				if (menuItem != null)
				{
					UnityEngine.Object.Destroy(menuItem.instance);
				}
			}
		}

		private void ShowMenuItemDescription(UNOMenuItem _item)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)Resources.Load("Menu/2D_Assets/Prefabs/MenuItemDes", typeof(GameObject)));
			if (OpenFrom == EOpenFrom.PauseMenu)
			{
				gameObject.GetComponent<UIPanel>().depth = 2;
				gameObject.transform.localPosition = new Vector3(0f, -1.7f, 15f);
			}
			Util.SetLayerRecursively(gameObject, spawnLayer);
			GameObject gameObject2 = gameObject.transform.Find("Container/text").gameObject;
			gameObject2.GetComponent<UILabel>().text = Singleton<LocalizationManager>.Instance.GetTermTranslationAndFont(_item.itemDescription, gameObject2, null, false);
			if ((UnityEngine.Object)GameObject.Find("Desktop") != (UnityEngine.Object)null)
			{
				gameObject.transform.SetParent(GameObject.Find("Desktop").transform);
			}
		}

		private void ToggleOnFxOfSelectedItem(bool _flag)
		{
			if ((UnityEngine.Object)selectedMenuItem.instance != (UnityEngine.Object)null)
			{
				Animator componentInChildren = ((Component)selectedMenuItem.instance.transform).GetComponentInChildren<Animator>();
				if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null)
				{
					componentInChildren.SetTrigger((!_flag) ? "Unselect" : "Select");
				}
			}
			if ((UnityEngine.Object)selectedMenuItem.instance != (UnityEngine.Object)null)
			{
				Transform transform = selectedMenuItem.instance.transform.Find("BoneRoot/Card_Shadow");
				if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
				{
					TweenAlpha.Begin(transform.gameObject, 0.5f, (!_flag) ? 1f : 0f);
				}
			}
			if (_flag)
			{
				GameObject gameObject = (GameObject)Resources.Load("Menu/2D_Assets/Prefabs/TextPanel", typeof(GameObject));
				if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
					gameObject2.transform.SetParent(selectedMenuItem.instance.transform, false);
					if (OpenFrom == EOpenFrom.PauseMenu)
					{
						gameObject2.GetComponent<UIPanel>().depth = 1;
					}
					UILabel componentInChildren2 = gameObject2.GetComponentInChildren<UILabel>();
					componentInChildren2.text = Singleton<LocalizationManager>.Instance.GetTermTranslationAndFont(selectedMenuItem.itemName, gameObject2.transform.Find("Text").gameObject, null, false);
					Util.SetLayerRecursively(gameObject2, spawnLayer);
				}
				GameObject original = (GameObject)Resources.Load("Theme/Classic/GFX/Particle_CardGlow_Star", typeof(GameObject));
				if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
				{
					GameObject gameObject3 = UnityEngine.Object.Instantiate(original);
					gameObject3.transform.SetParent(selectedMenuItem.instance.transform, false);
					Util.SetLayerRecursively(gameObject3, spawnLayer);
				}
			}
			else
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Transient");
				GameObject[] array2 = array;
				foreach (GameObject obj in array2)
				{
					UnityEngine.Object.Destroy(obj);
				}
			}
		}

		public void UpdateBtnWhenMainMenuItemsSwtich(string newBtnContent)
		{
			if (currentMenu.MenuID == "mainmenu" && newBtnContent != m_lastMainMenuBtnContent)
			{
				GameObject gameObject = GameObject.Find("Desktop/BtnPanel");
				gameObject.GetComponent<ButtonsManager_v2>().destroyButtonsImmediately();
				gameObject.GetComponent<ButtonsManager_v2>().ShowButtonsByMenuConfig(newBtnContent, ButtonsManager_v2.Pivot.MIDDLE, false, false);
				m_lastMainMenuBtnContent = newBtnContent;
			}
		}

		private IEnumerator animationOnBackClose(UNOMenu _menu)
		{
			yield return (object)new WaitForEndOfFrame();
			MenuManagerUtil.ResetUnityMsgRecievingStatus();
			_menu.menuItems[0].instance.SendMessage("OverridingOnBackClose", SendMessageOptions.DontRequireReceiver);
			if (!MenuManagerUtil.UNITYMSGRECIEVED)
			{
				GameObject obj = _menu.menuItems[0].instance;
				Vector3 initPos = obj.transform.position;
				TweenPosition.Begin(obj, 0.25f, initPos + new Vector3(0f, 0f, 2f));
				TweenAlpha.Begin(obj, 0.25f, 0f);
				yield return (object)WaitForRealSeconds(0.25f);
				onMenuHidden(null);
			}
		}

		private IEnumerator animationOnValidateClose(UNOMenu _menu)
		{
			yield return (object)new WaitForEndOfFrame();
			MenuManagerUtil.ResetUnityMsgRecievingStatus();
			_menu.menuItems[0].instance.SendMessage("OverridingOnValidateClose", SendMessageOptions.DontRequireReceiver);
			if (!MenuManagerUtil.UNITYMSGRECIEVED)
			{
				GameObject obj = _menu.menuItems[0].instance;
				Vector3 initPos = obj.transform.position;
				TweenPosition.Begin(obj, 0.25f, initPos + new Vector3(0f, 0f, 2f));
				TweenAlpha.Begin(obj, 0.25f, 0f);
				yield return (object)WaitForRealSeconds(0.25f);
				onMenuHidden(null);
			}
		}

		private IEnumerator animationOnOpen(UNOMenu _menu)
		{
			yield return (object)new WaitForEndOfFrame();
			MenuManagerUtil.ResetUnityMsgRecievingStatus();
			_menu.menuItems[0].instance.SendMessage("OverridingOnOpen", SendMessageOptions.DontRequireReceiver);
			if (!MenuManagerUtil.UNITYMSGRECIEVED)
			{
				GameObject obj = _menu.menuItems[0].instance;
				Vector3 initPos = obj.transform.position;
				TweenPosition.Begin(obj, 0f, initPos + new Vector3(0f, 0f, 2f));
				TweenAlpha.Begin(obj, 0f, 0f);
				yield return (object)WaitForRealSeconds(0.2f);
				TweenPosition.Begin(obj, 0.25f, initPos);
				TweenAlpha.Begin(obj, 0.25f, 1f);
				yield return (object)WaitForRealSeconds(0.25f);
				onMenuShown();
			}
		}

		public void FoldMenu(TweenDelegate.TweenCallback _callBack)
		{
			desktop.SetActive(false);
			Vector3 vector = new Vector3(0f, -4f, 0f);
			HOTween.To(GameObject.Find("SpawnManager").transform, 0.3f, new TweenParms().Prop("position", vector).UpdateType(UpdateType.TimeScaleIndependentUpdate).OnComplete(_callBack));
			int count = currentMenu.menuItems.Count;
			List<TransformMetaData> list = MenuManagerUtil.generateMenuItemOriginalInfo(count);
			for (int i = 0; i < count; i++)
			{
				Tweener tweener = HOTween.To(currentMenu.menuItems[i].instance.transform, 0.5f, new TweenParms().Prop("localPosition", list[i].localPosition).UpdateType(UpdateType.TimeScaleIndependentUpdate).Prop("localRotation", Quaternion.Euler(list[i].localEulerAngles))
					.Prop("localScale", list[i].localScale)
					.Ease(EaseType.EaseOutQuad));
			}
			ToggleOnFxOfSelectedItem(false);
		}

		public void UnFoldMenu(TweenDelegate.TweenCallback _callBack)
		{
			desktop.SetActive(true);
			Vector3 zero = Vector3.zero;
			HOTween.To(GameObject.Find("SpawnManager").transform, 0.3f, new TweenParms().UpdateType(UpdateType.TimeScaleIndependentUpdate).Prop("position", zero).OnComplete(_callBack));
			int count = currentMenu.menuItems.Count;
			int sel = currentMenu.menuItems.IndexOf(selectedMenuItem);
			List<TransformMetaData> list = MenuManagerUtil.generateMenuItemInfo(count, sel);
			for (int i = 0; i < count; i++)
			{
				Tweener tweener = HOTween.To(currentMenu.menuItems[i].instance.transform, 0.5f, new TweenParms().UpdateType(UpdateType.TimeScaleIndependentUpdate).Prop("localPosition", list[i].localPosition).Prop("localRotation", Quaternion.Euler(list[i].localEulerAngles))
					.Prop("localScale", list[i].localScale)
					.Ease(EaseType.EaseOutQuad));
			}
			ToggleOnFxOfSelectedItem(true);
		}

		public IEnumerator onBack(UNOMenu _menu)
		{
			if (_menu.parentMenu != null || OpenFrom == EOpenFrom.PauseMenu || HyperJump)
			{
				mLastMenuBeforeBack = _menu;
				this.menuHiddenCallBack = (menuHiddenCallBackDelegate)Delegate.Combine(this.menuHiddenCallBack, new menuHiddenCallBackDelegate(menuHiddenbyBackCallBack));
				onMenuHides();
				if (HyperJump)
				{
					HyperJump = false;
				}
				int menuItemCount = _menu.menuItems.Count;
				float singleAnimTime = 0.5f / (float)menuItemCount;
				if (_menu.isCustomMenu)
				{
					if (_menu.menuItems.Count == 1)
					{
						base.StartCoroutine(animationOnBackClose(_menu));
					}
					yield return (object)null;
				}
				else
				{
					SimpleSingleton<AudioManager>.Instance.playEvent("Play_UI_Menu_Action_Card_Receive");
					for (int i = menuItemCount - 1; i >= 0; i--)
					{
						bool isLastItem = i == 0;
						_menu.menuItems[i].instance.GetComponentInChildren<Animator>().SetTrigger("Back");
						Sequence sequence = new Sequence(new SequenceParms().Loops(1).UpdateType(UpdateType.TimeScaleIndependentUpdate));
						TweenParms parm3 = new TweenParms().Prop("position", trans_menuItemInter.position).Prop("rotation", Quaternion.identity).Prop("localScale", trans_menuItemInter.localScale)
							.UpdateType(UpdateType.TimeScaleIndependentUpdate)
							.Ease(EaseType.EaseOutQuad);
						sequence.Append(HOTween.To(_menu.menuItems[i].instance.transform, 0.4f, parm3));
						TweenParms parm2 = new TweenParms().Prop("position", trans_menuItemStart.position).Prop("rotation", Quaternion.identity).Prop("localScale", trans_menuItemStart.localScale)
							.UpdateType(UpdateType.TimeScaleIndependentUpdate)
							.Ease(EaseType.EaseOutQuad);
						if (isLastItem)
						{
							parm2.OnComplete(onMenuHidden, false);
						}
						sequence.Append(HOTween.To(_menu.menuItems[i].instance.transform, 0.2f, parm2));
						sequence.Play();
						if (!ignoreTimeScale)
						{
							yield return (object)new WaitForSeconds(singleAnimTime);
						}
						else
						{
							base.StartCoroutine(WaitForRealSeconds(singleAnimTime));
						}
					}
				}
			}
			else
			{
				mLastMenuBeforeBack = null;
			}
		}

		public void onValidateGo()
		{
			base.StartCoroutine(onValidate(currentMenu, selectedMenuItem));
		}

		public void onBackGo()
		{
			if (currentMenu.menuDescription == "UI_MAIN_MENU_TITLE")
			{
				GlobalPopupController.Instance.ShowDefaultPopup("UI_PC_MAIN_MENU_QUIT_GAME_POP_UP_TITLE", "UI_PC_MAIN_MENU_QUIT_GAME_POP_UP_DES", "UI_OPTION_YES", "UI_OPTION_NO", delegate
				{
					Application.Quit();
				}, null);
			}
			else
			{
				base.StartCoroutine(onBack(currentMenu));
			}
		}

		public IEnumerator onValidate(UNOMenu _menu, UNOMenuItem _selectedMenuItem)
		{
			if (!((UnityEngine.Object)_selectedMenuItem.instance == (UnityEngine.Object)null))
			{
				object[] parameters = new object[3]
				{
					currentMenu.MenuID,
					_selectedMenuItem.itemName,
					_selectedMenuItem.prefab
				};
				if ((UnityEngine.Object)_selectedMenuItem.instance != (UnityEngine.Object)null)
				{
					_selectedMenuItem.instance.SendMessage("OnItemPressed", parameters, SendMessageOptions.DontRequireReceiver);
				}
				mLastMenuBeforeBack = null;
				if (!_selectedMenuItem.isDisabled)
				{
					SimpleSingleton<AudioManager>.Instance.playEvent("Play_UI_Menu_Action_Card_Confirm");
				}
				bool hasRelatedMenu = !_selectedMenuItem.isDisabled && _selectedMenuItem.relatedMenu != null;
				if (!hasRelatedMenu && !HyperJump)
				{
					yield break;
				}
				if (hasRelatedMenu)
				{
					this.menuHiddenCallBack = (menuHiddenCallBackDelegate)Delegate.Combine(this.menuHiddenCallBack, new menuHiddenCallBackDelegate(menuHiddenbyValidateCallBack));
				}
				if (HyperJump)
				{
					HyperJump = false;
				}
				onMenuHides();
				int menuItemCount = _menu.menuItems.Count;
				float singleAnimTime = 0.5f / (float)menuItemCount;
				int lastHidingIdx = (_menu.menuItems.IndexOf(selectedMenuItem) == 0) ? 1 : 0;
				if (_menu.isCustomMenu)
				{
					if (_menu.menuItems.Count == 1)
					{
						base.StartCoroutine(animationOnValidateClose(_menu));
					}
					yield return (object)null;
				}
				else if (_menu.isCustomAnimation && _menu.MenuID == "MedalSystem")
				{
					destroyMedalMenu(false);
					foreach (UNOMenuItem menuItem in currentMenu.menuItems)
					{
						menuItem.instance.transform.SetParent(trans_medalMenuItemHolder, false);
					}
					TweenParms tp4 = new TweenParms().Ease(EaseType.EaseOutQuad);
					tp4.Prop("localPosition", trans_medalMenuItemLeft1.localPosition).Prop("localRotation", trans_medalMenuItemLeft1.localRotation).UpdateType(UpdateType.TimeScaleIndependentUpdate)
						.Prop("localScale", trans_medalMenuItemLeft1.localScale);
					TweenParms tp3 = new TweenParms().Ease(EaseType.EaseOutQuad);
					tp3.Prop("localPosition", trans_medalMenuItemLeft2.localPosition).Prop("localRotation", trans_medalMenuItemLeft2.localRotation).UpdateType(UpdateType.TimeScaleIndependentUpdate)
						.Prop("localScale", trans_medalMenuItemLeft2.localScale);
					TweenParms tp2 = new TweenParms().Ease(EaseType.EaseOutQuad);
					tp2.Prop("localPosition", trans_medalMenuItemLeft3.localPosition).Prop("localRotation", trans_medalMenuItemLeft3.localRotation).UpdateType(UpdateType.TimeScaleIndependentUpdate)
						.Prop("localScale", trans_medalMenuItemLeft3.localScale);
					tp2.OnComplete(onMedalMenuHidden);
					List<UNOMenuItem> medalMenuItems = new List<UNOMenuItem>();
					for (int j = menuItemCount - 1; j >= 0; j--)
					{
						if (!(_menu.menuItems[j].prefab.name == selectedMenuItem.prefab.name))
						{
							AddShadowToMedalMenu(_menu.menuItems[j]);
							medalMenuItems.Add(_menu.menuItems[j]);
						}
					}
					AddShadowToMedalMenu(selectedMenuItem);
					if (!ignoreTimeScale)
					{
						yield return (object)new WaitForSeconds(0.1f);
					}
					else
					{
						base.StartCoroutine(WaitForRealSeconds(0.1f));
					}
					HOTween.To(medalMenuItems[1].instance.transform, 0.4f, tp4);
					if (!ignoreTimeScale)
					{
						yield return (object)new WaitForSeconds(singleAnimTime);
					}
					else
					{
						base.StartCoroutine(WaitForRealSeconds(singleAnimTime));
					}
					HOTween.To(medalMenuItems[0].instance.transform, 0.4f, tp3);
					if (!ignoreTimeScale)
					{
						yield return (object)new WaitForSeconds(singleAnimTime);
					}
					else
					{
						base.StartCoroutine(WaitForRealSeconds(singleAnimTime));
					}
					HOTween.To(selectedMenuItem.instance.transform, 0.4f, tp2);
					if (!ignoreTimeScale)
					{
						yield return (object)new WaitForSeconds(singleAnimTime);
					}
					else
					{
						base.StartCoroutine(WaitForRealSeconds(singleAnimTime));
					}
				}
				else
				{
					TweenParms tp = new TweenParms().Ease(EaseType.EaseOutQuad);
					tp.Prop("position", trans_menuItemEnd.position).Prop("rotation", trans_menuItemEnd.rotation).UpdateType(UpdateType.TimeScaleIndependentUpdate)
						.Prop("localScale", trans_menuItemEnd.localScale);
					if (menuItemCount == 1)
					{
						tp.OnComplete(onMenuHidden, true);
					}
					selectedMenuItem.instance.GetComponentsInChildren<Animator>();
					SimpleSingleton<AudioManager>.Instance.playEvent("Play_UI_Menu_Action_Card_Receive");
					HOTween.To(selectedMenuItem.instance.transform, 0.5f, tp);
					if (!ignoreTimeScale)
					{
						yield return (object)new WaitForSeconds(singleAnimTime);
					}
					else
					{
						base.StartCoroutine(WaitForRealSeconds(singleAnimTime));
					}
					for (int i = menuItemCount - 1; i >= 0; i--)
					{
						new TweenParms().Ease(EaseType.EaseOutQuad);
						if (_menu.menuItems[i] != selectedMenuItem)
						{
							_menu.menuItems[i].instance.GetComponentInChildren<Animator>().SetTrigger("Back");
							Sequence sequence = new Sequence(new SequenceParms().Loops(1).UpdateType(UpdateType.TimeScaleIndependentUpdate));
							TweenParms parm3 = new TweenParms().Prop("position", trans_menuItemInter.position).Prop("rotation", Quaternion.identity).Prop("localScale", trans_menuItemInter.localScale)
								.UpdateType(UpdateType.TimeScaleIndependentUpdate)
								.Ease(EaseType.EaseOutQuad);
							sequence.Append(HOTween.To(_menu.menuItems[i].instance.transform, 0.4f, parm3));
							TweenParms parm2 = new TweenParms().Prop("position", trans_menuItemStart.position).Prop("rotation", Quaternion.identity).Prop("localScale", trans_menuItemStart.localScale)
								.UpdateType(UpdateType.TimeScaleIndependentUpdate)
								.Ease(EaseType.EaseOutQuad);
							if (i == lastHidingIdx)
							{
								parm2.OnComplete(onMenuHidden, true);
							}
							sequence.Append(HOTween.To(_menu.menuItems[i].instance.transform, 0.2f, parm2));
							sequence.Play();
							if (!ignoreTimeScale)
							{
								yield return (object)new WaitForSeconds(singleAnimTime);
							}
							else
							{
								base.StartCoroutine(WaitForRealSeconds(singleAnimTime));
							}
						}
					}
				}
			}
		}

		private void onDirection(EUNOMenuDirection _direction, UNOMenu _menu)
		{
			switch (_direction)
			{
			case EUNOMenuDirection.DIR_LEFT:
				if (selectedMenuItem != null && _menu.menuItems.Count > 1)
				{
					int num2 = _menu.menuItems.IndexOf(selectedMenuItem);
					if (num2 != -1 && num2 > 0)
					{
						onMenuItemDeselected(selectedMenuItem);
						selectedMenuItem = _menu.menuItems[num2 - 1];
						onMenuItemSelected(selectedMenuItem);
						SimpleSingleton<AudioManager>.Instance.playEvent("Play_UI_Menu_Action_Card_Select");
					}
				}
				break;
			case EUNOMenuDirection.DIR_RIGHT:
				if (selectedMenuItem != null && _menu.menuItems.Count > 1)
				{
					int num = _menu.menuItems.IndexOf(selectedMenuItem);
					if (num != -1 && num < _menu.menuItems.Count - 1)
					{
						onMenuItemDeselected(selectedMenuItem);
						selectedMenuItem = _menu.menuItems[num + 1];
						onMenuItemSelected(selectedMenuItem);
						SimpleSingleton<AudioManager>.Instance.playEvent("Play_UI_Menu_Action_Card_Select");
					}
				}
				break;
			}
		}

		public void MenuInputListener(int _index, InputEventID _event)
		{
			if (currentMenu != null && !currentMenu.isCustomMenu)
			{
				GameObject gameObject = null;
				if (OpenFrom == EOpenFrom.MainMenu)
				{
					gameObject = GameObject.Find("Desktop/BtnPanel");
				}
				else if (OpenFrom == EOpenFrom.PauseMenu)
				{
					gameObject = PauseMenuController.Instance.ButtonPanel;
					gameObject.GetComponent<ButtonsManager_v2>().lockBtnManger(false);
				}
				bool flag = (UnityEngine.Object)selectedMenuItem.prefab.GetComponent<MenuItemBase>() == (UnityEngine.Object)null;
				switch (_event)
				{
				case InputEventID.RELEASEA:
					if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null && gameObject.GetComponent<ButtonsManager_v2>().pressBtn(ButtonsManager_v2.Btn.A, true, 0.8f))
					{
						InputHandler.GapInput("Menu->Release A", 0.5f);
						if (!flag)
						{
							object[] value2 = new object[3]
							{
								currentMenu.MenuID,
								null,
								null
							};
							selectedMenuItem.instance.SendMessage("OnItemPreValidate", value2, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							onValidateGo();
						}
					}
					break;
				case InputEventID.RELEASEB:
					if (OpenFrom == EOpenFrom.PauseMenu && currentMenu.MenuID == "helpoption")
					{
						if (gameObject.GetComponent<ButtonsManager_v2>().pressBtn(ButtonsManager_v2.Btn.B, false, 1f))
						{
							InputHandler.GapInput("Menu->Release B", 0.5f);
							onBackGo();
						}
					}
					else if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null && gameObject.GetComponent<ButtonsManager_v2>().pressBtn(ButtonsManager_v2.Btn.B, false, 0.8f))
					{
						InputHandler.GapInput("Menu->Release B", 0.5f);
						if (!flag)
						{
							if (currentMenu.menuDescription != "UI_MAIN_MENU_TITLE")
							{
								removeMouseTriggerForCurMenus();
							}
							object[] value = new object[3]
							{
								currentMenu.MenuID,
								null,
								null
							};
							selectedMenuItem.instance.SendMessage("OnItemPreBack", value, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							onBackGo();
						}
					}
					break;
				case InputEventID.DPAD_LEFT:
				case InputEventID.LEFTSTICKLEFT:
					onDirection(EUNOMenuDirection.DIR_LEFT, currentMenu);
					if ((UnityEngine.Object)MenuBackgroundManager.Instance != (UnityEngine.Object)null)
					{
						MenuBackgroundManager.Instance.onDirection(currentMenu.selectedIdx);
					}
					break;
				case InputEventID.DPAD_RIGHT:
				case InputEventID.LEFTSTICKRIGHT:
					onDirection(EUNOMenuDirection.DIR_RIGHT, currentMenu);
					if ((UnityEngine.Object)MenuBackgroundManager.Instance != (UnityEngine.Object)null)
					{
						MenuBackgroundManager.Instance.onDirection(currentMenu.selectedIdx);
					}
					break;
				}
			}
		}

		private void BackToPauseMenu()
		{
			if ((UnityEngine.Object)base.transform != (UnityEngine.Object)null)
			{
				UITweener[] components = base.GetComponents<UITweener>();
				UITweener[] array = components;
				foreach (UITweener uITweener in array)
				{
					uITweener.tweenFactor = 0f;
					uITweener.ignoreTimeScale = false;
					uITweener.PlayReverse();
				}
			}
			if (this.OnBackToPauseMenu != null)
			{
				this.OnBackToPauseMenu();
			}
			base.StartCoroutine(OnBackToPauseMenuDone());
		}

		private IEnumerator OnBackToPauseMenuDone()
		{
			yield return (object)null;
			m_menuObj.SetActive(false);
			Transform desc = desktop.transform.FindChild("MenuItemDes(Clone)");
			if ((UnityEngine.Object)desc != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy(desc.gameObject);
			}
			SimpleSingleton<InputHandler>.Instance.EnterInputCategory(Category.INPUT_POPUP, "PauseMenuController");
		}

		public void EnableInputListener(bool enabled)
		{
			ClearAllEvents();
			BindMainPlayerInputListener(enabled, MenuInputListener);
		}

		private static void UnbindInputEvents(GamePad.Index index, CompositeButtonEvent.ButtonEvent function)
		{
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEA, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEB, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEX, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEY, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPAD_LEFT, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPAD_RIGHT, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPADUP, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPADDOWN, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASESTART, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKLEFT, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKRIGHT, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKUP, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKDOWN, index).OnButtonEvt -= function;
			SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RIGHT_SHOULDER, index).OnButtonEvt -= function;
		}

		public static void BindMainPlayerInputListener(bool enabled, CompositeButtonEvent.ButtonEvent function)
		{
			if (enabled)
			{
				GamePad.Index index = SimpleSingleton<UserManagerAdapter>.Instance.MainUserGamepadIndex();
				UnbindInputEvents(index, function);
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEA, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEB, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEX, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEY, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPAD_LEFT, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPAD_RIGHT, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPADUP, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPADDOWN, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASESTART, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKLEFT, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKRIGHT, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKUP, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKDOWN, index).OnButtonEvt += function;
				SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RIGHT_SHOULDER, index).OnButtonEvt += function;
			}
			else if (InputHandler.Valid())
			{
				foreach (int value in Enum.GetValues(typeof(GamePad.Index)))
				{
					UnbindInputEvents((GamePad.Index)value, function);
				}
			}
		}

		private void menuHiddenbyBackCallBack()
		{
			if (this.beforeMenuHiddenByBackCallBack == null)
			{
				menuHiddenbyBackCallBackGo();
			}
			else
			{
				this.beforeMenuHiddenByBackCallBack();
				this.beforeMenuHiddenByBackCallBack = null;
			}
		}

		public void menuHiddenbyBackCallBackGo()
		{
			if (currentMenu != null && currentMenu.parentMenu != null)
			{
				base.StartCoroutine(showMenu(currentMenu.parentMenu));
			}
		}

		private void menuHiddenbyValidateCallBack()
		{
			if (this.beforeMenuHiddenByValidateCallBack == null)
			{
				menuHiddenbyValidateCallBackGo();
			}
			else
			{
				this.beforeMenuHiddenByValidateCallBack();
				this.beforeMenuHiddenByValidateCallBack = null;
			}
		}

		public void menuHiddenbyValidateCallBackGo()
		{
			if (currentMenu != null && selectedMenuItem.relatedMenu != null)
			{
				base.StartCoroutine(showMenu(selectedMenuItem.relatedMenu));
			}
		}

		public void menuHiddenbyValidateFailureCallBackGo()
		{
			base.StartCoroutine(showMenu(currentMenu));
		}

		private void ClearAllEvents()
		{
			if (InputHandler.Valid())
			{
				foreach (int value in Enum.GetValues(typeof(GamePad.Index)))
				{
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEA, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEB, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEX, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASEY, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPAD_LEFT, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPAD_RIGHT, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPADUP, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.DPADDOWN, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.RELEASESTART, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKLEFT, (GamePad.Index)value).ClearButtonEvents();
					SimpleSingleton<InputHandler>.Instance.GetEvent(InputEventID.LEFTSTICKRIGHT, (GamePad.Index)value).ClearButtonEvents();
				}
			}
		}
	}
}
