using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RestartLocationWindow : L2PopupWindow
{
    private Button _toVillageButton;
    private Button _toClanHallButton;
    private Button _toCastleButton;
    private Button _toSiegeButton;
    private Button _fixedResButton;

    private static RestartLocationWindow _instance;
    public static RestartLocationWindow Instance { get { return _instance; } }

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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/RestartLocationWindow");
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        _toVillageButton = _windowEle.Q<Button>("ToVillageButton");
        _toVillageButton.AddManipulator(new ButtonClickSoundManipulator(_toVillageButton));
        _toVillageButton.RegisterCallback<ClickEvent>((evt) => ButtonClicked(0));
        _toClanHallButton = _windowEle.Q<Button>("ToClanHallButton");
        _toClanHallButton.AddManipulator(new ButtonClickSoundManipulator(_toClanHallButton));
        _toClanHallButton.RegisterCallback<ClickEvent>((evt) => ButtonClicked(1));
        _toCastleButton = _windowEle.Q<Button>("ToCastleButton");
        _toCastleButton.AddManipulator(new ButtonClickSoundManipulator(_toCastleButton));
        _toCastleButton.RegisterCallback<ClickEvent>((evt) => ButtonClicked(2));
        _toSiegeButton = _windowEle.Q<Button>("ToSiegeButton");
        _toSiegeButton.AddManipulator(new ButtonClickSoundManipulator(_toSiegeButton));
        _toSiegeButton.RegisterCallback<ClickEvent>((evt) => ButtonClicked(3));
        _fixedResButton = _windowEle.Q<Button>("FixedResButton");
        _fixedResButton.AddManipulator(new ButtonClickSoundManipulator(_fixedResButton));
        _fixedResButton.RegisterCallback<ClickEvent>((evt) => ButtonClicked(4));

        root.Add(_windowEle);

        yield return new WaitForEndOfFrame();

        CenterWindow();

        HideWindow();

        var dragAreaEle = _windowEle.Q<VisualElement>(null, "drag-area");
        DragManipulator drag = new DragManipulator(dragAreaEle, _windowEle);
        dragAreaEle.AddManipulator(drag);
    }

    public Vector2 GetWindowPosition()
    {
        return _windowEle.worldBound.position;
    }

    private void ButtonClicked(int restartPoint)
    {
        Debug.Log($"Restart point clicked: {restartPoint}.");
        HideWindow();
        GameClient.Instance.ClientPacketHandler.SendRequestRestartPoint(restartPoint);
    }

    public void ShowWindowWithParams(bool toVillageAllowed, bool toClanHallAllowed, bool toCastleAllowed, bool toSiegeHQAllowed, bool fixedResAllowed)
    {
        Debug.LogWarning(toVillageAllowed);
        Debug.LogWarning(toClanHallAllowed);
        Debug.LogWarning(toCastleAllowed);
        Debug.LogWarning(toSiegeHQAllowed);
        Debug.LogWarning(fixedResAllowed);
        _toVillageButton.style.display = toVillageAllowed ? DisplayStyle.Flex : DisplayStyle.None;
        _toClanHallButton.style.display = toClanHallAllowed ? DisplayStyle.Flex : DisplayStyle.None;
        _toCastleButton.style.display = toCastleAllowed ? DisplayStyle.Flex : DisplayStyle.None;
        _toSiegeButton.style.display = toSiegeHQAllowed ? DisplayStyle.Flex : DisplayStyle.None;
        _fixedResButton.style.display = fixedResAllowed ? DisplayStyle.Flex : DisplayStyle.None;
        ShowWindow();
    }
}