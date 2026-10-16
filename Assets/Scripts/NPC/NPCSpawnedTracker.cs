using UnityEngine;

public class NPCSpawnedTracker : MonoBehaviour
{
    private NPCSpawner spawner;

    private bool initialized = false;

    public void Initialize(
        NPCSpawner npcSpawner)
    {
        spawner =
            npcSpawner;

        initialized = true;
    }

    private void OnDestroy()
    {
        if (!initialized)
            return;

        if (spawner != null)
        {
            spawner.NotifyNPCDestroyed();
        }
    }
}