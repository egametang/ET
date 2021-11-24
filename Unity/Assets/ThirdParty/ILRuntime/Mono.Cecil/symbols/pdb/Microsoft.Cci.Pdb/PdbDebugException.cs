// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.Cci.Pdb {
  internal class PdbDebugException : IOException {
    internal PdbDebugException(String format, params object[] args)
      : base(String.Format(format, args)) {
    }
  }
}
