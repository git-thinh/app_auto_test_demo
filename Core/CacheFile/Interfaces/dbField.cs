using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{

    [ProtoContract]
    public class dbField : Attribute, IDbField
    {
        [ProtoMember(1)]
        public string Name
        {
            set
            {
                _name = value;
                if (_name == null) _name = string.Empty;
                else _name = _name.Replace(' ', '_').ToUpper().Trim();
            }
            get
            {
                return _name;
            }
        }

        [ProtoMember(2)]
        public string TypeName
        {
            set
            {
                IsDateTime = value == typeof(DateTime).Name;
                Type = dbType.GetByName(value);
                _type = Type.Name;
            }
            get
            {
                return _type;
            }
        }

        [ProtoMember(3)]
        public bool IsDateTime { set; get; }

        [ProtoMember(4)]
        public bool IsKeyAuto { set; get; }

        [ProtoMember(5)]
        public ControlKit Kit { set; get; }

        [ProtoMember(6)]
        public bool IsAllowNull { set; get; }

        [ProtoMember(7)]
        public bool IsIndex { set; get; }

        [ProtoMember(8)]
        public bool IsEncrypt { set; get; }

        [ProtoMember(9)]
        public bool IsDuplicate { set; get; }

        //////////////////////////////////////////

        [ProtoMember(10)]
        public string Caption { set; get; }

        [ProtoMember(11)]
        public string CaptionShort { set; get; }

        [ProtoMember(12)]
        public string Description { set; get; }

        //////////////////////////////////////////

        [ProtoMember(13)]
        public JoinType JoinType { set; get; }

        [ProtoMember(14)]
        public string JoinModel { set; get; }

        [ProtoMember(15)]
        public string JoinField { set; get; }

        [ProtoMember(16)]
        public string[] ValueDefault { set; get; }

        //////////////////////////////////////////

        [ProtoMember(17)]
        public bool Mobi { set; get; }

        [ProtoMember(18)]
        public bool Tablet { set; get; }

        [ProtoIgnore]
        private string _name = string.Empty;
        [ProtoIgnore]
        private string _type = typeof(String).Name;

        [Newtonsoft.Json.JsonIgnore]
        [ProtoIgnore]
        public Type Type { private set; get; }

        public dbField()
        {
            JoinType = JoinType.NONE;
            Kit = ControlKit.TEXT;
            IsEncrypt = false;
        }
    }
}
