using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;
    private static readonly Vector3 SCALED_SIZE = new Vector3(0.7f, 0.7f, 0.7f);

    private void Update()
    {
        transform.localScale = Input.GetMouseButtonDown(0) ? SCALED_SIZE : Vector3.one;
    }
}