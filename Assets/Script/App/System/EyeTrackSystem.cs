using Unity.XR.PXR;

namespace Framework.CMFR
{
    public interface IEyeTrackSystem : ISystem
    {
    }
    
    // public class RenderSystem : MonoBehaviour
    public class EyeTrackingSystem : AbstractSystem , IEyeTrackSystem 
    {
        protected override void OnInit()
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

            // 开始眼动追踪
            EyeTrackingStartInfo info = new EyeTrackingStartInfo();
            info.needCalibration = 1;
            info.mode = eyeTrackingMode;
            trackingState = (TrackingStateCode)PXR_MotionTracking.StartEyeTracking(ref info);

            
        }

        

    }
}