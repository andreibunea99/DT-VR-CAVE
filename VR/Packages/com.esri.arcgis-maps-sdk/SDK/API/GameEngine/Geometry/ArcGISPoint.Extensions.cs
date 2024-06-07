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
using System;
using System.Collections.Generic;

namespace Esri.GameEngine.Geometry
{
	public partial class ArcGISPoint : ICloneable
	{
		public object Clone()
		{
			return new ArcGISPoint(X, Y, Z, (ArcGISSpatialReference)SpatialReference.Clone());
		}

		public override bool Equals(object obj)
		{
			return obj is ArcGISPoint point &&
				   Handle.Equals(point.Handle);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Handle);
		}

		public static bool operator ==(ArcGISPoint left, ArcGISPoint right)
		{
			return EqualityComparer<ArcGISPoint>.Default.Equals(left, right);
		}

		public static bool operator !=(ArcGISPoint left, ArcGISPoint right)
		{
			return !(left == right);
		}

		public bool IsValid
		{
			get
			{
				return IntPtr.Zero != Handle && !double.IsNaN(X) && !double.IsNaN(Y) && (HasZ || !double.IsNaN(Z)) && SpatialReference != null;
			}
		}
	}
}
