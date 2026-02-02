using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ScreenCaptureAgent.Core.Models;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;

namespace ScreenCaptureAgent.Core.Services;

/// <summary>
/// Screen capture service using Windows.Graphics.Capture API (WinRT)
/// Handles Chrome/Edge black screen issues properly
/// </summary>
public class WinRtCaptureService : IDisposable
{
    private IDirect3DDevice? _device;
    private SharpDX.Direct3D11.Device? _d3dDevice;
    private bool _isInitialized;

    public WinRtCaptureService()
    {
        try
        {
            InitializeDirect3D();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WinRT initialization failed: {ex.Message}");
            _isInitialized = false;
            throw;
        }
    }

    private void InitializeDirect3D()
    {
        // Create SharpDX Device
        _d3dDevice = new SharpDX.Direct3D11.Device(
            SharpDX.Direct3D.DriverType.Hardware,
            SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport);

        // Create IDirect3DDevice using the helper
        _device = Direct3D11Helper.CreateDevice(_d3dDevice);

        if (_device == null)
        {
            throw new InvalidOperationException("Failed to create IDirect3DDevice");
        }

        _isInitialized = true;
        Debug.WriteLine("WinRT Direct3D device initialized successfully");
    }

    public async Task<Bitmap?> CaptureWindowAsync(IntPtr windowHandle)
    {
        if (!_isInitialized || _device == null)
        {
            Debug.WriteLine("Direct3D device not initialized");
            return null;
        }

        try
        {
            var item = CaptureHelper.CreateItemForWindow(windowHandle);
            if (item == null)
            {
                Debug.WriteLine("Failed to create GraphicsCaptureItem for window");
                return null;
            }

            return await CaptureItemAsync(item);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WinRT window capture failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Bitmap?> CaptureMonitorAsync(IntPtr monitorHandle)
    {
        if (!_isInitialized || _device == null)
        {
            Debug.WriteLine("Direct3D device not initialized");
            return null;
        }

        try
        {
            var item = CaptureHelper.CreateItemForMonitor(monitorHandle);
            if (item == null)
            {
                Debug.WriteLine("Failed to create GraphicsCaptureItem for monitor");
                return null;
            }

            return await CaptureItemAsync(item);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WinRT monitor capture failed: {ex.Message}");
            return null;
        }
    }

    private async Task<Bitmap?> CaptureItemAsync(GraphicsCaptureItem item)
    {
        Direct3D11CaptureFramePool? framePool = null;
        GraphicsCaptureSession? session = null;

        try
        {
            framePool = Direct3D11CaptureFramePool.Create(
                _device!,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                item.Size);

            session = framePool.CreateCaptureSession(item);

            Bitmap? result = null;
            var frameCaptured = new TaskCompletionSource<bool>();

            framePool.FrameArrived += (sender, args) =>
            {
                try
                {
                    using var frame = sender.TryGetNextFrame();
                    if (frame != null)
                    {
                        result = ProcessFrame(frame);
                        frameCaptured.TrySetResult(true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Frame processing error: {ex.Message}");
                    frameCaptured.TrySetException(ex);
                }
            };

            session.StartCapture();

            var timeoutTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(frameCaptured.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.WriteLine("Capture timeout");
                return null;
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CaptureItemAsync error: {ex.Message}");
            return null;
        }
        finally
        {
            session?.Dispose();
            framePool?.Dispose();
        }
    }

    private Bitmap? ProcessFrame(Direct3D11CaptureFrame frame)
    {
        try
        {
            using var surfaceTexture = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface);
            if (surfaceTexture == null)
            {
                Debug.WriteLine("Failed to get surface texture");
                return null;
            }

            var description = surfaceTexture.Description;
            var bitmap = new Bitmap(description.Width, description.Height, PixelFormat.Format32bppArgb);

            var stagingDesc = new SharpDX.Direct3D11.Texture2DDescription
            {
                Width = description.Width,
                Height = description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = SharpDX.Direct3D11.ResourceUsage.Staging,
                BindFlags = SharpDX.Direct3D11.BindFlags.None,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None
            };

            using var stagingTexture = new SharpDX.Direct3D11.Texture2D(_d3dDevice!, stagingDesc);
            _d3dDevice!.ImmediateContext.CopyResource(surfaceTexture, stagingTexture);

            var dataBox = _d3dDevice.ImmediateContext.MapSubresource(
                stagingTexture, 0,
                SharpDX.Direct3D11.MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None);

            try
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                try
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        SharpDX.Utilities.CopyMemory(
                            bitmapData.Scan0 + y * bitmapData.Stride,
                            dataBox.DataPointer + y * dataBox.RowPitch,
                            bitmap.Width * 4);
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            finally
            {
                _d3dDevice.ImmediateContext.UnmapSubresource(stagingTexture, 0);
            }

            return bitmap;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Frame processing error: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        _d3dDevice?.Dispose();
        _device = null;
        _isInitialized = false;
    }
}

// Helper class for Direct3D interop
internal static class Direct3D11Helper
{
    [ComImport]
    [Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDirect3DDxgiInterfaceAccess
    {
        IntPtr GetInterface([In] ref Guid iid);
    }

    private static readonly Guid IInspectable = new Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90");
    private static readonly Guid ID3D11Texture2D = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

    public static IDirect3DDevice? CreateDevice(SharpDX.Direct3D11.Device d3dDevice)
    {
        try
        {
            // Get the DXGI device
            using var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();

            // Get the IInspectable pointer
            var inspectablePtr = GetDirect3DDeviceFromDXGIDevice(dxgiDevice.NativePointer);
            if (inspectablePtr == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                return MarshalInterface<IDirect3DDevice>.FromAbi(inspectablePtr);
            }
            finally
            {
                Marshal.Release(inspectablePtr);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateDevice error: {ex.Message}");
            return null;
        }
    }

    public static SharpDX.Direct3D11.Texture2D? CreateSharpDXTexture2D(IDirect3DSurface surface)
    {
        try
        {
            var access = surface.As<IDirect3DDxgiInterfaceAccess>();
            var guid = ID3D11Texture2D;
            var texturePtr = access.GetInterface(ref guid);

            if (texturePtr == IntPtr.Zero)
            {
                return null;
            }

            return new SharpDX.Direct3D11.Texture2D(texturePtr);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateSharpDXTexture2D error: {ex.Message}");
            return null;
        }
    }

    [DllImport("d3d11.dll", EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice", CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
    private static extern IntPtr GetDirect3DDeviceFromDXGIDevice(IntPtr dxgiDevice);
}

// Helper for creating GraphicsCaptureItem
internal static class CaptureHelper
{
    [ComImport]
    [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphicsCaptureItemInterop
    {
        IntPtr CreateForWindow([In] IntPtr window, [In] ref Guid iid);
        IntPtr CreateForMonitor([In] IntPtr monitor, [In] ref Guid iid);
    }

    public static GraphicsCaptureItem? CreateItemForWindow(IntPtr windowHandle)
    {
        try
        {
            var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            var guid = typeof(GraphicsCaptureItem).GUID;
            var itemPtr = interop.CreateForWindow(windowHandle, ref guid);

            if (itemPtr == IntPtr.Zero)
            {
                return null;
            }

            return MarshalInterface<GraphicsCaptureItem>.FromAbi(itemPtr);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateItemForWindow error: {ex.Message}");
            return null;
        }
    }

    public static GraphicsCaptureItem? CreateItemForMonitor(IntPtr monitorHandle)
    {
        try
        {
            var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            var guid = typeof(GraphicsCaptureItem).GUID;
            var itemPtr = interop.CreateForMonitor(monitorHandle, ref guid);

            if (itemPtr == IntPtr.Zero)
            {
                return null;
            }

            return MarshalInterface<GraphicsCaptureItem>.FromAbi(itemPtr);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateItemForMonitor error: {ex.Message}");
            return null;
        }
    }
}