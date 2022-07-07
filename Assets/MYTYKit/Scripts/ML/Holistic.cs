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
    private readonly int _id;


    public TextAsset configText;
    public CameraSource imageSource;

    public List<RiggingModel> poseRig = new();
    public List<RiggingModel> faceRig = new();
    public List<RiggingModel> leftHandRig = new();
    public List<RiggingModel> rightHandRig = new();

    public bool inputFlipped = true;
    
    public Vector3 _landmarkScale = new Vector3(10, -10, 10);

    public UnityEvent<bool> detectedEvent;

    private Stopwatch _stopwatch;
    private Texture2D _sourceTexture;
    private CalculatorGraph _graph;

    private UnityEvent<NormalizedLandmarkList> m_poseEvent = new UnityEvent<NormalizedLandmarkList>();
    private UnityEvent<NormalizedLandmarkList> m_faceEvent = new UnityEvent<NormalizedLandmarkList>();
    private UnityEvent<NormalizedLandmarkList> m_LHEvent = new UnityEvent<NormalizedLandmarkList>();
    private UnityEvent<NormalizedLandmarkList> m_RHEvent = new UnityEvent<NormalizedLandmarkList>();
    private UnityEvent<ClassificationList> m_emotionEvent = new UnityEvent<ClassificationList>();

    private int _sourceWidth = 0;
    private int _sourceHeight = 0;

    private Color32[] _pixels;

    private ResourceManager _resourceManager;


    Holistic()
    {
        _id = System.Threading.Interlocked.Increment(ref _Counter);
        _InstanceTable.Add(_id, this);
    }
    

    // Start is called before the first frame update
    void Start()
    {


        _resourceManager = new StreamingAssetsResourceManager();

        _graph = new CalculatorGraph(configText.text);


        var sidePacket = new SidePacket();
        sidePacket.Emplace("input_rotation", new IntPacket(180));
        sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(inputFlipped));
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(false));
        sidePacket.Emplace("refine_face_landmarks", new BoolPacket(true));
        //_emotionPoller = _graph.AddOutputStreamPoller<ClassificationList>("face_emotions", true).Value();

        _graph.ObserveOutputStream("pose_landmarks", _id, PoseCallback, true).AssertOk();
        _graph.ObserveOutputStream("face_landmarks", _id, FaceCallback, true).AssertOk();
        _graph.ObserveOutputStream("left_hand_landmarks", _id, LHCallback, true).AssertOk();
        _graph.ObserveOutputStream("right_hand_landmarks", _id, RHCallback, true).AssertOk();
        _graph.ObserveOutputStream("face_emotions", _id, EmotionCallback, true).AssertOk();

        m_poseEvent.AddListener(ProcessPose);
        m_faceEvent.AddListener(ProcessFace);
        m_LHEvent.AddListener(ProcessLH);
        m_RHEvent.AddListener(ProcessRH);
        m_emotionEvent.AddListener(ProcessEmotion);

        _graph.StartRun(sidePacket).AssertOk();

        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    // Update is called once per frame
    void Update()
    {

        if (imageSource.camTexture == null || !imageSource.camTexture.didUpdateThisFrame) {
            if (detectedEvent != null) detectedEvent.Invoke(false);
            return;
        }

        if (_sourceWidth != imageSource.camTexture.width || _sourceHeight != imageSource.camTexture.height)
        {
            _sourceWidth = imageSource.camTexture.width;
            _sourceHeight = imageSource.camTexture.height;
            _pixels = new Color32[_sourceWidth * _sourceHeight];

            _sourceTexture = new Texture2D(_sourceWidth, _sourceHeight, TextureFormat.RGBA32, false);
        }

        imageSource.camTexture.GetPixels32(_pixels);
        if (_pixels == null)
        {
            Debug.Log("Null image");
            return;
        }
        _sourceTexture.SetPixels32(_pixels);
        _sourceTexture.Apply();
        

        var mpImageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _sourceWidth, _sourceHeight, _sourceWidth * 4, _sourceTexture.GetRawTextureData<byte>());
        _graph.AddPacketToInputStream("input_video", new ImageFramePacket(mpImageFrame, GetCurrentTimestamp())).AssertOk();

    }

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
                UnityMainThreadDispatcher.Instance().Enqueue(()=> holistic.m_poseEvent.Invoke(pose));
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
                UnityMainThreadDispatcher.Instance().Enqueue(()=> {
                    holistic.m_faceEvent.Invoke(face);
                    if (holistic.detectedEvent != null) holistic.detectedEvent.Invoke(true);
                });
                
            }else
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
                UnityMainThreadDispatcher.Instance().Enqueue(()=> holistic.m_LHEvent.Invoke(hand));
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
        _graph.CloseAllPacketSources().AssertOk();
        _graph.WaitUntilDone().AssertOk();
        _graph.Dispose();
    }

    long GetCurrentTimestampMicrosec()
    {
        return _stopwatch == null || !_stopwatch.IsRunning ? -1 : _stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);
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
            model.SetPoint(index, new Vector3(elem.X * _landmarkScale.x, elem.Y * _landmarkScale.y, elem.Z * _landmarkScale.z));
            index++;
        }

    }

   
}
