using UnityEngine;
using System.Collections;

/// <summary>
/// Makinedeki fiziksel düğmeye (gördüğün kırmızı power ikonu) eklenir. Tek basışlı.
/// Sadece pitcherDock'ta (steam wand altındaki dock noktası) İÇİNDE SÜT OLAN bir
/// pitcher takılıyken çalışır. Basılınca: duman/buhar görselini birkaç saniye açar,
/// süre bitince sütün materyalini "buharlanmış/köpüklü" materyale çevirir.
/// </summary>
public class SteamButton : MonoBehaviour, IInteractable
{
    [Header("Referanslar")]
    [Tooltip("Steam wand altındaki, pitcher'ın takıldığı MachineDockPoint")]
    [SerializeField] private MachineDockPoint pitcherDock;
    [Tooltip("Duman/buhar efekti (particle system ya da basit bir mesh). Opsiyonel, boş bırakabilirsin.")]
    [SerializeField] private GameObject steamVisual;
    [Tooltip("Sütün buharlandıktan sonra alacağı materyal (köpüklü/beyaz görünüm)")]
    [SerializeField] private Material frothedMilkMaterial;

    [Header("Ayarlar")]
    [SerializeField] private float steamDuration = 3f;

    private bool isSteaming = false;

    public string GetInteractPrompt()
    {
        if (isSteaming) return "Buharlanıyor...";
        if (pitcherDock == null || !pitcherDock.IsOccupied) return "Önce pitcher'ı tak";

        MilkFiller filler = pitcherDock.DockedItem.GetComponent<MilkFiller>();
        if (filler == null || !filler.HasMilk) return "Önce süt doldur";

        return "E - Buharlandır";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isSteaming) return;
        if (pitcherDock == null || !pitcherDock.IsOccupied) return;

        MilkFiller filler = pitcherDock.DockedItem.GetComponent<MilkFiller>();
        if (filler == null || !filler.HasMilk) return; // önce süt lazım

        StartCoroutine(SteamRoutine(filler));
    }

    private IEnumerator SteamRoutine(MilkFiller filler)
    {
        isSteaming = true;

        if (steamVisual != null)
            steamVisual.SetActive(true);

        yield return new WaitForSeconds(steamDuration);

        if (steamVisual != null)
            steamVisual.SetActive(false);

        filler.SetFrothedMaterial(frothedMilkMaterial);

        isSteaming = false;
    }
}