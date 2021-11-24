// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Cci.Pdb {
  internal struct PdbLine {
    internal uint offset;
    internal uint lineBegin;
    internal uint lineEnd;
    internal ushort colBegin;
    internal ushort colEnd;

    internal PdbLine(uint offset, uint lineBegin, ushort colBegin, uint lineEnd, ushort colEnd) {
      this.offset = offset;
      this.lineBegin = lineBegin;
      this.colBegin = colBegin;
      this.lineEnd = lineEnd;
      this.colEnd = colEnd;
    }
  }
}
