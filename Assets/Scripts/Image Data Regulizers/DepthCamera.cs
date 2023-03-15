using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

[RequireComponent(typeof(Camera))]
public class DepthCamera : MonoBehaviour
{
    public Shader opticalFlowShader;
    public float opticalFlowSensitivity = 1.0f;
    private Material opticalFlowMaterial;

    DepthTextureMode depthTextureMode;

    CapturePass cam;

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


    // Start is called before the first frame update
    void Start()
    {
        if (!opticalFlowShader)
            opticalFlowShader = Shader.Find("Hidden/OpticalFlow");


        cam.camera = GetComponent<Camera>();

        OnCameraChange();
        OnSceneChange();

    }

    // Update is called once per frame



    public void OnCameraChange()
    {
        int targetDisplay = 1;
        var mainCamera = GetComponent<Camera>();


        // cleanup capturing camera
        cam.camera.RemoveAllCommandBuffers();

        // copy all "main" camera parameters into capturing camera
        cam.camera.CopyFrom(mainCamera);

        // set targetDisplay here since it gets overriden by CopyFrom()
        cam.camera.targetDisplay = targetDisplay++;

        // cache materials and setup material properties
        if (!opticalFlowMaterial || opticalFlowMaterial.shader != opticalFlowShader)
            opticalFlowMaterial = new Material(opticalFlowShader);
        opticalFlowMaterial.SetFloat("_Sensitivity", opticalFlowSensitivity);
        // setup command buffers and replacement shaders

        var cb = new CommandBuffer();
        cb.Blit(null, BuiltinRenderTextureType.CurrentActive, opticalFlowMaterial);
        cam.camera.AddCommandBuffer(CameraEvent.AfterEverything, cb);
        cam.camera.depthTextureMode = (DepthTextureMode.Depth | DepthTextureMode.MotionVectors);
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


    void Update()
    {

    }
}
