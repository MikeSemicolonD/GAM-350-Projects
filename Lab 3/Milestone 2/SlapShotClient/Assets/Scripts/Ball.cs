using UnityEngine;

public class Ball : MonoBehaviour {

    new Rigidbody2D rigidbody;
    NetworkSync networkSync;
    int bounces = 0;

    private void Awake()
    {
        networkSync = GetComponent<NetworkSync>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        networkSync.CallRPC("IAmTheBall", UCNetwork.MessageReceiver.ServerOnly);
        rigidbody.velocity = new Vector2(1.5f, -2.1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag=="BallHitZone")
        {
            Debug.Log("Ball Reverse called from Ball Hit Zone");
            networkSync.CallRPC("PassTurnFromBall", UCNetwork.MessageReceiver.ServerOnly);
        }

        if (collision.transform.tag == "BallHitBoundary")
        {
            Debug.Log("End Game called from Ball Hit Boundary");
            EndGame();    
        }
    }

    /// <summary>
    /// Increases the speed by 15%
    /// Gets called from the client when the ball hits the "BallHitZone"
    /// </summary>
    public void IncreaseBallSpeed()
    {
        bounces++;
        ExampleClient.GetInstance().UpdateScore(bounces);
        rigidbody.velocity += rigidbody.velocity * 0.15f;
    }

    /// <summary>
    /// Gets called by the ball when it hits a "BallHitBoundary"
    /// Whoever had it during this turn has lost
    /// </summary>
    public void EndGame()
    {
        networkSync.CallRPC("EndTheGame", UCNetwork.MessageReceiver.ServerOnly,bounces);
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Gets called by the ball when it hits a "BallHitZone"
    /// It just reverses direction on the y-axis
    /// </summary>
    public void Reverse()
    {
        rigidbody.velocity = new Vector3(rigidbody.velocity.x,-rigidbody.velocity.y);
        IncreaseBallSpeed();
    }
}
