// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Cci.Pdb {
  internal class PdbSlot {
    internal uint slot;
    internal uint typeToken;
    internal string name;
    internal ushort flags;
    //internal uint segment;
    //internal uint address;

    internal PdbSlot(uint slot, uint typeToken, string name, ushort flags)
    {
      this.slot = slot;
      this.typeToken = typeToken;
      this.name = name;
      this.flags = flags;
    }

    internal PdbSlot(BitAccess bits) {
      AttrSlotSym slot;

      bits.ReadUInt32(out slot.index);
      bits.ReadUInt32(out slot.typind);
      bits.ReadUInt32(out slot.offCod);
      bits.ReadUInt16(out slot.segCod);
      bits.ReadUInt16(out slot.flags);
      bits.ReadCString(out slot.name);

      this.slot = slot.index;
      this.typeToken = slot.typind;
      this.name = slot.name;
      this.flags = slot.flags;
      //this.segment = slot.segCod;
      //this.address = slot.offCod;

    }
  }
}
