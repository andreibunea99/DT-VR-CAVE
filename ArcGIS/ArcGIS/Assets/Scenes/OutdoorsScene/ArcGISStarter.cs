using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Extent;
using Esri.GameEngine.Geometry;
using Esri.Unity;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;


[ExecuteAlways]

public class ArcGISStarter : MonoBehaviour
{

    public string APIKey = "AAPK42ec43041eae459681484b28c3717d959nOInKrx0g1TjNZkLTYTSb_37kpzaVuo9RbPlHXmowC5WoFfv0QszVzZRvm8xJv_";
    private ArcGISMapComponent arcGISMapComponent;
    private ArcGISPoint geographicCoordinates = new ArcGISPoint(-74.0147094213076, 40.7078110988167, 0, ArcGISSpatialReference.WGS84());

    private ArcGISCameraComponent cameraComponent;

    private void CreateArcGISMapComponent()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();

        if (!arcGISMapComponent)
        {
            var arcGISMapGameObject = new GameObject("ArcGISMap");
            arcGISMapComponent = arcGISMapGameObject.AddComponent<ArcGISMapComponent>();
        }

        arcGISMapComponent.OriginPosition = geographicCoordinates;
        arcGISMapComponent.MapType = Esri.GameEngine.Map.ArcGISMapType.Local;

        arcGISMapComponent.MapTypeChanged += new ArcGISMapComponent.MapTypeChangedEventHandler(CreateArcGISMap);
    }


    public void CreateArcGISMap()
    {
        var arcGISMap = new Esri.GameEngine.Map.ArcGISMap(arcGISMapComponent.MapType);
        arcGISMap.Basemap = new Esri.GameEngine.Map.ArcGISBasemap(Esri.GameEngine.Map.ArcGISBasemapStyle.ArcGISImagery, APIKey);

        arcGISMap.Elevation = new Esri.GameEngine.Map.ArcGISMapElevation(new Esri.GameEngine.Elevation.ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Terrain 3D", ""));

        // -74.0064534551796 40.6777129285649

        // 26.0483135512482
        // 44.4348282137568
        // 322.709812164307


        // -74.0147094213076
        // 40.7078110988167

        /*var layer_risc_seismic = new Esri.GameEngine.Layers.ArcGISImageLayer("https://services8.arcgis.com/SXiEEy1skwB5SrYh/arcgis/rest/services/Puncte_cladiri_risc_seismic/FeatureServer/0", "MyLayer_0", 1.0f, true, "");
        arcGISMap.Layers.Add(layer_risc_seismic);*/

        var layer_1 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/UrbanObservatory_NYC_TransitFrequency/MapServer", "MyLayer_1", 1.0f, true, "");
        arcGISMap.Layers.Add(layer_1);

        var layer_2 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/New_York_Industrial/MapServer", "MyLayer_2", 1.0f, true, "");
        arcGISMap.Layers.Add(layer_2);

        var layer_3 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/4yjifSiIG17X0gW4/arcgis/rest/services/NewYorkCity_PopDensity/MapServer", "MyLayer_3", 1.0f, true, "");
        arcGISMap.Layers.Add(layer_3);

        var buildingLayer = new Esri.GameEngine.Layers.ArcGIS3DObjectSceneLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Building Layer", 1.0f, true, "");
        arcGISMap.Layers.Add(buildingLayer);


        arcGISMapComponent.EnableExtent = true;

        var extentCenter = new Esri.GameEngine.Geometry.ArcGISPoint(-74.0147094213076, 40.7078110988167, 10, ArcGISSpatialReference.WGS84());
        var extent = new ArcGISExtentCircle(extentCenter, 10000);

        arcGISMap.ClippingArea = extent;

        arcGISMapComponent.View.Map = arcGISMap;
    }


    private void CreateArcGISCamera()
    {
        cameraComponent = Camera.main.gameObject.GetComponent<ArcGISCameraComponent>();

        if (!cameraComponent)
        {
            var cameraGameObject = Camera.main.gameObject;

            cameraGameObject.transform.SetParent(arcGISMapComponent.transform, false);

            cameraComponent = cameraGameObject.AddComponent<ArcGISCameraComponent>();

            cameraGameObject.AddComponent<ArcGISCameraControllerComponent>();

            cameraGameObject.AddComponent<ArcGISRebaseComponent>();
        }

        var cameraLocationComponent = cameraComponent.GetComponent<ArcGISLocationComponent>();

        if (!cameraLocationComponent)
        {
            cameraLocationComponent = cameraComponent.gameObject.AddComponent<ArcGISLocationComponent>();

            cameraLocationComponent.Position = geographicCoordinates;
            //cameraLocationComponent.Rotation = new ArcGISRotation(65, 68, 0);
        }
    }


    private void CreateSkyComponent()
    {
#if USE_HDRP_PACKAGE
		var currentSky = FindObjectOfType<UnityEngine.Rendering.Volume>();
		if (currentSky)
		{
			ArcGISSkyRepositionComponent skyComponent = currentSky.gameObject.GetComponent<ArcGISSkyRepositionComponent>();

			if (!skyComponent)
			{
				skyComponent = currentSky.gameObject.AddComponent<ArcGISSkyRepositionComponent>();
			}

			if (!skyComponent.arcGISMapComponent)
			{
				skyComponent.arcGISMapComponent = arcGISMapComponent;
			}

			if (!skyComponent.CameraComponent)
			{
				skyComponent.CameraComponent = cameraComponent;
			}
		}
#endif
    }


    // Start is called before the first frame update
    void Start()
    {
        CreateArcGISMapComponent();
        CreateArcGISCamera();
        CreateSkyComponent();
        CreateArcGISMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
