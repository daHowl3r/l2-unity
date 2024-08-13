using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class SysMenu : L2Window
{

    private static SysMenu _instance;
    private float _height = 0;
    private bool _isShow;
    public static SysMenu Instance
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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/SysMenu/SystemMenu");
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        root.Add(_windowEle);

        yield return new WaitForEndOfFrame();
        _height = _windowEle.worldBound.height;
        VisualElement exit_row = GetElementByClass("exit_row");
       // OverTest(exit_row);
        //_mouseOverDetection = new MouseOverDetectionManipulator(_windowEle);
        //_windowEle.AddManipulator(_mouseOverDetection);
        HideWindow();
    }

    private void OverTest(VisualElement exit_row)
    {
        exit_row.RegisterCallback<MouseOverEvent>(evt =>
        {
            VisualElement ve = (VisualElement)evt.currentTarget;
            if (ve != null)
            {
                ve.style.backgroundColor = Color.red;
            }
        }, TrickleDown.TrickleDown);
    }

    public void Show(Vector2 position)
    {
        if (!_isShow)
        {
            _isShow = true;
            _windowEle.transform.position = new Vector2(position.x, position.y - _height);
            ShowWindow();
        }
        else
        {
            _isShow = false;
            _windowEle.transform.position = new Vector2(position.x, position.y - _height);
            HideWindow();
        }
       
    }

}
