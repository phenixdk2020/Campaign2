using UnityEngine;

/// <summary>
/// Drop this on any campaign scene. Draws version BELOW the Game-toolbar.
/// Tells you whether atlas actually runs or you are still on UNIFIED.
/// </summary>
public sealed class CampaignVersionHud : MonoBehaviour
{
    [Tooltip("Pixels from top of Game view. 72 sits under Unity's toolbar.")]
    public float offsetFromTop = 72f;

    void OnGUI()
    {
        bool atlas = false;
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb != null && mb.GetType().Name == "TheaterAtlasBootstrap")
            {
                atlas = true;
                break;
            }
        }
        string text = atlas
            ? "PROJECT 1864  ·  campaign2  ·  v00.00.03 ATLAS"
            : "PROJECT 1864  ·  campaign2  ·  v00.00.18 UNIFIED  —  atlas kører IKKE";

        var style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(14, 14, 8, 8)
        };
        style.normal.textColor = atlas
            ? new Color(0.93f, 0.86f, 0.70f)
            : new Color(1f, 0.82f, 0.45f);

        GUI.Box(new Rect(12f, offsetFromTop, 720f, 36f), text, style);
    }
}
