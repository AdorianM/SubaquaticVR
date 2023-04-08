using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColourGenerator : MonoBehaviour {
    public Material mat;
    public Gradient gradient;

    Texture2D tex;
    const int width = 50;
    const int height = 1;

    void Initialize () {
        if (tex == null || height != tex.height) {
            tex = new Texture2D (width, height, TextureFormat.RGBA32, false);
        }
    }

    void Update () {
        Initialize ();
        RefreshTexture ();

        MeshGenerator m = FindObjectOfType<MeshGenerator> ();
        float verticalBounds = m.boundsSize * m.numChunks.y;

        mat.SetFloat ("boundsY", verticalBounds);
        mat.SetTexture ("gradientramp", tex);
    }

    void RefreshTexture () {
        if (gradient != null) {
            Color[] colours = new Color[tex.width];
            for (int i = 0; i < width; i++) {
                Color gradientCol = gradient.Evaluate (i / (width - 1f));
                colours[i] = gradientCol;
            }

            // After colouring the pixels we can apply the texture
            tex.SetPixels (colours);
            tex.Apply ();
        }
    }
}