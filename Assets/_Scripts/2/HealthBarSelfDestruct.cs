using UnityEngine;

public class HealthBarSelfDestruct : MonoBehaviour
{
    private float _timer = 0f;

    public void OnUpdatePosition()
    {
        _timer = 0f; 
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > 0.2f) Destroy(gameObject);
    }
}