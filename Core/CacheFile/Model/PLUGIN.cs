using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{
    [Serializable]
    public class PLUGIN
    {
        public string NAME { set; get; }
        public Byte[] DATA { set; get; }
    }
}
