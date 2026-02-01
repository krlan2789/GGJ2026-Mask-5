using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class CollectableObject : MonoBehaviour
{
    //[SerializeField] private GameObject _obj;
    private CoinManager _coinManager;
    private bool _collected = false;

    private void Awake()
    {
        _coinManager = FindFirstObjectByType<CoinManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(ConstantHelper.Tags.PLAYER) && !_collected)
        {
            StartCoroutine(Collected());
        }
    }

    private IEnumerator Collected()
    {
        _collected = true;
        _coinManager.IncreaseCoin();
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
}
