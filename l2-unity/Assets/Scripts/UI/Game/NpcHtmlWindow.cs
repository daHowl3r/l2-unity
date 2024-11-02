using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class NpcHtmlWindow : L2PopupWindow
{
    private VisualTreeAsset _l2Button;
    private ScrollView _content;
    private static NpcHtmlWindow _instance;
    public static NpcHtmlWindow Instance
    {
        get { return _instance; }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    protected override void LoadAssets()
    {
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/NpcHtmlWindow");
        _l2Button = LoadAsset("Data/UI/_Elements/Template/L2Button");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        var dragArea = GetElementByClass("drag-area");
        DragManipulator drag = new DragManipulator(dragArea, _windowEle);
        dragArea.AddManipulator(drag);

        RegisterCloseWindowEvent("btn-close-frame");
        RegisterClickWindowEvent(_windowEle, dragArea);
    }
    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        _windowEle.style.left = new Length(50, LengthUnit.Percent);
        _windowEle.style.top = new Length(50, LengthUnit.Percent);
        _windowEle.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent)));

        Label _windowName = (Label)GetElementById("windows-name-label");
        _windowName.text = "Chat";

        _content = _windowEle.Q<ScrollView>("HtmlContent");
        var _scroller = _content.verticalScroller;
        var highBtn = _scroller.Q<RepeatButton>("unity-high-button");
        var lowBtn = _scroller.Q<RepeatButton>("unity-low-button");

        highBtn.AddManipulator(new ButtonClickSoundManipulator(highBtn));
        lowBtn.AddManipulator(new ButtonClickSoundManipulator(lowBtn));

        HideWindow();


        //         RefreshContent(0, @"<html><body>Newbie Helper:<br>
        // Welcome to Einhovant's School of Wizardry. I will be teaching you the basics of combat.<br>
        // Please click on <font color=""LEVEL"">Quest</font>, in your Chat window.<br>
        // <a action=""bypass -h npc_%objectId%_Quest"">Quest</a><br>
        // </body></html>
        // ", 0);
        //         RefreshContent(0, @"<html><body>Newbie Helper:<br>
        // Welcome! Are you ready for a mission?<br>
        // Have you seen the gremlins around here? They've stolen the precious blue gemstone!<br>
        // <font color=""LEVEL"">You must recover it from them! </font><br>
        // I'll tell you again how to kill the gremlins. Place your cursor over a gremlin and click the <font color=""FF0000"">left button</font>. The cursor will change to a sword. Click the <font color=""FF0000"">F2 key</font> to attack with <font color=""LEVEL"">Wind Strike</font> magic.<br>
        // <img src=""L2UI_CH3.tutorial_img12"" width=64 height=64><table border=0><tr><td><img src=""L2UI_CH3.tutorial_img133"" width=64 height=64></td><td><img src=""L2UI_CH3.tutorial_img16"" width=64 height=64></td></tr></table><br>
        // Complete this mission and I'll reward you with useful items. Good luck!
        // </body></html>", 0);
    }

    public override void ShowWindow()
    {
        base.ShowWindow();
        AudioManager.Instance.PlayUISound("window_open");
        L2GameUI.Instance.WindowOpened(this);
    }

    public override void HideWindow()
    {
        base.HideWindow();
        AudioManager.Instance.PlayUISound("window_close");
        L2GameUI.Instance.WindowClosed(this);
    }

    public void RefreshContent(int npcId, string htmlString, int itemId)
    {
        ShowWindow();

        _content.Clear();

        string processedHtml = htmlString
            .Replace("<html>", "")
            .Replace("</html>", "")
            .Replace("<body>", "")
            .Replace("</body>", "")
            .Replace("%objectId%", npcId.ToString())
            .Replace("%item%", itemId.ToString());

        // Convert font color tags to Unity rich text color tags
        processedHtml = Regex.Replace(
            processedHtml,
            @"<font color=""(.*?)"">(.*?)</font>",
            match =>
            {
                string colorValue = match.Groups[1].Value;
                string content = match.Groups[2].Value;

                // Handle LEVEL keyword
                if (colorValue == "LEVEL")
                {
                    return $"<color=#FFC900>{content}</color>";
                }
                // Handle hex color codes
                else
                {
                    return $"<color=#{colorValue}>{content}</color>";
                }
            }
        );

        // Split the content by hyperlinks
        while (processedHtml.Length > 0)
        {
            int hyperlinkStart = processedHtml.IndexOf("<a ");
            if (hyperlinkStart != -1)
            {
                // Add text before hyperlink
                if (hyperlinkStart > 0)
                {
                    AddTextElement(processedHtml.Substring(0, hyperlinkStart));
                }

                // Process hyperlink
                int hyperlinkEnd = processedHtml.IndexOf("</a>", hyperlinkStart);
                if (hyperlinkEnd != -1)
                {
                    string linkTag = processedHtml.Substring(hyperlinkStart, hyperlinkEnd - hyperlinkStart);
                    // Extract the action and link text
                    Match actionMatch = Regex.Match(linkTag, @"action=""([^""]+)"".*?>(.*?)$");
                    if (actionMatch.Success)
                    {
                        string action = actionMatch.Groups[1].Value;
                        action = action.Replace("bypass", "").Replace("-h", "").Trim();

                        string linkText = actionMatch.Groups[2].Value;
                        AddHyperlinkElement(action, linkText);
                    }

                    processedHtml = processedHtml.Substring(hyperlinkEnd + 4);
                }
                else
                {
                    // Malformed hyperlink, treat rest as text
                    AddTextElement(processedHtml);
                    break;
                }
            }
            else
            {
                // No more hyperlinks, add remaining text
                AddTextElement(processedHtml);
                break;
            }
        }
    }

    private void AddTextElement(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var label = new Label { text = text };
        label.enableRichText = true;
        label.AddToClassList("l2-color-4");
        label.AddToClassList("html-window-label");
        _content.Add(label);
    }

    private void AddHyperlinkElement(string action, string text)
    {
        Button button = (Button)_l2Button.Instantiate()[0];
        button.AddToClassList("npc-html-window-button");
        VisualElement buttonLabel = button.Q<Label>("ButtonLabel");
        VisualElement buttonBg = button.Q<VisualElement>("ButtonBg");
        Label label = button.Q<Label>("ButtonLabel");
        label.text = text;

        button.style.marginTop = 15;

        button.clicked += () => ButtonClicked(action);

        _content.Add(button);
    }

    private void ButtonClicked(string action)
    {
        Debug.Log(action);
        GameClient.Instance.ClientPacketHandler.RequestBypassToServer(action);
    }
}
