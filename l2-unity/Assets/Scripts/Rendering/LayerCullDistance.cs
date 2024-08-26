using UnityEngine;

[ExecuteInEditMode]
public class LayerCullDistance : MonoBehaviour
{
    [SerializeField] private float _decoCullDistance = 30f;
    [SerializeField] private float _lightCullDistance = 50f;
    void Start()
    {
        ApplyCulling(Camera.main);
    }

#if (UNITY_EDITOR)
    private void Update()
    {
        if (Camera.current != null)
        {
            ApplyCulling(Camera.current);
        }
    }
#endif

    private float[] ApplyCulling(Camera camera)
    {
        float[] distances = new float[32];
        distances[LayerMask.NameToLayer("Deco")] = _decoCullDistance;
        camera.layerCullDistances = distances;
        return distances;
    }
}
