using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.MemoryMappedFiles;

namespace app.Core
{
    public class DataFile
    {
        const string _NotOpened = "File not open";

        #region [ === VARIABLE === ]

        // HEADER = [8 byte id] + [4 bytes capacity] + [4 bytes count] + [984 byte fields] = 1,000
        const int m_HeaderSize = 1000;
        private long m_FileID = 0;
        private int m_FileSize = 0;
        private MemoryMappedFile m_mapFile;
        private readonly int m_BlobGrowSize = 1000;
        private const int m_BlobSizeMax = 255;
        private int m_BlobLEN = 0;
        private string m_FileName = string.Empty;
        private string m_FilePath = string.Empty;

        private IList m_listItems;
        private MemoryMappedFile m_mapPort;
        private readonly ReaderWriterLockSlim m_lockFile;
        private readonly ReaderWriterLockSlim m_lockCache;
        private int m_Count = 0;
        private int m_Capacity = 0;

        private readonly Dictionary<int, byte[]> m_bytes;
        private readonly Dictionary<SearchRequest, SearchResult> m_cacheSR;
        private readonly object _lockUpdate = new object();
        private readonly object _lockSearch = new object();

        ///////////////////////////////////////////////

        #endregion

        #region [ === MEMBER === ]

        public Type TypeDynamic { set; get; }
        public dbModel Model { set; get; }

        public bool Opened = false;
        public int Count
        {
            get { return m_Count; }
        }

        #endregion

        #region [ === OPEN - CLOSE === ]

        public void Close()
        {
            if (m_mapFile != null) m_mapFile.Close();
            if (m_mapPort != null) m_mapPort.Close();
            if (listener != null) listener.Abort();
        }

        public static DataFile Open(Type _typeModel)
        {
            return new DataFile(new dbModel(_typeModel));
        }

        public DataFile()
        {
            Opened = false;

            m_bytes = new Dictionary<int, byte[]>();
            m_cacheSR = new Dictionary<SearchRequest, SearchResult>(new SearchRequest.EqualityComparer());
            m_lockFile = new ReaderWriterLockSlim();
            m_lockCache = new ReaderWriterLockSlim();
        }

        public DataFile(dbModel model)
            : this()
        {
            Model = model;

            m_mapPort = MemoryMappedFile.Create(MapProtection.PageReadWrite, 4, Model.Name);
            m_FileName = Model.Name + ".df";
            string m_PathData = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");
            if (!Directory.Exists(m_PathData)) Directory.CreateDirectory(m_PathData);
            m_FilePath = Path.Combine(m_PathData, m_FileName);

            if (OpenOrCreateFile())
            {
                http_Init();
                Opened = true;

                if (m_listItems == null)
                    m_listItems = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            else Close();
        }

        public DataFile(string pathFile)
            : this()
        {
            m_FilePath = pathFile;

            if (OpenOrCreateFile())
            {
                http_Init();
                Opened = true;

                if (m_listItems == null)
                    m_listItems = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            else Close();
        }

        private bool OpenOrCreateFile()
        {
            try
            {
                if (File.Exists(m_FilePath))
                {
                    /////////////////////////////////////////////////
                    // OPEN m_FilePath

                    FileInfo fi = new FileInfo(m_FilePath);
                    m_FileSize = (int)fi.Length;
                    m_mapFile = MemoryMappedFile.Create(m_FilePath, MapProtection.PageReadWrite, m_FileSize);
                    if (m_FileSize > m_HeaderSize)
                    {
                        byte[] buf = new byte[m_FileSize];
                        using (Stream view = m_mapFile.MapView(MapAccess.FileMapRead, 0, m_FileSize))
                            view.Read(buf, 0, m_FileSize);

                        bool val = bindHeader(buf);
                        if (val)
                        {
                            m_mapPort = MemoryMappedFile.Create(MapProtection.PageReadWrite, 4, Model.Name);
                            m_FileName = Model.Name + ".df";
                            return true;
                        }
                    }
                }
                else
                {
                    /////////////////////////////////////////////////
                    // CREATE NEW m_FilePath

                    m_Capacity = m_BlobGrowSize;
                    m_FileSize = m_HeaderSize + (m_Capacity * m_BlobSizeMax) + 1;
                    m_mapFile = MemoryMappedFile.Create(m_FilePath, MapProtection.PageReadWrite, m_FileSize);

                    //string fs = _model.Name + ";" + string.Join(",", _model.Fields.Select(x => ((int)x.Type).ToString() + x.Name).ToArray());
                    writeHeaderBlank();

                    return true;
                }
            }
            catch
            {
            }
            return false;
        }



        ////////public void Close()
        ////////{
        ////////    listener.Close();
        ////////    m_mapFile.Close();
        ////////    m_mapPort.Close();
        ////////}

        ////////public static DataFile Open(Type _typeModel)
        ////////{
        ////////    return new DataFile(new dbModel(_typeModel));
        ////////}

        ////////public DataFile(dbModel model)
        ////////{
        ////////    Model = model;
        ////////    Opened = false;

        ////////    m_mapPort = MemoryMappedFile.Create(MapProtection.PageReadWrite, 4, model.Name);
        ////////    m_bytes = new Dictionary<int, byte[]>();
        ////////    m_cacheSR = new Dictionary<SearchRequest, SearchResult>(new SearchRequest.EqualityComparer());
        ////////    m_lockFile = new ReaderWriterLockSlim();
        ////////    m_lockCache = new ReaderWriterLockSlim();

        ////////    m_FileName = model.Name + ".df";
        ////////    m_FilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), m_FileName);

        ////////    http_Init(model);

        ////////    try
        ////////    {
        ////////        if (File.Exists(m_FilePath))
        ////////        {
        ////////            FileInfo fi = new FileInfo(m_FilePath);
        ////////            m_FileSize = (int)fi.Length;
        ////////            m_mapFile = MemoryMappedFile.Create(m_FilePath, MapProtection.PageReadWrite, m_FileSize);
        ////////            if (m_FileSize > m_HeaderSize)
        ////////            {
        ////////                byte[] buf = new byte[m_FileSize];
        ////////                using (Stream view = m_mapFile.MapView(MapAccess.FileMapRead, 0, m_FileSize))
        ////////                    view.Read(buf, 0, m_FileSize);

        ////////                if (bindHeader(buf) == false) return;
        ////////            }
        ////////        }
        ////////        else
        ////////        {
        ////////            m_Capacity = m_BlobGrowSize;
        ////////            m_FileSize = m_HeaderSize + (m_Capacity * m_BlobSizeMax) + 1;
        ////////            m_mapFile = MemoryMappedFile.Create(m_FilePath, MapProtection.PageReadWrite, m_FileSize);

        ////////            //string fs = _model.Name + ";" + string.Join(",", _model.Fields.Select(x => ((int)x.Type).ToString() + x.Name).ToArray());
        ////////            writeHeaderBlank(model);
        ////////        }

        ////////        Opened = true;
        ////////        if (m_listItems == null) m_listItems = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
        ////////    }
        ////////    catch
        ////////    {
        ////////    }

        ////////}

        #endregion

        #region [ === SEARCH === ]

        public SearchResult SearchGetIDs(SearchRequest search)
        {
            SearchResult sr = null;
            using (m_lockCache.ReadLock())
                m_cacheSR.TryGetValue(search, out sr);
            if (sr == null)
            {
                sr = SearchGetIDs_(search);
                using (m_lockCache.WriteLock())
                    m_cacheSR.Add(search, sr);
                sr.IsCache = false;
            }
            else
                sr.IsCache = true;

            if (sr.Total > 0)
            {
                int[] a = sr.IDs.Page(search.PageNumber, search.PageSize).ToArray();
                if (a.Length > 0)
                {
                    IList lo = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
                    lock (_lockSearch)
                    {
                        for (int k = 0; k < a.Length; k++)
                            lo.Add(m_listItems[a[k]]);
                    }
                    sr.Message = JsonConvert.SerializeObject(lo);
                }
            }

            sr.PageNumber = search.PageNumber;
            sr.PageSize = search.PageSize;
            return sr.Clone();
        }

        private SearchResult SearchGetIDs_(SearchRequest search)
        {
            if (Opened == false) return null; ;
            bool ok = false;
            int[] ids = new int[] { };
            string text = string.Empty;
            if (Opened == false)
                text = _NotOpened;
            else
            {
                try
                {
                    IQueryable lo = null;
                    lock (_lockSearch)
                    {
                        if (search.Values == null)
                            lo = m_listItems.AsQueryable().Where(search.Predicate);
                        else
                            lo = m_listItems.AsQueryable().Where(search.Predicate, search.Values); ;
                    }

                    if (lo != null)
                    {
                        List<int> li = new List<int>();
                        foreach (var o in lo)
                            li.Add(m_listItems.IndexOf(o));
                        ids = li.ToArray();
                    }

                    ok = true;
                }
                catch (Exception ex) { text = ex.Message; }
            }
            return new SearchResult(ok, ids.Length, ids, text);
        }

        private void SearchExecuteUpdateCache(ItemEditType type, int[] ids = null)
        {
            //new Thread(new ParameterizedThreadStart((obj) =>
            //{
            //  Tuple<ItemEditType, int[]> tu = (Tuple<ItemEditType, int[]>)obj;
            using (m_lockCache.WriteLock())
            {
                SearchRequest[] keys = m_cacheSR.Keys.ToArray();
                foreach (var ki in keys)
                {
                    SearchResult sr = null;
                    switch (type)
                    {
                        case ItemEditType.ADD_NEW_ITEM:
                            sr = SearchGetIDs_(ki);
                            break;
                        case ItemEditType.REMOVE_ITEM:
                            sr = m_cacheSR[ki];
                            if (sr.IDs != null && sr.IDs.Length > 0 && ids != null && ids.Length > 0)
                            {
                                int[] val = sr.IDs.Where(x => !ids.Any(o => o == x)).ToArray();
                                sr.IDs = val;
                                sr.Total = val.Length;
                            }
                            break;
                    }
                    m_cacheSR[ki] = sr;
                }
            }
            // })).Start(new Tuple<ItemEditType, int[]>(type, ids));
        }

        #endregion

        #region [ === ADD - UPDATE - REMOVE === ]

        public EditStatus[] AddItems(object[] items)
        {
            EditStatus[] rs = new EditStatus[items.Length];
            if (Opened == false || items == null || items.Length == 0) return rs;

            bool ok = false;
            int itemConverted = 0;
            /////////////////////////////////////////
            #region [ convert items[] to array dynamic object ]

            //IList lsDynObject = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
            var lsDynObject = items
                .Select((x, k) => convertToDynamicObject(x))
                .ToList();
            if (lsDynObject.Count > 0)
            {
                int[] ids;
                lock (_lockSearch)
                    // performance very bad !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    ids = lsDynObject.Select(x => x == null ? -2 : m_listItems.IndexOf(x)).ToArray();

                for (int k = ids.Length - 1; k >= 0; k--)
                {
                    switch (ids[k])
                    {
                        case -2:
                            rs[k] = EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
                            lsDynObject.RemoveAt(k);
                            break;
                        case -1:
                            itemConverted++;
                            break;
                        default:
                            rs[k] = EditStatus.FAIL_ITEM_EXIST;
                            lsDynObject.RemoveAt(k);
                            break;
                    }
                }
            }

            #endregion

            if (itemConverted == 0) return rs;

            /////////////////////////////////////////  
            Dictionary<int, byte[]> dicBytes = new Dictionary<int, byte[]>();
            List<byte> lsByte = new List<byte>() { };
            List<int> lsIndexItemNew = new List<int>();
            int itemCount = 0;

            if (lsDynObject.Count > 0)
            {
                using (m_lockFile.WriteLock())
                {
                    #region [ === CONVERT DYNAMIC OBJECT - SERIALIZE === ]

                    for (int k = 0; k < lsDynObject.Count; k++)
                    {
                        rs[k] = EditStatus.NONE;
                        int index_ = m_Count + itemCount + 1;

                        byte[] buf = serializeDynamicObject(lsDynObject[k], index_);
                        if (buf == null || buf.Length == 0)
                        {
                            rs[k] = EditStatus.FAIL_EXCEPTION_SERIALIZE_DYNAMIC_OBJECT;
                            continue;
                        }
                        else if (buf.Length > m_BlobSizeMax)
                        {
                            rs[k] = EditStatus.FAIL_MAX_LEN_IS_255_BYTE;
                            continue;
                        }
                        else
                        {
                            lsByte.AddRange(buf);
                            dicBytes.Add(index_ - 1, buf);
                            rs[k] = EditStatus.SUCCESS;
                            itemCount++;
                            lsIndexItemNew.Add(k);
                        }
                    } // end for each items

                    #endregion

                    #region [ === TEST === ]

                    ////////////////////var o1 = convertToDynamicObject(items[lsDynamicObject.Count - 1], 0);
                    ////////////////////byte[] b2;
                    ////////////////////using (var ms = new MemoryStream())
                    ////////////////////{
                    ////////////////////    ProtoBuf.Serializer.Serialize(ms, o1);
                    ////////////////////    b2 = ms.ToArray();
                    ////////////////////}
                    ////////////////////byte[] b3 = serializeDynamicObject(o1);
                    ////////////////////string j3 = string.Join(" ", b2.Select(x => x.ToString()).ToArray());
                    ////////////////////string j4 = string.Join(" ", b3.Select(x => x.ToString()).ToArray());
                    ////////////////////if (j3 == j4)
                    ////////////////////    b2 = null;

                    //////////lsDynamicObject.Add(convertToDynamicObject(items[0], 1));
                    //////////lsDynamicObject.Add(convertToDynamicObject(items[1], 2));
                    //////////lsDynamicObject.Add(convertToDynamicObject(items[2], 3));
                    //////////lsDynamicObject.Add(convertToDynamicObject(items[3], 4));

                    //////////byte[] b1;
                    //////////using (var ms = new MemoryStream())
                    //////////{
                    //////////    ProtoBuf.Serializer.Serialize(ms, lsDynamicObject);
                    //////////    b1 = ms.ToArray();
                    //////////}
                    ////////////string j1 = string.Join(" ", b1.Select(x => x.ToString()).ToArray());
                    //////////////string j2 = string.Join(" ", lsByte.Select(x => x.ToString()).ToArray());
                    //////////////if (j1 == j2)
                    //////////////    b1 = null;
                    //////////////byte[] bs = "10 9 8 0 18 5 16 1 82 1 49 10 2 8 1 0 0 0 0 10 2 8 2 10 9 8 3 18 5 16 2 82 1 51".Split(' ').Select(x => byte.Parse(x)).ToArray();
                    //////////////object vs;
                    //////////////Type typeList = typeof(List<>).MakeGenericType(TypeDynamic);
                    //////////////using (var ms = new MemoryStream(bs))
                    //////////////    vs = (IList)ProtoBuf.Serializer.NonGeneric.Deserialize(typeList, ms);

                    //////////return rs;

                    #endregion

                    #region [ === RESIZE GROW === ]

                    int freeStore = m_FileSize - (m_BlobLEN + m_HeaderSize);
                    if (freeStore < lsByte.Count + 1)
                    {
                        m_mapFile.Close();
                        FileStream fs = new FileStream(m_FilePath, FileMode.OpenOrCreate);
                        long fileSize = fs.Length + lsByte.Count + (m_BlobGrowSize * m_BlobSizeMax);
                        fs.SetLength(fileSize);
                        fs.Close();
                        m_FileSize = (int)fileSize;
                        m_mapFile = MemoryMappedFile.Create(m_FilePath, MapProtection.PageReadWrite, m_FileSize);
                    }

                    #endregion

                    #region [ === WRITE FILE === ]

                    bool w = false;
                    w = writeData(lsByte.ToArray(), itemCount);
                    if (w)
                    {
                        //string j1 = string.Join(" ", lsByte.Select(x => x.ToString()).ToArray());
                        lock (_lockSearch)
                        {
                            for (int k = 0; k < itemCount; k++)
                            {
                                Interlocked.Increment(ref m_Count);
                                Interlocked.Increment(ref m_Capacity);
                                m_listItems.Add(lsDynObject[lsIndexItemNew[k]]);
                            }
                        }
                        lock (_lockUpdate)
                        {
                            foreach (KeyValuePair<int, byte[]> kv in dicBytes)
                                m_bytes.Add(kv.Key, kv.Value);
                        }
                        ok = true;
                    }
                    if (w == false)
                    {
                        for (int k = 0; k < rs.Length; k++)
                            if (rs[k] == EditStatus.SUCCESS) rs[k] = EditStatus.FAIL_EXCEPTION_WRITE_ARRAY_BYTE_TO_FILE;
                    }

                    #endregion
                }// end lock
            }

            if (ok) SearchExecuteUpdateCache(ItemEditType.ADD_NEW_ITEM);

            return rs;
        }

        public EditStatus Update(object oCurrent, object oUpdate)
        {
            EditStatus ok = EditStatus.NONE;
            if (Opened == false) return ok;

            var o1 = convertToDynamicObject_GetIndex(oCurrent);
            var o2 = convertToDynamicObject_GetIndex(oUpdate);

            if (o1 == null || o2 == null)
                return EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
            else
            {
                if (o1.Index == -1) return EditStatus.FAIL_ITEM_NOT_EXIST;
                if (o2.Index != -1) return EditStatus.FAIL_ITEM_EXIST;

                int index = o1.Index;
                object val = o2.Item;

                byte[] buf = serializeDynamicObject(val, index + 1);
                if (buf == null || buf.Length == 0) return EditStatus.FAIL_EXCEPTION_SERIALIZE_DYNAMIC_OBJECT;

                if (index != -1)
                {
                    lock (_lockSearch)
                        m_listItems[index] = val;

                    lock (_lockUpdate)
                    {
                        if (m_bytes.ContainsKey(index))
                            m_bytes[index] = buf;

                        List<byte> lb = new List<byte>();
                        for (int k = 0; k < m_Count; k++)
                            if (m_bytes.ContainsKey(k))
                                lb.AddRange(m_bytes[k]);

                        //string j1 = string.Join(" ", lb.Select(x => x.ToString()).ToArray());

                        m_BlobLEN = 0;
                        writeData(lb.ToArray(), 0);
                        m_BlobLEN = lb.Count;
                        ok = EditStatus.SUCCESS;
                    }
                }
            }

            return ok;
        }

        public EditStatus Remove(object item)
        {
            EditStatus ok = EditStatus.NONE;
            if (Opened == false) return ok;

            var it = convertToDynamicObject_GetIndex(item);

            if (it == null)
                return EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
            else
            {
                if (it.Index == -1) return EditStatus.FAIL_ITEM_NOT_EXIST;

                int index = it.Index;

                if (index != -1)
                {
                    lock (_lockSearch)
                    {
                        m_listItems.RemoveAt(index);
                        m_Count = m_listItems.Count;
                    }

                    byte[] bf1 = m_bytes[0];
                    int p1 = -1;
                    for (int k = 1; k < bf1.Length; k++)
                    {
                        if (bf1[k - 1] == 1 && bf1[k] == 82)
                        {
                            p1 = k - 1;
                            break;
                        }
                    }

                    lock (_lockUpdate)
                    {
                        List<byte> lb = new List<byte>();
                        int km = m_bytes.Count - 1;
                        for (int k = 0; k < km; k++)
                        {
                            if (k < index)
                                lb.AddRange(m_bytes[k]);
                            else
                            {
                                byte[] bf = changeIndex(p1, m_bytes[k + 1], k + 1);
                                m_bytes[k] = bf;
                                lb.AddRange(bf);
                            }
                        }
                        m_bytes.Remove(km);

                        //string j1 = string.Join(" ", lb.Select(x => x.ToString()).ToArray());

                        m_BlobLEN = 0;
                        writeData(lb.ToArray(), 0);
                        m_BlobLEN = lb.Count;
                        ok = EditStatus.SUCCESS;
                    }
                }
            }

            return ok;
        }

        #endregion

        #region [ === HTTP === ] 
        public int Port = 0;
        private HttpListener listener;
        private void http_Init()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            Port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();

            using (Stream view = m_mapPort.MapView(MapAccess.FileMapWrite, 0, 4))
                view.Write(BitConverter.GetBytes(Port), 0, 4);

            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + Port.ToString() + "/");

            listener.Start();
            listener.BeginGetContext(ProcessRequest, listener);
        }

        private void ProcessRequest(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);

            string method = context.Request.HttpMethod;
            string path = context.Request.Url.LocalPath;
            switch (method)
            {
                case "POST":
                    #region [ === POST === ] 
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        dbMsg m = formatter.Deserialize(context.Request.InputStream) as dbMsg;
                        if (m != null)
                        {
                            switch (m.Action)
                            {
                                case dbAction.DB_SELECT:
                                    SearchRequest sr = (SearchRequest)m.Data;
                                    var rs = SearchGetIDs(sr);
                                    formatter.Serialize(context.Response.OutputStream, rs);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Serializer.NonGeneric.Serialize(context.Response.OutputStream, 500);
                        //context.Response.Close();
                    }

                    context.Response.Close();
                    //context.Response.Abort();

                    break;
                #endregion
                case "GET":
                    #region [ === GET === ]

                    switch (path)
                    {
                        case "/favicon.ico":
                            context.Response.Close();
                            return;
                        case "/ping":
                            byte[] buffer = Encoding.UTF8.GetBytes("OK");
                            context.Response.ContentLength64 = buffer.Length;
                            Stream output = context.Response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                            break;
                    }

                    break;
                    #endregion
            }

            listener.BeginGetContext(ProcessRequest, listener);
        }

        #endregion

        #region [ === FUNCTION PRIVATE === ]

        private bool bindHeader(byte[] buf)
        {
            if (buf.Length < m_HeaderSize) return false;
            try
            {
                ////////////////////////////////////////////////
                // HEADER = [4 bytes blob len] + [8 byte ID] + [4 bytes Capacity] + [4 bytes Count] + [980 byte fields] = 1,000

                // [4 bytes blob LEN]
                m_BlobLEN = BitConverter.ToInt32(buf.Take(4).ToArray(), 0);

                // [8 byte ID]
                m_FileID = BitConverter.ToInt64(buf.Skip(4).Take(8).ToArray(), 0);

                // [4 bytes Capacity]
                m_Capacity = BitConverter.ToInt32(buf.Skip(12).Take(4).ToArray(), 0);

                // [4 bytes Count]
                m_Count = BitConverter.ToInt32(buf.Skip(16).Take(4).ToArray(), 0);

                // [980 byte fields]
                int lenModel = BitConverter.ToInt32(buf.Skip(20).Take(4).ToArray(), 0);
                byte[] _fields = buf.Skip(24).Take(lenModel).ToArray();
                byte[] bm = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(_fields);
                dbModel m;
                using (var ms = new MemoryStream(bm))
                    m = new JsonSerializer().Deserialize<dbModel>(new BsonReader(ms));
                TypeDynamic = buildTypeDynamic(m);

                if (m_BlobLEN > 0)
                {
                    byte[] items = buf.Skip(m_HeaderSize).Take(m_BlobLEN).ToArray();
                    //string j1 = string.Join(" ", items.Select(x => x.ToString()).ToArray());


                    Type type = typeof(List<>).MakeGenericType(TypeDynamic);
                    using (var ms = new MemoryStream(items))
                        m_listItems = (IList)ProtoBuf.Serializer.NonGeneric.Deserialize(type, ms);

                    if (m_listItems.Count > 0)
                    {
                        for (int k = 0; k < m_listItems.Count; k++)
                        {
                            byte[] bfo = serializeDynamicObject(m_listItems[k], k + 1);
                            if (bfo != null) m_bytes.Add(k, bfo);
                        }
                    }
                }

                return true;
            }
            catch
            {
            }
            return false;
        }

        private void writeHeaderBlank()
        {
            ////////////////////////////////////////////////
            // HEADER = [4 bytes blob len] + [8 byte ID] + [4 bytes Capacity] + [4 bytes Count] + [980 byte fields] = 1,000

            // [4 bytes blob LEN]
            byte[] _byteBlobLEN = BitConverter.GetBytes(m_BlobLEN);

            // [8 byte ID]
            m_FileID = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff"));
            byte[] _byteFileID = BitConverter.GetBytes(m_FileID).ToArray();

            // [4 bytes Capacity]
            byte[] _byteCapacity = BitConverter.GetBytes(m_Capacity);

            // [4 bytes Count]
            byte[] _byteCount = BitConverter.GetBytes(m_Count);

            // [980 byte fields]
            byte[] bm;
            using (var ms = new MemoryStream())
            {
                new JsonSerializer().Serialize(new BsonWriter(ms), Model);
                bm = ms.ToArray();
            }
            byte[] bm7 = SevenZip.Compression.LZMA.SevenZipHelper.Compress(bm);

            List<byte> _byteFields = new List<byte>();
            _byteFields.AddRange(BitConverter.GetBytes(bm7.Length));
            _byteFields.AddRange(bm7);
            _byteFields.AddRange(new byte[980 - _byteFields.Count]);
            TypeDynamic = buildTypeDynamic(Model);

            List<byte> ls = new List<byte>();
            ls.AddRange(_byteBlobLEN);
            ls.AddRange(_byteFileID);
            ls.AddRange(_byteCapacity);
            ls.AddRange(_byteCount);
            ls.AddRange(_byteFields);

            using (Stream view = m_mapFile.MapView(MapAccess.FileMapWrite, 0, ls.Count))
                view.Write(ls.ToArray(), 0, ls.Count);
        }

        private bool writeData(byte[] item, int countItem = 1)
        {
            if (countItem < 0 || item == null || item.Length == 0) return false;
            try
            {
                ////////////////////////////////////////////////
                // HEADER = [4 bytes blob len] + [8 byte ID] + [4 bytes Capacity] + [4 bytes Count] + [980 byte fields] = 1,000

                // [4 bytes blob LEN]
                byte[] _byteBlobLEN = BitConverter.GetBytes(m_BlobLEN + item.Length);

                // [8 byte ID] 

                // [4 bytes Capacity] 

                // [4 bytes Count]
                byte[] _byteCount = BitConverter.GetBytes(m_Count + countItem);

                int offset = m_HeaderSize + m_BlobLEN;
                if (offset < m_FileSize)
                {
                    using (Stream view = m_mapFile.MapView(MapAccess.FileMapWrite, 0, m_FileSize))
                    {
                        //view.Seek(0, SeekOrigin.Begin);
                        view.Write(_byteBlobLEN, 0, 4);
                        view.Seek(16, SeekOrigin.Begin);
                        view.Write(_byteCount, 0, 4);
                        view.Seek(offset, SeekOrigin.Begin);
                        view.Write(item, 0, item.Length);
                        if (countItem == 0)
                        {
                            //string j1 = string.Join(" ", item.Select(x => x.ToString()).ToArray());
                            // UPDATE ITEMS ///////
                            int lenBlank = m_FileSize - (item.Length + m_HeaderSize + 1);
                            byte[] bb = new byte[lenBlank];
                            offset = offset + item.Length;
                            view.Seek(offset, SeekOrigin.Begin);
                            view.Write(bb, 0, lenBlank);
                        }
                    }
                }
                else
                {
                    // Grow resize file after write item
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            m_BlobLEN = m_BlobLEN + item.Length;
            return true;
        }

        private byte[] changeIndex(int pos, byte[] buf, int index)
        {
            if (pos < 0 || buf == null || buf.Length == 0 || index < 1) return null;
            List<byte> tem = new List<byte>();
            try
            {
                pos = pos - 2;
                byte[] bo = buf.Skip(2).ToArray();
                if (pos > 0)
                {
                    byte[] bi;
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.NonGeneric.Serialize(ms, index);
                        byte[] bii = ms.ToArray();
                        bi = bii.Skip(1).ToArray();
                    }

                    byte i_ = 0;
                    if (index > 2097151) i_ = 3;
                    else if (index > 16383) i_ = 2;
                    else if (index > 127) i_ = 1;
                    if (i_ != 0) bo[pos - 2] = (byte)(bo[pos - 2] + i_);

                    tem.AddRange(bo.Take(pos));
                    tem.AddRange(bi);
                    tem.AddRange(bo.Skip(pos + 1));

                    tem.Insert(0, (byte)tem.Count);
                    tem.Insert(0, 10);

                    string j1 = string.Join(" ", buf.Select(x => x.ToString()).ToArray());
                    string j2 = string.Join(" ", tem.Select(x => x.ToString()).ToArray());

                    return tem.ToArray();
                }
            }
            catch
            {
            }
            return tem.ToArray();
        }

        /// <summary>
        /// Serialize dynamic object to array bytes. index begin position 1 to m_Count
        /// </summary>
        /// <param name="o">Dynamic object type DynamicClass</param>
        /// <param name="index">Index begin value 1 to m_Count</param>
        /// <returns></returns>
        private byte[] serializeDynamicObject(object o, int index = 1)
        {
            if (o == null) return null;
            try
            {
                List<byte> tem = new List<byte>();
                byte[] bo;
                using (var ms = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(ms, o);
                    bo = ms.ToArray();
                }

                int pos = -1;
                for (int k = 1; k < bo.Length; k++)
                    if (bo[k - 1] == 1 && bo[k] == 82)
                    {
                        pos = k - 1;
                        break;
                    }

                if (pos > 0)
                {
                    byte[] bi;
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.NonGeneric.Serialize(ms, index);
                        bi = ms.ToArray().Skip(1).ToArray();
                    }

                    byte i_ = 0;
                    if (index > 2097151) i_ = 3;
                    else if (index > 16383) i_ = 2;
                    else if (index > 127) i_ = 1;
                    if (i_ != 0) bo[pos - 2] = (byte)(bo[pos - 2] + i_);

                    tem.AddRange(bo.Take(pos));
                    tem.AddRange(bi);
                    tem.AddRange(bo.Skip(pos + 1));

                    tem.Insert(0, (byte)tem.Count);
                    tem.Insert(0, 10);
                    return tem.ToArray();
                }
            }
            catch
            {
            }
            return null;
        }

        private object convertToDynamicObject(object item)
        {
            if (item == null) return null;
            bool ex = false;
            var o = Activator.CreateInstance(TypeDynamic);
            foreach (PropertyInfo pi in item.GetType().GetProperties())
            {
                try
                {
                    object val = pi.GetValue(item, null);
                    o.GetType().GetProperty(pi.Name).SetValue(o, val, null);
                }
                catch
                {
                    ex = true;
                }
            }
            if (ex) return null;

            return o;
        }

        private IndexDO convertToDynamicObject_GetIndex(object item)
        {
            if (item == null) return null;
            bool ex = false;
            var o = Activator.CreateInstance(TypeDynamic);
            foreach (PropertyInfo pi in item.GetType().GetProperties())
            {
                try
                {
                    object val = pi.GetValue(item, null);
                    o.GetType().GetProperty(pi.Name).SetValue(o, val, null);
                }
                catch
                {
                    ex = true;
                }
            }
            if (ex) return null;

            int index = -1;
            lock (_lockSearch)
                index = m_listItems.IndexOf(o);

            return new IndexDO(index, o);
        }

        private Type buildTypeDynamic(dbModel m)
        {
            if (m == null || string.IsNullOrEmpty(m.Name) || m.Fields == null || m.Fields.Length == 0) return null;

            var fields = m.Fields.Select(x => new DynamicProperty(x.Name, x.Type)).OrderBy(x => x.Name).ToArray();
            Type type = DynamicExpression.CreateClass(fields);
            //DynamicProperty[] at = new DynamicProperty[]
            //{
            //    new DynamicProperty("Name", typeof(string)),
            //    new DynamicProperty("Birthday", typeof(DateTime))
            //};
            //object obj = Activator.CreateInstance(type);
            //t.GetProperty("Name").SetValue(obj, "Albert", null);
            //t.GetProperty("Birthday").SetValue(obj, new DateTime(1879, 3, 14), null);

            var model = ProtoBuf.Meta.RuntimeTypeModel.Default;
            // Obtain all serializable types having no explicit proto contract
            var serializableTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSerializable && !Attribute.IsDefined(t, typeof(ProtoContractAttribute)));

            var metaType = model.Add(type, false);
            metaType.AsReferenceDefault = true;
            metaType.UseConstructor = false;

            // Add contract for all the serializable fields
            var serializableFields = type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fi => !Attribute.IsDefined(fi, typeof(NonSerializedAttribute)))
                .OrderBy(fi => fi.Name)  // it's important to keep the same fields order in all the AppDomains
                .Select((fi, k) => new { info = fi, index = k });
            foreach (var field in serializableFields)
            {
                var metaField = metaType.AddField(field.index + 1, field.info.Name);
                metaField.AsReference = !field.info.FieldType.IsValueType;       // cyclic references support
                metaField.DynamicType = field.info.FieldType == typeof(object);  // any type support
            }
            // Compile model in place for better performance, .Compile() can be used if all types are known beforehand
            model.CompileInPlace();

            return type;
        }

        #endregion
    }

}
