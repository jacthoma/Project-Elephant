////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using System.Drawing;
using System.Drawing.Imaging;
using Color = UnityEngine.Color;

using zSpace.Common;

/// <summary>
/// Loads a variety of image formats into a Texture2D.
/// </summary>
public class ImageLoader
{

    /// <summary> A function for converting between color spaces. </summary>
    public delegate Color32 ColorTransform(Color32 src);

    /// <summary> Retuns the same color given to it. </summary>
    public static ColorTransform CTIdentity = c => c;

    /// <summary>
    /// Converts the given System.Drawing.Color to a Unity-compatible Color32.
    /// </summary>
    public static Color32 ToColor32(System.Drawing.Color drawingColor)
    {
        return new Color32(drawingColor.R, drawingColor.G, drawingColor.B, drawingColor.A);
    }

    /// <summary> Loads the specified file into a Texture2D. </summary>
    /// <param name="fileName">The relative or absolute path to the image file to load.</param>
    /// <param name="startDir">The absolute path to search if fileName is relative.</param>
    public static Texture2D LoadTextureFromFile(string fileName, string startDir = null)
    {
        startDir = startDir ?? Directory.GetCurrentDirectory();

        string absFileName = (!Path.IsPathRooted(fileName)) ? Path.Combine(startDir, fileName) : fileName;

        // ** Debug.Log("Texture file name -- " + fileName + " fullPath " + Path.GetFullPath(fileName));
        if (!File.Exists(absFileName))
        {
            Debug.LogError("Image file does not exist: " + fileName);
            return null;
        }

        if (Regex.IsMatch(absFileName, "\\.dcm$", RegexOptions.IgnoreCase))
        {
            var aspectRatio = Vector3.one;
            ColorTransform colorTransform;
            var bitmaps = LoadDicomBitmaps(new string[] { absFileName }, out aspectRatio, out colorTransform);
            return ToTexture<Texture2D>(bitmaps, colorTransform);
        }

        Stream stream = new FileStream(absFileName, FileMode.Open, FileAccess.Read);
        return LoadTextureFromStream(stream);
    }
 
 
    /// <summary>
    /// Loads the specifiedstream into a Texture2D.
    /// </summary>
    /// <remarks>
    /// The stream position must be set appropriately before loading.
    /// </remarks>
    public static Texture2D LoadTextureFromStream(Stream stream, int layer = 0)
    {
        Bitmap[] bitmaps = LoadBitmaps(stream, layer);
        if (bitmaps == null || bitmaps.Length != 1)
            return null;

        return ToTexture<Texture2D>(bitmaps);
    }
    
    
    /// <summary> Loads the specified file into a Texture3D. </summary>
    public static Texture3D LoadTexturesFromFile(string fileName, string startDir = null)
    {
        startDir = startDir ?? Directory.GetCurrentDirectory();

        string absFileName = (!Path.IsPathRooted(fileName)) ? Path.Combine(startDir, fileName) : fileName;

        if (!File.Exists(absFileName))
        {
            Debug.LogError("Image file does not exist: " + fileName);
            return null;
        }
     
        Stream stream = new FileStream(absFileName, FileMode.Open, FileAccess.Read);
        return LoadTexturesFromStream(stream);
    }


    /// <summary> Loads the specified slice files into a Texture3D. </summary>
    public static Texture3D LoadTexturesFromDicomFiles(string[] fileNames, out Vector3 aspectRatio)
    {
        List<Bitmap> bitmaps = new List<Bitmap>();
        foreach (string fileName in fileNames)
        {
            // ** Debug.Log("Texture file name -- " + fileName + " fullPath " + Path.GetFullPath(fileName));
            if (!File.Exists(fileName))
            {
                Debug.LogError("Image file does not exist: " + fileName);
                aspectRatio = Vector3.one;
                return null;
            }
        }

        ColorTransform colorTransform;
        bitmaps.AddRange(LoadDicomBitmaps(fileNames, out aspectRatio, out colorTransform));
        var result = ToTexture<Texture3D>(bitmaps.ToArray(), colorTransform);

        if (bitmaps.Count() > 0)
        {
#if true
            var dimensions = new Vector3(bitmaps[0].Width, bitmaps[0].Height, bitmaps.Count());
            dimensions = dimensions.DivideComponents(new Vector3(result.width, result.height, result.depth));
#else
            var dimensions = new Vector3(result.width, result.height, result.depth);
#endif
            aspectRatio = aspectRatio.MultiplyComponents(dimensions);
            aspectRatio /= aspectRatio.Maximum();

            Debug.Log("Aspect ratio is " + aspectRatio.ToString("F4"));
        }

        return result;
    }
 
 
    /// <summary>
    /// Loads the specified stream into a Texture3D.
    /// </summary>
    /// <remarks>
    /// The stream position must be set appropriately before loading.
    /// </remarks>
    public static Texture3D LoadTexturesFromStream(Stream stream)
    {
        Bitmap[] bitmaps = LoadBitmaps(stream);
        if (bitmaps == null)
            return null;

        return ToTexture<Texture3D>(bitmaps);
    }


    /// <summary>
    /// Generates a Unity Texture based on the given Bitmap(s).
    /// </summary>
    protected static TTexture ToTexture<TTexture>(Bitmap[] bitmaps, ColorTransform colorTransform = null) where TTexture : Texture
    {
        if (colorTransform == null)
            colorTransform = CTIdentity;

        int sourceDepth = bitmaps.Length;
        if (sourceDepth < 1)
            return null;
        
        int sourceHeight = bitmaps[0].Height;
        int sourceWidth = bitmaps[0].Width;

        int destDepth = sourceDepth;
        int destWidth = sourceWidth;
        int destHeight = sourceHeight;

        if (typeof(TTexture) == typeof(Texture3D))
        {
            int maxSize = 256;
            destDepth = Mathf.Min(maxSize, Utility.ComputeNextPowerOfTwo(sourceDepth));
            destWidth = Mathf.Min(maxSize, Utility.ComputeNextPowerOfTwo(sourceWidth));
            destHeight = Mathf.Min(maxSize, Utility.ComputeNextPowerOfTwo(sourceHeight));

            Debug.Log("Rounding dimensions (" + sourceWidth + ", " + sourceHeight + ", " + sourceDepth +
                      ") to (" + destWidth + ", " + destHeight + ", " + destDepth + ").");
        }

        Color32[] colors = new Color32[destWidth * destHeight * destDepth];

        bool allZeros = true;
        bool allOnes = true;
        
        int depthIter = Mathf.Min(destDepth, sourceDepth);
        int heightIter = Mathf.Min(destHeight, sourceHeight);
        int widthIter = Mathf.Min(destWidth, sourceWidth);
        PixelFormat pixelFormat = PixelFormat.Undefined;
        
        for (int k = 0; k < depthIter; ++k)
        {
            for (int j = 0; j < heightIter; ++j)
            {
                for (int i = 0; i < widthIter; ++i)
                {
                    System.Drawing.Color drawingColor = bitmaps[k].GetPixel(i, j);
                    Color32 color = colorTransform(ToColor32(drawingColor));

                    int index = k * destWidth * destHeight + (destHeight - j - 1) * destWidth + i;
                    colors[index] = color;

                    allZeros &= color.a == 0;
                    allOnes &= color.a == 255;
                }
            }

            pixelFormat = bitmaps[k].PixelFormat; //TODO: Assumes all bitmaps have the same format, for efficiency.
        }
        
        bool hasAlpha = !allZeros && !allOnes;
        TextureFormat textureFormat =  (pixelFormat == PixelFormat.Alpha) ? TextureFormat.Alpha8 :
                                       (hasAlpha) ? TextureFormat.RGBA32 : TextureFormat.RGB24;
                
        if (typeof(TTexture) == typeof(Texture2D))
        {
            Texture2D result = new Texture2D(destWidth, destHeight, textureFormat, true);
            result.name = "Texture2D" + _textureCount;
            _textureCount++;
            result.SetPixels32(colors);
            result.Apply();
            return result as TTexture;
        }
        else if (typeof(TTexture) == typeof(Texture3D))
        {
            //Debug.Log("Converting Color32 to Color");
            Color[] colorsF = new Color[destDepth * destHeight * destWidth];
            for (int i = 0; i < depthIter; ++i)
            {
                for (int j = 0; j < heightIter; ++j)
                {
                    for (int k = 0; k < widthIter; ++k)
                    {
                        int index = i * destHeight * destWidth + j * destWidth + k;
                        colorsF[index] = colors[index];
                    }
                }
            }
            Texture3D result = new Texture3D(destWidth, destHeight, destDepth, textureFormat, true);
            result.name = "Texture3D" + _textureCount;
            _textureCount++;
            result.wrapMode = TextureWrapMode.Clamp;
            result.filterMode = FilterMode.Bilinear;
            result.anisoLevel = 0;
            result.SetPixels(colorsF);
            result.Apply();
            return result as TTexture;
        }
        else
        {
            Debug.LogError("Only a 2D or 3D Texture can be loaded.");
            return null;
        }
    }


    /// <summary>
    /// Loads an array of Bitmaps from the given set of image files, assumed to be in DICOM format.
    /// </summary>
    protected static Bitmap[] LoadDicomBitmaps(string[] fileNames, out Vector3 aspectRatio, out ColorTransform colorTransform)
    {
        List<Bitmap> bitmaps = new List<Bitmap>();
        aspectRatio = Vector3.one;

        colorTransform = CTIdentity;

        foreach (string fileName in fileNames)
        {
            var reader = new gdcm.ImageReader();
            reader.SetFileName(fileName);
            if (!reader.Read())
            {
                Debug.LogError("Failed to load file " + fileName + ".  Please make sure the file exists and is valid.");
                bitmaps.Add(null);
                continue;
            }

            var image = reader.GetImage();

            // Save the voxel dimensions so the overall volume can be sized appropriately.
            for (int i = 0; i < 3; ++i)
                aspectRatio[i] = (float)image.GetSpacing((uint)i);

            byte[] decompressedData = new byte[(int)image.GetBufferLength()];
            image.GetBuffer(decompressedData);

            int Width = (int)image.GetDimension(0);
            int Height = (int)image.GetDimension(1);
            var pi = image.GetPhotometricInterpretation();

            // Copy the input bytes straight to the output, but notify caller of the necessary color space conversion.

            PixelFormat pixelFormat = ChoosePixelFormat(pi);
            colorTransform = ChoosePixelTransform(pi); //TODO: This assumes all slices use the same color transform, for efficiency.

            Bitmap rawSlice = FromByteArray(Width, Height, decompressedData, PixelFormat.Format32bppArgb);
            bitmaps.Add(rawSlice);
        }

        return bitmaps.ToArray();
    }

    /// <summary>
    /// Returns the bytes in an input ARGB32 pixel to a true ARGB32 color.
    /// </summary>
    protected static ColorTransform ChoosePixelTransform(gdcm.PhotometricInterpretation pi)
    {
        //TODO: Support all pixel formats.
        switch (pi.GetType())
        {
            case gdcm.PhotometricInterpretation.PIType.MONOCHROME1:
                return x =>
                {
                    byte a = (byte)(255 - x.a);
                    return new Color32(a, a, a, a);
                };

            case gdcm.PhotometricInterpretation.PIType.MONOCHROME2:
                return x => new Color32(x.a, x.a, x.a, x.a);

            //TODO: Add LUT support and confirm color component sizes.
            //case gdcm.PhotometricInterpretation.PIType.PALETTE_COLOR:

            case gdcm.PhotometricInterpretation.PIType.RGB:
                return CTIdentity;

            //case gdcm.PhotometricInterpretation.PIType.HSV:

            case gdcm.PhotometricInterpretation.PIType.ARGB:
                return CTIdentity;

            //case gdcm.PhotometricInterpretation.PIType.CMYK:
            //case gdcm.PhotometricInterpretation.PIType.YBR_FULL:
            //case gdcm.PhotometricInterpretation.PIType.YBR_FULL_422:
            //case gdcm.PhotometricInterpretation.PIType.YBR_PARTIAL_422:
            //case gdcm.PhotometricInterpretation.PIType.YBR_PARTIAL_420:
            //case gdcm.PhotometricInterpretation.PIType.YBR_ICT:
            //case gdcm.PhotometricInterpretation.PIType.YBR_RCT:

            default:
                Debug.LogWarning("Could not find a suitable transform for photometric interpretation " + pi.GetString() + ". Ignoring pixel format.");
                return CTIdentity;
        }
    }

    protected static PixelFormat ChoosePixelFormat(gdcm.PhotometricInterpretation pi)
    {
        //TODO: Support all pixel formats.
        switch (pi.GetType())
        {
            case gdcm.PhotometricInterpretation.PIType.MONOCHROME1:
                return PixelFormat.Alpha;

            case gdcm.PhotometricInterpretation.PIType.MONOCHROME2:
                return PixelFormat.Alpha;

            case gdcm.PhotometricInterpretation.PIType.PALETTE_COLOR:
                return PixelFormat.Format32bppArgb;

            case gdcm.PhotometricInterpretation.PIType.RGB:
                return PixelFormat.Format24bppRgb;

            //case gdcm.PhotometricInterpretation.PIType.HSV:

            case gdcm.PhotometricInterpretation.PIType.ARGB:
                return PixelFormat.Format32bppArgb;

            //case gdcm.PhotometricInterpretation.PIType.CMYK:
            //case gdcm.PhotometricInterpretation.PIType.YBR_FULL:
            //case gdcm.PhotometricInterpretation.PIType.YBR_FULL_422:
            //case gdcm.PhotometricInterpretation.PIType.YBR_PARTIAL_422:
            //case gdcm.PhotometricInterpretation.PIType.YBR_PARTIAL_420:
            //case gdcm.PhotometricInterpretation.PIType.YBR_ICT:
            //case gdcm.PhotometricInterpretation.PIType.YBR_RCT:

            default:
                Debug.LogWarning("Could not find a suitable format for photometric interpretation " + pi.GetString() + ".");
                return PixelFormat.Undefined;
        }
    }

    /// <summary>
    /// Constructs a Bitmap by copying data from a byte array.
    /// </summary>
    protected static Bitmap FromByteArray(int width, int height, byte[] data, PixelFormat format)
    {
        var b = new Bitmap(width, height, format);

        var boundsRect = new Rectangle(0, 0, width, height);

        //Debug.Log("Building Bitmap from " + width + "x" + height + " byte array with PixelFormat " + b.PixelFormat);
        BitmapData bmpData = b.LockBits(boundsRect, ImageLockMode.WriteOnly, format);

        Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

        b.UnlockBits(bmpData);
        return b;
    }


    /// <summary>
    /// Converts the given stream to a .NET Bitmap for futher processing.
    /// </summary>
    protected static Bitmap[] LoadBitmaps(Stream stream, int layer = -1)
    {
        DevIL.ImageImporter importer = new DevIL.ImageImporter();
        DevIL.Image image = importer.LoadImageFromStream(stream);
        image.Bind();
        DevIL.Unmanaged.ImageInfo info = image.GetImageInfo();
        
        Bitmap[] bitmaps = new Bitmap[(layer >= 0) ? 1 : info.Depth];
        
        for (int i = 0; i < bitmaps.Length; ++i)
        {
            Bitmap bitmap = new Bitmap(info.Width, info.Height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, info.Width, info.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        
            //Since Scan0 is an IntPtr to the bitmap data and we're all the same size...just do a copy.
            DevIL.Unmanaged.IL.CopyPixels(0, 0, (layer >= 0) ? layer : i, info.Width, info.Height, 1, DevIL.DataFormat.BGRA, DevIL.DataType.UnsignedByte, data.Scan0);
        
            bitmap.UnlockBits(data);
            
            bitmaps[i] = bitmap;
        }
        return bitmaps;
    }

    /// <summary>
    /// Packs the given grayscale texture into the alpha channel of the target texture.
    /// Preserves any existing data by multiplying the new and old alpha components.
    /// </summary>
    public static Texture2D ToAlphaChannel(Texture2D srcMap, Texture2D destMap = null)
    {
        var result = new Texture2D(srcMap.width, srcMap.height, TextureFormat.RGBA32, srcMap.mipmapCount > 1);
        result.name = srcMap.name + "Alpha" + _textureCount;
        _textureCount++;

        for (int j = 0; j < srcMap.height; ++j)
        {
            for (int i = 0; i < srcMap.width; ++i)
            {
                Color destColor = (destMap != null) ? destMap.GetPixel(i, j) : Color.black;
                Color srcColor = srcMap.GetPixel(i, j);
                destColor.a *= srcColor.grayscale;
                result.SetPixel(i, j, destColor);
            }
        }

        result.Apply();
        return result;
    }


    ///<summary> Converts a tangent-space normal map to the format Unity's built-in shaders expect (g, g, g, r). </summary>
    public static Texture2D ToNormalMap(Texture2D normalMap)
    {
        Texture2D result = new Texture2D(normalMap.width, normalMap.height, TextureFormat.RGBA32, true);
        result.name = normalMap.name + "Normal" + _textureCount;
        _textureCount++;

        for (int j = 0; j < normalMap.height; ++j)
        {
            for (int i = 0; i < normalMap.width; ++i)
            {
                Color normal = normalMap.GetPixel(i, j);
                normal.a = normal.r;
                normal.r = normal.b = normal.g;
                result.SetPixel(i, j, normal);
            }
        }

        result.Apply();
        return result;
    }


    ///<summary> Converts an RGB emission map to an RGBA self-illumination map. </summary>
    public static Texture2D ToSelfIlluminMap(Texture2D emissionMap)
    {
        Texture2D result = new Texture2D(emissionMap.width, emissionMap.height);
        result.name = emissionMap.name + "Emission" + _textureCount;
        _textureCount++;

        for (int j = 0; j < emissionMap.height; ++j)
        {
            for (int i = 0; i < emissionMap.width; ++i)
            {
                Color pixel = emissionMap.GetPixel(i, j);
                pixel.a = Mathf.Max(new float[] {pixel.r, pixel.g, pixel.b});
                result.SetPixel(i, j, pixel);
            }
        }

        result.Apply();
        return result;
    }

    protected static int _textureCount = 0;
}
