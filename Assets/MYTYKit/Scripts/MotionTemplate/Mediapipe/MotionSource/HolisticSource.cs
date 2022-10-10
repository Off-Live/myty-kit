using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Mediapipe;
using Mediapipe.Unity;
using MYTYKit.MotionTemplates.Mediapipe.Model;
using Debug = UnityEngine.Debug;

namespace MYTYKit.MotionTemplates.Mediapipe
{
    public class HolisticSource : MonoBehaviour
    {

        static int _Counter = 0;

        static readonly GlobalInstanceTable<int, HolisticSource> _InstanceTable =
            new GlobalInstanceTable<int, HolisticSource>(20);

        readonly int m_id;
        
        public MotionSource motionSource;

        public TextAsset configText;
        public TextAsset configWithoutHandText;
        public CameraSource imageSource;

        public bool inputFlipped = true;
        public bool trackingHands = false;

        public Vector3 landmarkScale = new Vector3(10, -10, 10);

        public UnityEvent<bool> detectedEvent;
        public UnityEvent<int> fpsEmitter;
        
        Stopwatch m_stopwatch;
        CalculatorGraph m_graph;

        UnityEvent<NormalizedLandmarkList> m_poseEvent = new();
        UnityEvent<NormalizedLandmarkList> m_faceEvent = new();
        UnityEvent<NormalizedLandmarkList> m_LHEvent = new();
        UnityEvent<NormalizedLandmarkList> m_RHEvent = new();
        UnityEvent<ClassificationList> m_emotionEvent = new();

        Color32[] m_pixels;

        ResourceManager m_resourceManager;

        int m_fpsCounter = 0;
        float m_fpsTimer = 0;

        Coroutine m_fpsRoutine;
        bool m_isQuit = false;

        int m_sourceWidth;
        int m_sourceHeight;
        Texture2D m_sourceTexture;

        [SerializeField] int m_targetFps = 10;

        HolisticSource()
        {
            m_id = System.Threading.Interlocked.Increment(ref _Counter);
            _InstanceTable.Add(m_id, this);
        }

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
            if (m_resourceManager == null) m_resourceManager = new StreamingAssetsResourceManager();

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
            sidePacket.Emplace("input_vertically_flipped", new BoolPacket(false));
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


            var mpImageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, m_sourceWidth, m_sourceHeight,
                m_sourceWidth * 4, m_sourceTexture.GetRawTextureData<byte>());
            m_graph.AddPacketToInputStream("input_video", new ImageFramePacket(mpImageFrame, GetCurrentTimestamp()))
                .AssertOk();
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
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        holistic.m_faceEvent.Invoke(face);
                        if (holistic.detectedEvent != null) holistic.detectedEvent.Invoke(true);
                    });

                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
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
            return m_stopwatch == null || !m_stopwatch.IsRunning
                ? -1
                : m_stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);
            //return TimeSpan.TicksPerMillisecond / 1000;
        }

        Timestamp GetCurrentTimestamp()
        {
            var microsec = GetCurrentTimestampMicrosec();
            return microsec < 0 ? Timestamp.Unset() : new Timestamp(microsec);
        }

        void ProcessPose(NormalizedLandmarkList pose)
        {
            var poseRig = motionSource.GetBridgesInCategory("PoseLandmark");
            foreach (var model in poseRig)
            {
                
                ProcessNormalized(model as MPBaseModel, pose.Landmark);
            }

        }
        void ProcessFace(NormalizedLandmarkList faceLM)
        {
            var faceRig = motionSource.GetBridgesInCategory("FaceLandmark");
            foreach (var model in faceRig)
            {
                ProcessNormalized(model as MPBaseModel, faceLM.Landmark);
            }
        }

        private void ProcessLH(NormalizedLandmarkList leftHand)
        {
            var leftHandRig = motionSource.GetBridgesInCategory("LeftHandLandmark");
            foreach (var model in leftHandRig)
            {
                ProcessNormalized(model as MPBaseModel, leftHand.Landmark);
            }
        }

        private void ProcessRH(NormalizedLandmarkList rightHand)
        {
            var rightHandRig = motionSource.GetBridgesInCategory("RightHandLandmark");
            foreach (var model in rightHandRig)
            {
                ProcessNormalized(model as MPBaseModel , rightHand.Landmark);
            }
        }

        private void ProcessEmotion(ClassificationList cflist)
        {
            foreach (var cls in cflist.Classification)
            {
                //Debug.Log("emotion "+ cls.Label);

            }
        }

        public void ProcessNormalized(MPBaseModel model, IList<NormalizedLandmark> landmarkList)
        {
            if (model == null || landmarkList == null) return;

            if (model.GetNumPoints() != landmarkList.Count)
            {
                model.Alloc(landmarkList.Count);
            }

            int index = 0;

            foreach (var elem in landmarkList)
            {
                model.SetPoint(index,
                    new Vector3(elem.X * landmarkScale.x, elem.Y * landmarkScale.y, elem.Z * landmarkScale.z));
                index++;
            }

        }


    }
}

