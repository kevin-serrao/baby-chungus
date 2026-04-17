using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    public GameObject bot;
    public LogicManager logic;
    public void spawnBot()
    {
        GameObject thisBot = Instantiate(bot, new Vector3(transform.position.x + Random.Range(-5, 5), transform.position.y + Random.Range(-5, 5), 0), transform.rotation);
    }
}
