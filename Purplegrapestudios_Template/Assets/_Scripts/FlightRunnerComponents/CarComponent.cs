using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


public class CarComponent : MonoBehaviour
{
    public struct CarComponentData
    {
        private int _PlayerID;
        private string _PlayerName;
        private int _Health;

        public int PlayerID
        {
            get { return _PlayerID; }
            set { _PlayerID = value; }
        }

        public string PlayerName
        {
            get { return _PlayerName; }
            set { _PlayerName = value; }
        }

        public int Health
        {
            get { return _Health; }
            set { _Health = value; }
        }

    }

    public CarComponentData instance;

    private void Awake()
    {
        instance.Health = 99;
    }

}
