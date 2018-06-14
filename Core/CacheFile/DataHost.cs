 
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace app.Core
{ 
    public class DataHost : IDataFile
    {
        private readonly MemoryMappedFile m_mapPort;
        private readonly ConcurrentDictionary<string, DataFile> m_dataFile;
        private readonly string m_PathData;
        private List<string> m_listDataName;

        public bool Open = false;
        public delegate void EventOpen(string[] fs);
        public EventOpen OnOpen;

        public DataHost()
        {
            m_mapPort = MemoryMappedFile.Create(MapProtection.PageReadWrite, 4, "datafile");
            m_listDataName = new List<string>();
            m_dataFile = new ConcurrentDictionary<string, DataFile>();

            m_PathData = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "Data").Replace(@"file:\", string.Empty);
            if (!Directory.Exists(m_PathData)) Directory.CreateDirectory(m_PathData);
        }

        #region [ === START - CLOSE === ]

        public void Start()
        {
            //new Thread(() =>
            //{
            string dbUSER = typeof(USER).Name;
            DataFile dfUSER = null;

            string[] fs = Directory.GetFiles(m_PathData, "*.df");
            if (fs.Length > 0)
            {
                for (int k = 0; k < fs.Length; k++)
                {
                    DataFile df = new DataFile(fs[k]);
                    if (df.Opened)
                    {
                        if (df.Model.Name == dbUSER) dfUSER = df;

                        string name = df.Model.Name;
                        if (m_dataFile.TryAdd(name, df))
                            m_listDataName.Add(name);
                    }
                }
            }

            if (dfUSER == null)
            {
                dfUSER = DataFile.Open(typeof(USER));
                if (dfUSER.Opened)
                {
                    if (m_dataFile.TryAdd(dbUSER, dfUSER))
                        m_listDataName.Add(dbUSER);
                }
            }

            if (dfUSER != null && dfUSER.Count == 0)
            {
                var ra = dfUSER.AddItems(new USER[] {
                    new USER() { FULLNAME = "Admin", PASSWORD = "12345", USERNAME = "admin" },
                    new USER() { FULLNAME = "user", PASSWORD = "12345", USERNAME = "user" },
                    //new USER() { FULLNAME = "free", PASSWORD = "12345", USERNAME = "free" },
                });
                //var ra2 = dfUSER.AddItems(new USER[] { new USER() { FULLNAME = "user", PASSWORD = "12345", USERNAME = "user" }, });
                if (ra[0] != EditStatus.SUCCESS)
                {
                    if (OnOpen != null) OnOpen(m_listDataName.ToArray());
                    return;
                }
            }

            Open = true;
            if (OnOpen != null) OnOpen(m_listDataName.ToArray());
            //}).Start();
        }

        public void Close()
        {
            m_mapPort.Close();
            foreach (var db in m_dataFile.Values)
                db.Close();
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
                                    SearchResult val = Search(m.Name, sr);
                                    if (val != null)
                                        formatter.Serialize(context.Response.OutputStream, val);
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

        #region [ === IDATAFILE === ]

        private DataFile GetDF(string dbName)
        {
            //string predicate, params object[] values
            DataFile db = null;
            m_dataFile.TryGetValue(dbName, out db);
            return db;
        }

        public SearchResult Search(string dbName, SearchRequest sr)
        {
            SearchResult val = null;
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                val = db.SearchGetIDs(sr);
            return val;
        }

        public string[] GetListDB()
        {
            return m_listDataName.ToArray();
        }

        public bool Login(string user, string pass)
        {
            bool ok = false;
            DataFile db = GetDF(typeof(USER).Name);
            if (db != null && db.Opened)
                ok = db.Exist("Username == @0 && Password == @1", user, pass);
            return ok;
        }

        public dbField[] GetFields(string dbName)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.Model.Fields;
            return new dbField[] { };
        }

        public DataFileInfoSelectTop GetInfoSelectTop(string dbName, int selectTop)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.GetInfoSelectTop(selectTop);
            return null;
        }
        public IList GetSelectPage(string dbName, int pageNumber, int pageSize)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.GetSelectPage(pageNumber, pageSize);
            return null;
        }

        public bool ExistModel(string dbName)
        {
            return m_listDataName.FindIndex(x => x.ToLower() == dbName.ToLower()) != -1;
        }

        public bool CreateDb(dbModel model)
        {
            DataFile df = new DataFile(model);
            if (df.Opened)
            {
                string name = df.Model.Name;
                if (m_dataFile.TryAdd(name, df))
                    m_listDataName.Add(name);
                return true;
            }
            return false;
        }

        public dbModel GetModel(string dbName)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.Model;
            return null;
        }

        public Type GetTypeDynamic(string dbName)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.TypeDynamic;
            return null;
        }

        public EditStatus AddItem(string dbName, Dictionary<string, object> data)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.AddItem(data);
            return EditStatus.NONE;
        }

        #endregion

    }

}
