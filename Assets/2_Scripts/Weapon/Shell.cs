using System.Collections;
using UnityEngine;

public enum ShellType
{
    HE,
    AP
}

public abstract class Shell : MonoBehaviour
{
    [SerializeField] protected float velocity;
    
    private Vector3 _impactPoint;
    private IEnumerator _shellFireRoutine;
    
    public abstract void OnHit();

    public void OnStartFire(Vector3 start, Vector3 end)
    {
        transform.position = start;
        _impactPoint = end;
        _shellFireRoutine = OnFire();
        
        StartCoroutine(_shellFireRoutine);
    }

    public void OnEndFireRoutine()
    {
        if(_shellFireRoutine != null)
            StopCoroutine(_shellFireRoutine);
    }

    private IEnumerator OnFire()
    {
        while (Vector3.Distance(transform.position, _impactPoint) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _impactPoint, 
                velocity * Time.deltaTime);
            
            yield return new WaitForEndOfFrame();
        }
        
        OnHit();
    }

    private void OnTriggerEnter(Collider other)
    {
    }
}
