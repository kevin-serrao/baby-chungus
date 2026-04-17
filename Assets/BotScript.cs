using UnityEngine;

public class BotScript : MonoBehaviour
{
    public float acceleration = (float)0.8;
    public Rigidbody2D myRigidBody;
    public float moveFrequency;
    public float secondsSinceLastMove = 0;
    public LogicManager logic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManager>();
    }

    void OnEnable()
    {
        BeatConductor.OnBeatGlobal += OnBeat;
    }

    void OnDisable()
    {
        BeatConductor.OnBeatGlobal -= OnBeat;
    }

    void OnBeat(int beatIndex, float songTime)
    {
        // React to the beat
        wiggle();
    }

    // Update is called once per frame
    void Update()
    {
        if (secondsSinceLastMove < moveFrequency)
        { 
            secondsSinceLastMove += Time.deltaTime;
        } else { 
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            myRigidBody.linearVelocity += randomDirection * acceleration;
            secondsSinceLastMove = 0;
        }
    }

    public void wiggle()
    {
        Debug.Log("wiggling");
        myRigidBody.AddTorque(myRigidBody.angularVelocity > 0 ? -80f : 80f, ForceMode2D.Impulse);
        //MoveRotation(myRigidBody.rotation > 0 ? -30 : 30);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided!");
        logic.addScore(1);
        Destroy(gameObject);
    }
}
