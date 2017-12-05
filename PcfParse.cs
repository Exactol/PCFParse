using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCFReader
{
    public class Reader
    {
        private BinaryReader _binaryReader;
        public string file;
        public PCF pcf;

        public Reader(string targetFile)
        {
            file = targetFile;
        }

        public PCF Read()
        {
            FileStream pcf = null;
            try
            {
                pcf = new FileStream(file, FileMode.Open);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Error!: " + e.Message);
                Utils.Exit();
            }

            _binaryReader = new BinaryReader(pcf);

            this.pcf = new PCF();

            ReadHeader();
            ReadStringDict();
            ReadElementDict();

            return this.pcf;
        }

        private void ReadHeader()
        {
            Console.WriteLine("-----Header-----");

            //Read magic string identifier
            string magicString = "";
            for (int i = 0; i < 43; i++)
            {
                magicString += _binaryReader.ReadChar();
            }

            Console.WriteLine("Magic String: " + magicString);

            //Throw away unneccesary info
            magicString = magicString.Replace("<!-- dmx encoding binary ", "");
            magicString = magicString.Replace(" -->", "");

            //Extract info from magic string
            string[] magicSplit = magicString.Split(' ');

            pcf.MagicString = magicString;
            pcf.PcfFormat = magicSplit[2];

            Int32.TryParse(magicSplit[0], out pcf.BinaryVersion); //Store pcf binaryVersion
            Int32.TryParse(magicSplit[3], out pcf.PcfVersion);

            Console.WriteLine($"PCF Binary Version: {pcf.BinaryVersion}");
            Console.WriteLine($"PCF Format: {pcf.PcfFormat}");
            Console.WriteLine($"PCF Version: {pcf.PcfVersion}");
        }


        private void ReadStringDict()
        {
            Console.WriteLine("\n-----String Dictionary-----");
            //Get past some padding?
            _binaryReader.ReadInt16();

            //Different versions have different string dict sizes
            if (pcf.BinaryVersion != 4)
            {
                pcf.NumDictStrings = _binaryReader.ReadInt16(); //Read as short
            }
            else
            {
                pcf.NumDictStrings = _binaryReader.ReadInt32(); //Read as int
            }
            Console.WriteLine($"Number of strings: {pcf.NumDictStrings}");

            //Get numDictStrings number of strings
            for (int i = 0; i < pcf.NumDictStrings; i++)
            {
                //Strings are null terminated
                pcf.StringDict.Add(ReadNullTerminatedString());
            }
        }

        private void ReadElementDict()
        {
            Console.WriteLine("\n-----Element Dictionary-----");
            pcf.NumElements = _binaryReader.ReadInt32();
            
            //Read the elements
            Console.WriteLine($"Number Of Elements: {pcf.NumElements}");
            for (int i = 0; i < pcf.NumElements; i++)
            {
                if (pcf.BinaryVersion != 4)
                {
                    DmxElement element;
                    //Get name index and string from string dict
                    element.typeNameIndex = _binaryReader.ReadUInt16();
                    element.typeName = pcf.StringDict[element.typeNameIndex];

                    //Get Element name (null terminated)
                    element.elementName = ReadNullTerminatedString();

                    //Get 16 bit unsigned char array 
                    List<byte> charBuf = new List<byte>();
                    for (int z = 0; z < 16; z++)
                    {
                        charBuf.Add(_binaryReader.ReadByte());
                    }
                    element.dataSignature = charBuf.ToArray();

                    pcf.Elements.Add(element);
                }
                else
                {
                    DmxElementV4 element;
                    //Get name index and string from string dict
                    element.typeNameIndex = _binaryReader.ReadUInt16();
                    element.typeName = pcf.StringDict[element.typeNameIndex];

                    //Get element index
                    element.elementNameIndex = _binaryReader.ReadUInt16();
                    element.elementName = pcf.StringDict[element.elementNameIndex];

                    //Get 16 bit unsigned char array 
                    List<byte> byteBuf = new List<byte>();
                    for (int z = 0; z < 16; z++)
                    {
                        byteBuf.Add(_binaryReader.ReadByte());
                    }
                    element.dataSignature = byteBuf.ToArray();

                    pcf.ElementsV4.Add(element);
                }
            }
        }

        private string ReadNullTerminatedString()
        {
            string tempString = "";
            char charVal;

            //Read until char is 0, which is null terminated
            while ((int)(charVal = _binaryReader.ReadChar()) != 0)
            {
                tempString += charVal;
            }
            return tempString;
        }
    }
}
