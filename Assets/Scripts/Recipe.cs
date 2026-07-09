using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public class Recipe
{
    public string coffeeName;

    public Sprite coffeeImage;

    [TextArea(5, 10)]
    public string recipeSteps;

    public int price;
}