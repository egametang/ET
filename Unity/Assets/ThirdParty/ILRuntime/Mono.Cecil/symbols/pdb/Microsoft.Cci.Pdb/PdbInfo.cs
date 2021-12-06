// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Cci.Pdb {

  /// <summary>
  /// This class represents the information read from a PDB file (both legacy Windows and Portable).
  /// </summary>
  internal class PdbInfo {
    /// <summary>
    /// Enumeration of per-function information contained in the PDB file.
    /// </summary>
    public PdbFunction[] Functions;

    /// <summary>
    /// Mapping from tokens to source files and line numbers.
    /// </summary>
    public Dictionary<uint, PdbTokenLine> TokenToSourceMapping;

    /// <summary>
    /// Source server data information.
    /// </summary>
    public string SourceServerData;

    /// <summary>
    /// Age of the PDB file is used to match the PDB against the PE binary.
    /// </summary>
    public int Age;

    /// <summary>
    /// GUID of the PDB file is used to match the PDB against the PE binary.
    /// </summary>
    public Guid Guid;

    /// <summary>
    /// Source link data information.
    /// </summary>
    public byte[] SourceLinkData;
  }
}
