using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;
using Microsoft.Kinect.Face;
using Microsoft.Kinect;
using BesterUI.Helpers;

namespace BesterUI.DataCollectors
{
    class FaceCollector
    {
        private KinectSensor sensor = null;
        private BodyFrameSource bodySource = null;
        private BodyFrameReader bodyReader = null;
        private HighDefinitionFaceFrameSource highDefinitionFaceFrameSource = null;
        private HighDefinitionFaceFrameReader highDefinitionFaceFrameReader = null;

        private ulong currentTrackingId = 0;
        private Body currentTrackedBody = null;
        private FaceModel currentFaceModel = null;
        private FaceModelBuilder faceModelBuilder = null;
        private FaceAlignment currentFaceAlignment = null;

        private bool collectData = false;

        private FusionData fd;

        public bool CollectData
        {
            get
            {
                return collectData;
            }

            set
            {
                collectData = value;
            }
        }

        public event Action<bool, bool, bool, bool, bool> OnAskIfCaptured; //front left right tilted complete

        public FaceCollector(FusionData fd)
        {
            this.fd = fd;
        }

        public bool GetKinectStatus()
        {
            try
            {
                var s = KinectSensor.GetDefault();
                Log.LogMessageSameLine("Kinect isAvailable: " + s.IsAvailable);
                return s.IsAvailable;
            }

            catch (Exception e)
            {
                Log.LogMessageSameLine("Kinect isAvailable: Exception thrown");
                return false;
            }

        }

        public void PrepareSensor()
        {
            KinectSensor.GetDefault().Open();

            //Yes this is necessary in order to wait for the kinect to open fully.
            float kinectWait = 1000f;
            for (int i = 0; i < kinectWait; i++)
            {
                Log.LogMessageSameLine("Waiting for kinect (" + i / kinectWait * 100f + "/100)");
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < 5000)
            {
                if (this.GetKinectStatus())
                {
                    InitializeHDFace();
                    StartCollecting();
                    break;
                }
                else
                {
                    Log.LogMessageSameLine("KINECT ERROR: NO SENSOR AVAILABLE");
                }
            }

        }

        /// <summary>
        /// Returns the length of a vector from origin
        /// </summary>
        /// <param name="point">Point in space to find it's distance from origin</param>
        /// <returns>Distance from origin</returns>
        private static double VectorLength(CameraSpacePoint point)
        {
            var result = Math.Pow(point.X, 2) + Math.Pow(point.Y, 2) + Math.Pow(point.Z, 2);

            result = Math.Sqrt(result);

            return result;
        }

        /// <summary>
        /// Finds the closest body from the sensor if any
        /// </summary>
        /// <param name="bodyFrame">A body frame</param>
        /// <returns>Closest body, null of none</returns>
        private static Body FindClosestBody(BodyFrame bodyFrame)
        {
            Body result = null;
            double closestBodyDistance = double.MaxValue;

            Body[] bodies = new Body[bodyFrame.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    var currentLocation = body.Joints[JointType.SpineBase].Position;

                    var currentDistance = VectorLength(currentLocation);

                    if (result == null || currentDistance < closestBodyDistance)
                    {
                        result = body;
                        closestBodyDistance = currentDistance;
                    }
                }
            }

            return result;
        }




        /// <summary>
        /// Gets the current collection status
        /// </summary>
        /// <param name="status">Status value</param>
        /// <returns>Status value as text</returns>
        private void GetCollectionStatus(FaceModelBuilderCollectionStatus status)
        {
            bool frontCaptured, leftCaptured, rightCaptured, tiltedCaptured;

            Log.LogMessageSameLine((int)status);
            if ((int)status == 1)
            {
                frontCaptured = leftCaptured = rightCaptured = tiltedCaptured = false;
            }
            else
            {
                if ((status & FaceModelBuilderCollectionStatus.FrontViewFramesNeeded) != 0)
                {
                    frontCaptured = false;
                }
                else
                {
                    frontCaptured = true;
                }

                if ((status & FaceModelBuilderCollectionStatus.LeftViewsNeeded) != 0)
                {
                    leftCaptured = false;
                }
                else
                {
                    leftCaptured = true;
                }

                if ((status & FaceModelBuilderCollectionStatus.RightViewsNeeded) != 0)
                {
                    rightCaptured = false;
                }
                else
                {
                    rightCaptured = true;
                }

                if ((status & FaceModelBuilderCollectionStatus.TiltedUpViewsNeeded) != 0)
                {
                    tiltedCaptured = false;
                }
                else
                {
                    tiltedCaptured = true;
                }
            }

            if (OnAskIfCaptured != null)
                OnAskIfCaptured(frontCaptured, rightCaptured, leftCaptured, tiltedCaptured, false);

        }

        public void StartCollecting()
        {
            this.StopCollecting();

            this.faceModelBuilder = this.highDefinitionFaceFrameSource.OpenModelBuilder(FaceModelBuilderAttributes.None);

            this.faceModelBuilder.BeginFaceDataCollection();

            this.faceModelBuilder.CollectionCompleted += this.HdFaceBuilder_CollectionCompleted;
        }

        /// <summary>
        /// This event fires when the face capture operation is completed
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void HdFaceBuilder_CollectionCompleted(object sender, FaceModelBuilderCollectionCompletedEventArgs e)
        {
            Log.LogMessage("FaceTracking complete");
            if (OnAskIfCaptured != null)
                OnAskIfCaptured(true, true, true, true, true);

            var modelData = e.ModelData;

            this.currentFaceModel = modelData.ProduceFaceModel();

            this.faceModelBuilder.Dispose();
            this.faceModelBuilder = null;

            //The kinect is done preparing here.
        }

        /// <summary>
        /// Cancel the current face capture operation
        /// </summary>
        private void StopCollecting()
        {
            if (this.faceModelBuilder != null)
            {
                this.faceModelBuilder.Dispose();
                this.faceModelBuilder = null;
            }
        }


        /// <summary>
        /// This event is fired when a tracking is lost for a body tracked by HDFace Tracker
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void HdFaceSource_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            var lostTrackingID = e.TrackingId;

            if (this.currentTrackingId == lostTrackingID)
            {
                this.currentTrackingId = 0;
                this.currentTrackedBody = null;
                if (this.faceModelBuilder != null)
                {
                    this.faceModelBuilder.Dispose();
                    this.faceModelBuilder = null;
                }

                this.highDefinitionFaceFrameSource.TrackingId = 0;
            }
        }

        /// <summary>
        /// Initialize Kinect object
        /// </summary>
        public void InitializeHDFace()
        {

            this.sensor = KinectSensor.GetDefault();
            this.bodySource = this.sensor.BodyFrameSource;
            this.bodyReader = this.bodySource.OpenReader();
            this.bodyReader.FrameArrived += this.BodyReader_FrameArrived;

            this.highDefinitionFaceFrameSource = new HighDefinitionFaceFrameSource(this.sensor);
            this.highDefinitionFaceFrameSource.TrackingIdLost += this.HdFaceSource_TrackingIdLost;

            
            this.highDefinitionFaceFrameReader = this.highDefinitionFaceFrameSource.OpenReader();
            this.highDefinitionFaceFrameReader.FrameArrived += this.HdFaceReader_FrameArrived;

            this.highDefinitionFaceFrameSource.TrackingIdLost += (x, y) => Log.LogMessage("Lost tracking id " + y.TrackingId);

            this.currentFaceModel = new FaceModel();

            this.currentFaceAlignment = new FaceAlignment();

            this.sensor.Open();
        }


        /// <summary>
        /// This event is fired when a new HDFace frame is ready for consumption
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void HdFaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                // We might miss the chance to acquire the frame; it will be null if it's missed.
                // Also ignore this frame if face tracking failed
                if (frame == null || !frame.IsFaceTracked)
                {
                    return;
                }

                frame.GetAndRefreshFaceAlignmentResult(this.currentFaceAlignment);
                var captureValues = this.currentFaceAlignment.AnimationUnits;

                if (collectData)
                {
                    FaceDataReading f = new FaceDataReading(true);

                    //Format data from IReadOnlyDic -> Dictionary
                    f.AddData(captureValues);
                    fd.AddFaceData(f);
                }

            }
        }

        /// <summary>
        /// Check the face model builder status
        /// </summary>
        private void CheckOnBuilderStatus()
        {
            if (this.faceModelBuilder == null)
            {
                return;
            }

            //This is just the status of the builder, e.g. "NEEDVIEWLEFT";
            GetCollectionStatus(this.faceModelBuilder.CollectionStatus);
        }


        /// <summary>
        /// This event fires when a BodyFrame is ready for consumption
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            this.CheckOnBuilderStatus();

            var frameReference = e.FrameReference;
            using (var frame = frameReference.AcquireFrame())
            {
                if (frame == null)
                {
                    // We might miss the chance to acquire the frame, it will be null if it's missed
                    return;
                }

                if (this.currentTrackedBody != null)
                {
                    this.currentTrackedBody = FindBodyWithTrackingId(frame, this.currentTrackingId);

                    if (this.currentTrackedBody != null)
                    {
                        return;
                    }
                }

                Body selectedBody = FindClosestBody(frame);

                if (selectedBody == null)
                {
                    return;
                }
                this.currentTrackedBody = selectedBody;
                this.currentTrackingId = selectedBody.TrackingId;

                this.highDefinitionFaceFrameSource.TrackingId = this.currentTrackingId;
            }
        }

        /// <summary>
        /// Find if there is a body tracked with the given trackingId
        /// </summary>
        /// <param name="bodyFrame">A body frame</param>
        /// <param name="trackingId">The tracking Id</param>
        /// <returns>The body object, null of none</returns>
        private static Body FindBodyWithTrackingId(BodyFrame bodyFrame, ulong trackingId)
        {
            Body result = null;

            Body[] bodies = new Body[bodyFrame.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    if (body.TrackingId == trackingId)
                    {
                        result = body;
                        break;
                    }
                }
            }

            return result;
        }
    }

}

