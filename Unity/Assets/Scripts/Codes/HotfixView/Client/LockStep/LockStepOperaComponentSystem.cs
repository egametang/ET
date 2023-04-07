using System;
using UnityEngine;

namespace ET.Client
{
    public static class LockStepOperaComponentSystem
    {
        [ObjectSystem]
        public class UpdateSystem: UpdateSystem<LockStepOperaComponent>
        {
            protected override void Update(LockStepOperaComponent self)
            {
                int degree = 0;
                if (Input.GetKeyDown(KeyCode.W))
                {
                    degree = 90;
                }
                
                if (Input.GetKeyDown(KeyCode.A))
                {
                    degree = 180;
                }
                
                if (Input.GetKeyDown(KeyCode.S))
                {
                    degree = 270;
                }
                
                if (Input.GetKeyDown(KeyCode.D))
                {
                    degree = 360;
                }
            }
        }
    }
}