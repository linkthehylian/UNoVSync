using UnityEngine;

public class HOInteraction : MonoBehaviour
{
    public static bool vsync = false;

    void OnGUI()
    {
        GUILayout.BeginArea(GUIHelpers.AlignRect(200, 50, GUIHelpers.Alignment.RIGHT, 0, 0), "", GUI.skin.box);
        vsync = GUILayout.Toggle(vsync, "V-Sync");
        if (vsync)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }
        GUILayout.EndArea();
    }

    private void Start()
	{
	}

	private void Update()
	{
	}

	public void playForwardTweeners()
	{
		UITweener[] components = base.GetComponents<UITweener>();
		UITweener[] array = components;
		foreach (UITweener uITweener in array)
		{
			uITweener.PlayForward();
		}
	}

	public void playReverseTweeners()
	{
		UITweener[] components = base.GetComponents<UITweener>();
		UITweener[] array = components;
		foreach (UITweener uITweener in array)
		{
			uITweener.PlayReverse();
		}
	}
}
