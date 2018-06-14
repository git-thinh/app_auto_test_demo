using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{
    public interface IDataFile
    {
        bool CreateDb(dbModel model);

        bool Login(string user, string pass);
        dbField[] GetFields(string dbName);
        dbModel GetModel(string dbName);
        Type GetTypeDynamic(string dbName);
        bool ExistModel(string dbName);

        DataFileInfoSelectTop GetInfoSelectTop(string dbName, int selectTop);
        IList GetSelectPage(string dbName, int pageNumber, int pageSize);
        string[] GetListDB();
        SearchResult Search(string dbName, SearchRequest sr);
        EditStatus AddItem(string dbName, Dictionary<string, object> data);
    }
}
