using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace app.Core
{

    [ProtoContract]
    public class dbModel
    {
        [ProtoMember(1)]
        public string Name { set; get; }

        [ProtoMember(2)]
        public dbField[] Fields { set; get; }

        public dbModel() { }
        public dbModel(Type model)
        {

            List<dbField> lf = new List<dbField>() { };

            foreach (var prop in model.GetProperties())
            {
                var at = (dbField[])prop.GetCustomAttributes(typeof(dbField), false);
                if (at.Length > 0)
                {
                    dbField o = at[0];
                    o.Name = prop.Name;
                    o.TypeName = prop.PropertyType.Name;
                    lf.Add(o);
                }
                else
                    lf.Add(new dbField() { Name = prop.Name, TypeName = prop.PropertyType.Name });
            }


            Name = model.Name;
            Fields = lf.ToArray();
            //model.GetProperties().Select(x => new dbField() { Name = x.Name, TypeName = x.PropertyType.Name }).ToArray();
        }

        public override string ToString()
        {
            return this.Name + ";" + string.Join(",", this.Fields.Select(x => x.TypeName + "." + x.Name).ToArray());
        }
    }
}
