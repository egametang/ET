using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace PSDUIImporter
{
    public class DropDownLayerImport : ILayerImport
    {
        PSDImportCtrl ctrl;
        public DropDownLayerImport(PSDImportCtrl ctrl)
        {
            this.ctrl = ctrl;
        }
        public void DrawLayer(Layer layer, GameObject parent)
        {
            throw new NotImplementedException();
        }
    }
}
