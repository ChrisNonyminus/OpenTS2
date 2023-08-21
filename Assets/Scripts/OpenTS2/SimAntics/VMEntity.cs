﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMEntity
    {
        public ushort ID = 1;
        public short[] Temps = new short[20];
        public VMStack Stack = new VMStack();
        public void Tick()
        {
            Stack.Tick();
        }
    }
}
