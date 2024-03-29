using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.IO;
using System.Collections.Generic;

// @TODO:
// . support custom color wheels in optical flow via lookup textures
// . support custom depth encoding
// . support multiple overlay cameras
// . tests
// . better example scene(s)

// @KNOWN ISSUES
// . Motion Vectors can produce incorrect results in Unity 5.5.f3 when
//      1) during the first rendering frame
//      2) rendering several cameras with different aspect ratios - vectors do stretch to the sides of the screen

/*
    MY NOTES:
    the capture precision is miliseconds.
    
*/




[RequireComponent(typeof(Camera))]
public class ImageCaptureScript : MonoBehaviour

{

    [Header("Shader Setup")]
    public Shader uberReplacementShader;
    public Shader opticalFlowShader;
    public float opticalFlowSensitivity = 1.0f;
    private bool saveImage = true;
    private bool saveDepth = true;
    private static string currentPath = Directory.GetCurrentDirectory();
    private string filepath = currentPath + "\\Assets\\Recordings\\ImageData";
    private static double timeInMilisecond = 0;
    private string filename = "capture_";
    //private Vector2 gameViewSize = UnityEditor.Handles.GetMainGameViewSize();

    [SerializeField]
    private int saveRateInMiliseconds = 1000;

    // pass configuration
    private CapturePass[] capturePasses = new CapturePass[] {
        new CapturePass() { name = "_img" },
        new CapturePass() { name = "_depth" },
    };

    struct CapturePass
    {
        // configuration
        public string name;
        public bool supportsAntialiasing;
        public bool needsRescale;
        public CapturePass(string name_) { name = name_; supportsAntialiasing = true; needsRescale = false; camera = null; }

        // impl
        public Camera camera;
    };

    // cached materials
    private Material opticalFlowMaterial;

    void Start()
    {
        // default fallbacks, if shaders are unspecified

        if (!opticalFlowShader)
            opticalFlowShader = Shader.Find("Hidden/OpticalFlow");

        // use real camera to capture final image
        capturePasses[0].camera = GetComponent<Camera>();

        capturePasses[1].camera = CreateHiddenCamera(capturePasses[1].name);

        OnCameraChange();
        
        #if UNITY_EDITOR
        if (DetectPotentialSceneChangeInEditor())
            OnSceneChange();
        #endif

        timeInMilisecond = 0; //a little off synchrone, it lags Time.realtimeSinceStartup amount of seconds
        var passfilename = Path.GetFileNameWithoutExtension(filename + timeInMilisecond);
        //Save(filename + timeInMilisecond, width: (int)gameViewSize.x, height: (int)gameViewSize.y, filepath);
        Save(passfilename, width: Screen.width, height: Screen.height, filepath); //!!A
    }

    //The capture times claimed may be erroneous under a certain treshold value for saveRateInMiliseconds, due to the way we chose to do increment in line 101 
    //This treshold value hasn't been determined yet and requires bruteforce testing.
    void Update()
    {
        #if UNITY_EDITOR
        if (DetectPotentialSceneChangeInEditor())
            OnSceneChange();
        #endif // UNITY_EDITOR

        // @TODO: detect if camera properties actually changed
        OnCameraChange();

        float currentTime = Time.realtimeSinceStartup * 1000;
        if (currentTime >= timeInMilisecond + saveRateInMiliseconds)
        {
            timeInMilisecond = timeInMilisecond + saveRateInMiliseconds;
            //Save(filename + time, width: (int)gameViewSize.x, height: (int)gameViewSize.y, filepath);
            var passfilename = Path.GetFileNameWithoutExtension(filename + timeInMilisecond);
            Save(passfilename, width: Screen.width, height: Screen.height, filepath);//!!A
        }
        
    }

    private Camera CreateHiddenCamera(string name)
    {
        var go = new GameObject(name, typeof(Camera));
        go.hideFlags = HideFlags.HideAndDontSave;
        go.transform.parent = transform;
        //go.depthTextureMode = true;
        var newCamera = go.GetComponent<Camera>();
        return newCamera;
    }


    static private void SetupCameraWithReplacementShader(Camera cam, Shader shader, ReplacementMode mode, Color clearColor)
    {
        var cb = new CommandBuffer();
        cb.SetGlobalFloat("_OutputMode", (int)mode); // @TODO: CommandBuffer is missing SetGlobalInt() method
        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
        cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
        cam.SetReplacementShader(shader, "");
        cam.backgroundColor = clearColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.allowHDR = false;
        cam.allowMSAA = false;
    }

    //BELOW IS IMPORTANT
    static private void SetupCameraWithPostShader(Camera cam, Material material, DepthTextureMode depthTextureMode = DepthTextureMode.None)
    {
        var cb = new CommandBuffer();
        cb.Blit(null, BuiltinRenderTextureType.CurrentActive, material);
        cam.AddCommandBuffer(CameraEvent.AfterEverything, cb);
        cam.depthTextureMode = depthTextureMode;
    }

    public enum ReplacementMode
    {
        ObjectId = 0,
        CatergoryId = 1,
        DepthCompressed = 2,
        DepthMultichannel = 3,
        Normals = 4
    };

    public void OnCameraChange()
    {
        int targetDisplay = 1;
        var mainCamera = GetComponent<Camera>();
        foreach (var pass in capturePasses)
        {
            if (pass.camera == mainCamera)
                continue;

            // cleanup capturing camera
            pass.camera.RemoveAllCommandBuffers();

            // copy all "main" camera parameters into capturing camera
            pass.camera.CopyFrom(mainCamera);

            // set targetDisplay here since it gets overriden by CopyFrom()
            pass.camera.targetDisplay = targetDisplay++;
        }

        // cache materials and setup material properties
        if (!opticalFlowMaterial || opticalFlowMaterial.shader != opticalFlowShader)
            opticalFlowMaterial = new Material(opticalFlowShader);
        opticalFlowMaterial.SetFloat("_Sensitivity", opticalFlowSensitivity);
        // setup command buffers and replacement shaders
        SetupCameraWithReplacementShader(capturePasses[1].camera, uberReplacementShader, ReplacementMode.DepthCompressed, Color.white);
    }


    public void OnSceneChange()
    {
        var renderers = Object.FindObjectsOfType<Renderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {
            var id = r.gameObject.GetInstanceID();
            var layer = r.gameObject.layer;
            var tag = r.gameObject.tag;

            mpb.SetColor("_ObjectColor", ColorEncoding.EncodeIDAsColor(id));
            mpb.SetColor("_CategoryColor", ColorEncoding.EncodeLayerAsColor(layer));
            r.SetPropertyBlock(mpb);
        }
    }

    public void Save(string filename, int width = -1, int height = -1, string path = "") //!!A
    {
        if (width <= 0 || height <= 0)
        {
            width = Screen.width;
            height = Screen.height;
        }

        var filenameExtension = System.IO.Path.GetExtension(filename);
        if (filenameExtension == "")
            filenameExtension = ".png";
        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        var pathWithoutExtension = Path.Combine(path, filenameWithoutExtension);

        // execute as coroutine to wait for the EndOfFrame before starting capture
        StartCoroutine(
            WaitForEndOfFrameAndSave(pathWithoutExtension, filenameExtension, width, height));
    }

    private IEnumerator WaitForEndOfFrameAndSave(string filenameWithoutExtension, string filenameExtension, int width, int height)
    {
        yield return new WaitForEndOfFrame();
        foreach (var pass in capturePasses)
        {
            // Perform a check to make sure that the capture pass should be saved
            if (
                (pass.name == "_img" && saveImage) ||
                (pass.name == "_depth" && saveDepth)
            )
            {
                string filename = filenameWithoutExtension + pass.name + filenameExtension;
                var mainCamera = GetComponent<Camera>();
                var depth = 24;
                var format = RenderTextureFormat.Default;
                var readWrite = RenderTextureReadWrite.Default;
                var antiAliasing = (pass.supportsAntialiasing) ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;

                var finalRT =
                    RenderTexture.GetTemporary(width, height, depth, format, readWrite, antiAliasing);
                var renderRT = (!pass.needsRescale) ? finalRT :
                    RenderTexture.GetTemporary(mainCamera.pixelWidth, mainCamera.pixelHeight, depth, format, readWrite, antiAliasing);
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

                var prevActiveRT = RenderTexture.active;
                var prevCameraRT = pass.camera.targetTexture;

                // render to offscreen texture (readonly from CPU side)
                RenderTexture.active = renderRT;
                pass.camera.targetTexture = renderRT;

                pass.camera.Render();

                if (pass.needsRescale)
                {
                    // blit to rescale (see issue with Motion Vectors in @KNOWN ISSUES)
                    RenderTexture.active = finalRT;
                    Graphics.Blit(renderRT, finalRT);
                    RenderTexture.ReleaseTemporary(renderRT);
                }

                // read offsreen texture contents into the CPU readable texture
                tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
                tex.Apply();
                pass.camera.targetTexture = prevCameraRT;


                // encode texture into PNG
                var bytes = tex.EncodeToPNG();
                yield return null;
                File.WriteAllBytes(filename, bytes);
                yield return null;
                // restore state and cleanup
                
                RenderTexture.active = prevActiveRT;

                Object.Destroy(tex);
                RenderTexture.ReleaseTemporary(finalRT);
            }

            yield return null;
        }
    }


    #if UNITY_EDITOR
    private GameObject lastSelectedGO;
    private int lastSelectedGOLayer = -1;
    private string lastSelectedGOTag = "unknown";
    private bool DetectPotentialSceneChangeInEditor()
    {
        bool change = false;
        // there is no callback in Unity Editor to automatically detect changes in scene objects
        // as a workaround lets track selected objects and check, if properties that are 
        // interesting for us (layer or tag) did not change since the last frame
        if (UnityEditor.Selection.transforms.Length > 1)
        {
            // multiple objects are selected, all bets are off!
            // we have to assume these objects are being edited
            change = true;
            lastSelectedGO = null;
        }
        else if (UnityEditor.Selection.activeGameObject)
        {
            var go = UnityEditor.Selection.activeGameObject;
            // check if layer or tag of a selected object have changed since the last frame
            var potentialChangeHappened = lastSelectedGOLayer != go.layer || lastSelectedGOTag != go.tag;
            if (go == lastSelectedGO && potentialChangeHappened)
                change = true;

            lastSelectedGO = go;
            lastSelectedGOLayer = go.layer;
            lastSelectedGOTag = go.tag;
        }

        return change;
    }
    #endif // UNITY_EDITOR
}