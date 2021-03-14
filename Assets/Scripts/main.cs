using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;

public class main : MonoBehaviour
{

    // Kinectの制御
    private BackgroundDataProvider m_backgroundDataProvider;
    
    // kinectの取得データ
    public BackgroundData m_lastFrameData = new BackgroundData();

    public CanvasView canvasView;
    void Start()
    {
        SkeletalTrackingProvider m_skeletalTrackingProvider = new SkeletalTrackingProvider();

        //tracker ids needed for when there are two trackers
        const int TRACKER_ID = 0;
        m_skeletalTrackingProvider.StartClientThread(TRACKER_ID);
        m_backgroundDataProvider = m_skeletalTrackingProvider;
    }

    void Update()
    {
        if (m_backgroundDataProvider.IsRunning)
        {
            // 別スレッド処理で取得したフレーム情報
            if (m_backgroundDataProvider.GetCurrentFrameData(ref m_lastFrameData))
            {
                // 人が存在してれば
                if (m_lastFrameData.NumOfBodies != 0)
                {

                    if (canvasView) {
                        canvasView.UpdateImage(m_lastFrameData);
                    }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        // Stop background threads.
        if (m_backgroundDataProvider != null)
        {
            m_backgroundDataProvider.StopClientThread();
        }
    }
}
