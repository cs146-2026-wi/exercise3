using UnityEngine;

public class TilemapFollowCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector3 offset = Vector3.zero;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            transform.position = targetCamera.transform.position + offset;
        }
    }
}
