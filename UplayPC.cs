using System.IO;
using UnityEngine;
using UNO;
using UNO.TRC;

internal class UplayPC : SimpleSingleton<UplayPC>
{
	private const int kTargetFrameRate = 60;

	private int m_frameCount;

	private bool last_UplayRunState;

	public bool IsRunning => last_UplayRunState;

	public UplayPCClient Client => UplayPCClient.Instance;

	public UplayPC()
	{
		base.Debug(UplayPCConstants.kUplayDevFlagPath);
		base.Info("Initilize Uplay PC");
		HookUnity.SetTimeFromUnity(0f);
		int num = 3360;
		int num3 = SteamManager.SteamAppId = 470220;
		SimpleSingleton<SteamManager>.Instance.Attach();
		base.ClassNameTag = "UplayPC";
		UplayPCClient client = Client;
		UplayPCNative.NativeMethods.InitWrapper(ref client.Meta);
		int startFlag = 0;
		if (File.Exists(UplayPCConstants.kUplayDevFlagPath))
		{
			startFlag = 6;
		}
		UnityEngine.Debug.Log("uplayAppId " + num);
		UplayPCNative.NativeMethods.InitUplay(num, startFlag);
	}

	protected override void OnAwake()
	{
		Singleton<RootScript>.Instance.Register(base.ClassNameTag, this);
	}

	public override void Update()
	{
		m_frameCount++;
		if (m_frameCount % 2 == 0)
		{
			bool flag = UplayPCNative.NativeMethods.UpdateUplay();
			if (last_UplayRunState && !flag)
			{
				UplayNotAvailableYetMessage message = new UplayNotAvailableYetMessage();
				TRCManager.Instance.AddMessage(message);
			}
			last_UplayRunState = flag;
		}
		if (m_frameCount >= 60)
		{
			m_frameCount = 0;
		}
	}
}
