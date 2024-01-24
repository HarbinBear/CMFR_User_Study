using System.Reflection;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Framework.CMFR
{
    public class CMFREffect : IPostProcessEffect
    {
        RenderTexture TexPass1;
        RenderTexture TexPass2;
        
        public Material MatPass1;
        public Material MatPass2;
        
        private int lastScreenWidth = 0;
        private int lastScreenHeight = 0;

        public void Init()
        {
            Debug.Log("[CMFR Effect] Init");
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
            
            // init material
            MatPass1 = new Material(Shader.Find("CMFR/CMFR_Pass"));
            MatPass2 = new Material(Shader.Find("CMFR/Inv_CMFR_Pass"));
            
            // init render texture
            float sigma = model.sigma.Value;
            TexPass1 = new RenderTexture(Mathf.RoundToInt(Screen.width / sigma), Mathf.RoundToInt(Screen.height / sigma), 24, RenderTextureFormat.Default);
            TexPass1.Create();

            TexPass2 = new RenderTexture(Mathf.RoundToInt(Screen.width), Mathf.RoundToInt(Screen.height), 24, RenderTextureFormat.Default);
            TexPass2.Create();
            
            // add model's listening
            model.sigma.Register(OnSigmaChanged);

        }



        public void Update()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                lastScreenHeight = Screen.height;
                lastScreenWidth = Screen.width;
                OnSigmaChanged( CMFRDemo.Interface.GetModel<ICMFRModel>().sigma );
                
            }
        }

        public void RenderEffect(RenderTexture source, RenderTexture destination)
        {
            CMFRPass( source  );
            InvCMFRPass( destination  );
        }

        public void OnDestroy()
        {
            
        }

        void CMFRPass(RenderTexture source)
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
            if (MatPass1 == null)
            {
                Debug.Log("[CMFREffect] MatPass1 missing");
                return;
            }
            MatPass1.SetFloat("_eyeX", model.eyeX);
            MatPass1.SetFloat("_eyeY", model.eyeY);
            MatPass1.SetFloat("_scaleRatio",model.sigma);
            MatPass1.SetFloat("_fx", model.fx);
            MatPass1.SetFloat("_fy", model.fy);
            MatPass1.SetInt("_MappingStrategy" , (int)model.mappingStrategy.Value);
            MatPass1.SetInt("_OutputMode", model.outputMode);
            MatPass1.SetInt("_iApplyRFRMap1", model.iApplyRFRMap1);

            
            
            Graphics.Blit( model.TexPass0 , TexPass1, MatPass1);
        }


        void InvCMFRPass(RenderTexture destination)
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
            if (MatPass2 == null)
            {
                Debug.Log("[CMFREffect] MatPass2 missing");
                return;
            }
            MatPass2.SetFloat("_eyeX", model.eyeX);
            MatPass2.SetFloat("_eyeY", model.eyeY);
            MatPass2.SetFloat("_scaleRatio",model.sigma);
            MatPass2.SetFloat("_fx", model.fx);
            MatPass2.SetFloat("_fy", model.fy);
            MatPass2.SetTexture("_MidTex", TexPass1);
            MatPass2.SetInt("_MappingStrategy" , (int) model.mappingStrategy.Value);
            MatPass2.SetInt("_OutputMode", model.outputMode);
            MatPass2.SetInt("_iApplyRFRMap2", model.iApplyRFRMap1);
            
            Graphics.Blit(null, destination, MatPass2);
        }

        void OnSigmaChanged(float sigma)
        {
            
            
            TexPass1.Release();
            TexPass1 = new RenderTexture(Mathf.RoundToInt(Screen.width / sigma), Mathf.RoundToInt(Screen.height / sigma), 24, RenderTextureFormat.Default);
            TexPass1.Create();
        }

        void OnModelChanged<T>(string name, T value)
        {
            
            PropertyInfo info = typeof(ICMFRModel).GetProperty(name);
            if (info != null)
            {
                
            } 
        }
        
        

    }
}