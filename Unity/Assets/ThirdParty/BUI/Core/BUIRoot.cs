using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BUI
{
    public class BUIRoot : BUIMember
    {
        public List<BUIMember> SearchMembers()
        {
            List<BUIMember> bUIMembers = new List<BUIMember>();
            AddMembers(transform, bUIMembers);
            return bUIMembers;
        }

        private void AddMembers(Transform root, List<BUIMember> members)
        {
            int count = root.childCount;
            if (count == 0)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                Transform child = root.GetChild(i);
                var member = child.GetComponent<BUIMember>();
                bool isRoot = false;
                if (member != null)
                {
                    if (member is BUIRoot)
                    {
                        isRoot = true;
                    }
                    if (members.Any(m => m.name == member.name))
                    {
                        throw new Exception($"一个BUIRoot下不允许有相同名字的BUIMember:{member.name},物体:{member.name}");
                    }
                    members.Add(member);
                }
                if (!isRoot)
                {
                    AddMembers(child, members);
                }
            }
        }
    }
}