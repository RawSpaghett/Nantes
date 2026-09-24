using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NantesGame.UI;

public static class ControlsPanelBuilder
{
    public static GameObject Create(Transform parent, TMP_FontAsset font, out Button back)
    {
        var page = new GameObject("Controls page", typeof(RectTransform), typeof(Image));
        page.transform.SetParent(parent, false);
        var rect = (RectTransform)page.transform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        page.GetComponent<Image>().color = new Color(.008f, .016f, .021f, 1);
        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(page.transform, false);
        var contentRect = (RectTransform)content.transform;
        contentRect.anchorMin = Vector2.zero; contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = contentRect.offsetMax = Vector2.zero;
        Label(page.transform, font, "CONTROLS", -700, 403, 700, 60, 35, new Color(.8f,.87f,.82f));
        Label(page.transform, font, "KEYBOARD / MOUSE", -700, 354, 700, 30, 14, new Color(.39f,.58f,.56f));
        Group(page.transform, font, "ON FOOT", -700, 286, new[] {
            "W A S D|Move", "MOUSE|Look around", "HOLD SHIFT|Sprint", "HOLD CTRL|Creep / slow walk",
            "E|Collect food / interact", "HOLD R|Crank the flashlight", "HOLD C|Crouch", "Y|Yell", "ESC|Pause / resume"
        });
        Group(page.transform, font, "TABLET", 70, 286, new[] {
            "TAB|Raise / stow tablet", "MOUSE|Move tablet cursor",
            "CLICK|Use scanner / power button", "SPACE|Scan while tablet is open",
            "25 METRES|Scanner range"
        });
        Group(page.transform, font, "MENUS", 70, -60, new[] {
            "ARROWS|Move between options", "ENTER / CLICK|Select", "ESC|Back / close the pause menu", "F11|Toggle fullscreen in game"
        });
        var button = new GameObject("Back", typeof(RectTransform), typeof(ControlsBackGlow), typeof(Button));
        button.transform.SetParent(page.transform, false);
        var br = (RectTransform)button.transform; br.anchoredPosition = new Vector2(-570, -409); br.sizeDelta = new Vector2(260, 58);
        back = button.GetComponent<Button>();
        var glow = button.GetComponent<ControlsBackGlow>();
        back.targetGraphic = glow; back.transition = Selectable.Transition.None;
        var label = Label(button.transform, font, "BACK", -105, 0, 210, 48, 20, new Color(.8f,.87f,.82f));
        label.alignment = TextAlignmentOptions.Center;
        glow.label = label;
        for (int i = page.transform.childCount - 1; i >= 0; i--)
        {
            var child = page.transform.GetChild(i);
            if (child != content.transform) child.SetParent(content.transform, false);
        }
        page.SetActive(false);
        return page;
    }

    static void Group(Transform parent, TMP_FontAsset font, string title, float x, float y, string[] rows)
    {
        Label(parent, font, title, x, y, 650, 32, 20, new Color(.43f,.7f,.66f));
        for(int i=0;i<rows.Length;i++)
        {
            var parts = rows[i].Split('|'); float rowY = y - 47 - i * 36;
            Label(parent, font, parts[0], x, rowY, 237, 35, 16, new Color(.8f,.86f,.83f));
            Label(parent, font, parts[1], x+250, rowY, 405, 35, 16, new Color(.59f,.67f,.65f));
        }
    }

    static TMP_Text Label(Transform parent, TMP_FontAsset font, string text, float x, float y, float width, float height, float size, Color color)
    {
        var go = new GameObject(text, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform; rect.pivot = new Vector2(0,.5f); rect.anchoredPosition = new Vector2(x,y); rect.sizeDelta = new Vector2(width,height);
        var label = go.GetComponent<TMP_Text>(); label.font=font; label.text=text; label.fontSize=size; label.color=color;
        label.alignment=TextAlignmentOptions.MidlineLeft; label.raycastTarget=false; label.textWrappingMode=TextWrappingModes.Normal;
        return label;
    }
}
