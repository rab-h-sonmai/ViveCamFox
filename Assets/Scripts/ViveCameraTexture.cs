using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Valve.VR;

public class ViveCameraTexture : MonoBehaviour {

    public Texture2D Texture
    {
        get { return m_texture; }
    }

    private Texture2D m_texture = null;
    private IntPtr m_frameBuffer = IntPtr.Zero;
    private uint m_frameBufferSize = 0;
    private CVRTrackedCamera m_camera = null;
    private ulong m_streamHandle = 0;
    private CameraVideoStreamFrameHeader_t m_frameHeader;
    private uint m_frameHeaderSize = 0;

    // Use this for initialization
    void Start()
    {
        m_camera = OpenVR.TrackedCamera;
        if (m_camera == null)
        {
            Debug.LogError("No camera found");
            return;
        }

        // First get the size of a frame
        uint width = 0;
        uint height = 0;
        uint bufferSize = 0;
        EVRTrackedCameraError cameraError = m_camera.GetCameraFrameSize(0, EVRTrackedCameraFrameType.Undistorted, ref width, ref height, ref bufferSize);
        if (cameraError != EVRTrackedCameraError.None)
        {
            Debug.LogError("Could not get frame size (error=" + cameraError + ")");
            return;
        }

        if (width * height == 0)
        {
            Debug.LogError("Frame size of 0, are you sure you've enabled the camera in the SteamVR settings panel?");
            return;
        }

        uint bitsPerPixel = bufferSize / (width * height);
        m_frameBufferSize = bufferSize;

        // Then get a handle to the stream
        cameraError = m_camera.AcquireVideoStreamingService(0, ref m_streamHandle);
        if (cameraError == EVRTrackedCameraError.None)
        {
            m_frameBuffer = Marshal.AllocHGlobal((int)bufferSize);
            m_frameHeader = new CameraVideoStreamFrameHeader_t();
            m_frameHeaderSize = (uint)Marshal.SizeOf(m_frameHeader);

            if (bitsPerPixel == 3)
                m_texture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
            else if (bitsPerPixel == 4)
                m_texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            // if there's some other texture format here, we simply won't be able to do anything in Update()

            if (m_texture && GetComponent<Renderer>())
                GetComponent<Renderer>().material.mainTexture = m_texture;
        }
        else
        {
            Debug.LogError("Could not acquire handle to stream (error=" + cameraError + ")");
        }
    }

    private void OnDestroy()
    {
        if (m_streamHandle != 0)
            m_camera.ReleaseVideoStreamingService(m_streamHandle);

        if (m_frameBuffer != IntPtr.Zero)
            Marshal.FreeHGlobal(m_frameBuffer);
    }

    void Update()
    {
        if (!m_texture)
            return;

        Valve.VR.EVRTrackedCameraError cameraError = m_camera.GetVideoStreamFrameBuffer(m_streamHandle, Valve.VR.EVRTrackedCameraFrameType.Undistorted, m_frameBuffer, m_frameBufferSize, ref m_frameHeader, m_frameHeaderSize);
        // Check for an error here Debug.Log("STREAM: error=" + cameraError);
        m_texture.LoadRawTextureData(m_frameBuffer, (int)m_frameBufferSize);
        m_texture.Apply();
    }

}
