using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PCFReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Reader pcfReader = new Reader("butterflies.pcf");
            PCF pcf = pcfReader.Read();
            pcf.GetParticleNames();
            
            Console.ReadLine();
        }
    }

    class Utils
    {
        public static void Exit()
        {
            Console.ReadLine();
            Environment.Exit(-1);
        }
    }

}
