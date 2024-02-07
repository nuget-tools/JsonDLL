using JsonDLL;
using static JsonDLL.Util;
using LiteDB;

namespace JsonDLL;

public class LiteDBProps
{
    public class Prop
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public object Data { get; set; }
    }
    private string filePath = null;
    private LiteDatabase connection = null;
    ILiteCollection<Prop> collection = null;
    public LiteDBProps(DirectoryInfo di)
    {
        this.filePath = Path.Combine(di.FullName, "properties.litedb");
        Dirs.PrepareForFile(this.filePath);
        this.connection = new LiteDatabase(new ConnectionString(this.filePath)
        {
            Connection = ConnectionType.Shared
        });
        this.collection = this.connection.GetCollection<Prop>("properties");
        // Nameをユニークインデックスにする
        this.collection.EnsureIndex(x => x.Name, true);
    }
    public LiteDBProps(string orgName, string appNam) : this(new DirectoryInfo(Dirs.ProfilePath(orgName, appNam)))
    {
    }
    public dynamic? Get(string name)
    {
        this.connection.BeginTrans();
        var result = this.collection.Find(x => x.Name == name).FirstOrDefault();
        this.connection.Commit();
        if (result == null) return null;
        return FromObject(result.Data);
    }
    public void Put(string name, dynamic? data)
    {
        this.connection.BeginTrans();
        var result = this.collection.Find(x => x.Name == name).FirstOrDefault();
        if (result == null)
        {
            result = new Prop { Name = name, Data = ToObject(data) };
            this.collection.Insert(result);
            this.connection.Commit();
            return;
        }
        result.Data = ToObject(data);
        this.collection.Update(result);
        this.connection.Commit();
    }
    public void Delete(string name)
    {
        this.connection.BeginTrans();
        var result = this.collection.Find(x => x.Name == name).FirstOrDefault();
        if (result == null)
        {
            this.connection.Commit();
            return;
        }
        this.collection.Delete(result.Id);
        this.connection.Commit();
    }
    public void DeleteAll()
    {
        this.connection.BeginTrans();
        this.collection.DeleteAll();
        this.connection.Commit();
    }
}
