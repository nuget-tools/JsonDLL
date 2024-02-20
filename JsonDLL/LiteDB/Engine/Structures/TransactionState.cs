using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using static JsonDLL.LiteDB.Constants;

namespace JsonDLL.LiteDB.Engine
{
    internal enum TransactionState
    {
        Active,
        Committed,
        Aborted,
        Disposed
    }
}