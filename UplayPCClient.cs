using AOT;
using RDV;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UNO;
using UNO.GamePlay;
using UNO.Networking;
using UNO.TRC;

internal class UplayPCClient
{
	public delegate void AchievementUnlockResult(bool isSuccess, uint achievementID);

	public delegate void GotUserAvatar(string profileId, IntPtr rgbaData, int sizeOfBuffer);

	public delegate void InitGameSaveListSuccess(int count, [In] [MarshalAs(UnmanagedType.LPStr)] string list);

	public delegate void InitGameSaveListFailed(uint errorCode);

	public delegate void ReadGameSaveSuccess(uint slotId, uint sizeOfData, IntPtr buffer);

	public delegate void ReadGameSaveFailed(uint slotId, uint errorCode);

	public delegate void WriteGameSaveSuccess(uint slotId, uint sizeOfData);

	public delegate void WriteGameSaveFailed(uint slotId, uint errorCode);

	public delegate void DeleteGameSaveSuccess(uint slotId);

	public delegate void DeleteGameSaveFailed(uint slotId, uint errorCode);

	public delegate void NetworkConnectionHandle();

	private static UplayPCClient m_instance;

	public UplayPCNative.UplayWrapperManagedMeta Meta;

	public static UplayPCClient Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new UplayPCClient();
			}
			return m_instance;
		}
	}

	public event AchievementUnlockResult onAchievementUnlockResult;

	public event GotUserAvatar onGotUserAvatar;

	public event InitGameSaveListSuccess onInitGameSaveListSuccess;

	public event InitGameSaveListFailed onInitGameSaveListFailed;

	public event ReadGameSaveSuccess onReadGameSaveSuccess;

	public event ReadGameSaveFailed onReadGameSaveFailed;

	public event WriteGameSaveSuccess onWriteGameSaveSuccess;

	public event WriteGameSaveFailed onWriteGameSaveFailed;

	public event DeleteGameSaveSuccess onDeleteGameSaveSuccess;

	public event DeleteGameSaveFailed onDeleteGameSaveFailed;

	public event NetworkConnectionHandle OnNetworkDisconnect;

	public static event Action OnOverlayHide;

	private UplayPCClient()
	{
		int hashCode = GetHashCode();
		Meta = default(UplayPCNative.UplayWrapperManagedMeta);
		Meta.handler = (IntPtr)hashCode;
		Meta.achievementCallback = OnAchievementUnlock;
		Meta.eventCallback = OnUplayEvent;
		Meta.startCallback = OnUplayStarted;
		Meta.gameSaveInitFailureCallback = OnInitGameSaveListFailed;
		Meta.gameSaveInitSuccessCallback = OnInitGameSaveListSuccess;
		Meta.gameSaveOpenFailureCallback = OnOpenSaveFailed;
		Meta.gameSaveOpenSuccessCallback = OnOpenSaveSuccess;
		Meta.gameSaveReadFailureCallback = OnReadSaveFailed;
		Meta.gameSaveReadSuccessCallback = OnReadSaveSuccess;
		Meta.gameSaveWriteFailureCallback = OnWriteSaveFailed;
		Meta.gameSaveWriteSuccessCallback = OnWriteSaveSuccess;
		Meta.gameSaveRemoveFailureCallback = OnRemoveSaveFailed;
		Meta.gameSaveRemoveSuccessCallback = OnRemoveSaveSuccess;
		Meta.selectFriendCallback = OnFriendSelectedFromInviteUI;
		Meta.invitationCallback = OnGameInviteSentCallback;
		Meta.invaitationRecievedCallback = OnGameInviteRecievedCallback;
		Meta.invaitationAcceptCallback = OnGameInviteAcceptCallback;
		Meta.userAvatarGotCallback = OnUserAvatarGot;
		Meta.connectionChangeCallback = OnConnectionChanged;
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnUplayStarted))]
	private static void OnUplayStarted(bool quit, int errorCode)
	{
		if (quit)
		{
			if (errorCode == 3)
			{
				if (File.Exists(".\\Support\\redist\\UplayInstaller.exe"))
				{
					Application.OpenURL(".\\Support\\redist\\UplayInstaller.exe");
				}
				else
				{
					Application.OpenURL("https://uplay.ubi.com/");
				}
			}
            SplashScreen.readyForMain = false;
            SplashScreen.onlyUplay = true;
            Application.Quit();
		}
		else
		{
			Debug.Log("Uplay PC started");
			UplayPCNative.NativeMethods.InitSaveGameList();
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnUplayEvent))]
	private static void OnUplayEvent(uint eventType)
	{
		Singleton<LogManager>.Instance.LogDebug("UPLAY_EVENT", "OnUplayEvent: " + ((UPLAY_EventType)eventType).ToString());
		switch (eventType)
		{
		case 10002u:
			Matchmaker_Rdv.Instance.OnRdvAcceptInvitation(default(RdvMatchmakingMessages.PluginMessage));
			break;
		case 30000u:
			SimpleSingleton<InputHandler>.Instance.Enabled = false;
			break;
		case 30001u:
			if (Matchmaker_Rdv.Instance.IsTitleScreenProcessComplete)
			{
				UbiservicesNative.NativeMethods.SyncUplayRewards((int)SimpleSingleton<UserManagerAdapter>.Instance.MainUserGamepadIndex());
				UbiservicesNative.NativeMethods.SyncUplayActions((int)SimpleSingleton<UserManagerAdapter>.Instance.MainUserGamepadIndex());
				SimpleSingleton<ProductManager>.Instance.CheckUplayDLC();
			}
			SimpleSingleton<InputHandler>.Instance.EnableAfterAllKeyReleased();
			if (UplayPCClient.OnOverlayHide != null)
			{
				UplayPCClient.OnOverlayHide();
			}
			break;
		case 50000u:
		{
			UplayAccountSharingMessage uplayAccountSharingMessage = new UplayAccountSharingMessage();
			uplayAccountSharingMessage.RegisterConfirmEvent(delegate
			{
				Application.Quit();
			});
			TRCManager.Instance.AddMessage(uplayAccountSharingMessage);
			break;
		}
		case 50001u:
		{
			if (Instance.OnNetworkDisconnect != null)
			{
				Instance.OnNetworkDisconnect();
			}
			UplayNotAvailableYetMessage message = new UplayNotAvailableYetMessage();
			TRCManager.Instance.AddMessage(message);
			break;
		}
		case 40000u:
			SimpleSingleton<UbiservicesPortal>.Instance.StartSyncUbiservice();
			break;
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnAchievementUnlock))]
	private static void OnAchievementUnlock(bool success, uint id)
	{
		LogInfo("Uplay Achievement unlock {0}!", (!success) ? "failed" : "succeed");
		if (Instance.onAchievementUnlockResult != null)
		{
			Instance.onAchievementUnlockResult(success, id);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnInitGameSaveListSuccess))]
	private static void OnInitGameSaveListSuccess(int count, [In] [MarshalAs(UnmanagedType.LPStr)] string list)
	{
		LogInfo("Init game save list succeed: {0} saves found!", count);
		if (Instance.onInitGameSaveListSuccess != null)
		{
			Instance.onInitGameSaveListSuccess(count, list);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnInitGameSaveListFailed))]
	private static void OnInitGameSaveListFailed(uint errorCode)
	{
		LogError("Failed to init game save list with error code {0}", errorCode);
		if (Instance.onInitGameSaveListFailed != null)
		{
			Instance.onInitGameSaveListFailed(errorCode);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnOpenSaveSuccess))]
	private static void OnOpenSaveSuccess(uint slotId)
	{
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnOpenSaveFailed))]
	private static void OnOpenSaveFailed(uint slotId, uint errorCode)
	{
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnReadSaveSuccess))]
	private static void OnReadSaveSuccess(uint slotId, uint sizeOfData, IntPtr buffer)
	{
		if (Instance.onReadGameSaveSuccess != null)
		{
			Instance.onReadGameSaveSuccess(slotId, sizeOfData, buffer);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnReadSaveFailed))]
	private static void OnReadSaveFailed(uint slotId, uint errorCode)
	{
		if (Instance.onReadGameSaveFailed != null)
		{
			Instance.onReadGameSaveFailed(slotId, errorCode);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnWriteSaveSuccess))]
	private static void OnWriteSaveSuccess(uint slotId, uint sizeOfData)
	{
		if (Instance.onWriteGameSaveSuccess != null)
		{
			Instance.onWriteGameSaveSuccess(slotId, sizeOfData);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnWriteSaveFailed))]
	private static void OnWriteSaveFailed(uint slotId, uint errorCode)
	{
		if (Instance.onWriteGameSaveFailed != null)
		{
			Instance.onWriteGameSaveFailed(slotId, errorCode);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnRemoveSaveSuccess))]
	private static void OnRemoveSaveSuccess(uint slotId)
	{
		if (Instance.onDeleteGameSaveSuccess != null)
		{
			Instance.onDeleteGameSaveSuccess(slotId);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnRemoveSaveFailed))]
	private static void OnRemoveSaveFailed(uint slotId, uint errorCode)
	{
		if (Instance.onDeleteGameSaveFailed != null)
		{
			Instance.onDeleteGameSaveFailed(slotId, errorCode);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnFriendSelectedFromInviteUI))]
	private static void OnFriendSelectedFromInviteUI(string name, string profileId)
	{
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnGameInviteSentCallback))]
	private static void OnGameInviteSentCallback(string name, bool invitationSentSuccess)
	{
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnGameInviteRecievedCallback))]
	private static void OnGameInviteRecievedCallback(uint invitationID, string sessionData)
	{
		Singleton<LogManager>.Instance.LogDebug("UPLAY_PC", "OnGameInviteRecievedCallback invitationID: {0} sessionData: {1}", invitationID, sessionData);
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnGameInviteAcceptCallback))]
	private static void OnGameInviteAcceptCallback(uint invitationID, string sessionData)
	{
		Singleton<LogManager>.Instance.LogDebug("UPLAY_PC", "OnGameInviteAcceptCallback invitationID: {0} sessionData: {1}", invitationID, sessionData);
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnUserAvatarGot))]
	private static void OnUserAvatarGot(string profileId, IntPtr rgbaData, int sizeOfBuffer)
	{
		if (Instance.onGotUserAvatar != null)
		{
			Instance.onGotUserAvatar(profileId, rgbaData, sizeOfBuffer);
		}
	}

	[MonoPInvokeCallback(typeof(UplayPCNative.OnConnectionChanged))]
	private static void OnConnectionChanged(bool isConnected)
	{
		LogInfo("OnConnectionChanged " + isConnected);
		if (isConnected)
		{
			SimpleSingleton<UbiservicesPortal>.Instance.LogInUbiservice();
		}
	}

	private static void LogDebug(string message)
	{
		Singleton<LogManager>.Instance.LogDebug("UplayPC", message);
	}

	private static void LogInfo(string message)
	{
		Singleton<LogManager>.Instance.LogInfo("UplayPC", message);
	}

	private static void LogWarning(string message)
	{
		Singleton<LogManager>.Instance.LogWarn("UplayPC", message);
	}

	private static void LogError(string message)
	{
		Singleton<LogManager>.Instance.LogError("UplayPC", message);
	}

	private static void LogDebug(string format, params object[] objects)
	{
		Singleton<LogManager>.Instance.LogDebug("UplayPC", format, objects);
	}

	private static void LogInfo(string format, params object[] objects)
	{
		Singleton<LogManager>.Instance.LogInfo("UplayPC", format, objects);
	}

	private static void LogWarning(string format, params object[] objects)
	{
		Singleton<LogManager>.Instance.LogWarn("UplayPC", format, objects);
	}

	private static void LogError(string format, params object[] objects)
	{
		Singleton<LogManager>.Instance.LogError("UplayPC", format, objects);
	}
}
