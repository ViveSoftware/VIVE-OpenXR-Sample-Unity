// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
#if DEFINE_VIVE_OPENXR
using VIVE.OpenXR.CompositionLayer;
using UnityEngine.InputSystem;
#endif


public class CompositionLayerManager : MonoBehaviour
{

		public GameObject layerAnchorGO, mainCameraGO, playerRigGO;

		public GameObject contentLayerTextureGameObjectRef, compositionLayerGameObjectRef;
		protected List<GameObject> contentLayerTextureGameObjects, compositionLayerGameObjects;

		private CompositionLayerManager compositionLayerManagerInstance = null;
		private float analogDetectionThreshold = 0.7f;
		private Vector3 layerAnchorOriginalPosition, layerAnchorOriginalRotation;
		private bool compositionLayerActive = true;

#if DEFINE_VIVE_OPENXR
	private ViveCompositionLayer compositionLayerFeature = null;
#endif

	// Start is called before the first frame update
	void Start()
		{
			if (contentLayerTextureGameObjects == null)
			{
				contentLayerTextureGameObjects = new List<GameObject>();
			}

			if (compositionLayerGameObjects == null)
			{
				compositionLayerGameObjects = new List<GameObject>();
			}

			if (contentLayerTextureGameObjects != null && compositionLayerGameObjects != null)
			{
				GameObject newContentLayerTextureGameObjectInstance = Instantiate(contentLayerTextureGameObjectRef);
				GameObject newCompositionLayerGameObjectInstance = Instantiate(compositionLayerGameObjectRef);
				newCompositionLayerGameObjectInstance.name = newCompositionLayerGameObjectInstance.name + " " + compositionLayerGameObjects.Count;
#if DEFINE_VIVE_OPENXR
				CompositionLayer compositionLayerComponent = newCompositionLayerGameObjectInstance.GetComponentInChildren<CompositionLayer>();
				if (compositionLayerComponent != null)
				{
					compositionLayerComponent.SetRenderPriority((uint)compositionLayerGameObjects.Count);
					compositionLayerComponent.compositionDepth = (uint)compositionLayerGameObjects.Count;
					compositionLayerComponent.trackingOrigin = playerRigGO;
				}
#endif
			GameObject newObjectContainer = new GameObject("Layer Content");
				RectTransform newObjectContainerTransform = newObjectContainer.AddComponent<RectTransform>();
				newObjectContainerTransform.sizeDelta = new Vector2(1, 0);
				newObjectContainer.transform.SetParent(layerAnchorGO.transform);

				newObjectContainer.transform.localPosition = Vector3.zero;
				newObjectContainer.transform.localRotation = Quaternion.identity;

				newContentLayerTextureGameObjectInstance.transform.SetParent(newObjectContainer.transform);
				newCompositionLayerGameObjectInstance.transform.SetParent(newObjectContainer.transform);

				newContentLayerTextureGameObjectInstance.transform.localPosition = new Vector3(0.5f, 0.0f, 0.0f);
				newContentLayerTextureGameObjectInstance.transform.localRotation = Quaternion.identity;

				newCompositionLayerGameObjectInstance.transform.localPosition = new Vector3(-0.5f, 0.0f, 0.0f);
				newCompositionLayerGameObjectInstance.transform.localRotation = Quaternion.identity;

				contentLayerTextureGameObjects.Add(newContentLayerTextureGameObjectInstance);
				compositionLayerGameObjects.Add(newCompositionLayerGameObjectInstance);

			}
		}

		

	}

