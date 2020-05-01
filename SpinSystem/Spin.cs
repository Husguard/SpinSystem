using System;
using System.Collections.Generic;
using System.Text;

namespace SpinSystem
{
    public class Spin
    {
        bool spin;
        public Spin(int spin)
        {
            SetSpin(spin);
        }
        public int GetSpin()
        {
            if (spin == false) return -1;
            else return 1;
        }
        public void SetSpin(int spin)
        {
            if (spin == 0 || spin == -1) this.spin = false;
            else this.spin = true;
        }
    }
}
