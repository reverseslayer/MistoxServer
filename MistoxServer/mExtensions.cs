using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MistoxServer {

    static class Extensions {
        public static T[] Join<T>( this T[] first, T[] second ) {
            T[] bytes = new T[first.Length + second.Length];
            Buffer.BlockCopy( first, 0, bytes, 0, first.Length );
            Buffer.BlockCopy( second, 0, bytes, first.Length, second.Length );
            return bytes;
        }

        public static T[] Sub<T>( this T[] data, int index, int length ) {
            T[] result = new T[length];
            Array.Copy( data, index, result, 0, length );
            return result;
        }

        public static void StreamSend<T>( NetworkStream Stream, T Packet ) {
            byte[] typeName = Encoding.UTF8.GetBytes(typeof(T).FullName);
            byte[] typeLength = BitConverter.GetBytes(typeName.Length);
            byte[] packetdata = mSerialize.Serialize(Packet);
            byte[] length = BitConverter.GetBytes(packetdata.Length);
            Stream.Write( typeLength, 0, 4 );
            Stream.Write( typeName, 0, typeName.Length );
            Stream.Write( length, 0, 4 );
            Stream.Write( packetdata, 0, packetdata.Length );
        }
    }

    [AttributeUsage( AttributeTargets.Field )]
    public class mSerialize : Attribute {
        public TypeTag type;
        public mSerialize( TypeTag type ) {
            this.type = type;
        }

        public static byte[] Serialize( object obj ) {
            if( obj == null ) {
                return new byte[0];
            }
            FieldInfo[] properties = obj.GetType().GetFields();
            List<byte> packetBuilder = new List<byte>();
            foreach( FieldInfo Variable in properties ) {
                if( Variable.CustomAttributes.Count() > 0 ) {
                    CustomAttributeData prop = Variable.CustomAttributes.Where( x => x.AttributeType == typeof(mSerialize) ).First();
                    if( prop != null ) {
                        TypeTag CreateType = (TypeTag)prop.ConstructorArguments[0].Value;
                        if( CreateType == TypeTag.Integer ) {
                            int objValue = (int)Variable.GetValue(obj);
                            byte[] x = BitConverter.GetBytes(objValue);
                            foreach( byte b in x ) {
                                packetBuilder.Add( b );
                            }
                        } else if( CreateType == TypeTag.Float ) {
                            float objValue = (float)Variable.GetValue(obj);
                            byte[] x = BitConverter.GetBytes(objValue);
                            foreach( byte b in x ) {
                                packetBuilder.Add( b );
                            }
                        } else if( CreateType == TypeTag.Boolean ) {
                            bool objValue = (bool)Variable.GetValue(obj);
                            byte[] x = BitConverter.GetBytes(objValue);
                            foreach( byte b in x ) {
                                packetBuilder.Add( b );
                            }
                        } else if( CreateType == TypeTag.String ) {
                            string objValue = (string)Variable.GetValue(obj);
                            byte[] x = Encoding.UTF8.GetBytes(objValue);
                            foreach( byte b in x ) {
                                packetBuilder.Add( b );
                            }
                            packetBuilder.Add( 0 ); // String Terminator
                        } else if( CreateType == TypeTag.Char ) {
                            string objValue = ((char)Variable.GetValue(obj)).ToString();
                            byte[] x = Encoding.UTF8.GetBytes(objValue);
                            foreach( byte b in x ) {
                                packetBuilder.Add( b );
                            }
                            packetBuilder.Add( 0 ); // String Terminator
                        }
                    }
                }
            }
            return packetBuilder.ToArray();
        }
        public static dynamic Deserialize( byte[] obj, Type objType ) {
            byte[] workingObj = obj;
            dynamic builder = Activator.CreateInstance(objType);
            FieldInfo[] properties = objType.GetFields();
            foreach( FieldInfo Variable in properties ) {
                if( Variable.CustomAttributes.Count() > 0 ) {
                    CustomAttributeData prop = Variable.CustomAttributes.Where( x => x.AttributeType == typeof(mSerialize) ).First();
                    if( prop != null ) {
                        TypeTag CreateType = (TypeTag)prop.ConstructorArguments[0].Value;
                        if( CreateType == TypeTag.Integer ) {
                            byte[] intBytes = workingObj.Sub(0, 4);
                            workingObj = workingObj.Sub( 4, workingObj.Length - 4 );
                            int x = BitConverter.ToInt32(intBytes);
                            Variable.SetValue( builder, x );
                        } else if( CreateType == TypeTag.Float ) {
                            byte[] floatBytes = workingObj.Sub(0, 4);
                            workingObj = workingObj.Sub( 4, workingObj.Length - 4 );
                            float x = BitConverter.ToSingle(floatBytes);
                            Variable.SetValue( builder, x );
                        } else if( CreateType == TypeTag.Boolean ) {
                            byte[] boolBytes = workingObj.Sub(0, 1);
                            workingObj = workingObj.Sub( 1, workingObj.Length - 1 );
                            bool x = BitConverter.ToBoolean(boolBytes);
                            Variable.SetValue( builder, x );
                        } else if( CreateType == TypeTag.String ) {
                            // Find String
                            int index = 0;
                            bool found = false;
                            do {
                                if( workingObj[index] == 0 ) {
                                    found = true;
                                }
                                index++;
                            } while( !found );
                            // Pull string bytes
                            byte[] String = workingObj.Sub(0, index-1);
                            workingObj = workingObj.Sub( index, workingObj.Length - index );
                            string text = Encoding.UTF8.GetString(String);
                            Variable.SetValue( builder, text );
                        } else if( CreateType == TypeTag.Char ) {
                            // Find String
                            int index = 0;
                            bool found = false;
                            do {
                                if( workingObj[index] == 0 ) {
                                    found = true;
                                }
                                index++;
                            } while( !found );
                            // Pull string bytes
                            byte[] String = workingObj.Sub(0, index-1);
                            workingObj = workingObj.Sub( index, workingObj.Length - index );
                            string text = Encoding.UTF8.GetString(String);
                            Variable.SetValue( builder, text.ToCharArray()[0] );
                        }
                    }
                }
            }
            return builder;
        }

    }

    public enum TypeTag {
        Integer,            // 4 bytes
        Float,              // 4 bytes
        Boolean,            // 1 byte
        String,             // utf8 + 1 byte null terminator
        Char,               // utf8 + 1 byte null terminator
        Null                // Returned in rare cases meaning empty
    }
}