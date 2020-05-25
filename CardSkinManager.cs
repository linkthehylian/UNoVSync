using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CardSkinManager : MonoBehaviour
{
    public static bool downloading = false;

    public static string cardsUrl = "";
    public static string deckUrl = "";

    public static Texture2D cardsTexture;
    public static Texture2D deckTexture;

    public static Dictionary<string, string> states;

    public Rect windowRect = new Rect(10, 10, 300, 400);

    void Start()
    {
        DontDestroyOnLoad(this);
        states = new Dictionary<string, string>
        {
            { "cardsUrl", "<color=yellow>[ Not Loaded ]</color>" },
            { "deckUrl", "<color=yellow>[ Not Loaded ]</color>" }
        };
        cardsUrl = PlayerPrefs.GetString("cardsUrl", "");
        deckUrl = PlayerPrefs.GetString("deckUrl", "");
    }

    public Object[] ObjectsWithName(string name)
    {
        return FindObjectsOfType(typeof(GameObject)).Where(obj => obj.name == name).ToArray();
    }

    public void Tween()
    {
        Rect collapsed = new Rect(Screen.width - 400, Screen.height - 18, 300, 400);
        Rect normal = new Rect(Screen.width - 400, Screen.height - 250, 300, 400);

        if (col)
        {
            windowRect = normal;
        }
        else
        {
            windowRect = collapsed;
        }
    }

    public void OnGUI()
    {
        //GUILayout.Button("Loaded");
        //try
        //{
        //    RaycastHit hit;
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    if (Physics.Raycast(ray, out hit, 100.0f))
        //    {
        //        GameObject test = hit.transform.gameObject;
        //        GUILayout.Button($"GO: {test.name} || TAG: {test.tag}");
        //    }
        //}
        //catch { }

        Tween();

        windowRect = GUI.Window(69, windowRect, Window, "ExiMichi [ UNO Skins ]");
    }

    void Update()
    {
        if (cardsTexture != null)
        {
            foreach (GameObject f in ObjectsWithName("CardMesh"))
            {
                f.GetComponent<Renderer>().material.mainTexture = cardsTexture;
            }
        }
        if (deckTexture != null)
        {
            GameObject.Find("Classic_Card_Deck").GetComponent<Renderer>().material.mainTexture = deckTexture;
        }
    }

    bool col = true;
    public void Window(int id)
    {
        col = GUI.Toggle(new Rect(-1, -5, 302, 21), col, "", GUI.skin.label);

        GUILayout.Label("[ Cards URL ] " + states["cardsUrl"]);

        GUILayout.BeginHorizontal();
        cardsUrl = GUILayout.TextField(cardsUrl);
        if (GUILayout.Button("Load", GUILayout.Width(60)))
        {
            cardsUrl = PlayerPrefs.GetString("cardsUrl", "");
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("[ Deck URL ] " + states["deckUrl"]);

        GUILayout.BeginHorizontal();
        deckUrl = GUILayout.TextField(deckUrl);
        if (GUILayout.Button("Load", GUILayout.Width(60)))
        {
            deckUrl = PlayerPrefs.GetString("deckUrl", "");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button(downloading ? "Please wait" : "Apply Skins"))
        {
            if (!downloading)
            {
                base.StartCoroutine(applySkins());
            }
        }
        GUILayout.Label("Please ensure URLs are valid!", GUI.skin.box);
    }

    public IEnumerator applySkins()
    {
        downloading = true;
        if (string.IsNullOrEmpty(cardsUrl))
        {
            states["cardsUrl"] = "<color=yellow>[ Not Loaded ]</color>";
        }
        else
        {
            using (WWW card = new WWW(cardsUrl))
            {
                yield return card;
                if (card.error != null)
                {
                    states["cardsUrl"] = "<color=red>[ URL Error ]</color>";
                }
                else
                {
                    cardsTexture = card.texture;
                    PlayerPrefs.SetString("cardsUrl", cardsUrl);
                    states["cardsUrl"] = "<color=lime>[ Loaded ]</color>";
                }
            }
        }
        if (string.IsNullOrEmpty(deckUrl))
        {
            states["deckUrl"] = "<color=yellow>[ Not Loaded ]</color>";
        }
        else
        {
            using (WWW deck = new WWW(deckUrl))
            {
                yield return deck;
                if (deck.error != null)
                {
                    states["deckUrl"] = "<color=red>[ URL Error ]</color>";
                }
                else
                {
                    deckTexture = deck.texture;
                    PlayerPrefs.SetString("deckUrl", deckUrl);
                    states["deckUrl"] = "<color=lime>[ Loaded ]</color>";
                }
            }
        }
        yield return new WaitForSeconds(.5f);
        downloading = false;
    }
}