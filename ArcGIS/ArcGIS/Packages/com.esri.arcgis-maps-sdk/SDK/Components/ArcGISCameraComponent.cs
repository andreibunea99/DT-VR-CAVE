// COPYRIGHT 1995-2022 ESRI
// TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
// Unpublished material - all rights reserved under the
// Copyright Laws of the United States and applicable international
// laws, treaties, and conventions.
//
// For additional information, contact:
// Attn: Contracts and Legal Department
// Environmental Systems Research Institute, Inc.
// 380 New York Street
// Redlands, California 92373
// USA
//
// email: legal@esri.com
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.ArcGISMapsSDK.Utils.Math;
using Esri.GameEngine.Geometry;
using Esri.GameEngine.MapView;
using Esri.HPFramework;
using System;
using UnityEngine;
using Unity.Mathematics;

namespace Esri.ArcGISMapsSDK.Components
{
	[DisallowMultipleComponent]
	[ExecuteAlways]
	[AddComponentMenu("ArcGIS Maps SDK/ArcGIS Camera")]
	public class ArcGISCameraComponent : MonoBehaviour
	{
		public bool UpdateClippingPlanes = true;
		public bool UseCameraViewportProperties = true;

		private ArcGISMapComponent mapComponent;
		private Camera cameraComponent;

		public float verticalFov = 0;
		public float horizontalFov = 0;
		public uint viewportSizeX = 0;
		public uint viewportSizeY = 0;
		public float qualityScalingFactor = 1.0f;

		private float lastVerticalFov = 0;
		private float lastHorizontalFov = 0;
		private uint lastViewportSizeX = 0;
		private uint lastViewportSizeY = 0;
		private float lastQualityScalingFactor = 0;

		ArcGISPoint lastPosition = null;
		ArcGISRotation lastRotation;

		private void Initialize()
		{
			mapComponent = gameObject.GetComponentInParent<ArcGISMapComponent>();

			if (mapComponent == null)
			{
				Debug.LogError("Unable to find a parent ArcGISMapComponent.");

				enabled = false;
				return;
			}

			cameraComponent = GetComponent<Camera>();

#if UNITY_EDITOR
			// If the camera component is added after the map component this component
			// needs to determine if it should be active in editor mode
			if (!Application.isPlaying)
			{
				mapComponent.EnableMainCameraView(!(mapComponent.DataFetchWithSceneView));
			}
#endif
			mapComponent.CheckNumArcGISCameraComponentsEnabled();
		}

		private void OnEnable()
		{
			Initialize();
			mapComponent.EditorModeEnabledChanged += new ArcGISMapComponent.EditorModeEnabledChangedEventHandler(OnResetProperties);
		}

		private void OnDisable()
		{
			mapComponent.EditorModeEnabledChanged -= new ArcGISMapComponent.EditorModeEnabledChangedEventHandler(OnResetProperties);
		}

		private void OnTransformParentChanged()
		{
			Initialize();

			Update();
		}

		private void PushPosition()
		{
			if (!mapComponent)
			{
				return;
			}

			var map = mapComponent.Map;
			var spatialReference = mapComponent.View.SpatialReference;

			if (map == null || spatialReference == null)
			{
				return;
			}

			double3 worldPosition;
			quaterniond worldRotation;

			var hpTransform = GetComponent<HPTransform>();

			if (hpTransform != null)
			{
				worldPosition = hpTransform.UniversePosition;
				worldRotation = hpTransform.UniverseRotation.ToQuaterniond();
			}
			else
			{
				worldPosition = math.inverse(mapComponent.WorldMatrix).HomogeneousTransformPoint(transform.position.ToDouble3());
				worldRotation = quaternionExtensions.ToQuaterniond(mapComponent.UniverseRotation * transform.rotation);
			}

			var newGeographicPosition = mapComponent.View.WorldToGeographic(worldPosition);

			if (!newGeographicPosition.IsValid)
			{
				return;
			}

			var newGeographicRotation = GeoUtils.FromCartesianRotation(worldPosition, worldRotation, spatialReference, map.MapType);

			if (lastPosition == null || !lastRotation.Equals(newGeographicRotation) || !lastPosition.Equals(newGeographicPosition))
			{
				lastPosition = newGeographicPosition;
				lastRotation = newGeographicRotation;

				mapComponent.View.Camera = new ArcGISCamera(newGeographicPosition, newGeographicRotation);
			}

			if (UpdateClippingPlanes && cameraComponent != null)
			{
				var z = newGeographicPosition.Z;

				var near = Utils.FrustumHelpers.CalculateNearPlaneDistance(z, cameraComponent.fieldOfView, cameraComponent.aspect);
				var far = Math.Max(near, Utils.FrustumHelpers.CalculateFarPlaneDistance(z, map.MapType, spatialReference));

				cameraComponent.farClipPlane = (float)far;
				cameraComponent.nearClipPlane = (float)Math.Min(500000.0, near);
			}
		}

		private void PushViewportProperties()
		{
			if (!mapComponent)
			{
				return;
			}

			if (UseCameraViewportProperties && cameraComponent != null)
			{
				verticalFov = cameraComponent.fieldOfView;
				viewportSizeX = (uint)cameraComponent.pixelWidth;
				viewportSizeY = (uint)cameraComponent.pixelHeight;

				var vFovRadians = verticalFov * MathUtils.DegreesToRadians;

				// Comes from aspect_ratio = width / height = tan (hfov / 2) / tan (vfov / 2)
				var hFovRadians = 2.0 * Math.Atan(Math.Tan(vFovRadians / 2.0) * cameraComponent.aspect);

				horizontalFov = (float)(hFovRadians * MathUtils.RadiansToDegrees);
			}

			if (lastVerticalFov != verticalFov || lastHorizontalFov != horizontalFov || lastViewportSizeX != viewportSizeX || lastViewportSizeY != viewportSizeY || qualityScalingFactor != lastQualityScalingFactor)
			{
				lastVerticalFov = verticalFov;
				lastHorizontalFov = horizontalFov;
				lastViewportSizeX = viewportSizeX;
				lastViewportSizeY = viewportSizeY;
				lastQualityScalingFactor = qualityScalingFactor;

				if (viewportSizeX > 0 && viewportSizeY > 0 && horizontalFov > 0 && verticalFov > 0 && qualityScalingFactor > 0)
				{
					mapComponent.View.SetViewportProperties((uint)(qualityScalingFactor * viewportSizeX), (uint)(qualityScalingFactor * viewportSizeY), horizontalFov, verticalFov, 1);
				}
			}
		}

		private void OnResetProperties()
		{
			lastPosition = null;
			lastRotation = new ArcGISRotation(0, 0, 0);
			lastVerticalFov = 0;
			lastHorizontalFov = 0;
			lastViewportSizeX = 0;
			lastViewportSizeY = 0;
			lastQualityScalingFactor = 0;
		}

		private void Update()
		{
			if (mapComponent && mapComponent.ShouldEditorComponentBeUpdated())
			{
				PushViewportProperties();

				PushPosition();
			}
		}
	}
}
