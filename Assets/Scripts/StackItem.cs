using UnityEngine;

public class StackItem : MonoBehaviour
{
    public Product product;

    Vector3 _targetLocalPos;
    bool _moving;

    // Makes a new item at a world position, shaped according to its product
    public static StackItem Create(Product product, Vector3 worldPosition)
    {
        var go = new GameObject(product.name);
        go.transform.position = worldPosition;
        Transform t = go.transform;

        switch (product.shape)
        {
            case ProductShape.Bottle:
                Look.Part(t, PrimitiveType.Cylinder, product.color, Vector3.zero, new Vector3(0.22f, 0.16f, 0.22f));
                Look.Part(t, PrimitiveType.Cylinder, product.color, new Vector3(0f, 0.21f, 0f), new Vector3(0.1f, 0.06f, 0.1f));
                Look.Part(t, PrimitiveType.Sphere, Color.white, new Vector3(0f, 0.3f, 0f), Vector3.one * 0.1f);
                break;

            case ProductShape.Egg:
                Look.Part(t, PrimitiveType.Sphere, product.color, Vector3.zero, new Vector3(0.24f, 0.32f, 0.24f));
                break;

            case ProductShape.Carton:
                Look.Part(t, PrimitiveType.Cube, product.color, Vector3.zero, new Vector3(0.26f, 0.34f, 0.2f));
                Look.Part(t, PrimitiveType.Cube, new Color(0.3f, 0.55f, 0.95f), new Vector3(0f, 0.2f, 0f), new Vector3(0.26f, 0.08f, 0.2f));
                break;

            case ProductShape.Box:
                Look.Part(t, PrimitiveType.Cube, product.color, Vector3.zero, new Vector3(0.4f, 0.26f, 0.3f));
                break;

            default:
                Look.Part(t, PrimitiveType.Sphere, product.color, Vector3.zero, Vector3.one * 0.36f);
                break;
        }

        var item = go.AddComponent<StackItem>();
        item.product = product;
        return item;
    }

    // Re-parents the item and slides it to a spot relative to the new parent
    public void MoveTo(Transform parent, Vector3 localPos)
    {
        transform.SetParent(parent, true);
        _targetLocalPos = localPos;
        _moving = true;
    }

    void Update()
    {
        if (!_moving) return;

        float t = 1f - Mathf.Exp(-14f * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetLocalPos, t);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, t);

        if ((transform.localPosition - _targetLocalPos).sqrMagnitude < 0.0001f)
        {
            transform.localPosition = _targetLocalPos;
            transform.localRotation = Quaternion.identity;
            _moving = false;
        }
    }
}