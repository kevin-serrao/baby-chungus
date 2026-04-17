using UnityEngine;

/// <summary>
/// Spawns bots and assigns them to hover above the player.
/// </summary>
public class BotSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bot;
    [SerializeField] private LogicManager logic;

    private void Start()
    {
        // Only spawn one bot at game start
        SpawnBot();
    }

    /// <summary>
    /// Spawns a bot above the player and sets it to hover phase.
    /// </summary>
    public void SpawnBot()
    {
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 spawnPos = transform.position;
        if (player != null)
        {
            spawnPos = player.transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 6f, 0f); // Above player
        }
        GameObject thisBot = Instantiate(bot, spawnPos, Quaternion.identity);
        BotScript botScript = thisBot.GetComponent<BotScript>();
        if (botScript != null)
        {
            botScript.EnterHoverPhase(player);
        }
    }
}
