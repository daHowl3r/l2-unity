using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private GameObject _locator;
    [SerializeField] private GameObject _locatorBaseEffect;
    [SerializeField] private GameObject _locatorReachedEffect;
    [SerializeField] private ObjectData _targetObjectData;
    [SerializeField] private ObjectData _hoverObjectData;

    public ObjectData HoverObjectData { get { return _hoverObjectData; } }

    private Vector3 _lastClickPosition = Vector3.zero;
    [SerializeField] private LayerMask _entityMask;
    [SerializeField] private LayerMask _clickThroughMask;

    private static ClickManager _instance;
    public static ClickManager Instance { get { return _instance; } }

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

    void OnDestroy()
    {
        _instance = null;
    }

    void Start()
    {
        _locator = GameObject.Find("Locator");
        _locatorBaseEffect = _locator.transform.GetChild(0).gameObject;
        _locatorReachedEffect = _locator.transform.GetChild(1).gameObject;

        HideLocator(false);
    }

    public void SetMasks(LayerMask entityMask, LayerMask clickThroughMask)
    {
        _entityMask = entityMask;
        _clickThroughMask = clickThroughMask;
    }

    void Update()
    {
        if (L2GameUI.Instance.MouseOverUI)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, ~_clickThroughMask))
        {
            int hitLayer = hit.collider.gameObject.layer;
            if (_entityMask == (_entityMask | (1 << hitLayer)))
            {
                _hoverObjectData = new ObjectData(hit.transform.parent.gameObject);
            }
            else
            {
                _hoverObjectData = new ObjectData(hit.collider.gameObject);
            }

            if (InputManager.Instance.LeftClickDown &&
                !InputManager.Instance.RightClickHeld)
            {
                _targetObjectData = _hoverObjectData;

                if (_entityMask == (_entityMask | (1 << hitLayer)) && _targetObjectData.ObjectTag != "Player")
                {
                    OnClickOnEntity();
                }
                else if (_targetObjectData != null)
                {
                    OnClickToMove(hit);
                }
            }
        }
    }

    public void OnClickToMove(RaycastHit hit)
    {
        _lastClickPosition = hit.point;
        //  PlayerCombatController.Instance.RunningToTarget = false;

        if (PlayerStateMachine.Instance != null)
        {
            PlayerStateMachine.Instance.ChangeIntention(Intention.INTENTION_MOVE_TO, _lastClickPosition);
        }

        if (TargetManager.Instance != null)
        {
            TargetManager.Instance.ClearAttackTarget();
        }

        //  PathFinderController.Instance.MoveTo(_lastClickPosition);
        float angle = Vector3.Angle(hit.normal, Vector3.up);
        if (angle < 85f)
        {
            PlaceLocator(_lastClickPosition, hit.normal);
        }
        else
        {
            HideLocator(false);
        }
    }

    public void OnClickOnEntity()
    {
        Debug.Log("Click on entity");
        TargetManager.Instance.SetTarget(_targetObjectData);
    }

    public void PlaceLocator(Vector3 position, Vector3 normal)
    {
        _locator.gameObject.transform.position = position;
        _locatorBaseEffect.GetComponent<ParticleTimerResetGroup>().SurfaceNormal = normal;
        _locatorReachedEffect.SetActive(false);
        _locatorBaseEffect.SetActive(false);
        _locatorBaseEffect.SetActive(true);
    }

    public void HideLocator(bool targetReached)
    {
        if (targetReached)
        {
            Vector3 normal = _locatorBaseEffect.GetComponent<ParticleTimerResetGroup>().SurfaceNormal;
            _locatorReachedEffect.GetComponent<ParticleTimerResetGroup>().SurfaceNormal = normal;
            _locatorReachedEffect.SetActive(true);
        }
        else
        {
            _locatorReachedEffect.SetActive(false);
        }

        _locatorBaseEffect.SetActive(false);
    }
}
