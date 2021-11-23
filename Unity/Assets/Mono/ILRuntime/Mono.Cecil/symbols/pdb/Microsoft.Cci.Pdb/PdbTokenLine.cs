// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Cci.Pdb {
  internal class PdbTokenLine {
    internal uint token;
    internal uint file_id;
    internal uint line;
    internal uint column;
    internal uint endLine;
    internal uint endColumn;
    internal PdbSource sourceFile;
    internal PdbTokenLine/*?*/ nextLine;

    internal PdbTokenLine(uint token, uint file_id, uint line, uint column, uint endLine, uint endColumn) {
      this.token = token;
      this.file_id = file_id;
      this.line = line;
      this.column = column;
      this.endLine = endLine;
      this.endColumn = endColumn;
    }
  }
}
