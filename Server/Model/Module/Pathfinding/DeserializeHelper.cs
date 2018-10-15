using System;
using System.Collections.Generic;
using PF;
using Guid = PF.Guid;

namespace ETModel
{
    public static class DeserializeHelper
    {
        public static NavGraph[] Load(string filePath)
        {
            byte[] bytes = AstarSerializer.LoadFromFile(filePath);
            
            AstarSerializer sr = new AstarSerializer();

            if (!sr.OpenDeserialize(bytes))
            {
                throw new Exception("Invalid data file (cannot read zip).\nThe data is either corrupt or it was saved using a 3.0.x or earlier version of the system");
            }

            var gr = new List<NavGraph>();

            // Set an offset so that the deserializer will load
            // the graphs with the correct graph indexes
            sr.SetGraphIndexOffset(gr.Count);

            gr.AddRange(sr.DeserializeGraphs());
            
            NavGraph[] graphs = gr.ToArray();

            sr.DeserializeEditorSettingsCompatibility();
            sr.DeserializeExtraInfo();

            //Assign correct graph indices.
            for (int i = 0; i < graphs.Length; i++) 
            {
                if (graphs[i] == null)
                {
                    continue;
                }
                int i1 = i;
                graphs[i].GetNodes(node => node.GraphIndex = (uint)i1);
            }

            for (int i = 0; i < graphs.Length; i++) 
            {
                for (int j = i+1; j < graphs.Length; j++) 
                {
                    if (graphs[i] != null && graphs[j] != null && graphs[i].guid == graphs[j].guid) 
                    {
                        graphs[i].guid = Guid.NewGuid();
                        break;
                    }
                }
            }

            sr.PostDeserialization();
            
            sr.CloseDeserialize();
            return graphs;
        }
    }
}