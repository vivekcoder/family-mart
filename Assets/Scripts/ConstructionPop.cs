using UnityEngine;

public class ConstructionPop : MonoBehaviour
{
    const float Duration = 1.2f;

    Vector3 _targetScale;
    Renderer _renderer;
    Material _finalMaterial;
    bool _highlight;
    bool _bounce;
    float _age;

    // highlight: show the ground lit up first. puffs: dust. bounce: overshoot like a cartoon pop.
    public static void Play(GameObject target, bool highlight = false, bool puffs = true, bool bounce = true)
    {
        var pop = target.AddComponent<ConstructionPop>();
        pop._targetScale = target.transform.localScale;
        pop._bounce = bounce;
        target.transform.localScale = pop._targetScale * 0.02f;

        if (highlight)
        {
            pop._renderer = target.GetComponent<Renderer>();
            if (pop._renderer != null)
            {
                pop._finalMaterial = pop._renderer.sharedMaterial;
                pop._renderer.sharedMaterial = Look.Mat(new Color(1f, 0.95f, 0.55f));
                pop._highlight = true;
            }
        }

        if (puffs) DustPuff.Burst(target.transform.position);
    }

    void Update()
    {
        _age += Time.deltaTime;
        float k = Mathf.Clamp01(_age / Duration);
        transform.localScale = _targetScale * Mathf.Max(0.02f, Ease(k, _bounce));

        // The lit-up ground settles into its real material
        if (_highlight && k > 0.55f)
        {
            _renderer.sharedMaterial = _finalMaterial;
            _highlight = false;
        }

        if (k >= 1f)
        {
            transform.localScale = _targetScale;
            Destroy(this);
        }
    }

    static float Ease(float k, bool bounce)
    {
        if (!bounce) return 1f - Mathf.Pow(1f - k, 3f);

        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float x = k - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }
}