﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

namespace JsonDLL.Json.Linq
{
    /// <summary>
    /// Specifies how duplicate property names are handled when loading JSON.
    /// </summary>
    public enum DuplicatePropertyNameHandling
    {
        /// <summary>
        /// Replace the existing value when there is a duplicate property. The value of the last property in the JSON object will be used.
        /// </summary>
        Replace = 0,
        /// <summary>
        /// Ignore the new value when there is a duplicate property. The value of the first property in the JSON object will be used.
        /// </summary>
        Ignore = 1,
        /// <summary>
        /// Throw a <see cref="JsonReaderException"/> when a duplicate property is encountered.
        /// </summary>
        Error = 2
    }
}