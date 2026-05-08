using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 270f;

    private void Update()
    {
        transform.Rotate(0f, 0f, -degreesPerSecond * Time.deltaTime);
    }
}