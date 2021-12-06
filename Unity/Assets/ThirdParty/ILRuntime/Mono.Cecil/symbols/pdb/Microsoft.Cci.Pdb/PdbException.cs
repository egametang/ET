// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.Cci.Pdb {
  internal class PdbException : IOException {
    internal PdbException(String format, params object[] args)
      : base(String.Format(format, args)) {
    }
  }
}
