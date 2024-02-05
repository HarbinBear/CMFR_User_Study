using UnityEngine;
using UnityEngine.Scripting;

namespace Framework.CMFR
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineViewController : MonoBehaviour
    {
        public Vector3 startPoint; // 设定线条的起点
        public Vector3 endPoint;   // 设定线条的终点

        private LineRenderer lineRenderer;

        void Start()
        {
            // 获取当前GameObject上的LineRenderer组件，如果没有则自动添加
            lineRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
        
            // 配置LineRenderer，例如设置线条颜色和宽度
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.green;
            lineRenderer.startWidth = 0.003f;
            lineRenderer.endWidth = 0.003f;
            lineRenderer.positionCount = 2; // 设置LineRenderer只有两个点
        }

        void Update()
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
            
            Ray ray = Camera.main.ScreenPointToRay(
                new Vector2(
                    model.eyeX * Camera.main.pixelWidth,
                    model.eyeY * Camera.main.pixelHeight
                )
            );
            
            RaycastHit hit;
            
            //  线的终点为碰撞点。
            if (Physics.Raycast(ray, out hit))
            {
                endPoint = hit.point; 
            }
            else
            {
                endPoint = ray.origin + ray.direction * 1000f; 
            }
            
            // 线的起点为ray的origin。
            startPoint = ray.origin + new Vector3(0 ,-0.01f , 0 );
            
            
            // 在每一帧更新线条的起点和终点
            if (startPoint != null && endPoint != null)
            {
                lineRenderer.SetPosition(0, startPoint); // 第一个点的位置
                lineRenderer.SetPosition(1, endPoint);   // 第二个点的位置
            }
        }
    }
}