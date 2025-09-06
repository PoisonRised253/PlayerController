using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Effects : MonoBehaviour
{
    private static Transform cam => Player.Instance.cam;
    private static PostProcessProfile pp => cam.GetComponent<PostProcessVolume>().profile;
    private static Vignette vig => pp.GetSetting<Vignette>();
    private static MotionBlur blur => pp.GetSetting<MotionBlur>();
    private static ChromaticAberration crom => pp.GetSetting<ChromaticAberration>();
    private static Bloom bloom => pp.GetSetting<Bloom>();
    public static Global Host => Global.Instance;
    public void Toggle()
    {
        cam.GetComponent<PostProcessVolume>().enabled = !cam.GetComponent<PostProcessVolume>().enabled;
    }

    public IEnumerator OnHit()
    {
        //Idk how to make vignette go beeg then smol so i cant make this :/
        Color adaptCol = Color.Lerp(Color.red, Color.black, 1f);
        float adaptInt = Mathf.Lerp(1f, 0f, 1f);
        while (true)
        {
            vig.intensity.Override(adaptInt);
            vig.color.Override(adaptCol);
            if (adaptInt < 0.25f)
            {
                adaptInt = 0.25f;
            }
        }
        yield return new WaitForSeconds(0.125f);
    }

    public struct DefaultShaders
    {
        public Color vigColor { get { return new Color(0, 0, 0, 255); } }
        public float vigIntense { get { return 0.25f; } }
        public float vigSmooth { get { return 0.5f; } }
        public int blurSamples { get { return 10; } }
        public float bloomIntense { get { return 1; } }
        public float bloomThres { get { return 1; } }
        public float cromIntense { get { return 0.125f; } }
    }

    private static FloatParameter FloatToParameter(float f)
    {
        var x = new FloatParameter();
        x.value = f;
        return x;
    }
}
