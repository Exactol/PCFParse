using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
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

            
            List<ArrayList> elementList = new List<ArrayList>();
            //List<ArrayList> elementList = new List<List<ArrayList>>();

            for (int i = 0; i < pcf.NumElements; i++)
            {
                //List that stores all types of dmxAttributes
                ArrayList attributeList = new ArrayList();

                //Get number of element attribs
                int numElementAttribs = _binaryReader.ReadInt32();
                for (int w = 0; w < numElementAttribs; w++)
                {
                    //Console.WriteLine($"\n-----Index: {w}-----");

                    DmxAttribute dmxAttribute = new DmxAttribute();

                    dmxAttribute.typeNameIndex = _binaryReader.ReadUInt16();
                    dmxAttribute.attributeType = _binaryReader.ReadByte();
                    dmxAttribute.typeName = pcf.StringDict[dmxAttribute.typeNameIndex];

                    //Console.WriteLine($"Number Of Element Attributes: {numElementAttribs}");
                    //Console.WriteLine($"TypeName Index: {dmxAttribute.typeNameIndex}");
                    //Console.WriteLine($"TypeName: {pcf.StringDict[dmxAttribute.typeNameIndex]}");
                    //Console.WriteLine($"Attribute Type: {dmxAttribute.attributeType}");

                    //Read attribute info and store in list
                    
                    attributeList.Add(ReadAttrib(dmxAttribute));

                    //Console.WriteLine("--------------------");
                }
                //Add attributes to element
                pcf.ElementAttributes.Add(attributeList);
            }

            //pcf.ElementAttributes = elementList;
        }

        //Returns generic type
        private dynamic ReadAttrib(DmxAttribute attribute)
        {
            int attributeType = attribute.attributeType;

            //Read different data types depending on what attribute type is
            switch (attributeType)
            {
                case (ELEMENT):
                    //Console.WriteLine("Attribute Type: Element");

                    DmxAttributeElement dmxElement = new DmxAttributeElement(attribute);
                    dmxElement.index = _binaryReader.ReadInt32();

                    //Console.WriteLine($"Attribute Int Value: {dmxElement.index}");
                    return dmxElement;

                case (INTEGER):
                    //Console.WriteLine("Attribute Type: Integer");

                    DmxAttributeInteger dmxInteger = new DmxAttributeInteger(attribute);
                    dmxInteger.attribInt = _binaryReader.ReadInt32();

                    //Console.WriteLine($"Attribute Int Value: {dmxInteger.attribInt}");
                    return dmxInteger;

                case (FLOAT):
                    //Console.WriteLine("Attribute Type: Float");

                    DmxAttributeFloat dmxFloat = new DmxAttributeFloat(attribute);
                    dmxFloat.attribFloat = _binaryReader.ReadSingle();

                    //Console.WriteLine($"Attribute Float Value: {dmxFloat.attribFloat}");
                    return dmxFloat;

                case (BOOLEAN):
                    //Console.WriteLine("Attribute Type: Boolean");

                    DmxAttributeBoolean dmxBool = new DmxAttributeBoolean(attribute);
                    dmxBool.attribBool = _binaryReader.ReadBoolean();
                    
                    //Console.WriteLine($"Attribute Bool Value: {dmxBool.attribBool}");
                    return dmxBool;

                case (STRING):
                    //Console.WriteLine("Attribute Type: String");

                    DmxAttributeString dmxString = new DmxAttributeString(attribute);

                    if (pcf.BinaryVersion != 4)
                        dmxString.attribString = ReadNullTerminatedString();
                    else
                    {
                        //Binary 4 pcfs store only index into string dict
                        dmxString.stringIndex = _binaryReader.ReadUInt16();
                        dmxString.attribString = pcf.StringDict[dmxString.stringIndex];
                    }
                    //Console.WriteLine($"Attribute String Value: {dmxString.attribString}");
                    return dmxString;

                case (BINARY):
                    //Console.WriteLine("Attribute Type: Binary");

                    DmxAttributeBinary dmxBinary = new DmxAttributeBinary(attribute);
                    dmxBinary.length = _binaryReader.ReadUInt32();
                    dmxBinary.attribByte = new byte[dmxBinary.length];

                    //Read specified # of bytes
                    List<byte> byteBuff = new List<byte>();
                    //Console.WriteLine($"Binary Length: {dmxBinary.length}");
                    for (int y = 0; y < dmxBinary.length; y++)
                    {
                        byteBuff.Add(_binaryReader.ReadByte());
                    }
                    dmxBinary.attribByte = byteBuff.ToArray();
                    byteBuff.Clear();

                    //Console.WriteLine($"Attribute Byte Value: {BitConverter.ToString(dmxBinary.attribByte)}");
                    return dmxBinary;

                case (TIME):
                    //Console.WriteLine("Attribute Type: Time");

                    DmxAttributeTime dmxTime = new DmxAttributeTime(attribute);
                    dmxTime.attribTime = _binaryReader.ReadInt32();

                    //Console.WriteLine($"Attribute Time Value: {dmxTime.attribTime}");
                    return dmxTime;

                case (COLOR):
                    //Console.WriteLine("Attribute Type: Color");

                    DmxAttributeColor dmxColor = new DmxAttributeColor(attribute);
                    dmxColor.attribRed = _binaryReader.ReadByte();
                    dmxColor.attribGreen = _binaryReader.ReadByte();
                    dmxColor.attribBlue = _binaryReader.ReadByte();
                    dmxColor.attribAlpha = _binaryReader.ReadByte();

                    //Console.WriteLine($"Attribute Color Value: {dmxColor.attribRed}, {dmxColor.attribBlue}, {dmxColor.attribGreen}, {dmxColor.attribAlpha}");
                    return dmxColor;

                case (VECTOR2):
                    //Console.WriteLine("Attribute Type: Vector2");

                    DmxAttributeVector2 dmxVec2 = new DmxAttributeVector2(attribute);
                    dmxVec2.attribX = _binaryReader.ReadSingle();
                    dmxVec2.attribY = _binaryReader.ReadSingle();

                    //Console.WriteLine($"Attribute Vector2 Value: {dmxVec2.attribX}, {dmxVec2.attribY}");
                    return dmxVec2;

                case (VECTOR3):
                    //Console.WriteLine("Attribute Type: Vector3");

                    DmxAttributeVector3 dmxVec3 = new DmxAttributeVector3(attribute);
                    dmxVec3.attribX = _binaryReader.ReadSingle();
                    dmxVec3.attribY = _binaryReader.ReadSingle();
                    dmxVec3.attribZ = _binaryReader.ReadSingle();

                    //Console.WriteLine($"Attribute Vector3 Value: {dmxVec3.attribX}, {dmxVec3.attribY}, {dmxVec3.attribZ}");
                    return dmxVec3;

                case (VECTOR4):
                    //Console.WriteLine("Attribute Type: Vector4");

                    DmxAttributeVector4 dmxVec4 = new DmxAttributeVector4(attribute);
                    dmxVec4.attribX = _binaryReader.ReadSingle();
                    dmxVec4.attribY = _binaryReader.ReadSingle();
                    dmxVec4.attribZ = _binaryReader.ReadSingle();
                    dmxVec4.attribW = _binaryReader.ReadSingle();

                    //Console.WriteLine($"Attribute Vector3 Value: {dmxVec4.attribX}, {dmxVec4.attribY}, {dmxVec4.attribZ}, {dmxVec4.attribW}");
                    return dmxVec4;

                case (QANGLE):
                    //Console.WriteLine("Attribute Type: QAngle");

                    DmxAttributeQAngle dmxQangle = new DmxAttributeQAngle(attribute);
                    dmxQangle.attribX = _binaryReader.ReadSingle();
                    dmxQangle.attribY = _binaryReader.ReadSingle();
                    dmxQangle.attribZ = _binaryReader.ReadSingle();

                    //Console.WriteLine($"Attribute QAngle Value: {dmxQangle.attribX}, {dmxQangle.attribY}, {dmxQangle.attribZ}");
                    return dmxQangle;

                case (QUATERNION):
                    //Console.WriteLine("Attribute Type: Quaternion");

                    DmxAttributeQuaternion dmxQuat = new DmxAttributeQuaternion(attribute);
                    dmxQuat.attribX = _binaryReader.ReadSingle();
                    dmxQuat.attribY = _binaryReader.ReadSingle();
                    dmxQuat.attribZ = _binaryReader.ReadSingle();
                    dmxQuat.attribW = _binaryReader.ReadSingle();


                    //Console.WriteLine($"Attribute Vector3 Value: {dmxQuat.attribX}, {dmxQuat.attribY}, {dmxQuat.attribZ}, {dmxQuat.attribW}");
                    return dmxQuat;

                case (MATRIX):
                    //Console.WriteLine("Attribute Type: Matrix");

                    DmxAttributeMatrix dmxMatrix = new DmxAttributeMatrix(attribute);
                    dmxMatrix.attribByte = new byte[64];

                    List<byte> byteBuff2 = new List<byte>();
                    for (int y = 0; y < 64; y++)
                    {
                        byteBuff2.Add(_binaryReader.ReadByte());
                    }
                    dmxMatrix.attribByte = byteBuff2.ToArray();
                    byteBuff2.Clear();
                    return dmxMatrix;

                case (ELEMENT_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    int numElements = _binaryReader.ReadInt32();
                    DmxAttribute[] elementArray = new DmxAttribute[numElements];

                    for (int i = 0; i < numElements; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = ELEMENT;
                        elementArray[i] = ReadAttrib(tempAttribute);
                    }
                    return elementArray;
                    //TODO handle other arrays
            }
            //Default return null
            return null;
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
