using System;
using System.Collections;
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
        public List<ArrayList> ElementAttributes = new List<ArrayList>();
        
        public void GetParticleNames()
        {
            Console.WriteLine("\n-----Particle Names-----");
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

        public List<string> GetMaterialNames()
        {
            if (ElementAttributes != null)
            {
                Console.WriteLine("\n-----Materials-----");
                List<string> materialNames = new List<string>();

                foreach (ArrayList element in ElementAttributes)
                {
                    foreach (var attribute in element)
                    {
                        if (attribute is DmxAttributeString dmxString)
                        {
                            if (dmxString.typeName == "material")
                            {
                                materialNames.Add(dmxString.attribString);
                            }

                        }

                        //DmxAttribute dmxAttribute = (DmxAttribute)attribute;
                        //Console.WriteLine(attribute.GetType());
                    }
                }

                return materialNames;
            }
            else
            {
                return null;
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

    //Base attribute class
    public class DmxAttribute
    {
        public ushort typeNameIndex; //String dict index
        public byte attributeType; //https://developer.valvesoftware.com/wiki/PCF_File_Format for attribute table

        //Name from string index
        public string typeName;

        //Constructor to construct derived classes from this class
        public DmxAttribute(DmxAttribute copy)
        {
            typeNameIndex = copy.typeNameIndex;
            attributeType = copy.attributeType;
            typeName = copy.typeName;
        }

        public DmxAttribute()
        {
            
        }
    }
    public class DmxAttributeElement : DmxAttribute
    {
        //Index into element array
        public int index;

        //Constructor that copies values from base attribute class
        public DmxAttributeElement(DmxAttribute attribute) : base(attribute)
        {    
        }
    }
    public class DmxAttributeInteger : DmxAttribute
    {
        public int attribInt;

        public DmxAttributeInteger(DmxAttribute attribute) : base(attribute)
        {
        }
    }
    public class DmxAttributeFloat : DmxAttribute
    {
        public float attribFloat;

        public DmxAttributeFloat(DmxAttribute attribute) : base(attribute)
        {
        }
    }
    public class DmxAttributeBoolean : DmxAttribute
    {
        public bool attribBool;

        public DmxAttributeBoolean(DmxAttribute attribute) : base(attribute)
        {
        }
    }
    public class DmxAttributeString : DmxAttribute
    {
        public ushort stringIndex;
        public string attribString;

        public DmxAttributeString(DmxAttribute attribute) : base(attribute)
        {
        }
    }

    public class DmxAttributeBinary : DmxAttribute
    {
        public uint length;
        public byte[] attribByte;
        

        public DmxAttributeBinary(DmxAttribute attribute) : base(attribute)
        {
        }
    }
    public class DmxAttributeTime : DmxAttribute
    {
        //Technically float, written as (int)( float * 10000.0 ), read as ( int / 10000.0 )
        public int attribTime;

        public DmxAttributeTime(DmxAttribute attribute) : base(attribute)
        {
        }
    }

    public class DmxAttributeColor : DmxAttribute
    {
        //RGBA
        public int attribRed;
        public int attribGreen;
        public int attribBlue;
        public int attribAlpha;

        public DmxAttributeColor(DmxAttribute attribute) : base(attribute)
        {
        }
    }

    public class DmxAttributeVector2 : DmxAttribute
    {
        //XY
        public float attribX;
        public float attribY;

        public DmxAttributeVector2(DmxAttribute attribute) : base(attribute)
        {
        }
    }

    public class DmxAttributeVector3 : DmxAttribute
    {
        //XYZ
        public float attribX;
        public float attribY;
        public float attribZ;

        public DmxAttributeVector3(DmxAttribute attribute) : base(attribute)
        {
        }
    }

    public class DmxAttributeVector4 : DmxAttribute
    {
        //XYZW
        public float attribX;
        public float attribY;
        public float attribZ;
        public float attribW;

        public DmxAttributeVector4(DmxAttribute attribute) : base(attribute)
        {
        }
    }

    public class DmxAttributeQAngle : DmxAttribute
    {
        //XYZ Rotation
        public float attribX;
        public float attribY;
        public float attribZ;

        public DmxAttributeQAngle(DmxAttribute attribute) : base(attribute)
        {
        }
    }

    public class DmxAttributeQuaternion : DmxAttribute
    {
        //XYZW Rotation
        public float attribX;
        public float attribY;
        public float attribZ;
        public float attribW;

        public DmxAttributeQuaternion(DmxAttribute attribute) : base(attribute)
        {
        }
    }

    public class DmxAttributeMatrix : DmxAttribute
    {
        //float[4][4]
        //TODO put into 2d float array
        public byte[] attribByte;

        public DmxAttributeMatrix(DmxAttribute attribute) : base(attribute)
        {
        }
    }
}
