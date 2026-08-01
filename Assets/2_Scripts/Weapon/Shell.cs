using System.Collections;
using UnityEngine;

public abstract class Shell : MonoBehaviour
{
    protected ShellData _shellData;   
    private Vector3 _impactPoint;
    private IEnumerator _shellFireRoutine;
    
    public abstract void OnHit();

    public void OnStartFire(ShellData data, Vector3 start, Vector3 end)
    {
        _shellData = data;
        
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
                _shellData.Velocity * Time.deltaTime);
            
            yield return new WaitForEndOfFrame();
        }
        
        OnHit();
    }

    private void OnTriggerEnter(Collider other)
    {
    }
}
