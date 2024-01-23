using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CMFR
{
    public class PostProcessManager : MonoBehaviour
    {
        
        private List<IPostProcessEffect> effects = new List<IPostProcessEffect>();

        public RenderTexture texPass0;

        [Range(1,3)]
        public float _sigma;
        [Range(0.01f,0.99f)]
        public float _fx;
        [Range(0.01f,0.99f)]
        public float _fy;
        
        
        
        private void Start()
        {
            IPostProcessEffect cmfr = new CMFREffect();
            // IPostProcessEffect taa = new TAAEffect();
            // IPostProcessEffect bokeh = new BokehEffect();
            
            RegisterEffect( cmfr );
            // RegisterEffect( taa );
            // RegisterEffect( bokeh );

            foreach (var effect in effects)
            {
                effect.Init();
            }
            
            InitProperties();
        }


        public void RegisterEffect(IPostProcessEffect effect) {
            effects.Add(effect);
        }
        
        
        
        // 在Camera的OnRenderImage事件中调用
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            
            
            RenderTexture currentSource = source;
            RenderTexture currentDestination;

            for (int i = 0; i < effects.Count - 1; i++)
            {
                // 为每个效果创建一个新的destination纹理
                currentDestination = RenderTexture.GetTemporary(source.width, source.height);
            
                // 调用效果渲染
                effects[i].RenderEffect(currentSource, currentDestination);

                // 交换源和目标纹理，为下个效果准备
                if (currentSource != source)
                {
                    RenderTexture.ReleaseTemporary(currentSource);
                }

                currentSource = currentDestination;
            }

            // 最后一个效果直接渲染到最终的destination纹理
            effects[effects.Count - 1].RenderEffect(currentSource, destination);

            // 释放最后一个临时纹理（如果有使用的话）
            if (currentSource != source)
            {
                RenderTexture.ReleaseTemporary(currentSource);
            }
        }

        private void OnValidate()
        {
            Debug.Log("[Post Process Manager] OnValidate");
            UpdateModel();
        }

        private void InitProperties()
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
            
            // model -> view
            _sigma = model.sigma.Value;
            _fx = model.fx.Value;
            _fy = model.fy.Value;
            
            // view -> model 
            model.TexPass0.Value = texPass0;
        }
        private void UpdateModel()
        {
            if (_sigma == 0) return;
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
            model.sigma.Value = _sigma;
            model.fx.Value = _fx;
            model.fy.Value = _fy;
            

        }


    }
}