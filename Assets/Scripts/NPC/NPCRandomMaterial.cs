using System.Collections.Generic;
using UnityEngine;

public class NPCRandomMaterial : MonoBehaviour
{
    [Header("Kıyafet Materyali")]
    [SerializeField] private Material clothesMaterial;

    [Header("Saç Materyali")]
    [SerializeField] private Material hairMaterial;

    [Header("Kıyafet Textureleri")]
    [SerializeField]
    private List<Texture2D> clothesTextures =
        new List<Texture2D>();

    [Header("Saç Textureleri")]
    [SerializeField]
    private List<Texture2D> hairTextures =
        new List<Texture2D>();


    private void Awake()
    {
        RandomizeTextures();
    }


    public void RandomizeTextures()
    {
        // Kıyafet texture seç
        if (clothesMaterial != null &&
            clothesTextures.Count > 0)
        {
            Texture2D randomClothes =
                clothesTextures[
                    Random.Range(0, clothesTextures.Count)
                ];

            clothesMaterial.SetTexture(
                "_BaseMap",
                randomClothes
            );

            Debug.Log(
                $"{gameObject.name} kıyafet texture: " +
                randomClothes.name
            );
        }


        // Saç texture seç
        if (hairMaterial != null &&
            hairTextures.Count > 0)
        {
            Texture2D randomHair =
                hairTextures[
                    Random.Range(0, hairTextures.Count)
                ];

            hairMaterial.SetTexture(
                "_BaseMap",
                randomHair
            );

            Debug.Log(
                $"{gameObject.name} saç texture: " +
                randomHair.name
            );
        }
    }
}