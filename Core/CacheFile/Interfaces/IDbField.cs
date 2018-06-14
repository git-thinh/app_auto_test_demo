using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{ 
    [ProtoContract]
    public interface IDbField
    {
        [ProtoMember(1)]
        string Name { set; get; }

        [ProtoMember(2)]
        string TypeName { set; get; }

        [ProtoMember(3)]
        bool IsDateTime { set; get; }

        [ProtoMember(4)]
        bool IsKeyAuto { set; get; }

        [ProtoMember(5)]
        ControlKit Kit { set; get; }

        [ProtoMember(6)]
        bool IsAllowNull { set; get; }

        [ProtoMember(7)]
        bool IsIndex { set; get; }

        [ProtoMember(8)]
        bool IsEncrypt { set; get; }

        [ProtoMember(9)]
        bool IsDuplicate { set; get; }

        //////////////////////////////////////////

        [ProtoMember(10)]
        string Caption { set; get; }

        [ProtoMember(11)]
        string CaptionShort { set; get; }

        [ProtoMember(12)]
        string Description { set; get; }

        //////////////////////////////////////////

        [ProtoMember(13)]
        JoinType JoinType { set; get; }

        [ProtoMember(14)]
        string JoinModel { set; get; }

        [ProtoMember(15)]
        string JoinField { set; get; }

        [ProtoMember(16)]
        string[] ValueDefault { set; get; }

        //////////////////////////////////////////

        [ProtoMember(17)]
        bool Mobi { set; get; }

        [ProtoMember(18)]
        bool Tablet { set; get; }
    }
}
