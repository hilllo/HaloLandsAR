// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// A Tagalong that stays at a fixed distance from the camera and always
    /// seeks to have a part of itself in the view frustum of the camera.
    /// </summary>
    [RequireComponent(typeof(BoxCollider), typeof(Interpolator))]
    public class SimpleTagalong : MonoBehaviour
    {
        // Simple Tagalongs seek to stay at a fixed distance from the Camera.
        [Tooltip("The distance in meters from the camera for the Tagalong to seek when updating its position.")]
        public float TagalongDistance = 2.0f;
        [Tooltip("If true, forces the Tagalong to be TagalongDistance from the camera, even if it didn't need to move otherwise.")]
        public bool EnforceDistance = true;

        [Tooltip("The speed at which to move the Tagalong when updating its position (meters/second).")]
        public float PositionUpdateSpeed = 9.8f;

        public float RotationUpdateSpeed = 30f;

        [Tooltip("When true, the Tagalong's motion is smoothed.")]
        public bool SmoothMotion = true;
        [Range(0.0f, 1.0f), Tooltip("The factor applied to the smoothing algorithm. 1.0f is super smooth. But slows things down a lot.")]
        public float SmoothingFactor = 0.75f;

        // The BoxCollider represents the volume of the object that is tagging
        // along. It is a required component.
        protected BoxCollider tagalongCollider;

        // The Interpolator is a helper class that handles various changes to an
        // object's transform. It is used by Tagalong to adjust the object's
        // transform.position.
        protected Interpolator interpolator;

        // This is an array of planes that define the camera's view frustum along
        // with some helpful indices into the array. The array is updated each
        // time through FixedUpdate().
        protected Plane[] frustumPlanes;
        protected const int frustumLeft = 0;
        protected const int frustumRight = 1;
        protected const int frustumBottom = 2;
        protected const int frustumTop = 3;

        private Vector3 lastCameraCenterPosition;
        public Vector3 destPosition;
        //[Range(0.0f, 1.0f), Tooltip("The factor applied to the small movement")]
        //public float InSightMoveDamp = 0.1f;


        private float raycastLength = 0f;
        private float colliderWidth = 0f;

        public bool faceForward = true;
        public bool stayYValue = true;

        void OnEnable()
        {
            // Make sure the Tagalong object has a BoxCollider.
            tagalongCollider = GetComponent<BoxCollider>();
            colliderWidth = tagalongCollider.bounds.size.x;

            // Get the Interpolator component and set some default parameters for
            // it. These parameters can be adjusted in Unity's Inspector as well.
            interpolator = gameObject.GetComponent<Interpolator>();
            interpolator.SmoothLerpToTarget = SmoothMotion;
            interpolator.SmoothPositionLerpRatio = SmoothingFactor;
            interpolator.PositionPerSecond = PositionUpdateSpeed;
            interpolator.RotationDegreesPerSecond = RotationUpdateSpeed;
            //Camera Position Init
            lastCameraCenterPosition = Camera.main.transform.position + Camera.main.transform.forward * TagalongDistance;
            destPosition = lastCameraCenterPosition;

            frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            Vector3 center = Camera.main.transform.position + Camera.main.transform.forward * TagalongDistance;
            Plane plane = frustumPlanes[frustumLeft];
            Ray ray = new Ray(center, -Camera.main.transform.right);

            plane.Raycast(ray, out raycastLength);

        }

        void Update()
        {
            // Retrieve the frustum planes from the camera.
            frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

            // Determine if the Tagalong needs to move based on whether its
            // BoxCollider is in or out of the camera's view frustum.
            Vector3 tagalongTargetPosition;
            CalculateTagalongTargetPosition(transform.position, out tagalongTargetPosition);

            destPosition = tagalongTargetPosition;

            Vector3 reletivePos = destPosition - Camera.main.transform.position;
            reletivePos.y = 0;
            Quaternion rotation;
            if (faceForward)
            {
                rotation = Quaternion.LookRotation(reletivePos, Vector3.up);
            }
            else
            {
                rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            }

            interpolator.SetTargetPosition(destPosition);
            interpolator.SetTargetRotation(rotation);
        }

        /// <summary>
        /// Determines if the Tagalong needs to move based on the provided
        /// position.
        /// </summary>
        /// <param name="fromPosition">Where the Tagalong is.</param>
        /// <param name="toPosition">Where the Tagalong needs to go.</param>
        /// <returns>True if the Tagalong needs to move to satisfy requirements; false otherwise.</returns>
        protected virtual void CalculateTagalongTargetPosition(Vector3 fromPosition, out Vector3 toPosition)
        {

            // Calculate a default position where the Tagalong should go. In this
            // case TagalongDistance from the camera along the gaze vector.
            Vector3 currentCameraCenter = Camera.main.transform.position + Camera.main.transform.forward * TagalongDistance;

            // Create a Ray and set it's origin to be the default toPosition that
            // was calculated above.
            Ray ray = new Ray(currentCameraCenter, Vector3.zero);

            bool moveRight = frustumPlanes[frustumLeft].GetDistanceToPoint(fromPosition) < -colliderWidth;
            bool moveLeft = frustumPlanes[frustumRight].GetDistanceToPoint(fromPosition) < -colliderWidth;

            if (moveRight || moveLeft)
            {
                if (moveRight)
                {
                    ray.direction = -Camera.main.transform.right;
                }
                else if (moveLeft)
                {
                    ray.direction = Camera.main.transform.right;
                }

                //lastCameraCenterPosition = currentCameraCenter;

                toPosition = ray.GetPoint(raycastLength - colliderWidth);
            }
            else
            {
                toPosition = destPosition;
                //Vector3 cameraPosDiff = currentCameraCenter - lastCameraCenterPosition;
                //lastCameraCenterPosition = currentCameraCenter;
                //toPosition += InSightMoveDamp * cameraPosDiff;
            }

            toPosition.y = Camera.main.transform.position.y;

            // Create a ray that starts at the camera and points in the direction
            // of the calculated toPosition.
            ray = new Ray(Camera.main.transform.position, toPosition - Camera.main.transform.position);

            // Find the point along that ray that is the right distance away and
            // update the calculated toPosition to be that point.
            toPosition = ray.GetPoint(TagalongDistance);

        }
    }
}