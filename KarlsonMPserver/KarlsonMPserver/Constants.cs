using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPserver
{
    class Constants
    {
        // constants
        public const string version = "1.1.0";

        public static readonly string[] allowedSceneNames = new string[]
        {
            "0Tutorial",
            "1Sandbox0", "2Sandbox1", "3Sandbox2",
            "4Escape0", "5Escape1", "6Escape2", "7Escape3",
            "8Sky0", "9Sky1", "10Sky2"
        };
        public static readonly string[] sceneNames = new string[]
        {
            "Tutorial",
            "Sandbox 0", "Sandbox 1", "Sandbox 2",
            "Escape 0", "Escape 1", "Escape 2", "Escape 3",
            "Sky 0", "Sky 1", "Sky 2"
        };

        public static string FormatMiliseconds(int _miliseconds)
        {
            int _ms = _miliseconds % 1000;
            _miliseconds /= 1000;
            int _seconds = _miliseconds % 60;
            _miliseconds /= 60;
            int _minutes = _miliseconds % 60;
            return Twodigit(_minutes) + ":" + Twodigit(_seconds) + "." + Twodigit(_ms / 10);
        }
        private static string Twodigit(int i)
        {
            if (i < 10)
                return "0" + i;
            return "" + i;
        }
    }
}
