using System;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using UnityEngine;

public class SkeletalTrackingProvider : BackgroundDataProvider
{
    bool readFirstFrame = false;
    TimeSpan initialTimestamp;

    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter { get; set; } = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

    public Stream RawDataLoggingFile = null;
    float MaximumDisplayedDepthInMillimeters = 5000;

    protected override void RunBackgroundThreadAsync(int id)
    {
        try
        {
            UnityEngine.Debug.Log("Starting body tracker background thread.");

            // キネクト所得データ初期化
            BackgroundData currentFrameData = new BackgroundData();
            
            // キネクトに接続しようとしている
            using (Device device = Device.Open(id))
            {
                // キネクトの初期設定
                device.StartCameras(new DeviceConfiguration()
                {
                    CameraFPS = FPS.FPS30,
                    ColorFormat = ImageFormat.ColorBGRA32,
                    ColorResolution = ColorResolution.R720p,
                    DepthMode = DepthMode.NFOV_Unbinned,
                    SynchronizedImagesOnly = true,
                });

                UnityEngine.Debug.Log("Open K4A device successful. id " + id + "sn:" + device.SerialNum );
                
                //キネクトのキャリブレーション情報を取得
                var deviceCalibration = device.GetCalibration();

                // BodyTrackingの初期設定
                using (Tracker tracker = Tracker.Create(
                    deviceCalibration,
                    new TrackerConfiguration() {
                        ProcessingMode = TrackerProcessingMode.Gpu,
                        SensorOrientation = SensorOrientation.Default
                    }))
                {
                    UnityEngine.Debug.Log("Body tracker created.");

                    // センサーのアップデート処理
                    // m_runBackgroundThread はアプリ起動中はずっとtrue
                    while (m_runBackgroundThread)
                    {
                        // kinectカメラのキャプチャーを取得
                        using (Capture sensorCapture = device.GetCapture())
                        {
                            // Queue latest frame from the sensor.
                            tracker.EnqueueCapture(sensorCapture);
                        }

                        // Try getting latest tracker frame.
                        // 前回のフレームと同じじゃなければ実行する
                        using (Frame frame = tracker.PopResult(TimeSpan.Zero, throwOnTimeout: false))
                        {
                            //フレームがうまく取れてない場合
                            // frame == null てっことはカメラに人が移ってない
                            if (frame == null)
                            {
                                UnityEngine.Debug.Log("Pop result from tracker timeout!");
                            }
                            else //フレームが正常に取れているとき
                            {
                                IsRunning = true;
                                // Get number of bodies in the current frame.
                                //人が移りこんでる人数
                                currentFrameData.NumOfBodies = frame.NumberOfBodies;

                                // Copy bodies.
                                // 人数分のbody情報を解析しています
                                for (uint i = 0; i < currentFrameData.NumOfBodies; i++)
                                {
                                    currentFrameData.Bodies[i].CopyFromBodyTrackingSdk(frame.GetBody(i), deviceCalibration);
                                }

                                // Store depth image.
                                // 深度カメラの情報取得しようとしている
                                Capture bodyFrameCapture = frame.Capture;
                                Image depthImage = bodyFrameCapture.Depth;
                                if (!readFirstFrame)
                                {
                                    readFirstFrame = true;
                                    initialTimestamp = depthImage.DeviceTimestamp;
                                }
                                currentFrameData.TimestampInMs = (float)(depthImage.DeviceTimestamp - initialTimestamp).TotalMilliseconds;
                                currentFrameData.DepthImageWidth = depthImage.WidthPixels;
                                currentFrameData.DepthImageHeight = depthImage.HeightPixels;

                                // Read image data from the SDK.
                                var depthFrame = MemoryMarshal.Cast<byte, ushort>(depthImage.Memory.Span);

                                // Repack data and store image data.
                                int byteCounter = 0;
                                currentFrameData.DepthImageSize = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight * 3;

                                for (int it = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight - 1; it > 0; it--)
                                {
                                    byte b = (byte)(depthFrame[it] / (MaximumDisplayedDepthInMillimeters) * 255);
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                }


                                // 深度カメラ
                                int byteCounter2 = 0;
                                currentFrameData._DepthImage = new Color32[currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight];
                                currentFrameData.DepthImageSize = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight * 4;

                                for (int it = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight - 1; it > 0; it--)
                                {
                                    byte b = (byte)(depthFrame[it] / (MaximumDisplayedDepthInMillimeters) * 255);
                                    currentFrameData._DepthImage[byteCounter2].r = b;
                                    currentFrameData._DepthImage[byteCounter2].g = b;
                                    currentFrameData._DepthImage[byteCounter2].b = b;
                                    currentFrameData._DepthImage[byteCounter2].a = 255;
                                    byteCounter2 += 1;
                                }

                                // カラーカメラ設定
                                Image ColorImage = bodyFrameCapture.Color;
                                currentFrameData.ColorImage = ColorImage.GetPixels<Color32>().ToArray();
                                currentFrameData.ColorImageWidth = ColorImage.WidthPixels;
                                currentFrameData.ColorImageHeight = ColorImage.HeightPixels;

                                if (RawDataLoggingFile != null && RawDataLoggingFile.CanWrite)
                                {
                                    binaryFormatter.Serialize(RawDataLoggingFile, currentFrameData);
                                }

                                // Update data variable that is being read in the UI thread.
                                SetCurrentFrameData(ref currentFrameData);
                            }

                        }
                    }
                    tracker.Dispose();
                }
                device.Dispose();
            }
            if (RawDataLoggingFile != null)
            {
                RawDataLoggingFile.Close();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
    }
}
