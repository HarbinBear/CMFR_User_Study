using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public enum MappingStrategy
{
    RMFR                        = 0 ,    
    
    Elliptical_Grid_Mapping     = 1 ,
    Squelched_Grid_Mapping      = 2 ,
    Blended_E_Grid_Mapping      = 3 ,
                                  
    FG_Squircular_Mapping       = 4 ,
    Two_Squircular_Mapping      = 5 ,
    Sham_Quartic_Mapping        = 6 ,
    
    Schwarz_Christoffel_Mapping = 7 ,
    
    Hyperbolic_Mapping          = 8 ,
    cornerific_tapered2         = 9 ,
    Non_axial_2_pinch           = 10,
    
    Simple_Strech               = 11,
}

public enum OutputTex
{
    TexturePass0,
    TexturePass1,
    TexturePass2,
    TextureDenoise,
}

public enum DebugMode
{
    Render          = 0 ,
    PixelDensity    = 1 ,
    
}


public class RFR_SingleEye : MonoBehaviour
{
    public RenderTexture TexturePass0;
    RenderTexture TexturePass1 = null;
    RenderTexture TexturePass2;
    RenderTexture TextureDenoise;
    public Material Pass1Material;
    public Material Pass2Material;
    public Material DenoiseMaterial;

    [Range(1.0f , 3f)]
    public float sigma ;
    [Range(0.01f, 0.99f)]
    public float fx;
    [Range(0.01f, 0.99f)]
    public float fy;
    [Range(0.01f, 0.99f)]
    public float eyeX;
    [Range(0.01f, 0.99f)]
    public float eyeY;
    [Range(0.01f, 1.0f)] 
    public float SquelchedGridMappingBeta;

    [Range(0, 1)] 
    public float SampleDensityWithNoise;
    [FormerlySerializedAs("MappingStrategy")] 
    public MappingStrategy mappingStrategy;

    public DebugMode debugMode;

    public OutputTex outputTex;
    
    int iApplyRFRMap1;
    int iApplyRFRMap2;

    public string savePath;
    private float fx0;
    private float fy0;
    float sigma0;
    private MappingStrategy mappingStrategy0;

    bool b_save = false;
    
    private float _validPercent;

    // Start is called before the first frame update
    void Start()
    {
        sigma0 = sigma;
        iApplyRFRMap1 = 1;
        iApplyRFRMap2 = 1;

        TexturePass1 = new RenderTexture(Mathf.RoundToInt(Screen.width / sigma), Mathf.RoundToInt(Screen.height / sigma), 24, RenderTextureFormat.Default);
        TexturePass1.Create();

        TexturePass2 = new RenderTexture(Mathf.RoundToInt(Screen.width), Mathf.RoundToInt(Screen.height), 24, RenderTextureFormat.Default);
        TexturePass2.Create();

        TextureDenoise = new RenderTexture(Mathf.RoundToInt(Screen.width), Mathf.RoundToInt(Screen.height), 24, RenderTextureFormat.Default);
        TextureDenoise.Create();
    }

    // Update is called once per frame
    void Update()
    {
        keyControl();
        saveImages();
        // CalcPixelPercent();
        if (sigma0 != sigma)
        {
            updateTextureSize();
            sigma0 = sigma;
        }

        if( fx != fx0 )
        {
            // CalcPixelPercent();
            fx0 = fx;
        }

        if ( fy != fy0 )
        {
            // CalcPixelPercent();
            fy0 = fy;
        }

        if (mappingStrategy != mappingStrategy0)
        {
            mappingStrategy0 = mappingStrategy;
            // CalcPixelPercent();
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Pass1MainL();
        Pass2MainL();
        Pass3DenoiseL();
        // Graphics.Blit(TextureDenoise, dst);

        switch (outputTex)
        {
            case OutputTex.TexturePass0:
            {
                Graphics.Blit(TexturePass0, dst);
                break;
            }
            case OutputTex.TexturePass1:
            {
                Graphics.Blit(TexturePass1, dst);
                break;
            }
            case OutputTex.TexturePass2:
            {
                Graphics.Blit(TexturePass2, dst);
                break;
            }
                
        }
    }

    void Pass1MainL()
    {
        Pass1Material.SetFloat("_eyeX", eyeX);
        Pass1Material.SetFloat("_eyeY", eyeY);
        Pass1Material.SetFloat("_scaleRatio", sigma);
        Pass1Material.SetFloat("_fx", fx);
        Pass1Material.SetFloat("_fy", fy);
        Pass1Material.SetInt("_iApplyRFRMap1", iApplyRFRMap1);
        
        Pass1Material.SetFloat("_SquelchedGridMappingBeta",SquelchedGridMappingBeta);
        Pass1Material.SetInt("_MappingStrategy" , (int)mappingStrategy);
        Pass1Material.SetInt("_DebugMode" , (int)debugMode);
        
        
        Graphics.Blit(TexturePass0, TexturePass1, Pass1Material);
    }
    void Pass2MainL()
    {
        Pass2Material.SetFloat("_eyeX", eyeX);
        Pass2Material.SetFloat("_eyeY", eyeY);
        Pass2Material.SetFloat("_scaleRatio", sigma);
        Pass2Material.SetFloat("_fx", fx);
        Pass2Material.SetFloat("_fy", fy);
        Pass2Material.SetInt("_iApplyRFRMap2", iApplyRFRMap2);
        Pass2Material.SetTexture("_MidTex", TexturePass1);
        
        Pass2Material.SetFloat("_SquelchedGridMappingBeta",SquelchedGridMappingBeta);
        Pass2Material.SetInt("_MappingStrategy" , (int)mappingStrategy);
        Pass2Material.SetFloat("_bSampleDensityWithNoise" ,  SampleDensityWithNoise );
        Pass2Material.SetInt("_DebugMode" , (int)debugMode);
        Pass2Material.SetFloat("_validPercent" , _validPercent);
        
        Graphics.Blit(null, TexturePass2, Pass2Material);
    }

    void Pass3DenoiseL()
    {
        DenoiseMaterial.SetFloat("_iResolutionX", Screen.width);
        DenoiseMaterial.SetFloat("_iResolutionY", Screen.height);
        DenoiseMaterial.SetFloat("_eyeX", eyeX);
        DenoiseMaterial.SetFloat("_eyeY", eyeY);
        DenoiseMaterial.SetTexture("_Pass2Tex", TexturePass2);
        Graphics.Blit(null, TextureDenoise, DenoiseMaterial);
    }

    void keyControl()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            sigma0 = sigma0 <= 1.2f ? 1.2f : sigma0 - 0.2f;
        if (Input.GetKeyDown(KeyCode.E))
            sigma0 = sigma0 >= 2.8f ? 2.8f : sigma0 + 0.2f;
        
        if (Input.GetKeyDown(KeyCode.A))
            eyeX = eyeX <= 0.05f ? 0.05f : eyeX - 0.05f;
        if (Input.GetKeyDown(KeyCode.D))
            eyeX = eyeX >= 0.95f ? 0.95f : eyeX + 0.05f;

        if (Input.GetKeyDown(KeyCode.W))
            eyeY = eyeY >= 0.95f ? 0.95f : eyeY + 0.05f;
        if (Input.GetKeyDown(KeyCode.S))
            eyeY = eyeY <= 0.05f ? 0.05f : eyeY - 0.05f;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            fx = fx <= 0.15f ? 0.1f : fx - 0.1f;
        if (Input.GetKeyDown(KeyCode.RightArrow))
            fx = fx >= 0.85f ? 0.9f : fx + 0.1f;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            fy = fy >= 0.85f ? 0.9f : fy + 0.1f;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            fy = fy <= 0.15f ? 0.1f : fy - 0.1f;

        if (Input.GetKeyDown(KeyCode.F9))
            b_save = !b_save;
    }

    void updateTextureSize()
    {
        if(TexturePass1 != null)
        {
            TexturePass1.Release();
        }
        
        RenderTexture tempTexture = new RenderTexture(Mathf.RoundToInt(Screen.width / sigma0), Mathf.RoundToInt(Screen.height / sigma0), 24, RenderTextureFormat.Default);
        tempTexture.Create();

        TexturePass1 = tempTexture;
    }

    void saveImages()
    {
        if (b_save)
        {
            b_save = false;
            Debug.Log("eyeXL:" + eyeX.ToString() + "\teyeYL:" + eyeY.ToString() + 
            "sigma:" + sigma + "\n" + "fx:" + fx.ToString() + "\tfy:" + fy.ToString());

            SaveToFile(TexturePass0, savePath + "_original_left_n.png");

            SaveToFile(TexturePass1, savePath  + "_p1.png");

            SaveToFile(TexturePass2, savePath + "_p2.png");
            //
            // SaveToFile(TexturePass2, savePath + "/sigma_" +
            //                          sigma.ToString() + "_fx_" + fx.ToString() + "_fy_" + fy.ToString() + "_p2.png");
            
            // SaveToFile(TextureDenoise, savePath + "/sigma_" +
            //    sigma.ToString() + "_fx_" + fx.ToString() + "_fy_" + fy.ToString() +
            //    "_eX_" + eyeX.ToString() + "_eY_" + eyeY.ToString() + "_dn.png");

        }

    }

    // public void CalcPixelPercent()
    // {
    //     RenderTexture currentActiveRT = RenderTexture.active;
    //     RenderTexture.active = TexturePass1;
    //     Texture2D tex = new Texture2D(TexturePass1.width, TexturePass1.height);
    //     tex.ReadPixels(new Rect(0,0,tex.width , tex.height) , 0,0);
    //     Color[] colors = tex.GetPixels(0, 0, tex.width, tex.height);
    //     int invalidCount = 0;
    //     foreach (var color in colors)
    //     {
    //         if (color == Color.white)
    //         {
    //             invalidCount++;
    //         }
    //     }
    //
    //     int validCount = colors.Length - invalidCount;
    //     float validPercent = (float)validCount / (float)colors.Length;
    //     _validPercent = validPercent;
    //     colors = null;
    //     // UnityEngine.Object.Destroy(tex);
    //     RenderTexture.active = currentActiveRT;
    // }

    public void SaveToFile(RenderTexture renderTexture, string name)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        var bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(name, bytes);
        // UnityEngine.Object.Destroy(tex);
        RenderTexture.active = currentActiveRT;
    }

    void DispText(int idx, float variable, string name)
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        guiStyle.normal.textColor = Color.red;
        
        
        string text = string.Format(name + " = {0}", variable);
        GUI.contentColor = Color.red;
        GUI.Label(new Rect(0, idx * 50, Screen.width, Screen.height), text, guiStyle);
    }

    void OnGUI()
    {
        int idx = 0;
        DispText(idx++, sigma, "sigma");
        DispText(idx++, fx, "fx");
        DispText(idx++, fy, "fy");
        if(debugMode > 0)DispText(idx++, _validPercent, "validPercent");
        // DispText(idx++, eyeX, "eyeX");
        // DispText(idx++, eyeY, "eyeY");
        DispText(idx++ , (float)mappingStrategy , "MappingStrategy");
        
    }
}
