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

        // // sigma
        // [SerializeField, Range(1, 3)]
        // private float _sigma;
        // public float Sigma
        // {
        //     get => _sigma;
        //     set => UpdateModelValue(ref _sigma, value, nameof(Sigma));
        // }
        //
        // // FX
        // [SerializeField, Range(0.01f, 0.99f)]
        // private float _fx;
        // public float Fx
        // {
        //     get => _fx;
        //     set => UpdateModelValue(ref _fx, value, nameof(Fx));
        // }
        //
        // // FY
        // [SerializeField, Range(0.01f, 0.99f)]
        // private float _fy;
        // public float Fy
        // {
        //     get => _fy;
        //     set => UpdateModelValue(ref _fy, value, nameof(Fy));
        // }
        //
        
        // Instantiate and register all effects, and init them.
        private void Start()
        {
            Debug.Log("[Post Process Manager] Start");

            RegisterEffect( new CMFREffect() );
            
            foreach (var effect in effects)
            {
                effect.Init();
            }
            
            InitProperties();
            
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();

            model.sigma.Register(OnModelChanged);
            model.fx.Register(OnModelChanged);
            model.fy.Register(OnModelChanged);
        }

        // update all effects
        private void Update()
        {
            foreach (var effect in effects)
            {
                effect.Update();
            }
        }

        public void RegisterEffect(IPostProcessEffect effect) {
            effects.Add(effect);
        }
        
        
        
        // 后处理的总入口。
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

        // private void UpdateModelValue<T>(ref T field, T newValue, string modelName)
        // {
        //     if (!EqualityComparer<T>.Default.Equals(field, newValue))
        //     {
        //         field = newValue;
        //
        //         var model = CMFRDemo.Interface.GetModel<ICMFRModel>();
        //         var modelProperty = model.GetType().GetProperty(modelName);
        //
        //         if (modelProperty != null)
        //         {
        //             modelProperty.SetValue(model, newValue, null);
        //         }
        //         else
        //         {
        //             Debug.LogErrorFormat("Property '{0}' not found on model.", modelName);
        //         }
        //     }
        // }

        private void OnValidate()
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();

            if (_sigma == 0) return;
            
            model.sigma.Value = _sigma;
            model.fx.Value = _fx;
            model.fy.Value = _fy;
        }

        private void OnDestroy()
        {
            foreach (var effect in effects)
            {
                effect.OnDestroy();
            }
        }

        private void OnModelChanged<T>(T value)
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();

            _sigma = model.sigma.Value;
            _fx = model.fx.Value;
            _fy = model.fy.Value;
        }
    }
}