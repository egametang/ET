// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

#pragma warning disable 1591 // TODO: doc comments

namespace Microsoft.Cci.Pdb {

  internal sealed class PdbIteratorScope : ILocalScope {

    internal PdbIteratorScope(uint offset, uint length) {
      this.offset = offset;
      this.length = length;
    }

    public uint Offset {
      get { return this.offset; }
    }
    uint offset;

    public uint Length {
      get { return this.length; }
    }
    uint length;
  }
}