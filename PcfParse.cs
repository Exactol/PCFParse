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

        //Attribute types
        private const int ELEMENT =  1;
        private const int INTEGER = 2;
        private const int FLOAT = 3;
        private const int BOOLEAN = 4;
        private const int STRING = 5;
        private const int BINARY = 6;
        private const int TIME = 7;
        private const int COLOR = 8;
        private const int VECTOR2 = 9;
        private const int VECTOR3 = 10;
        private const int VECTOR4 = 11;
        private const int QANGLE = 12;
        private const int QUATERNION = 13;
        private const int MATRIX = 14;

        private const int ELEMENT_ARRAY = 15;
        private const int INTEGER_ARRAY = 16;
        private const int FLOAT_ARRAY = 17;
        private const int BOOLEAN_ARRAY = 18;
        private const int STRING_ARRAY = 19;
        private const int BINARY_ARRAY = 20;
        private const int TIME_ARRAY = 21;
        private const int COLOR_ARRAY = 22;
        private const int VECTOR2_ARRAY = 23;
        private const int VECTOR3_ARRAY = 24;
        private const int VECTOR4_ARRAY = 25;
        private const int QANGLE_ARRAY = 26;
        private const int QUATERNION_ARRAY = 27;
        private const int MATRIX_ARRAY = 28;

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
            ReadData();

            _binaryReader.Close();

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
                    List<byte> byteBuf = new List<byte>();
                    for (int z = 0; z < 16; z++)
                    {
                        byteBuf.Add(_binaryReader.ReadByte());
                    }
                    element.dataSignature = byteBuf.ToArray();
                    byteBuf.Clear();

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
                    byteBuf.Clear();

                    pcf.ElementsV4.Add(element);
                }
            }
        }

        private void ReadData()
        {
            Console.WriteLine("\n-----Attributes-----");
            Console.WriteLine(_binaryReader.BaseStream.Position);
            for (int i = 0; i < pcf.NumElements; i++)
            {
                //Get number of element attribs
                int numElementAttribs = _binaryReader.ReadInt32();
                for (int w = 0; w < numElementAttribs; w++)
                {
                    Console.WriteLine($"\n-----Index: {w}-----");
                    ushort typeNameIndex = _binaryReader.ReadUInt16();
                    int attributeType = _binaryReader.ReadByte();

                    Console.WriteLine($"Number Of Element Attributes: {numElementAttribs}");
                    Console.WriteLine($"TypeName Index: {typeNameIndex}");
                    Console.WriteLine($"TypeName: {pcf.StringDict[typeNameIndex]}");
                    Console.WriteLine($"Attribute Type: {attributeType}");
                    ReadAttrib(attributeType);
                    
                    Console.WriteLine("--------------------");
                }
            }
        }

        private void ReadAttrib(int attributeType)
        {
            switch (attributeType)
            {
                case (ELEMENT):
                    Console.WriteLine("Attribute Type: Element");
                    int attribElement = _binaryReader.ReadInt32();
                    Console.WriteLine($"Offset into element array: {attribElement}");
                    break;

                case (INTEGER):
                    Console.WriteLine("Attribute Type: Integer");
                    int attribInt = _binaryReader.ReadInt32();
                    Console.WriteLine($"Attribute Int Value: {attribInt}");
                    break;

                case (FLOAT):
                    Console.WriteLine("Attribute Type: Float");
                    float attribFloat = _binaryReader.ReadSingle();
                    Console.WriteLine($"Attribute Float Value: {attribFloat}");
                    break;

                case (BOOLEAN):
                    Console.WriteLine("Attribute Type: Boolean");
                    bool attribBool = _binaryReader.ReadBoolean();
                    Console.WriteLine($"Attribute Bool Value: {attribBool}");
                    break;

                case (STRING):
                    Console.WriteLine("Attribute Type: String");
                    string attribString = ReadNullTerminatedString();
                    Console.WriteLine($"Attribute String Value: {attribString}");
                    break;

                case (BINARY):
                    Console.WriteLine("Attribute Type: Binary");
                    uint binaryLength = _binaryReader.ReadUInt32();
                    byte[] attribBinary;
                    List<byte> byteBuff = new List<byte>();

                    Console.WriteLine($"Binary Length: {binaryLength}");
                    for (int y = 0; y < binaryLength; y++)
                    {
                        byteBuff.Add(_binaryReader.ReadByte());
                    }
                    attribBinary = byteBuff.ToArray();
                    byteBuff.Clear();
                    Console.WriteLine($"Attribute Byte Value: {BitConverter.ToString(attribBinary)}");
                    break;

                case (TIME):
                    Console.WriteLine("Attribute Type: Time");
                    int attribTime = _binaryReader.ReadInt32();
                    Console.WriteLine($"Attribute Time Value: {attribTime}");
                    break;

                case (COLOR):
                    Console.WriteLine("Attribute Type: Color");
                    byte[] attribRed = { _binaryReader.ReadByte() };
                    byte[] attribGreen = { _binaryReader.ReadByte() };
                    byte[] attribBlue = { _binaryReader.ReadByte() };
                    byte[] attribAlpha = { _binaryReader.ReadByte() };
                    Console.WriteLine($"Attribute Color Value: {BitConverter.ToString(attribRed)}, {BitConverter.ToString(attribGreen)}, {BitConverter.ToString(attribBlue)}, {BitConverter.ToString(attribAlpha)}");
                    break;

                case (VECTOR2):
                    Console.WriteLine("Attribute Type: Vector2");
                    float attribV2X = _binaryReader.ReadSingle();
                    float attribV2Y = _binaryReader.ReadSingle();
                    Console.WriteLine($"Attribute Vector2 Value: {attribV2X}, {attribV2Y}");
                    break;

                case (VECTOR3):
                    Console.WriteLine("Attribute Type: Vector3");
                    float attribV3X = _binaryReader.ReadSingle();
                    float attribV3Y = _binaryReader.ReadSingle();
                    float attribV3Z = _binaryReader.ReadSingle();
                    Console.WriteLine($"Attribute Vector3 Value: {attribV3X}, {attribV3Y}, {attribV3Z}");
                    break;

                case (VECTOR4):
                    Console.WriteLine("Attribute Type: Vector4");
                    float attribV4X = _binaryReader.ReadSingle();
                    float attribV4Y = _binaryReader.ReadSingle();
                    float attribV4Z = _binaryReader.ReadSingle();
                    float attribV4W = _binaryReader.ReadSingle();
                    Console.WriteLine($"Attribute Vector3 Value: {attribV4X}, {attribV4Y}, {attribV4Z}, {attribV4W}");
                    break;

                case (QANGLE):
                    Console.WriteLine("Attribute Type: QAngle");
                    float attribQX = _binaryReader.ReadSingle();
                    float attribQY = _binaryReader.ReadSingle();
                    float attribQZ = _binaryReader.ReadSingle();
                    Console.WriteLine($"Attribute QAngle Value: {attribQX}, {attribQY}, {attribQZ}");
                    break;

                case (QUATERNION):
                    Console.WriteLine("Attribute Type: Quaternion");
                    float attribQTX = _binaryReader.ReadSingle();
                    float attribQTY = _binaryReader.ReadSingle();
                    float attribQTZ = _binaryReader.ReadSingle();
                    float attribQTW = _binaryReader.ReadSingle();
                    Console.WriteLine($"Attribute Vector3 Value: {attribQTX}, {attribQTY}, {attribQTZ}, {attribQTW}");
                break;

                case (MATRIX):
                    Console.WriteLine("Attribute Type: Matrix");
                    byte[] attribMatrix;
                    List<byte> byteBuff2 = new List<byte>();
                    for (int y = 0; y < 64; y++)
                    {
                        byteBuff2.Add(_binaryReader.ReadByte());
                    }
                    attribMatrix = byteBuff2.ToArray();
                    byteBuff2.Clear();
                    break;

                case (ELEMENT_ARRAY):
                    Console.WriteLine("Attribute Type: Element Array");
                    int numElements = _binaryReader.ReadInt32();
                    for (int i = 0; i < numElements; i++)
                    {
                        ReadAttrib(ELEMENT);
                    }
                    break;
                    //TODO handle other arrays
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
