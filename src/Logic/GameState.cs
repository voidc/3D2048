﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D2048.Logic
{
    class GameState
    {
        public int [, ,] field = new int[4, 4, 4];
        public bool lost = false;
        public bool won = true;

     
    }
}