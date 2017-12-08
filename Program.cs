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
            Reader pcfReader = new Reader("asw_environmental_fx.pcf");
            PCF pcf = pcfReader.Read();
            //pcf.GetParticleNames();

            List<string> materialNames = pcf.GetMaterialNames();

            foreach (string material in materialNames)
            {
                Console.WriteLine("Material: " + material);
            }

            List<string> modelNames = pcf.GetModelNames();

            foreach (string model in modelNames)
            {
                Console.WriteLine("Model: " + model);
            }
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
