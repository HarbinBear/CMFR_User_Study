using System;
using UnityEngine;
using UnityEngine.XR;

namespace Framework.CMFR
{
    public class KeyController : MonoBehaviour
    {
        private void Update()
        {
            KeyControll();
        }

        void KeyControll()
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();

            //  切换原图与压缩图
            if (Input.GetKeyDown(KeyCode.O))
            {
                model.bOriginal.Value = !model.bOriginal.Value ;
            }
            
            if (Input.GetKeyDown(KeyCode.Q))
                model.sigma.Value = model.sigma.Value <= 1.2f ? 1.2f : model.sigma.Value - 0.2f;
            if (Input.GetKeyDown(KeyCode.E))
                model.sigma.Value = model.sigma.Value >= 2.8f ? 2.8f : model.sigma.Value + 0.2f;
        
            if (Input.GetKeyDown(KeyCode.A))
                model.eyeX.Value = model.eyeX.Value <= 0.05f ? 0.05f : model.eyeX.Value - 0.02f;
            if (Input.GetKeyDown(KeyCode.D))
                model.eyeX.Value = model.eyeX.Value >= 0.95f ? 0.95f : model.eyeX.Value + 0.02f;

            if (Input.GetKeyDown(KeyCode.W))
                model.eyeY.Value = model.eyeY.Value >= 0.95f ? 0.95f : model.eyeY.Value + 0.02f;
            if (Input.GetKeyDown(KeyCode.S))
                model.eyeY.Value = model.eyeY.Value <= 0.05f ? 0.05f : model.eyeY.Value - 0.02f;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                model.fx.Value = model.fx.Value <= 0.1f ? 0.05f : model.fx.Value - 0.02f;
            if (Input.GetKeyDown(KeyCode.RightArrow))
                model.fx.Value = model.fx.Value >= 0.85f ? 0.9f : model.fx.Value + 0.02f;

            if (Input.GetKeyDown(KeyCode.UpArrow))
                model.fy.Value = model.fy.Value >= 0.85f ? 0.9f : model.fy.Value + 0.02f;
            if (Input.GetKeyDown(KeyCode.DownArrow))
                model.fy.Value = model.fy.Value <= 0.1f ? 0.05f : model.fy.Value - 0.02f;
            
            //获取扳机键是否被按下
            InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
          
            Vector2 axis = new Vector2();
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis,out axis )  )
            {
                float deltaX = axis.x * 0.01f;
                float deltaY = axis.y * 0.01f;
                
                model.eyeX.Value += deltaX;
                model.eyeY.Value += deltaY;

                Math.Clamp(model.eyeX.Value, 0.05f, 0.95f);
                Math.Clamp(model.eyeY.Value, 0.05f, 0.95f);



                // Debug.Log("[KeyControl] axis2D: " + axis.x + " , " + axis.y );
            }

            
            

            
        }
    }
}