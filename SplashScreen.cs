using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UNO;
using UNO.GamePlay;

public class SplashScreen : MonoBehaviour
{
    //private int m_movieIndex;

    public GameObject PS4ScreenPlane;

    public GameObject X1ScreenPlane;

    public string[] MovieList;

    public GameObject m_epilepsyWarning;

    //private readonly List<MovieTexture> m_movieList = new List<MovieTexture>();

    //private readonly bool m_showVideo;

    private void Start()
    {
        SimpleSingleton<UplayPC>.Instance.Attach();
        initGamePlayLogic();
        StartCoroutine(begin());
        QualitySettings.vSyncCount = 0; //Force vsync off.
    }

    IEnumerator begin()
    {
        //yield return new WaitForSeconds(0.05f);
        yield return new WaitForEndOfFrame();
        //m_epilepsyWarning.GetComponent<epliepsyController>().go(playMovies); //Skip intro videos.
        string url = "Menu/3D_Assets/MainMenu/SpawnManager";
        //yield return new WaitForSeconds(3f);
        ResourceRequest resourceRequest = Resources.LoadAsync(url);
        yield return resourceRequest;
        GameObject prefab = resourceRequest.asset as GameObject;
        Singleton<PreloadSystem>.Instance.cacheGameObject(prefab, url);
        SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single); //Immediately load the main menu.
    }

    /*private void playMovies()
	{
		loadMovieList();
		m_showVideo = true;
		playMovieTexture(m_movieIndex);
	}*/





    private void OnGUI()
    {
        /*if (m_showVideo)
		{
			if (!m_movieList[m_movieIndex].isPlaying)
			{
				if (m_movieIndex == m_movieList.Count - 1)
				{
					m_showVideo = false;
					onMovieFinished();
					return;
				}
				m_movieIndex++;
				playMovieTexture(m_movieIndex);
			}
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), m_movieList[m_movieIndex], ScaleMode.StretchToFill, false, 0f);
		}*/

    }

    /*private void loadMovieList()
	{
		string[] movieList = MovieList;
		foreach (string str in movieList)
		{
			MovieTexture item = Resources.Load("Video_PCX1/" + str) as MovieTexture;
			m_movieList.Add(item);
		}
	}*/

    /*private void playMovieTexture(int _idx)
	{
		m_movieList[_idx].Play();
		AudioSource component = base.GetComponent<AudioSource>();
		component.clip = m_movieList[_idx].audioClip;
		component.Play();
		if (_idx == 0)
		{
			SimpleSingleton<AudioManager>.Instance.playEvent("Play_Sfx_UNO_Logo");
		}
	}*/

    /*private string GenerateMoviePath(string _str)
	{
		return Path.Combine(Application.streamingAssetsPath, "Movies//intro//" + _str + ".mp4");
	}*/

    /*private void onMovieFinished()
	{
		StartCoroutine(end());
	}*/

    /*private IEnumerator end()
	{
        //m_epilepsyWarning.GetComponent<epliepsyController>().ShowVideoWarn();
        yield return new WaitForSeconds(5f);
		SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
	}*/

    private void initGamePlayLogic()
    {
        RootScript.RunInBackground(false, "SplashScreen.Start");
        SimpleSingleton<MultiplayerManager>.Instance.Attach();
        SimpleSingleton<ProductManager>.Instance.Attach();
        SimpleSingleton<UserManagerAdapter>.Instance.Attach();
        SimpleSingleton<AudioManager>.Instance.Attach();
        SimpleSingleton<AudioManager>.Instance.registerSoundBank();
    }
}