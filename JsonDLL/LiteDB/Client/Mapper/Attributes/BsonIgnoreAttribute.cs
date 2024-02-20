using System;
using static JsonDLL.LiteDB.Constants;

namespace JsonDLL.LiteDB
{
    /// <summary>
    /// Indicate that property will not be persist in Bson serialization
    /// </summary>
    public class BsonIgnoreAttribute : Attribute
    {
    }
}