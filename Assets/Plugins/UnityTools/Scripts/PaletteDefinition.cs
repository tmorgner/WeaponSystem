using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    [CreateAssetMenu(menuName = "Unity Tools/Palette Definition")]
    public class PaletteDefinition : ScriptableObject, ISerializationCallbackReceiver
    {
        [Tooltip("The number of sub-divisions of the hue scale.")]
        public int HueCount;

        [Tooltip("The number of hue shifts performed for each hue value.")]
        public int HueShiftCount;

        [Range(0, 1)]
        [Tooltip("The range of hue shifts for each hue band instance.")]
        public float HueRange;

        [Range(0, 1)]
        [Tooltip("The range of value shifts for each hue band instance.")]
        public float ValueRange;

        [Range(0, 1)]
        [Tooltip("The range of saturation shifts for each hue band instance.")]
        public float SaturationRange;

        [Tooltip("The starting color for the generation process.")]
        public Color InitialColor;

        public AnimationCurve SaturationCurve;
        public AnimationCurve ValueCurve;

        [ReadOnly]
        [ShowAssetPreview(128, 128)]
        public Texture2D Texture;

        void Reset()
        {
            HueCount = 9;
            HueShiftCount = 6;
            ValueRange = 0.4f;
            SaturationRange = 0.4f;
            SaturationCurve = DefaultSaturationAnimationCurve();
            ValueCurve = DefaultValueCurve();
            DestroyExisting();
        }

        static AnimationCurve DefaultValueCurve()
        {
            return new AnimationCurve
            {
                keys = new[]
                {
                    new Keyframe(0, 0),
                    new Keyframe(0.2f, 0.6f),
                    new Keyframe(0.35f, 0.9f),
                    new Keyframe(0.45f, 1.0f),
                    new Keyframe(0.5f, 1.0f),
                    new Keyframe(0.55f, 1.0f),
                    new Keyframe(0.65f, 0.9f),
                    new Keyframe(0.8f, 0.6f),
                    new Keyframe(1, 0),
                }
            };
        }

        static AnimationCurve DefaultSaturationAnimationCurve()
        {
            return new AnimationCurve
            {
                keys = new[]
                {
                    new Keyframe(0, 0),
                    new Keyframe(0.15f, 0.6f),
                    new Keyframe(0.2f, 0.7f),
                    new Keyframe(0.25f, 0.75f),
                    new Keyframe(0.50f, 0f),
                    new Keyframe(0.75f, 0.75f),
                    new Keyframe(0.8f, 0.7f),
                    new Keyframe(0.85f, 0.6f),
                    new Keyframe(1, 0),
                }
            };
        }


#if UNITY_EDITOR
        List<Texture2D> ListSubAssets()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            if (assetPath == null)
            {
                return new List<Texture2D>();
            }

            var a = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            return a.Where(x => x is Texture2D).Cast<Texture2D>().ToList();
        }

        [Button("Generate Palette")]
        public void GeneratePalette()
        {
            DestroyExisting();

            if (HueCount <= 0 || HueShiftCount <= 1)
            {
                Texture = null;
                return;
            }

            var tx = new Texture2D(HueShiftCount, HueCount);
            tx.anisoLevel = 0;
            tx.filterMode = FilterMode.Point;
            tx.wrapMode = TextureWrapMode.Clamp;

            for (var h = 0; h < HueCount; h += 1)
            {
                var hue = 0f;
                var sat = 0f;
                var val = 0f;
                Color.RGBToHSV(InitialColor, out hue, out sat, out val);
                hue = Mathf.Repeat(hue + h * 1.0f / HueCount, 1);

                for (var hs = 0; hs < HueShiftCount; hs += 1)
                {
                    var pct = (float)hs / (HueShiftCount - 1);
                    var hue2 = ComputeShiftedHue(pct, hue);
                    var sat2 = ComputeShiftedSat(pct);
                    var val2 = ComputeShiftedValue(pct);
                    tx.SetPixel(hs, h, Color.HSVToRGB(hue2, sat2, val2));
                }
            }

            tx.Apply();
            tx.name = "Palette Texture";

            AssetDatabase.AddObjectToAsset(tx, this);

            Texture = tx;
        }

        [Button]
        public void ExportPalette()
        {
            var path = AssetDatabase.GetAssetPath(this);
            string filename;
            if (path == null)
            {
                filename = "Palette.png";
            }
            else
            {
                var p = Path.GetFileNameWithoutExtension(path);
                filename = p + ".png";
            }

            var target = EditorUtility.SaveFilePanel("Save Palette", ".", filename, ".png");
            if (target == null)
            {
                return;
            }

            if (Texture == null)
            {
                GeneratePalette();
            }

            if (Texture == null)
            {
                return;
            }

            var pngData = Texture.EncodeToPNG();
            File.WriteAllBytes(target, pngData);
        }

        float ComputeShiftedValue(float pct)
        {
            var satAdjustment = ValueRange * ValueCurve.Evaluate(pct);
            var hue2 = Mathf.Clamp01(satAdjustment);
            return hue2;
        }

        float ComputeShiftedSat(float pct)
        {
            var satAdjustment = SaturationRange * SaturationCurve.Evaluate(pct);
            var hue2 = Mathf.Clamp01(satAdjustment);
            return hue2;
        }

        /// <summary>
        ///  Hue is shifted in two bands, mirrored at the midpoint.
        /// </summary>
        /// <param name="pct"></param>
        /// <param name="hue"></param>
        /// <returns></returns>
        float ComputeShiftedHue(float pct, float hue)
        {
            // map from 0..1 into Abs(-1..+1)
            var pctMapped = Mathf.Abs((pct - 0.5f) * 2);
            // then map that from 0..1 to -0.5..+0.5 to give a mid point shift
            var pctMidpoint = pctMapped - 0.5f;
            var hueAdjustment = pctMidpoint * HueRange;

            // and correct for the circular nature of the hue measure
            var hue2 = Mathf.Repeat(hue + hueAdjustment, 1);
            return hue2;
        }
#endif

        void DestroyExisting()
        {
#if UNITY_EDITOR
            var existingTexture = ListSubAssets();
            foreach (var a in existingTexture)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(a, true);
                }
                else
                {
                    Destroy(a);
                }
            }
#endif
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            var t = new Task(GeneratePalette);
            t.Start(TaskScheduler.FromCurrentSynchronizationContext());
#endif
        }
    }
}