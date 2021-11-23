// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Cci.Pdb {
  internal class PdbSource {
    //internal uint index;
    internal string name;
    internal Guid doctype;
    internal Guid language;
    internal Guid vendor;
    internal Guid checksumAlgorithm;
    internal byte[] checksum;

    internal PdbSource(/*uint index, */string name, Guid doctype, Guid language, Guid vendor, Guid checksumAlgorithm, byte[] checksum) {
      //this.index = index;
      this.name = name;
      this.doctype = doctype;
      this.language = language;
      this.vendor = vendor;
      this.checksumAlgorithm = checksumAlgorithm;
      this.checksum = checksum;
    }
  }
}
