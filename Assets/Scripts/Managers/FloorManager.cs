using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [SerializeField] private GameObject doorEnter;
    [SerializeField] private GameObject doorExit;

    public bool IsExit { private set; get; } = false;

    public void SetDoorAsExit(bool isExit)
    {
        IsExit = isExit;
        if (doorEnter != null)
        {
            doorEnter.SetActive(!isExit);
        }
        if (doorExit != null)
        {
            doorExit.SetActive(isExit);
        }
    }
}
