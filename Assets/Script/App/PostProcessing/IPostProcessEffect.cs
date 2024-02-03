using UnityEngine;

namespace Framework.CMFR
{
    public interface IPostProcessEffect
    {
        void Init();
        void Update();
        void RenderEffect(RenderTexture source, RenderTexture destination);
        void OnDestroy();
    }
}