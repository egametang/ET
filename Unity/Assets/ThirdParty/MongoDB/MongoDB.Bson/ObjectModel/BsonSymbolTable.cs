/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents the symbol table of BsonSymbols.
    /// </summary>
    public static class BsonSymbolTable
    {
        // private static fields
        private static object __staticLock = new object();
        private static Dictionary<string, BsonSymbol> __symbolTable = new Dictionary<string, BsonSymbol>();

        // public static methods
        /// <summary>
        /// Looks up a symbol (and creates a new one if necessary).
        /// </summary>
        /// <param name="name">The name of the symbol.</param>
        /// <returns>The symbol.</returns>
        public static BsonSymbol Lookup(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            lock (__staticLock)
            {
                BsonSymbol symbol;
                if (!__symbolTable.TryGetValue(name, out symbol))
                {
                    symbol = new BsonSymbol(name);
                    __symbolTable[name] = symbol;
                }
                return symbol;
            }
        }
    }
}
