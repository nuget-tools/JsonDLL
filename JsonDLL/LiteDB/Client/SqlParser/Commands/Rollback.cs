﻿using System;
using System.Collections.Generic;
using System.Linq;
using JsonDLL.LiteDB.Engine;
using static JsonDLL.LiteDB.Constants;

namespace JsonDLL.LiteDB
{
    internal partial class SqlParser
    {
        /// <summary>
        /// ROLLBACK [ TRANS | TRANSACTION ]
        /// </summary>
        private BsonDataReader ParseRollback()
        {
            _tokenizer.ReadToken().Expect("ROLLBACK");

            var token = _tokenizer.ReadToken().Expect(TokenType.Word, TokenType.EOF, TokenType.SemiColon);

            if (token.Is("TRANS") || token.Is("TRANSACTION"))
            {
                _tokenizer.ReadToken().Expect(TokenType.EOF, TokenType.SemiColon);
            }

            var result = _engine.Rollback();

            return new BsonDataReader(result);
        }
    }
}