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
            InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            //  按o键或者有手柄扳机/A键，切换原图与压缩图
            bool bTrigger = false;

            if (Input.GetKeyDown(KeyCode.O) 
                ||( Input.GetKeyDown(KeyCode.JoystickButton0) ) )
            {
                model.bOriginal.Value = !model.bOriginal.Value ;
            }
            
            // change sigma by Q/E
            if (Input.GetKeyDown(KeyCode.Q))
                model.sigma.Value = model.sigma.Value <= 1.2f ? 1.2f : model.sigma.Value - 0.2f;
            if (Input.GetKeyDown(KeyCode.E))
                model.sigma.Value = model.sigma.Value >= 2.8f ? 2.8f : model.sigma.Value + 0.2f;
        
            // change eyeX,Y by A/D,W/S
            if (Input.GetKeyDown(KeyCode.A))
                model.eyeX.Value = model.eyeX.Value <= 0.05f ? 0.05f : model.eyeX.Value - 0.02f;
            if (Input.GetKeyDown(KeyCode.D))
                model.eyeX.Value = model.eyeX.Value >= 0.95f ? 0.95f : model.eyeX.Value + 0.02f;

            if (Input.GetKeyDown(KeyCode.W))
                model.eyeY.Value = model.eyeY.Value >= 0.95f ? 0.95f : model.eyeY.Value + 0.02f;
            if (Input.GetKeyDown(KeyCode.S))
                model.eyeY.Value = model.eyeY.Value <= 0.05f ? 0.05f : model.eyeY.Value - 0.02f;

            // change fx,fy by 上下，左右
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeFx( -0.02f );
            if (Input.GetKeyDown(KeyCode.RightArrow))
                ChangeFx( 0.02f );

            if (Input.GetKeyDown(KeyCode.UpArrow))
                ChangeFy( 0.02f );
            if (Input.GetKeyDown(KeyCode.DownArrow))
                ChangeFy( -0.02f );

            // 将fx，fy统一加减
            if (Input.GetKeyDown(KeyCode.Z))
            {
                ChangeFx(-0.05f);
                ChangeFy(-0.05f);
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                ChangeFx(0.05f);
                ChangeFy(0.05f);
            }
            
            //获取右手摇杆数据，作为注视点的偏移量
          
            Vector2 axis = new Vector2();
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis,out axis )  )
            {
                float deltaX = axis.x * 0.005f;
                float deltaY = axis.y * 0.005f;
                
                model.eyeX.Value += deltaX;
                model.eyeY.Value += deltaY;

                Math.Clamp(model.eyeX.Value, 0.05f, 0.95f);
                Math.Clamp(model.eyeY.Value, 0.05f, 0.95f);

            }

        }


        void ChangeFx(float delta)
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
            model.fx.Value += delta; 
            Math.Clamp(model.fx.Value, 0.05f, 0.95f);

        }
        
        void ChangeFy(float delta)
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
            model.fy.Value += delta; 
            Math.Clamp(model.fy.Value, 0.05f, 0.95f);

        }
    }
}