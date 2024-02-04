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
    public LiteDBProps(string orgName, string appNam)
    {
        this.filePath = Path.Combine(Dirs.ProfilePath(orgName, appNam), "settings.litedb");
        Dirs.PrepareForFile(this.filePath);
        this.connection = new LiteDatabase(new ConnectionString(this.filePath)
        {
            Connection = ConnectionType.Shared
        });
        this.collection = this.connection.GetCollection<Prop>("properties");
        // Nameをユニークインデックスにする
        this.collection.EnsureIndex(x => x.Name, true);
    }

    public dynamic? Get(string name)
    {
        var result = this.collection.Find(x => x.Name == name).FirstOrDefault();
        if (result == null) return null;
        return FromObject(result.Data);
    }
    public void Put(string name, dynamic? data)
    {
        var result = this.collection.Find(x => x.Name == name).FirstOrDefault();
        if (result == null)
        {
            result = new Prop { Name = name, Data = ToObject(data) };
            this.collection.Insert(result);
            return;
        }
        result.Data = ToObject(data);
        this.collection.Update(result);
    }

}
