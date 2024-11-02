using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Nameplate
{

    private VisualElement _nameplateEle;
    private VisualElement _leftBubbleEle;
    private VisualElement _rightBubbleEle;
    private Label _nameplateEntityName;
    private Label _nameplateEntityTitle;

    [SerializeField] private float _nameplateOffsetHeight;
    [SerializeField] private Transform _target;
    [SerializeField] private bool _visible;
    [SerializeField] private Entity _entity;

    public VisualElement NameplateEle { get { return _nameplateEle; } set { _nameplateEle = value; } }
    public bool Visible { get { return _visible; } set { _visible = value; } }
    public Transform Target { get { return _target; } }
    public float NameplateOffsetHeight { get { return _nameplateOffsetHeight; } set { _nameplateOffsetHeight = value; } }
    public Entity Entity { get { return _entity; } }

    public Nameplate(
        VisualElement visualElement, Label entityName, Label entityTitle, Transform target, Entity entity,
        string title, string titleColor, float nameplateHeight, string name)
    {
        _nameplateEle = visualElement;
        _nameplateEntityName = entityName;
        _nameplateEntityTitle = entityTitle;
        _target = target;
        _nameplateOffsetHeight = nameplateHeight;
        _visible = true;
        _entity = entity;

        _nameplateEntityName.text = name;
        _nameplateEntityTitle.text = title;
        _nameplateEntityTitle.style.color = StringUtils.HexToColor(titleColor);
    }

    public Nameplate(VisualElement visualElement, Label entityName, Label entityTitle, Entity entity)
    {
        _nameplateEle = visualElement;
        _nameplateEntityName = entityName;
        _nameplateEntityTitle = entityTitle;
        _target = entity.transform;
        _entity = entity;
        _nameplateEntityName.text = entity.Identity.Name;
        _nameplateEntityTitle.text = entity.Identity.Title;
        _nameplateEntityTitle.style.color = StringUtils.HexToColor(entity.Identity.TitleColor);
        _nameplateOffsetHeight = entity.Appearance.CollisionHeight * 2.1f;
        _visible = true;
    }

    public void SetStyle(string className)
    {
        if (_leftBubbleEle != null)
        {
            SetClassName(_leftBubbleEle, className);
        }
        else
        {
            _leftBubbleEle = _nameplateEle.Q<VisualElement>("TargetBubbleLeft");
        }
        if (_rightBubbleEle != null)
        {
            SetClassName(_rightBubbleEle, className);
        }
        else
        {
            _rightBubbleEle = _nameplateEle.Q<VisualElement>("TargetBubbleRight");
        }
    }

    public void RemoveStyle(string className)
    {
        if (_leftBubbleEle != null)
        {
            RemoveClassName(_leftBubbleEle, className);
        }
        else
        {
            _leftBubbleEle = _nameplateEle.Q<VisualElement>("TargetBubbleLeft");
        }
        if (_rightBubbleEle != null)
        {
            RemoveClassName(_rightBubbleEle, className);
        }
        else
        {
            _rightBubbleEle = _nameplateEle.Q<VisualElement>("TargetBubbleRight");
        }
    }

    private void SetClassName(VisualElement element, string className)
    {
        if (!element.ClassListContains(className))
        {
            element.AddToClassList(className);
        }
    }

    private void RemoveClassName(VisualElement element, string className)
    {
        if (element.ClassListContains(className))
        {
            element.RemoveFromClassList(className);
        }
    }
}
