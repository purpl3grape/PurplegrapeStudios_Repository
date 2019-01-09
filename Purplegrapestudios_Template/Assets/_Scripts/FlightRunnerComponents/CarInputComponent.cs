using UnityEngine;

public class CarInputComponent : MonoBehaviour
{
    public struct CarInputData
    {
        private float _Horizontal;
        private float _Vertical;
        private float _TurretRotY;
        private float _Acceleration;
        private int _Gear;
        private bool _Reverse;
        private float _Break;
        private bool _Fire;

        public float Horizontal
        {
            get { return _Horizontal; }
            set { _Horizontal = value; }
        }

        public float Vertical
        {
            get { return _Vertical; }
            set { _Vertical = value; }
        }
        public float TurretRotY
        {
            get { return _TurretRotY; }
            set { _TurretRotY = value; }
        }
        public float Acceleration
        {
            get { return _Acceleration; }
            set { _Acceleration = value; }
        }
        public int Gear {
            get { return _Gear; }
            set { _Gear = value; }
        }
        public bool Reverse
        {
            get { return _Reverse; }
            set { _Reverse = value; }
        }
        public float Break
        {
            get { return _Break; }
            set { _Break = value; }
        }
        public bool Fire
        {
            get { return _Fire; }
            set { _Fire = value; }
        }

    }

    public CarInputData instance;

}
