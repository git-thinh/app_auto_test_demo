using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{
    [Serializable]
    public class USER
    {
        [dbField(IsKeyAuto = true)]
        public long KEY { set; get; }

        [dbField(IsDuplicate = false)]
        public string USERNAME { set; get; }

        [dbField(IsEncrypt = true)]
        public string PASSWORD { set; get; }

        public string FULLNAME { set; get; }
    }
}
