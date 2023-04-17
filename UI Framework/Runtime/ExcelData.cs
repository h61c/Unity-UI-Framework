/*using System.Data;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelDataReader;

public abstract class ExcelReader
{
    public string path;
    private DataSet data;
    public abstract int LoadData(int rowOffset, int colOffset);
    public abstract int SaveData(DataSet _data);
    public virtual DataTable GetTable(int index)    { return data.Tables[index]; }
    public virtual DataTable GetTable(string name)  { return data.Tables[name]; }
    public virtual DataRow GetRow(string table, int index)
    {
        return GetTable(table).Rows[index];
    }
}
*/