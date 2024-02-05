using System;
using UnityEngine;

namespace Framework.CMFR
{
    public interface IRenderSystem : ISystem
    {
    }
    
    // public class RenderSystem : MonoBehaviour
    public class RenderSystem : AbstractSystem , IRenderSystem 
    {
        protected override void OnInit()
        {
            
            
        }

        

    }
}