using System.Collections.Generic;

[System.Serializable]
public class Order
{
    public CoffeeType coffeeType;

    public CupSize size;

    // Espresso için Tek Shot / Double Shot
    public EspressoShotButtonUI.ShotType espressoShot;

    public int reward;

    public float timeLimit;

    // Müşteri talebindeki ekstralar
    public List<string> requestedExtras =
        new List<string>();

    // Nakit / Kart
    public string preferredPaymentMethod = "";
}