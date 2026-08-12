using System.Collections.Generic;

[System.Serializable]
public class Order
{
    public CoffeeType coffeeType;
    public CupSize size;
    public int reward;
    public float timeLimit;

    // Sadece kasa ekranında "müşterinin isteği" metnini oluşturmak için - eşleşme kontrolü şimdilik yok
    public List<string> requestedExtras = new List<string>();
    public string preferredPaymentMethod = "";
}