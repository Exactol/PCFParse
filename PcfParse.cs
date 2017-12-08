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
            if (pcf.BinaryVersion != 4 && pcf.BinaryVersion != 5)
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
            pcf.NumElements = (int)_binaryReader.ReadUInt32();
            
            //Read the elements
            Console.WriteLine($"Number Of Elements: {pcf.NumElements}");
            for (int i = 0; i < pcf.NumElements; i++)
            {
                if (pcf.BinaryVersion != 4 && pcf.BinaryVersion != 5)
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
                else if (pcf.BinaryVersion == 4)
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
                else
                {
                    DmxElementV4 element;
                    //Get name index and string from string dict
                    element.typeNameIndex = _binaryReader.ReadUInt16();
                    element.typeName = pcf.StringDict[element.typeNameIndex];

                    //Get element index
                    element.elementNameIndex = _binaryReader.ReadUInt16();
                    element.elementName = pcf.StringDict[element.elementNameIndex];


                    Console.WriteLine($"\nType name: {element.typeName}");
                    Console.WriteLine($"Element name: {element.elementName}");
                    //It seems pcf binary v5 has 20 byte buffer
                    List<byte> byteBuf = new List<byte>();
                    for (int z = 0; z < 20; z++)
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

            //TODO add support for binary 5, undocumented not sure how to read
            if (pcf.BinaryVersion == 5)
            {
                return;
            }

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

            //number of elements in an attribute array
            int numArrayItems = 0;

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

                    if (pcf.BinaryVersion != 4 && pcf.BinaryVersion != 5)
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

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] elementArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = ELEMENT;
                        elementArray[i] = ReadAttrib(tempAttribute);
                    }
                    return elementArray;

                case (INTEGER_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] integerArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = INTEGER;
                        integerArray[i] = ReadAttrib(tempAttribute);
                    }
                    return integerArray;

                case (FLOAT_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] floatArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = FLOAT;
                        floatArray[i] = ReadAttrib(tempAttribute);
                    }
                    return floatArray;

                case (BOOLEAN_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] booleanArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = BOOLEAN;
                        booleanArray[i] = ReadAttrib(tempAttribute);
                    }
                    return booleanArray;

                case (STRING_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] stringArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = STRING;
                        stringArray[i] = ReadAttrib(tempAttribute);
                    }
                    return stringArray;

                case (BINARY_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] binaryArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = BINARY;

                        //Recursive call
                        binaryArray[i] = ReadAttrib(tempAttribute);
                    }
                    return binaryArray;

                case (TIME_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] timeArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = TIME;

                        //Recursive call
                        timeArray[i] = ReadAttrib(tempAttribute);
                    }
                    return timeArray;

                case (COLOR_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] colorArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = COLOR;

                        //Recursive call
                        colorArray[i] = ReadAttrib(tempAttribute);
                    }
                    return colorArray;

                case (VECTOR2_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] vec2Array = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = VECTOR2;

                        //Recursive call
                        vec2Array[i] = ReadAttrib(tempAttribute);
                    }
                    return vec2Array;

                case (VECTOR3_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] vec3Array = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = VECTOR3;

                        //Recursive call
                        vec3Array[i] = ReadAttrib(tempAttribute);
                    }
                    return vec3Array;

                case (VECTOR4_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] vec4Array = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = VECTOR4;

                        //Recursive call
                        vec4Array[i] = ReadAttrib(tempAttribute);
                    }
                    return vec4Array;

                case (QANGLE_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] qAngleArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = QANGLE;

                        //Recursive call
                        qAngleArray[i] = ReadAttrib(tempAttribute);
                    }
                    return qAngleArray;

                case (QUATERNION_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] quatArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = QUATERNION;

                        //Recursive call
                        quatArray[i] = ReadAttrib(tempAttribute);
                    }
                    return quatArray;

                case (MATRIX_ARRAY):
                    //Console.WriteLine("Attribute Type: Element Array");

                    numArrayItems = _binaryReader.ReadInt32();
                    DmxAttribute[] matrixArray = new DmxAttribute[numArrayItems];

                    for (int i = 0; i < numArrayItems; i++)
                    {
                        DmxAttribute tempAttribute = new DmxAttribute();
                        tempAttribute.attributeType = MATRIX;

                        //Recursive call
                        matrixArray[i] = ReadAttrib(tempAttribute);
                    }
                    return matrixArray;
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
