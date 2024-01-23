using UnityEngine;

namespace Framework.CMFR
{
    public interface IPostProcessEffect
    {
        void Init();
        void RenderEffect(RenderTexture source, RenderTexture destination);
        
    }
}