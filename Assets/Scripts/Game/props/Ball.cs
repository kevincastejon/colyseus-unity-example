using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private LTDescr moveTween;
    private LTDescr rotTween;
    private float lastPosUpdate;
    private float lastRotUpdate;
    private Transform originalParent;
    private void Start()
    {
        originalParent = transform.parent;
    }
    public void SetOwner(GameObject owner)
    {
        if (owner != null)
        {
            if (moveTween != null)
            {
                LeanTween.cancel(moveTween.id);
            }
            if (rotTween != null)
            {
                LeanTween.cancel(rotTween.id);
            }
            transform.SetParent(owner.transform);
            transform.position = Vector3.zero;
            transform.localPosition = new Vector3(0, 0.8f, 0.5f);
        }
        else
        {
            transform.SetParent(originalParent);
        }
    }
    public void SetPositionTarget(Vector3 posTarget)
    {
        if (!IsOwned())
        {
            if (moveTween != null)
            {
                LeanTween.cancel(moveTween.id);
            }
            moveTween = LeanTween.move(gameObject, posTarget, lastPosUpdate);
            moveTween.setOnComplete(() => moveTween = null);
            lastPosUpdate = 0;
        }
    }
    public void SetRotationTarget(Quaternion rotTarget)
    {
        if (!IsOwned())
        {
            if (rotTween != null)
            {
                LeanTween.cancel(rotTween.id);
            }
            rotTween = LeanTween.rotate(gameObject, rotTarget.eulerAngles, lastRotUpdate);
            rotTween.setOnComplete(() => rotTween = null);
            lastRotUpdate = 0;
        }
    }
    void Update()
    {
        lastPosUpdate += Time.deltaTime;
        lastRotUpdate += Time.deltaTime;
    }
    private bool IsOwned()
    {
        return (transform.parent != originalParent);
    }
}
