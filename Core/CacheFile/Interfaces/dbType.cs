using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{ 
    public class dbType
    {
        //private string[] aType = "String,Byte,SByte,Int32,UInt32,Int16,UInt16,Int64,UInt64,Single,Double,Char,Boolean,Object,Decimal".Split(',');
        public static readonly string[] Types = "String,Int32,Int64,DateTime,Byte,Boolean".Split(',');

        public static Type GetByName(string nameType)
        {
            Type type = typeof(String);
            switch (nameType)
            {
                case "Int32":
                    type = typeof(Int32);
                    break;
                case "Int64":
                    type = typeof(Int64);
                    break;
                case "String":
                    type = typeof(String);
                    break;
                case "DateTime":
                    type = typeof(Int64);
                    break;
                case "Byte":
                    type = typeof(Byte);
                    break;
                case "Boolean":
                    type = typeof(Byte);
                    break;
            }
            return type;
        }
    }
}
