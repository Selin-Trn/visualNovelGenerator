using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab;
    public Dictionary<string, NPC> npcs = new Dictionary<string, NPC>();
    private HashSet<NPCPosition> availablePositions = new HashSet<NPCPosition> { NPCPosition.Left, NPCPosition.Center, NPCPosition.Right };

    public enum NPCPosition
    {
        Left = 1,
        Center = 2,
        Right = 3
    }

    public void CreateNPCs(StoryData.NPCJsonData[] npcDatas)
    {
        foreach (var npcData in npcDatas)
        {
            GameObject npcObject = Instantiate(npcPrefab, transform);
            NPC npcComponent = npcObject.GetComponent<NPC>();
            NPCPosition position = DeterminePosition();
            npcComponent.Init(npcData.name, position);
            npcs.Add(npcData.name, npcComponent);
        }
    }

    private NPCPosition DeterminePosition()
    {
        // Find the first available position
        var position = availablePositions.FirstOrDefault();
        availablePositions.Remove(position);
        return position;
    }

    public void ShowNPC(string name)
    {
        if (npcs.TryGetValue(name, out NPC npc))
        {
            if (!npc.IsShowing)
            {
                npc.Show();
                availablePositions.Remove(npc.Position); // Mark position as occupied
            }
        }
    }

    public void HideNPC(string name)
    {
        if (npcs.TryGetValue(name, out NPC npc))
        {
            if (npc.IsShowing)
            {
                npc.Hide();
                availablePositions.Add(npc.Position); // Mark position as available
            }
        }
    }
}
