
using System; 

namespace app.Core
{  
    [Serializable]
    public class IndexDO
    {
        public int Index { set; get; }
        public object Item { set; get; }
        public IndexDO(int index, object item)
        {
            Index = index;
            Item = item;
        }
    } 
}
