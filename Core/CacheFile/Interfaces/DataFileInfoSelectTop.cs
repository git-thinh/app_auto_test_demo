using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{

    [Serializable]
    public class DataFileInfoSelectTop
    {
        public int PortHTTP { set; get; }

        public int TotalRecord { set; get; }

        public IList DataSelectTop { set; get; }

        public dbField[] Fields { set; get; }
    }
}
