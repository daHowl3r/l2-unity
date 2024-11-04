using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class StatusWindow : L2Window
{
    private Label _nameLabel;
    private Label _levelLabel;
    private Label _HPTextLabel;
    private Label _MPTextLabel;
    private Label _CPTextLabel;
    private Label _expTextLabel;
    private VisualElement _CPBar;
    private VisualElement _CPBarBG;
    private VisualElement _HPBar;
    private VisualElement _HPBarBG;
    private VisualElement _MPBar;
    private VisualElement _MPBarBG;
    private VisualElement _expBar;
    private VisualElement _expBarBG;
    private float _lastUpdateTime;

    [SerializeField] private float _statusWindowMinWidth = 175.0f;
    [SerializeField] private float _statusWindowMaxWidth = 400.0f;

    private static StatusWindow _instance;
    public static StatusWindow Instance { get { return _instance; } }

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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/StatusWindow");
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        var statusWindowDragArea = GetElementByClass("drag-area");
        DragManipulator drag = new DragManipulator(statusWindowDragArea, _windowEle);
        statusWindowDragArea.AddManipulator(drag);

        var horizontalResizeHandle = GetElementByClass("hor-resize-handle");
        HorizontalResizeManipulator horizontalResize = new HorizontalResizeManipulator(
            horizontalResizeHandle, _windowEle, _statusWindowMinWidth, _statusWindowMaxWidth);
        horizontalResizeHandle.AddManipulator(horizontalResize);

        _nameLabel = (Label)GetElementById("PlayerNameText");
        if (_nameLabel == null)
        {
            Debug.LogError("Status window PlayerNameText is null.");
        }

        _levelLabel = (Label)GetElementById("LevelText");
        if (_levelLabel == null)
        {
            Debug.LogError("Status window LevelText is null.");
        }

        _CPTextLabel = (Label)GetElementById("CPText");
        if (_CPTextLabel == null)
        {
            Debug.LogError("Status window CPText is null.");
        }

        _HPTextLabel = (Label)GetElementById("HPText");
        if (_HPTextLabel == null)
        {
            Debug.LogError("Status window Hp text is null.");
        }

        _MPTextLabel = (Label)GetElementById("MPText");
        if (_MPTextLabel == null)
        {
            Debug.LogError("Status window MPText is null.");
        }

        _expTextLabel = (Label)GetElementById("XPText");
        if (_expTextLabel == null)
        {
            Debug.LogError("Status window XPText is null.");
        }

        _CPBarBG = GetElementById("CPBarBG");
        if (_CPBarBG == null)
        {
            Debug.LogError("Status window CPBarBG is null");
        }

        _CPBar = GetElementById("CPBar");
        if (_CPBar == null)
        {
            Debug.LogError("Status window CPBar is null");
        }

        _HPBar = GetElementById("HPBar");
        if (_HPBar == null)
        {
            Debug.LogError("Status window HPBar is null");
        }

        _HPBarBG = GetElementById("HPBarBG");
        if (_HPBarBG == null)
        {
            Debug.LogError("Status window HPBarBG is null");
        }

        _MPBarBG = GetElementById("MPBarBG");
        if (_MPBarBG == null)
        {
            Debug.LogError("Status window MPBarBG is null");
        }

        _MPBar = GetElementById("MPBar");
        if (_MPBar == null)
        {
            Debug.LogError("Status windowar MPBar is null");
        }

        _expBarBG = GetElementById("XPBarBG");
        if (_expBarBG == null)
        {
            Debug.LogError("Status window XPBarBG is null");
        }

        _expBar = GetElementById("XPBar");
        if (_expBarBG == null)
        {
            Debug.LogError("Status windowar XPBar is null");
        }
    }

    void FixedUpdate()
    {
        if (Time.time - _lastUpdateTime < 0.5f)
        {
            return;
        }
        else
        {
            _lastUpdateTime = Time.time;
        }

        if (PlayerEntity.Instance == null)
        {
            return;
        }

        if (!(PlayerEntity.Instance.Status is PlayerStatus))
        {
            Debug.LogWarning("Player status is not of type playerstatus");
            return;
        }

        PlayerStatus status = (PlayerStatus)PlayerEntity.Instance.Status;
        PlayerStats stats = (PlayerStats)PlayerEntity.Instance.Stats;

        if (_levelLabel != null)
        {
            _levelLabel.text = stats.Level.ToString();
        }

        if (_nameLabel != null)
        {
            _nameLabel.text = PlayerEntity.Instance.Identity.Name;
        }

        if (_CPTextLabel != null)
        {
            _CPTextLabel.text = status.Cp + "/" + stats.MaxCp;
        }

        if (_HPTextLabel != null)
        {
            _HPTextLabel.text = status.Hp + "/" + stats.MaxHp;
        }

        if (_MPTextLabel != null)
        {
            _MPTextLabel.text = status.Mp + "/" + stats.MaxMp;
        }

        if (_expTextLabel != null && stats.ExpPercent > 0)
        {
            _expTextLabel.text = $"{(stats.ExpPercent * 100f).ToString("0.00")}%";
        }
        else
        {
            _expTextLabel.text = $"00.00%";
        }

        if (_CPBarBG != null && _CPBar != null)
        {
            float cpRatio = Mathf.Min(1, (float)status.Cp / stats.MaxCp);
            float bgWidth = _CPBarBG.resolvedStyle.width;
            float barWidth = bgWidth * cpRatio;
            _CPBar.style.width = barWidth;
        }

        if (_HPBarBG != null && _HPBar != null)
        {
            float hpRatio = Mathf.Min(1, (float)status.Hp / stats.MaxHp);
            float bgWidth = _HPBarBG.resolvedStyle.width;
            float barWidth = bgWidth * hpRatio;
            _HPBar.style.width = barWidth;
        }

        if (_MPBarBG != null && _MPBar != null)
        {
            float mpRatio = Mathf.Min(1, (float)status.Mp / stats.MaxMp);
            float bgWidth = _MPBarBG.resolvedStyle.width;
            float barWidth = bgWidth * mpRatio;
            _MPBar.style.width = barWidth;
        }

        if (_expBarBG != null && _expBar != null)
        {
            float bgWidth = _expBarBG.resolvedStyle.width;
            float expRatio = Mathf.Min(1, stats.ExpPercent);
            float barWidth = bgWidth * expRatio;
            _expBar.style.width = barWidth;
        }
    }

    public override void ShowWindow()
    {
        base.ShowWindow();
        AudioManager.Instance.PlayUISound("window_open");
    }

    public override void HideWindow()
    {
        base.HideWindow();
        AudioManager.Instance.PlayUISound("window_close");
    }
}
