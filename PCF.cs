using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCFReader
{
    public class PCF
    {
        //Header
        public string MagicString;
        public string PcfFormat;

        public int PcfVersion;
        public int BinaryVersion;

        //String dict
        public int NumDictStrings;
        public List<string> StringDict = new List<string>();

        //Element dict
        public int NumElements;

        public List<DmxElement> Elements = new List<DmxElement>();
        public List<DmxElementV4> ElementsV4 = new List<DmxElementV4>(); //TODO free memory if version isnt v4

        //Element Attributes
        public List<DmxAttribute> ElementAttributes = new List<DmxAttribute>();

        public void GetParticleNames()
        {
            if (Elements != null && Elements.Count != 0)
            {
                foreach (var element in Elements)
                {
                    if (element.typeName == "DmeParticleSystemDefinition")
                    {
                        Console.WriteLine(element.elementName);
                    }
                }
            }

        }

    }


    public struct DmxElement
    {
        public ushort typeNameIndex; //String dict index
        public string typeName;
        public string elementName; //Element name
        public byte[] dataSignature; //Global Unique ID

    }

    public struct DmxElementV4
    {
        public ushort typeNameIndex; //String dict index
        public string typeName;
        public ushort elementNameIndex; //String dict index
        public string elementName;
        public byte[] dataSignature; //Global Unique ID
    }

    public struct DmxAttribute
    {
        ushort typeNameIndex; //String dict index
        char attributeType; //https://developer.valvesoftware.com/wiki/PCF_File_Format for attribute table

    }
}
