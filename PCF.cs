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
            if (BinaryVersion != 5)
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
            //else
            //{
            //    //All strings are stored in string dict of binary 5
            //    foreach (string s in StringDict)
            //    {
            //        if (s.EndsWith(".vmt"))
            //        {
            //            Console.WriteLine(s);
            //        }
            //    }
            //}

        }

        public List<string> GetMaterialNames()
        {
            if (BinaryVersion != 5)
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
            }
            else
            {
                if (StringDict != null)
                {
                    //All strings including materials are stored in string dict of binary 5
                    Console.WriteLine("\n-----Materials-----");
                    List<string> materialNames = new List<string>();

                    foreach (string s in StringDict)
                    {

                        if (s.EndsWith(".vmt") || s.EndsWith(".vtf"))
                        {
                            materialNames.Add(s);
                        }
                    }
                    return materialNames; 
                }
            }

            return null;
        }

        public List<string> GetModelNames()
        {
            Console.WriteLine("\n-----Models-----");

            if (StringDict != null)
            {
                List<string> modelList = new List<string>();
                //All strings including model names are stored in string dict
                foreach (string s in StringDict)
                {
                    if (s.EndsWith(".mdl"))
                    {
                        modelList.Add(s);
                    }
                }
                return modelList;
            }
            else
            {
                return null;
            }
        }

        public void PrintFileStructure()
        {
            Console.WriteLine("\n-----FileStructure-----");

            int indexAttribs = 0;

            foreach (DmxElement element in Elements)
            {
                element.attributes = new ArrayList();
                element.attributes = ElementAttributes[indexAttribs];
                indexAttribs++;
            }

            List<Particle> particles = new List<Particle>();

            Particle particle = new Particle
            {
                elements = new List<DmxElement>()
            };

            int index = 0;
            foreach (DmxElement element in Elements)
            {
                //If element type is a particlesystemdefinition, add current particle to list and create new one
                if (element.typeName == "DmeParticleSystemDefinition")
                {
                    if (index != 0)
                    {
                        particles.Add(particle);
                    }

                    particle = new Particle
                    {
                        elements = new List<DmxElement>(),
                        name = element.elementName,
                    };


                }
                else
                {
                    //TODO investigate child particles incorrectly cutting off elements from parent particle. might have to do with DmeParticleSystemDefinition
                    //If not a particle system definition, add element to particle
                    particle.elements.Add(element);
                }

                index++;
            }

            foreach (Particle p in particles)
            {
                Console.WriteLine($"\n-----{p.name}-----");

                foreach (DmxElement element in p.elements)
                {
                    Console.WriteLine("    Element: " + element.elementName);
                    PrintFilestructure(element.attributes);
                }

            }

        }

        private void PrintFilestructure(ArrayList elements, int depth = 1)
        {
            string spacing = String.Concat(Enumerable.Repeat("       ", depth));

            
            foreach (var attribute in elements)
            {
                //Big if else switch to test for what type of attribute the attribute is
                //TODO replace with dictionary
                #region BigSwitch
                if (attribute is DmxAttribute[] attributeArray)
                {
                    Console.WriteLine(spacing + "-----Attribute array-----");
                    PrintFilestructure(new ArrayList(attributeArray), depth + 1);
                }
                else if (attribute is DmxAttributeElement elementTest)
                {
                    Console.WriteLine(spacing + Elements[elementTest.index].typeName);
                    Console.WriteLine(spacing + spacing + Elements[elementTest.index].elementName);
                    //Console.WriteLine(spacing + spacing + Elements[elementTest.index].typeName);

                }
                else if (attribute is DmxAttributeInteger integerAttrib)
                {
                    Console.WriteLine(spacing +$"{integerAttrib.typeName}: {integerAttrib.attribInt}");
                }
                else if (attribute is DmxAttributeFloat floatAttrib)
                {
                    Console.WriteLine(spacing + $"{floatAttrib.typeName}: {floatAttrib.attribFloat}");
                }
                else if (attribute is DmxAttributeBoolean boolAttrib)
                {

                    Console.WriteLine(spacing + $"{boolAttrib.typeName}: {boolAttrib.attribBool}");
                }
                else if (attribute is DmxAttributeString stringAttrib)
                {
                    Console.WriteLine(spacing + $"{stringAttrib.typeName}: {stringAttrib.attribString}");
                }
                else if (attribute is DmxAttributeBinary binaryAttrib)
                {
                    Console.WriteLine(spacing + $"{binaryAttrib.typeName}: {binaryAttrib.attribByte}");
                }
                else if (attribute is DmxAttributeTime timeAttrib)
                {
                    Console.WriteLine(spacing + $"{timeAttrib.typeName}: {timeAttrib.attribTime}");
                }
                else if (attribute is DmxAttributeColor colorAttrib)
                {
                    Console.WriteLine(spacing + $"{colorAttrib.typeName}: {colorAttrib.attribRed}, {colorAttrib.attribGreen}, + {colorAttrib.attribBlue}, {colorAttrib.attribAlpha}");
                }
                else if (attribute is DmxAttributeVector2 vec2Attrib)
                {
                    Console.WriteLine(spacing + $"{vec2Attrib.typeName}: {vec2Attrib.attribX}, {vec2Attrib.attribY}");
                }
                else if (attribute is DmxAttributeVector3 vec3Attrib)
                {
                    Console.WriteLine(spacing + $"{vec3Attrib.typeName}: {vec3Attrib.attribX}, {vec3Attrib.attribY}, {vec3Attrib.attribZ}");
                }
                else if (attribute is DmxAttributeVector4 vec4Attrib)
                {
                    Console.WriteLine(spacing + $"{vec4Attrib.typeName}: {vec4Attrib.attribX}, {vec4Attrib.attribY}, {vec4Attrib.attribZ}, {vec4Attrib.attribW}");
                }
                else if (attribute is DmxAttributeQAngle qAngleAttrib)
                {
                    Console.WriteLine(spacing + $"{qAngleAttrib.typeName}: {qAngleAttrib.attribX}, {qAngleAttrib.attribY}, {qAngleAttrib.attribZ}");
                }
                else if (attribute is DmxAttributeQuaternion quatAttrib)
                {
                    Console.WriteLine(spacing + $"{quatAttrib.typeName}: {quatAttrib.attribX}, {quatAttrib.attribY}, {quatAttrib.attribZ}, {quatAttrib.attribW}");
                }
                else if (attribute is DmxAttributeMatrix matrixAttrib)
                {
                    Console.WriteLine(spacing + "Matrix");
                }
                #endregion
            }
        }
    }


    public class Particle
    {
        public string name;
        public List<DmxElement> elements;
    }

    public class DmxElement
    {
        public ushort typeNameIndex; //String dict index
        public string typeName;
        public string elementName; //Element name
        public byte[] dataSignature; //Global Unique ID

        public ArrayList attributes; //attributes element owns

    }

    public class DmxElementV4
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
