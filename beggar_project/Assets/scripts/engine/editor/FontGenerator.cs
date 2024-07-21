using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEditor.TextCore.LowLevel;
using Object = UnityEngine.Object;
using TMPro;


public static class FontGenerator
{

    public static TMP_FontAsset GenerateFont(Font font, string text, int sampleSize, int atlasPad, GlyphRenderMode renderMode, int atlasW, int atlasH, string fontName, bool copyPreviousMaterialSettings, FilterMode filterMode)
    {
        var fontAsset = TMP_FontAsset.CreateFontAsset(font, sampleSize, atlasPad, renderMode, atlasW, atlasH);
        fontAsset.TryAddCharacters(text, out string missing);
        string path = $"Assets/Export/{fontName}.asset";
        string pathTex = $"Assets/Export/{fontName}_tex.asset";
        string pathMat = $"Assets/Export/{fontName}_mat.asset";

        fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
        fontAsset.name = fontName;
        if (fontAsset.atlas != null)
            AssetDatabase.CreateAsset(fontAsset.atlas, $"Assets/Export/{fontName}.png");

        fontAsset.material.name = fontName + " Material";
        fontAsset.atlasTexture.name = fontName + " Atlas";
        fontAsset.atlasTexture.filterMode = filterMode;

        var existingFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        var existingFontTex = AssetDatabase.LoadAssetAtPath<Texture2D>(pathTex);
        var existingFontMat = AssetDatabase.LoadAssetAtPath<Material>(pathMat);

        if (copyPreviousMaterialSettings && existingFontMat != null)
        {
            CopyMaterialSettings(existingFontMat, fontAsset.material);
        }

        if (existingFont == null)
        {
            AssetDatabase.CreateAsset(fontAsset, path);
        }
        else
        {
            /*string tmpPath = path + "tmp";
            AssetDatabase.CreateAsset(fontAsset, tmpPath);
            var pathRaw = Path.Combine(Application.dataPath, path);
            var pathRawTmp = Path.Combine(Application.dataPath, tmpPath);
            File.Delete(pathRaw);
            File.Copy(pathRawTmp, pathRaw);*/
            EditorUtility.CopySerialized(fontAsset, existingFont);

        }

        // CreateOrReplace(fontAsset, existingFont, path);
        DeleteAndCreate(fontAsset.atlasTexture, pathTex);
        DeleteAndCreate(fontAsset.material, pathMat);
        AssetDatabase.Refresh();
        if (!string.IsNullOrWhiteSpace(missing))
        {
            Debug.Log("MISSING CHARACTER FROM FONT");
            Debug.Log(missing);
        }
        //AssetDatabase.ImportAsset("Assets/Export/", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        return fontAsset;
    }

    static void DeleteAndCreate<T>(T source, string path) where T : Object
    {
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(source, path);
    }

    static T CreateOrReplace<T>(T source, T dest, string path) where T : Object
    {
        if (dest == null)
        {
            AssetDatabase.CreateAsset(source, path);
            return (T)source;
        }
        else
        {
            EditorUtility.CopySerialized(source, dest);
            return (T)dest;
        }
    }

    static void CopyMaterialSettings(Material source, Material destination)
    {
        // Ensure both materials use the same shader
        if (source.shader != destination.shader)
        {
            Debug.LogError("Materials have different shaders!");
            return;
        }

        if (source.IsKeywordEnabled("OUTLINE_ON")) {
            destination.EnableKeyword("OUTLINE_ON");
        }

        int propertyCount = ShaderUtil.GetPropertyCount(source.shader);

        for (int i = 0; i < propertyCount; i++)
        {
            // Get the property name
            string propertyName = ShaderUtil.GetPropertyName(source.shader, i);

            // Check if the property is exposed in the inspector
            if (ShaderUtil.IsShaderPropertyHidden(source.shader, i))
                continue;

            // Get the property type
            ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(source.shader, i);

            // Copy the property value based on its type
            switch (propertyType)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    destination.SetColor(propertyName, source.GetColor(propertyName));
                    break;

                case ShaderUtil.ShaderPropertyType.Vector:
                    destination.SetVector(propertyName, source.GetVector(propertyName));
                    break;

                case ShaderUtil.ShaderPropertyType.Float:
                    destination.SetFloat(propertyName, source.GetFloat(propertyName));
                    break;

                case ShaderUtil.ShaderPropertyType.Range:
                    destination.SetFloat(propertyName, source.GetFloat(propertyName));
                    break;

                case ShaderUtil.ShaderPropertyType.TexEnv:
                    /*
                    destination.SetTexture(propertyName, source.GetTexture(propertyName));
                    destination.SetTextureOffset(propertyName, source.GetTextureOffset(propertyName));
                    destination.SetTextureScale(propertyName, source.GetTextureScale(propertyName));
                    */
                    break;
            }
        }

        // Make sure changes are applied
        destination.SetShaderPassEnabled("Always", true);
    }

    /*
    public enum GlyphRasterModes
    {
        RASTER_MODE_8BIT = 1,
        RASTER_MODE_MONO = 2,
        RASTER_MODE_NO_HINTING = 4,
        RASTER_MODE_HINTED = 8,
        RASTER_MODE_BITMAP = 16,
        RASTER_MODE_SDF = 32,
        RASTER_MODE_SDFAA = 64,
        RASTER_MODE_MSDF = 256,
        RASTER_MODE_MSDFA = 512,
        RASTER_MODE_1X = 4096,
        RASTER_MODE_8X = 8192,
        RASTER_MODE_16X = 16384,
        RASTER_MODE_32X = 32768
    }
    public static void GenerateFont(Object m_SourceFontFile, string m_CharacterSequence, GlyphRasterModes m_GlyphRenderMode, int m_PointSize, int m_AtlasWidth, int m_AtlasHeight) {
        var fontAsset = TMP_FontAsset.CreateFontAsset();
        Dictionary<uint, uint> m_CharacterLookupMap = new Dictionary<uint, uint>();
        Dictionary<uint, List<uint>> m_GlyphLookupMap = new Dictionary<uint, List<uint>>();
        if (m_SourceFontFile != null)
        {
            Texture2D m_FontAtlasTexture = null;
            Texture2D m_SavedFontAtlas = null;
            string m_OutputFeedback = string.Empty;

            // Initialize font engine
            FontEngineError errorCode = FontEngine.InitializeFontEngine();
            if (errorCode != FontEngineError.Success)
            {
                Debug.Log("Font Asset Creator - Error [" + errorCode + "] has occurred while Initializing the FreeType Library.");
            }

            // Get file path of the source font file.
            string fontPath = AssetDatabase.GetAssetPath(m_SourceFontFile);

            if (errorCode == FontEngineError.Success)
            {
                errorCode = FontEngine.LoadFontFace(fontPath);

                if (errorCode != FontEngineError.Success)
                {
                    Debug.Log("Font Asset Creator - Error Code [" + errorCode + "] has occurred trying to load the [" + m_SourceFontFile.name + "] font file. This typically results from the use of an incompatible or corrupted font file.", m_SourceFontFile);
                }
            }


            // Define an array containing the characters we will render.
            if (errorCode == FontEngineError.Success)
            {
                uint[] characterSet = null;
                List<uint> char_List = new List<uint>();

                for (int i = 0; i < m_CharacterSequence.Length; i++)
                {
                    uint unicode = m_CharacterSequence[i];

                    // Handle surrogate pairs
                    if (i < m_CharacterSequence.Length - 1 && char.IsHighSurrogate((char)unicode) && char.IsLowSurrogate(m_CharacterSequence[i + 1]))
                    {
                        unicode = (uint)char.ConvertToUtf32(m_CharacterSequence[i], m_CharacterSequence[i + 1]);
                        i += 1;
                    }

                    // Check to make sure we don't include duplicates
                    if (char_List.FindIndex(item => item == unicode) == -1)
                        char_List.Add(unicode);
                }

                characterSet = char_List.ToArray();

                var m_CharacterCount = characterSet.Length;

                var m_AtlasGenerationProgress = 0;
                GlyphRenderMode renderMode;

                GlyphLoadFlags glyphLoadFlags = ((GlyphRasterModes)m_GlyphRenderMode & GlyphRasterModes.RASTER_MODE_HINTED) == GlyphRasterModes.RASTER_MODE_HINTED
                    ? GlyphLoadFlags.LOAD_RENDER
                    : GlyphLoadFlags.LOAD_RENDER | GlyphLoadFlags.LOAD_NO_HINTING;

                glyphLoadFlags = ((GlyphRasterModes)m_GlyphRenderMode & GlyphRasterModes.RASTER_MODE_MONO) == GlyphRasterModes.RASTER_MODE_MONO
                    ? glyphLoadFlags | GlyphLoadFlags.LOAD_MONOCHROME
                    : glyphLoadFlags;


                // Worker thread to pack glyphs in the given texture space.
                ThreadPool.QueueUserWorkItem(PackGlyphs =>
                {
                    List<Glyph> m_GlyphsToPack = new List<Glyph>();
                    List<Glyph> m_GlyphsPacked = new List<Glyph>();
                    List<GlyphRect> m_FreeGlyphRects = new List<GlyphRect>();
                    List<GlyphRect> m_UsedGlyphRects = new List<GlyphRect>();
                    List<Glyph> m_GlyphsToRender = new List<Glyph>();
                    List<uint> m_AvailableGlyphsToAdd = new List<uint>();
                    List<uint> m_MissingCharacters = new List<uint>();
                    List<uint> m_ExcludedCharacters = new List<uint>();
                    // Start Stop Watch
                    //m_StopWatch = System.Diagnostics.Stopwatch.StartNew();

                    // Clear the various lists used in the generation process.
                    m_AvailableGlyphsToAdd.Clear();
                    m_MissingCharacters.Clear();
                    m_ExcludedCharacters.Clear();
                    m_CharacterLookupMap.Clear();
                    m_GlyphLookupMap.Clear();
                    m_GlyphsToPack.Clear();
                    m_GlyphsPacked.Clear();

                    // Check if requested characters are available in the source font file.
                    for (int i = 0; i < characterSet.Length; i++)
                    {
                        uint unicode = characterSet[i];
                        uint glyphIndex;

                        if (FontEngine.TryGetGlyphIndex(unicode, out glyphIndex))
                        {
                            // Skip over potential duplicate characters.
                            if (m_CharacterLookupMap.ContainsKey(unicode))
                                continue;

                            // Add character to character lookup map.
                            m_CharacterLookupMap.Add(unicode, glyphIndex);

                            // Skip over potential duplicate glyph references.
                            if (m_GlyphLookupMap.ContainsKey(glyphIndex))
                            {
                                // Add additional glyph reference for this character.
                                m_GlyphLookupMap[glyphIndex].Add(unicode);
                                continue;
                            }

                            // Add glyph reference to glyph lookup map.
                            m_GlyphLookupMap.Add(glyphIndex, new List<uint>() { unicode });

                            // Add glyph index to list of glyphs to add to texture.
                            m_AvailableGlyphsToAdd.Add(glyphIndex);
                        }
                        else
                        {
                            // Add Unicode to list of missing characters.
                            m_MissingCharacters.Add(unicode);
                        }
                    }

                    // Pack available glyphs in the provided texture space.
                    if (m_AvailableGlyphsToAdd.Count > 0)
                    {
                        int packingModifier = ((GlyphRasterModes)m_GlyphRenderMode & GlyphRasterModes.RASTER_MODE_BITMAP) == GlyphRasterModes.RASTER_MODE_BITMAP ? 0 : 1;

                        {

                            // Set point size
                            FontEngine.SetFaceSize(m_PointSize);

                            m_GlyphsToPack.Clear();
                            m_GlyphsPacked.Clear();

                            m_FreeGlyphRects.Clear();
                            m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - packingModifier, m_AtlasHeight - packingModifier));
                            m_UsedGlyphRects.Clear();

                            for (int i = 0; i < m_AvailableGlyphsToAdd.Count; i++)
                            {
                                uint glyphIndex = m_AvailableGlyphsToAdd[i];
                                Glyph glyph;

                                if (FontEngine.TryGetGlyphWithIndexValue(glyphIndex, glyphLoadFlags, out glyph))
                                {
                                    if (glyph.glyphRect.width > 0 && glyph.glyphRect.height > 0)
                                    {
                                        m_GlyphsToPack.Add(glyph);
                                    }
                                    else
                                    {
                                        m_GlyphsPacked.Add(glyph);
                                    }
                                }
                            }

                            FontEngine.TryPackGlyphsInAtlas(m_GlyphsToPack, m_GlyphsPacked, m_Padding, (GlyphPackingMode)m_PackingMode, m_GlyphRenderMode, m_AtlasWidth, m_AtlasHeight, m_FreeGlyphRects, m_UsedGlyphRects);

                            if (m_IsGenerationCancelled)
                            {
                                DestroyImmediate(m_FontAtlasTexture);
                                m_FontAtlasTexture = null;
                                return;
                            }
                            //Debug.Log("Glyphs remaining to add [" + m_GlyphsToAdd.Count + "]. Glyphs added [" + m_GlyphsAdded.Count + "].");
                        }

                    }
                    else
                    {
                        int packingModifier = ((GlyphRasterModes)m_GlyphRenderMode & GlyphRasterModes.RASTER_MODE_BITMAP) == GlyphRasterModes.RASTER_MODE_BITMAP ? 0 : 1;

                        FontEngine.SetFaceSize(m_PointSize);

                        m_GlyphsToPack.Clear();
                        m_GlyphsPacked.Clear();

                        m_FreeGlyphRects.Clear();
                        m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - packingModifier, m_AtlasHeight - packingModifier));
                        m_UsedGlyphRects.Clear();
                    }

                    //Stop StopWatch
                    m_StopWatch.Stop();
                    m_GlyphPackingGenerationTime = m_StopWatch.Elapsed.TotalMilliseconds;
                    m_IsGlyphPackingDone = true;
                    m_StopWatch.Reset();

                    m_FontCharacterTable.Clear();
                    m_FontGlyphTable.Clear();
                    m_GlyphsToRender.Clear();

                    // Handle Results and potential cancellation of glyph rendering
                    if (m_GlyphRenderMode == GlyphRenderMode.SDF32 && m_PointSize > 512 || m_GlyphRenderMode == GlyphRenderMode.SDF16 && m_PointSize > 1024 || m_GlyphRenderMode == GlyphRenderMode.SDF8 && m_PointSize > 2048)
                    {
                        int upSampling = 1;
                        switch (m_GlyphRenderMode)
                        {
                            case GlyphRenderMode.SDF8:
                                upSampling = 8;
                                break;
                            case GlyphRenderMode.SDF16:
                                upSampling = 16;
                                break;
                            case GlyphRenderMode.SDF32:
                                upSampling = 32;
                                break;
                        }

                        Debug.Log("Glyph rendering has been aborted due to sampling point size of [" + m_PointSize + "] x SDF [" + upSampling + "] up sampling exceeds 16,384 point size. Please revise your generation settings to make sure the sampling point size x SDF up sampling mode does not exceed 16,384.");

                        m_IsRenderingDone = true;
                        m_AtlasGenerationProgress = 0;
                        m_IsGenerationCancelled = true;
                    }

                    // Add glyphs and characters successfully added to texture to their respective font tables.
                    foreach (Glyph glyph in m_GlyphsPacked)
                    {
                        uint glyphIndex = glyph.index;

                        m_FontGlyphTable.Add(glyph);

                        // Add glyphs to list of glyphs that need to be rendered.
                        if (glyph.glyphRect.width > 0 && glyph.glyphRect.height > 0)
                            m_GlyphsToRender.Add(glyph);

                        foreach (uint unicode in m_GlyphLookupMap[glyphIndex])
                        {
                            // Create new Character
                            m_FontCharacterTable.Add(new TMP_Character(unicode, glyph));
                        }
                    }

                    //
                    foreach (Glyph glyph in m_GlyphsToPack)
                    {
                        foreach (uint unicode in m_GlyphLookupMap[glyph.index])
                        {
                            m_ExcludedCharacters.Add(unicode);
                        }
                    }

                    // Get the face info for the current sampling point size.
                    m_FaceInfo = FontEngine.GetFaceInfo();

                    autoEvent.Set();
                });

                // Worker thread to render glyphs in texture buffer.
                ThreadPool.QueueUserWorkItem(RenderGlyphs =>
                {
                    autoEvent.WaitOne();

                    if (m_IsGenerationCancelled == false)
                    {
                        // Start Stop Watch
                        m_StopWatch = System.Diagnostics.Stopwatch.StartNew();

                        m_IsRenderingDone = false;

                        // Allocate texture data
                        m_AtlasTextureBuffer = new byte[m_AtlasWidth * m_AtlasHeight];

                        m_AtlasGenerationProgressLabel = "Rendering glyphs...";

                        // Render and add glyphs to the given atlas texture.
                        if (m_GlyphsToRender.Count > 0)
                        {
                            FontEngine.RenderGlyphsToTexture(m_GlyphsToRender, m_Padding, m_GlyphRenderMode, m_AtlasTextureBuffer, m_AtlasWidth, m_AtlasHeight);
                        }

                        m_IsRenderingDone = true;

                        // Stop StopWatch
                        m_StopWatch.Stop();
                        m_GlyphRenderingGenerationTime = m_StopWatch.Elapsed.TotalMilliseconds;
                        m_IsGlyphRenderingDone = true;
                        m_StopWatch.Reset();
                    }
                });
            }

            SaveCreationSettingsToEditorPrefs(SaveFontCreationSettings());
        }
    }
    */
}