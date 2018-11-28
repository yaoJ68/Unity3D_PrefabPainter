
#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;




namespace nTools.PrefabPainter
{
	static public class Styles
	{
		public static GUIStyle		iconLabelText;
		public static int			presetIconWidth  = 60;
		public static int			presetIconHeight = 72;
		public static int			presetWindowRows = 3;

		public static GUIStyle		boldFoldout;
		public static GUIStyle		precisePlaceTextStyle;


		public static Color32		colorBlue = new Color32 (62, 125, 231, 255);

		public static GUIContent	brushSizeText = new GUIContent("Size", "Brush radius in world units");

		public static string[]   toolNames = { "Paint", "Erase", "Settings" };



		static Styles()
		{
			iconLabelText = new GUIStyle(EditorStyles.miniLabel);
			iconLabelText.alignment = TextAnchor.LowerCenter;
			iconLabelText.clipping  = TextClipping.Clip;

			boldFoldout = new GUIStyle(EditorStyles.foldout);
			boldFoldout.fontStyle = FontStyle.Bold;

			precisePlaceTextStyle = new GUIStyle(EditorStyles.largeLabel);
			precisePlaceTextStyle.normal.textColor = Color.red;
			precisePlaceTextStyle.fontStyle = FontStyle.Bold;
			precisePlaceTextStyle.fontSize = 20;

		}

		public static string ShortenName(string name, GUIStyle style, int maxWidth)
		{			
			GUIContent ellipsis = new GUIContent("...");
			string shortName = "";

			float ellipsisSize = style.CalcSize(ellipsis).x;
			GUIContent textContent = new GUIContent("");

			foreach(char c in name.ToCharArray())
			{				
				textContent.text += c;

				float size = style.CalcSize(textContent).x;

				if (size > maxWidth - ellipsisSize)
				{
					shortName += ellipsis.text;
					break;
				}

				shortName += c;
			}

			return shortName;
		}
	}


    public enum OrientationMode
    {
        None,
        AlongSurfaceNormal,
        AlongBrushStroke,
        X,
        Y,
        Z,
    }

    public enum TransformMode
    {
        Relative,
        Absolute,
    }

    public enum Placement
    {
        World,
        HitObject,
        CustomObject,
    }

    public enum ScaleMode
    {
        Uniform,
        PerAxis,
    }

	public enum SurfaceCoords
	{
		AroundX,
		AroundY,
		AroundZ,
	}

    [System.Serializable]
    public class BrushPreset
    {        
		[SerializeField] private List<GameObject> prefabs = new List<GameObject>();

		[SerializeField] private string name = "";
		[NonSerialized]  private string _shortName = "";


        public float brushSize;
        public float eraseBrushSize;
        public float brushSpacing;

        public Vector3 positionOffset;

        public TransformMode    orientationTransformMode;
        public OrientationMode  orientationMode;
        public bool             flipOrientation;
        public Vector3          rotation;
        public Vector3          randomizeOrientation;

        public TransformMode    scaleTransformMode;
        public ScaleMode        scaleMode;
        public float            scaleUniformMin;
        public float            scaleUniformMax;
        public Vector3          scalePerAxisMin;
        public Vector3          scalePerAxisMax;

		public bool 			selected = false;



		[NonSerialized] private int       previewTextureAssetID; // handle undo changes 
		[NonSerialized] private Texture2D assetPreviewTexture = null;



        public GameObject prefab {
            get {
                return prefabs.Count > 0 ? prefabs[0] : null;
            }
        }


		public Texture2D prefabPreview {
			get
			{
				if (prefab == null)
				{
					assetPreviewTexture = null;
					return null;
				}
                
				if (assetPreviewTexture != null)
				{
					if (previewTextureAssetID != prefab.GetInstanceID())
						assetPreviewTexture = null;
					else
					{
						return assetPreviewTexture;
					}
				}

				assetPreviewTexture = AssetPreview.GetAssetPreview(prefab);
				if (assetPreviewTexture != null)
				{
					previewTextureAssetID = prefab.GetInstanceID();
					return assetPreviewTexture;
				}

				Texture2D previewTexture = AssetPreview.GetMiniThumbnail(prefab);
				if (previewTexture != null)
					return previewTexture;

				previewTexture = (Texture2D)AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(prefab));
				if (previewTexture != null)
					return previewTexture;

 				return AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
			}
		}



        public BrushPreset() { Reset(); }
        public BrushPreset(GameObject newPrefab) { Reset(); AssignPrefab(newPrefab);  }

        public BrushPreset(BrushPreset other)
        {
            Reset();

            prefabs = new List<GameObject>(other.prefabs);

			SetName(other.name);
            
			CopySettings(other);

			assetPreviewTexture = null;
        }

		public void AssignPrefab(GameObject newPrefab)
		{
			SetName(newPrefab.name);

			prefabs.Clear();
			prefabs.Add(newPrefab);

			assetPreviewTexture = null;
		}

		public void SetName(string newName)
		{
			if (newName != name)
			{
				name = newName;
				_shortName = "";
			}
		}

		public string GetName()
		{
			return name;
		}

		public string GetShortName()
		{
			if(_shortName.Length == 0 && name.Length > 0)
				_shortName = Styles.ShortenName(name, Styles.iconLabelText, Styles.presetIconWidth);

			return _shortName;
		}

		public void CopySettings(BrushPreset other)
		{
			brushSize = other.brushSize;
			eraseBrushSize = other.eraseBrushSize;
			brushSpacing = other.brushSpacing;

			positionOffset = other.positionOffset;

			orientationTransformMode = other.orientationTransformMode;
			orientationMode = other.orientationMode;
			flipOrientation = other.flipOrientation;
			rotation = other.rotation;
			randomizeOrientation = other.randomizeOrientation;

			scaleTransformMode = other.scaleTransformMode;
			scaleMode = other.scaleMode;
			scaleUniformMin = other.scaleUniformMin;
			scaleUniformMax = other.scaleUniformMax;
			scalePerAxisMin = other.scalePerAxisMin;
			scalePerAxisMax = other.scalePerAxisMax;
		}

        public void Reset()
        {
            brushSize = 1.0f;
            eraseBrushSize = 1.0f;
            brushSpacing = 0.5f;

            positionOffset = new Vector3(0 ,0 ,0);

            orientationTransformMode = TransformMode.Relative;
            orientationMode = OrientationMode.AlongSurfaceNormal;
            flipOrientation = false;
            rotation = new Vector3(0, 0, 0);
            randomizeOrientation = new Vector3(0, 0, 0);


            scaleTransformMode = TransformMode.Relative;
            scaleMode = ScaleMode.Uniform;
            scaleUniformMin = 1.0f;
            scaleUniformMax = 1.0f;
            scalePerAxisMin = new Vector3(1, 1, 1);
            scalePerAxisMax = new Vector3(1, 1, 1);
        }
    }





    //
    // class PrefabPainterSettings
    //
    public class PrefabPainterSettings : ScriptableObject
    {
        public bool paintOnSelected = false;
        public int  paintLayers = ~0;

        public bool overwritePrefabLayer = false;
        public int  prefabPlaceLayer = 0;

        public bool groupPrefabs = true;


		// Settings tab
        public float brushSizeMax = 20.0f;
        public float brushSpacingMax = 5.0f;
		public float precisePlaceSnapAngle = 15.0f;
		public SurfaceCoords surfaceCoords = SurfaceCoords.AroundX;

        public List<BrushPreset> presets = new List<BrushPreset>();


        public bool brushSettingsFoldout = true;
        public bool positionSettingsFoldout = true;
        public bool orientationSettingsFoldout = true;
        public bool scaleSettingsFoldout = true;
        public bool commonSettingsFoldout = true;


		[NonSerialized] private BrushPreset copyPreset = null;


        void OnEnable()
        {
        }




		public bool HasMultipleSelectedPresets()
		{
			int selectedCount = 0;
			for (int i = 0; i < presets.Count; i++)
			{
				if (presets[i].selected)
					selectedCount++;

				if (selectedCount > 1)
					return true;
			}
			return false;
		}

		public bool HasSelectedPresets()
		{
			for (int i = 0; i < presets.Count; i++)
			{
				if (presets[i].selected)
					return true;				
			}
			return false;
		}

		public BrushPreset GetFirstSelectedPreset()
		{
			for (int i = 0; i < presets.Count; i++)
			{
				if (presets[i].selected)
					return presets[i];
			}
			return null;
		}

		public bool IsPresetSelected(int presetIndex)
		{
			if (presetIndex >= 0 && presetIndex < presets.Count)
			{
				return presets[presetIndex].selected;
			}
			return false;
		}


		public void SelectPreset(int presetIndex)
		{
			if (presetIndex >= 0 && presetIndex < presets.Count)
			{
				presets.ForEach ((preset) => preset.selected = false);
				presets[presetIndex].selected = true;
			}
		}

		public void SelectPresetAdd(int presetIndex)
		{
			if (presetIndex >= 0 && presetIndex < presets.Count)
			{
				presets[presetIndex].selected = true;
			}
		}

		public void SelectPresetRange(int toPresetIndex)
		{
			if (toPresetIndex < 0 && toPresetIndex >= presets.Count)
				return;

			int rangeMin = toPresetIndex;
			int rangeMax = toPresetIndex;

			for (int i = 0; i < presets.Count; i++)
			{
				if (presets[i].selected)
				{
					rangeMin = Mathf.Min(rangeMin, i);
					rangeMax = Mathf.Max(rangeMax, i);
				}
			}
			for (int i = rangeMin; i <= rangeMax; i++) {
				presets[i].selected = true;
			}
		}

		public void DeselectAllPresets()
		{
			presets.ForEach ((preset) => preset.selected = false);
		}


		public void DuplicateSelectedPresets()
		{
			if (!HasSelectedPresets ())
				return;

			Undo.RegisterCompleteObjectUndo(this, "Duplicate Preset(s)");

			for (int presetIndex = 0; presetIndex < presets.Count; presetIndex++)
			{
				if (presets[presetIndex].selected)
				{
					BrushPreset duplicate = new BrushPreset (presets [presetIndex]);

					presets [presetIndex].selected = false;
					duplicate.selected = true;
					
					presets.Insert(presetIndex, duplicate);

					presetIndex++; // move over new inserted duplicate
				}
			}
		}

		public void DeleteSelectedPresets()
		{
			if (!HasSelectedPresets ())
				return;

			Undo.RegisterCompleteObjectUndo (this, "Delete Preset(s)");

			presets.RemoveAll ((preset) => preset.selected);
		}

		public void ResetSelectedPresets()
		{
			if (!HasSelectedPresets ())
				return;

			Undo.RegisterCompleteObjectUndo (this, "Reset Preset(s)");

			presets.ForEach ((preset) => preset.Reset());
		}


		public void ClipboardCopy()
		{
			BrushPreset preset = GetFirstSelectedPreset();
			copyPreset = null;

			if(preset != null)
			{
				copyPreset = new BrushPreset();
				copyPreset.CopySettings(preset);
			}
		}

		public void ClipboardPaste()
		{
			if (!HasSelectedPresets ())
				return;

			Undo.RegisterCompleteObjectUndo(this, "Paste Preset Settings");

			if(copyPreset != null)
			{
				for (int presetIndex = 0; presetIndex < presets.Count; presetIndex++)
				{
					if (presets[presetIndex].selected)
					{
						presets [presetIndex].CopySettings(copyPreset);
					}
				}
			}
		}

		public bool ClipboardIsCanPaste()
		{
			return copyPreset != null;
		}
    }

}

#endif //(UNITY_EDITOR)
