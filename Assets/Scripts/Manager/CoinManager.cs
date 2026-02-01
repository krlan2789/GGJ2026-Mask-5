using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CoinManager : MonoBehaviour
{
    public event Action<int> OnCoinUpdated;

    private int _coinValue = 0;
    public int CoinValue
    {
        get
        {
            _coinValue = (byte)PlayerPrefs.GetInt("Coin", _coinValue);
            return _coinValue;
        }
        set
        {
            _coinValue = value;
            PlayerPrefs.SetInt("Coin", _coinValue);
        }
    }

    public void ResetScore()
    {
        _coinValue = 0;
        OnCoinUpdated?.Invoke(_coinValue);
    }

    public void IncreaseCoin()
    {
        _coinValue++;
        OnCoinUpdated?.Invoke(_coinValue);
    }
}
