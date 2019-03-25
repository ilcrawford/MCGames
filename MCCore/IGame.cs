using System;
using System.Collections.Generic;
using System.Text;

namespace ILCrawford.MCGame.MCCore
{
    public interface IGame
    {
        void Run();
        string Name { get;}
    }
}
