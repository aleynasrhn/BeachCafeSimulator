using UnityEngine;

public class LidItem : MonoBehaviour
{
    [Header("Kapak Boyutu")]
    [SerializeField] private CupSize size;

    public CupSize Size => size;
}