using UnityEngine;

[System.Serializable]
public class Status
{
    [SerializeField] private int _hp;
    [SerializeField] private int _mp;
    public int Hp { get => _hp; set => _hp = value; }
    public int Mp { get => _mp; set => _mp = value; }

    public void UpdateStatus(Status status)
    {
        Hp = status.Hp;
        Mp = status.Mp;
    }
}