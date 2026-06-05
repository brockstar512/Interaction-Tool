using DG.Tweening;
using UnityEngine;

public class MoveConditionResponse : MonoBehaviour, IConditionResponse
{
    [SerializeField] private Transform target;           // defaults to this transform
    [SerializeField] private Vector3 metOffset = Vector3.up;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    private Vector3 _rest;
    private Tween _tween;

    private void Awake()
    {
        if (target == null) target = transform;
        _rest = target.localPosition;
    }

    public void OnConditionChanged(bool met)
    {
        _tween?.Kill();
        _tween = target.DOLocalMove(met ? _rest + metOffset : _rest, duration)
            .SetEase(ease).SetLink(gameObject);
    }
}