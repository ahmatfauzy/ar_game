using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
}