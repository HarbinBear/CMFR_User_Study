using System;
using UnityEngine;

namespace Framework.CMFR
{
    public class Game : MonoBehaviour,IController
    {

        // public RenderTexture rt;
        private void Awake()
        {
            

        }
        

        private void OnDestroy()
        {
        }

        public IArchitecture GetArchitecture()
        {
            return CMFRDemo.Interface;
        }
    }
}