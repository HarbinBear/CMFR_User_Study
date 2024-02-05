using System;
using UnityEngine;
using Unity.XR.PXR;

namespace Framework.CMFR
{
    public class EyeTrack : MonoBehaviour
    {
        public Transform Origin;

        private Vector3 combineEyeGazeVector;
        private Vector3 combineEyeGazeOriginOffset;
        private Vector3 combineEyeGazeOrigin;
        private Matrix4x4 headPoseMatrix;
        private Matrix4x4 originPoseMatrix;

        private Vector3 combineEyeGazeVectorInWorldSpace;
        private Vector3 combineEyeGazeOriginInWorldSpace;

        private void Start()
        {
            combineEyeGazeOriginOffset = Vector3.zero;
            combineEyeGazeVector = Vector3.zero;
            combineEyeGazeOrigin = Vector3.zero;
            originPoseMatrix = Origin.localToWorldMatrix;
        }

        private void Update()
        {
            CheckEyeTrackState();
            
            PXR_EyeTracking.GetHeadPosMatrix(out headPoseMatrix);
            PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector);
            PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin);
            //Translate Eye Gaze point and vector to world space
            combineEyeGazeOrigin += combineEyeGazeOriginOffset;
            combineEyeGazeOriginInWorldSpace = 
                originPoseMatrix.MultiplyPoint(
                    headPoseMatrix.MultiplyPoint(
                        combineEyeGazeOrigin
                    )
                );
            combineEyeGazeVectorInWorldSpace = 
                originPoseMatrix.MultiplyVector(
                    headPoseMatrix.MultiplyVector(
                        combineEyeGazeVector
                    )
                );

            GazeTargetControl( combineEyeGazeOrigin );

        }

        void CheckEyeTrackState()
        {
            // 当前应用需要眼动追踪能力
            TrackingStateCode trackingState;
            trackingState = (TrackingStateCode)PXR_MotionTracking.WantEyeTrackingService();

            // 查询当前设备是否支持眼动追踪
            EyeTrackingMode eyeTrackingMode = EyeTrackingMode.PXR_ETM_NONE;
            bool supported = false;
            int supportedModesCount = 0;
            trackingState = (TrackingStateCode)PXR_MotionTracking.GetEyeTrackingSupported(ref supported,ref supportedModesCount, ref eyeTrackingMode);

            // 获取眼动追踪状态
            bool tracking = false;
            EyeTrackingState eyeTrackingState = new EyeTrackingState();
            trackingState = (TrackingStateCode)PXR_MotionTracking.GetEyeTrackingState(ref tracking, ref eyeTrackingState);

            Debug.Log("[EyeTrack]" 
                      + "is support? " + supported + " ! " 
                      +" is tracking? " + tracking + " !" );
            
            
            // 开始眼动追踪
            EyeTrackingStartInfo info = new EyeTrackingStartInfo();
            info.needCalibration = 1;
            info.mode = eyeTrackingMode;
            trackingState = (TrackingStateCode)PXR_MotionTracking.StartEyeTracking(ref info);
        }

        void GazeTargetControl(Vector3 origin)
        {
            ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();

            Vector3 gazeView = Camera.main.WorldToViewportPoint(origin);

            model.eyeX.Value = gazeView.x;
            model.eyeY.Value = gazeView.y;

            Debug.Log("[EyeTrack] gaze point viewport: "
                      + gazeView.x 
                      + " , " 
                      + gazeView.y );
        }
        
        // void OnGUI()
        // {
        //     OnDrawFoveationPointGUI();
        // }
        //
        // void OnDrawFoveationPointGUI()
        // {
        //     ICMFRModel model = CMFRDemo.Interface.GetModel<ICMFRModel>();
        //
        //     Vector2 actualCoordinates = new Vector2(
        //         model.eyeX * Screen.width,
        //         (1.0f-model.eyeY) * Screen.height
        //     );
        //     GUI.DrawTexture(
        //         new Rect(actualCoordinates.x, actualCoordinates.y, 10, 10), 
        //         Texture2D.redTexture
        //     );
        // }
    }
}