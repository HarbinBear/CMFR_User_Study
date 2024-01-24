using UnityEngine;

namespace Framework.CMFR
{
    
    public enum OutputMode
    {
        Original,
        CMFR
    }

    

    public enum MappingStrategy
    {
        RMFR                        = 0 ,
        Elliptical_Grid_Mapping     = 1 ,
        FG_Squircular_Mapping       = 2 ,
        Two_Squircular_Mapping      = 3 ,

    }
    
    public interface ICMFRModel : IModel
    {
        BindableProperty<float> sigma { get;  }
        BindableProperty<float> fx { get;  }
        BindableProperty<float> fy { get;  }
        BindableProperty<float> eyeX { get;  }
        BindableProperty<float> eyeY { get;  }
        BindableProperty<float> scaleRatio { get;  }
        BindableProperty<float> validPercent { get;  }
        BindableProperty<float> sampleDensityWithNoise { get;  }
        BindableProperty<float> squelchedGridMappingBeta { get;  }
        BindableProperty<float> focusDistance { get; set; }
        BindableProperty<MappingStrategy> mappingStrategy { get;  }
        BindableProperty<int> iApplyRFRMap1 { get;  }
        BindableProperty<int> iApplyRFRMap2 { get;  }
        
        BindableProperty<int> outputMode { get;  }
        BindableProperty<bool> GM_On { get;  }
        BindableProperty<bool> TAA_On { get;  }
        BindableProperty<bool> Bokeh_On { get;  }
        BindableProperty<bool> FrustumJitter_On { get;  }

        BindableProperty<RenderTexture> TexPass0 { get; }

    }


    public class CMFRModel : AbstractModel, ICMFRModel
    {
        
        public BindableProperty<float> sigma { get; } = new BindableProperty<float>()
        {
            Value = 2.2f
        };
        public BindableProperty<float> fx { get; } = new BindableProperty<float>()
        {
            Value = 0.2f
        };
        public BindableProperty<float> fy { get; } = new BindableProperty<float>()
        {
            Value = 0.2f
        };
        public BindableProperty<float> eyeX { get; } = new BindableProperty<float>()
        {
            Value = 0.5f
        };
        public BindableProperty<float> eyeY { get; } = new BindableProperty<float>()
        {
            Value = 0.5f
        };
        public BindableProperty<float> squelchedGridMappingBeta { get; } = new BindableProperty<float>()
        {
            Value = 0.052f
        };
        public BindableProperty<float> sampleDensityWithNoise { get; } = new BindableProperty<float>()
        {
            Value = 1
        };
        public BindableProperty<float> focusDistance { get; set;  } = new BindableProperty<float>()
        {
            Value = 10
        };
        public BindableProperty<float> scaleRatio { get; } = new BindableProperty<float>()
        {
            Value = 1
        };
        public BindableProperty<float> validPercent { get; } = new BindableProperty<float>()
        {
            Value = 0.78f
        };
        public BindableProperty<int> iApplyRFRMap1 { get; } = new BindableProperty<int>()
        {
            Value = 1
        };        
        public BindableProperty<int> iApplyRFRMap2 { get; } = new BindableProperty<int>()
        {
            Value = 1
        };

        public BindableProperty<MappingStrategy> mappingStrategy { get; } = new BindableProperty<MappingStrategy>()
        {
            Value = MappingStrategy.Elliptical_Grid_Mapping
        }; 
        public BindableProperty<int> outputMode { get; } = new BindableProperty<int>()
        {
            Value = 1
        };
        
        public BindableProperty<bool> GM_On { get; } = new BindableProperty<bool>()
        {
            Value = false
        };        
        
        public BindableProperty<bool> TAA_On { get; } = new BindableProperty<bool>()
        {
            Value = false
        };       
        public BindableProperty<bool> Bokeh_On { get; } = new BindableProperty<bool>()
        {
            Value = true
        };    
        public BindableProperty<bool> FrustumJitter_On { get; } = new BindableProperty<bool>()
        {
            Value = false
        };

        public BindableProperty<RenderTexture> TexPass0 { get; } = new BindableProperty<RenderTexture>()
        {

        };

        protected override void OnInit()
        {

        
        }
    }
}