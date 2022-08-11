using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Mediapipe;
using Mediapipe.Unity;
using Debug = UnityEngine.Debug;

public class Holistic : MonoBehaviour
{

    private static int _Counter = 0;
    private static readonly GlobalInstanceTable<int, Holistic> _InstanceTable = new GlobalInstanceTable<int, Holistic>(20);
    private readonly int m_id;


    public TextAsset configText;
    public TextAsset configWithoutHandText;
    public CameraSource imageSource;

    public List<RiggingModel> poseRig = new();
    public List<RiggingModel> faceRig = new();
    public List<RiggingModel> leftHandRig = new();
    public List<RiggingModel> rightHandRig = new();

    public bool inputFlipped = true;
    public bool trackingHands = false;

    public Vector3 landmarkScale = new Vector3(10, -10, 10);

    public UnityEvent<bool> detectedEvent;
    public UnityEvent<int> fpsEmitter;

    private Stopwatch m_stopwatch;
    private CalculatorGraph m_graph;

    private UnityEvent<NormalizedLandmarkList> m_poseEvent = new UnityEvent<NormalizedLandmarkList>();
    private UnityEvent<NormalizedLandmarkList> m_faceEvent = new UnityEvent<NormalizedLandmarkList>();
    private UnityEvent<NormalizedLandmarkList> m_LHEvent = new UnityEvent<NormalizedLandmarkList>();
    private UnityEvent<NormalizedLandmarkList> m_RHEvent = new UnityEvent<NormalizedLandmarkList>();
    private UnityEvent<ClassificationList> m_emotionEvent = new UnityEvent<ClassificationList>();

    private Color32[] m_pixels;

    private ResourceManager _resourceManager;

    private int m_fpsCounter = 0;
    private float m_fpsTimer = 0;

    private Coroutine m_fpsRoutine;
    private bool m_isQuit = false;

    private int m_sourceWidth;
    private int m_sourceHeight;
    private Texture2D m_sourceTexture;

    [SerializeField]
    private int m_targetFps = 10;

    Holistic()
    {
        m_id = System.Threading.Interlocked.Increment(ref _Counter);
        _InstanceTable.Add(m_id, this);
    }


    // Start is called before the first frame update
    void Start()
    {
        OnRestartGraph(trackingHands);
    }

    public void SetTargetFps(int fps)
    {
        m_targetFps = fps;
    }

    public void OnRestartGraph(bool hands)
    {
        if (_resourceManager == null) _resourceManager = new StreamingAssetsResourceManager();

        var text = hands ? configText.text : configWithoutHandText.text;

        trackingHands = hands;

        if (m_graph == null)
        {
            m_graph = new CalculatorGraph(text);
        }
        else
        {
            m_graph.CloseAllPacketSources().AssertOk();
            m_graph.WaitUntilDone().AssertOk();
            m_graph.Dispose();
            m_graph = new CalculatorGraph(text);
        }

        var sidePacket = new SidePacket();
        sidePacket.Emplace("input_rotation", new IntPacket(180));
        sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(inputFlipped));
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(true));
#else
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(false));
#endif
        sidePacket.Emplace("refine_face_landmarks", new BoolPacket(true));
        sidePacket.Emplace("model_complexity", new IntPacket((int)0));

        m_graph.ObserveOutputStream("pose_landmarks", m_id, PoseCallback, true).AssertOk();
        m_graph.ObserveOutputStream("face_landmarks", m_id, FaceCallback, true).AssertOk();
        if (hands)
        {
            m_graph.ObserveOutputStream("left_hand_landmarks", m_id, LHCallback, true).AssertOk();
            m_graph.ObserveOutputStream("right_hand_landmarks", m_id, RHCallback, true).AssertOk();
        }
        m_graph.ObserveOutputStream("face_emotions", m_id, EmotionCallback, true).AssertOk();

        m_poseEvent.AddListener(ProcessPose);
        m_faceEvent.AddListener(ProcessFace);
        m_LHEvent.AddListener(ProcessLH);
        m_RHEvent.AddListener(ProcessRH);
        m_emotionEvent.AddListener(ProcessEmotion);

        m_graph.StartRun(sidePacket).AssertOk();

        m_stopwatch = new Stopwatch();
        m_stopwatch.Start();
    }

    IEnumerator FpsCount()
    {
        while (!m_isQuit)
        {
            yield return new WaitForSeconds(1.0f);
            fpsEmitter?.Invoke(m_fpsCounter);
            m_fpsCounter = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_fpsTimer += Time.deltaTime;
        if (m_fpsRoutine == null) m_fpsRoutine = StartCoroutine(FpsCount());

        if (imageSource.camTexture == null || !imageSource.camTexture.didUpdateThisFrame)
        {
            if (detectedEvent != null) detectedEvent.Invoke(false);
            return;
        }

        if (m_fpsTimer < 1.0 / m_targetFps) return;

        m_fpsTimer = 0;
        m_fpsCounter++;

        if (m_sourceWidth != imageSource.camTexture.width || m_sourceHeight != imageSource.camTexture.height)
        {
            m_sourceWidth = imageSource.camTexture.width;
            m_sourceHeight = imageSource.camTexture.height;
            m_pixels = new Color32[m_sourceWidth * m_sourceHeight];

            m_sourceTexture = new Texture2D(m_sourceWidth, m_sourceHeight, TextureFormat.RGBA32, false);
        }

        imageSource.camTexture.GetPixels32(m_pixels);
        if (m_pixels == null)
        {
            Debug.Log("Null image");
            return;
        }
        m_sourceTexture.SetPixels32(m_pixels);
        m_sourceTexture.Apply();


        var mpImageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, m_sourceWidth, m_sourceHeight, m_sourceWidth * 4, m_sourceTexture.GetRawTextureData<byte>());
        m_graph.AddPacketToInputStream("input_video", new ImageFramePacket(mpImageFrame, GetCurrentTimestamp())).AssertOk();
    }

    //void Update()
    //{



    //    if (_sourceWidth != imageSource.camTexture.width || _sourceHeight != imageSource.camTexture.height)
    //    {
    //        _sourceWidth = imageSource.camTexture.width;
    //        _sourceHeight = imageSource.camTexture.height;
    //        _pixels = new Color32[_sourceWidth * _sourceHeight];

    //        _sourceTexture = new Texture2D(_sourceWidth, _sourceHeight, TextureFormat.RGBA32, false);
    //    }

    //    imageSource.camTexture.GetPixels32(_pixels);
    //    if (_pixels == null)
    //    {
    //        Debug.Log("Null image");
    //        return;
    //    }
    //    _sourceTexture.SetPixels32(_pixels);
    //    _sourceTexture.Apply();


    //    var mpImageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _sourceWidth, _sourceHeight, _sourceWidth * 4, _sourceTexture.GetRawTextureData<byte>());
    //    _graph.AddPacketToInputStream("input_video", new ImageFramePacket(mpImageFrame, GetCurrentTimestamp())).AssertOk();

    //}

    [AOT.MonoPInvokeCallback(typeof(CalculatorGraph.NativePacketCallback))]
    private static IntPtr PoseCallback(IntPtr graphPtr, int streamId, IntPtr packetPtr)
    {
        var isFound = _InstanceTable.TryGetValue(streamId, out var holistic);
        if (!isFound)
        {
            return Status.FailedPrecondition("Invalid stream id").mpPtr;
        }
        using (var packet = new NormalizedLandmarkListPacket(packetPtr, false))
        {
            if (!packet.IsEmpty())
            {
                var pose = packet.Get();
                UnityMainThreadDispatcher.Instance().Enqueue(() => holistic.m_poseEvent.Invoke(pose));
            }
        }
        return Status.Ok().mpPtr;
    }

    [AOT.MonoPInvokeCallback(typeof(CalculatorGraph.NativePacketCallback))]
    private static IntPtr FaceCallback(IntPtr graphPtr, int streamId, IntPtr packetPtr)
    {
        var isFound = _InstanceTable.TryGetValue(streamId, out var holistic);
        if (!isFound)
        {
            return Status.FailedPrecondition("Invalid stream id").mpPtr;
        }
        using (var packet = new NormalizedLandmarkListPacket(packetPtr, false))
        {
            if (!packet.IsEmpty())
            {
                var face = packet.Get();
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    holistic.m_faceEvent.Invoke(face);
                    if (holistic.detectedEvent != null) holistic.detectedEvent.Invoke(true);
                });

            }
            else
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    if (holistic.detectedEvent != null) holistic.detectedEvent.Invoke(false);
                });
            }
        }
        return Status.Ok().mpPtr;
    }

    [AOT.MonoPInvokeCallback(typeof(CalculatorGraph.NativePacketCallback))]
    private static IntPtr LHCallback(IntPtr graphPtr, int streamId, IntPtr packetPtr)
    {
        var isFound = _InstanceTable.TryGetValue(streamId, out var holistic);
        if (!isFound)
        {
            return Status.FailedPrecondition("Invalid stream id").mpPtr;
        }
        using (var packet = new NormalizedLandmarkListPacket(packetPtr, false))
        {
            if (!packet.IsEmpty())
            {
                var hand = packet.Get();
                UnityMainThreadDispatcher.Instance().Enqueue(() => holistic.m_LHEvent.Invoke(hand));
            }
        }
        return Status.Ok().mpPtr;
    }

    [AOT.MonoPInvokeCallback(typeof(CalculatorGraph.NativePacketCallback))]
    private static IntPtr RHCallback(IntPtr graphPtr, int streamId, IntPtr packetPtr)
    {
        var isFound = _InstanceTable.TryGetValue(streamId, out var holistic);
        if (!isFound)
        {
            return Status.FailedPrecondition("Invalid stream id").mpPtr;
        }
        using (var packet = new NormalizedLandmarkListPacket(packetPtr, false))
        {
            if (!packet.IsEmpty())
            {
                var hand = packet.Get();
                UnityMainThreadDispatcher.Instance().Enqueue(() => holistic.m_RHEvent.Invoke(hand));
            }
        }
        return Status.Ok().mpPtr;
    }

    [AOT.MonoPInvokeCallback(typeof(CalculatorGraph.NativePacketCallback))]
    private static IntPtr EmotionCallback(IntPtr graphPtr, int streamId, IntPtr packetPtr)
    {
        var isFound = _InstanceTable.TryGetValue(streamId, out var holistic);
        if (!isFound)
        {
            return Status.FailedPrecondition("Invalid stream id").mpPtr;
        }
        using (var packet = new ClassificationListPacket(packetPtr, false))
        {
            if (!packet.IsEmpty())
            {
                var emotion = packet.Get();
                UnityMainThreadDispatcher.Instance().Enqueue(() => holistic.m_emotionEvent.Invoke(emotion));
            }
        }
        return Status.Ok().mpPtr;
    }


    private void OnDestroy()
    {
        m_isQuit = true;
        StopCoroutine(m_fpsRoutine);
        m_graph.CloseAllPacketSources().AssertOk();
        m_graph.WaitUntilDone().AssertOk();
        m_graph.Dispose();
    }

    long GetCurrentTimestampMicrosec()
    {
        return m_stopwatch == null || !m_stopwatch.IsRunning ? -1 : m_stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);
        //return TimeSpan.TicksPerMillisecond / 1000;
    }

    Timestamp GetCurrentTimestamp()
    {
        var microsec = GetCurrentTimestampMicrosec();
        return microsec < 0 ? Timestamp.Unset() : new Timestamp(microsec);
    }

    private void ProcessPose(NormalizedLandmarkList pose)
    {
        foreach (var model in poseRig)
        {
            ProcessNormalized(model, pose.Landmark);
        }

    }



    private void ProcessFace(NormalizedLandmarkList faceLM)
    {
        foreach (var model in faceRig)
        {
            ProcessNormalized(model, faceLM.Landmark);
        }
    }

    private void ProcessLH(NormalizedLandmarkList leftHand)
    {
        foreach (var model in leftHandRig)
        {
            ProcessNormalized(model, leftHand.Landmark);
        }
    }

    private void ProcessRH(NormalizedLandmarkList rightHand)
    {
        foreach (var model in rightHandRig)
        {
            ProcessNormalized(model, rightHand.Landmark);
        }
    }

    private void ProcessEmotion(ClassificationList cflist)
    {
        foreach (var cls in cflist.Classification)
        {
            //Debug.Log("emotion "+ cls.Label);

        }
    }

    public void ProcessNormalized(RiggingModel model, IList<NormalizedLandmark> landmarkList)
    {
        if (model == null || landmarkList == null) return;

        if (model.GetNumPoints() != landmarkList.Count)
        {
            model.Alloc(landmarkList.Count);
        }

        int index = 0;

        foreach (var elem in landmarkList)
        {
            model.SetPoint(index, new Vector3(elem.X * landmarkScale.x, elem.Y * landmarkScale.y, elem.Z * landmarkScale.z));
            index++;
        }

    }


}



//void Update()
//{

//    if (imageSource.camTexture == null || !imageSource.camTexture.didUpdateThisFrame) {
//        if (detectedEvent != null) detectedEvent.Invoke(false);
//        return;
//    }

//    if (_sourceWidth != imageSource.camTexture.width || _sourceHeight != imageSource.camTexture.height)
//    {
//        _sourceWidth = imageSource.camTexture.width;
//        _sourceHeight = imageSource.camTexture.height;
//        _pixels = new Color32[_sourceWidth * _sourceHeight];

//        _sourceTexture = new Texture2D(_sourceWidth, _sourceHeight, TextureFormat.RGBA32, false);
//    }

//    imageSource.camTexture.GetPixels32(_pixels);
//    if (_pixels == null)
//    {
//        Debug.Log("Null image");
//        return;
//    }
//    _sourceTexture.SetPixels32(_pixels);
//    _sourceTexture.Apply();


//    var mpImageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _sourceWidth, _sourceHeight, _sourceWidth * 4, _sourceTexture.GetRawTextureData<byte>());
//    _graph.AddPacketToInputStream("input_video", new ImageFramePacket(mpImageFrame, GetCurrentTimestamp())).AssertOk();

//}