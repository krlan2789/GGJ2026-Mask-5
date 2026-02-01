using UnityEngine;

public class Viewport : MonoBehaviour
{
    private static Viewport _instance;
    public static Viewport Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<Viewport>();
            }
            if (_instance == null)
            {
                _instance = new GameObject("Viewport").AddComponent<Viewport>();
            }
            return _instance;
        }
    }

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public Rect GetArea()
    {
        if (_camera == null)
        {
            return new Rect(0, 0, 0, 0);
        }

        float height = 2f * _camera.orthographicSize;
        float width = height * _camera.aspect;
        return new Rect(_camera.transform.position.x - width / 2, _camera.transform.position.y - height / 2, width, height);
    }
}