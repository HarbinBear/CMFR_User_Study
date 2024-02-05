using Kino;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.CMFR
{
    public class BokehEffect : IPostProcessEffect
    {
        public void Init()
        {
            Debug.Log("main camera name: " + Camera.main.name );
        }

        public void Update()
        {
            
        }

        public void RenderEffect(RenderTexture source, RenderTexture destination)
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();

            Bokeh bokeh = Camera.main.GetComponentInParent<Bokeh>();

            if (bokeh == null)
            {
                Debug.LogError("[BokehEffect] No Bokeh component.");
                return;
            }
            
            Ray ray = Camera.main.ScreenPointToRay(
                new Vector2(
                    model.eyeX * Camera.main.pixelWidth,
                    model.eyeY * Camera.main.pixelHeight
                )
            );
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.red, 2f);

            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hit_ViewSpace = Camera.main.WorldToViewportPoint(hit.point);
                float depth = hit_ViewSpace.z;
                model.focusDistance.Value = depth;
            }
            else
            {
                model.focusDistance.Value = 10000;
            }
            // Debug.Log("[BokehEffect] focus distance: " + model.focusDistance.Value);

            // CommandBuffer cmd = new CommandBuffer();
            // cmd.name = "Bokeh";
            bokeh.OnBokeh( source , destination );
            
            
        }

        public void OnDestroy()
        {
            
        }
    }
}