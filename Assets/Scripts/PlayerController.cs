using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public bool alive = true;
    float rotateSpeed = 120;
    float velocity = 6;
    float acceleration = 5;
    float maxVel = 15;
    float minVel = 6;
    public float score = 0;
    public float distanceTraveled = 0;
    public float averageVelocity = 6;
    int sampleCount = 0;
    bool fitnessBoost = true;
    float lastFitnessBoostDistance = 0;
    float fitnessBoostMargin = .25f;
    public int lap = 0;
    Collider2D[] nearbyColliders;


    public NeuralNetwork brain;

    private void Start()
    {
        if (!PlayerSpawner.instance.play)
        {
            //brain = new NeuralNetwork(3, 7, 2); // turn only
            brain = new NeuralNetwork(4, 9, 5); // accel/decel
        }

        nearbyColliders = new Collider2D[20];
    }

    private void Update()
    {
        if (!alive)
        {
            return;
        }

        //Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, 30) * transform.right * 100, Color.green);
        //Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, 0) * transform.right * 100, Color.green);
        //Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -30) * transform.right * 100, Color.green);

        if (PlayerSpawner.instance.play)
        {
            if (Input.GetAxisRaw("Horizontal") < 0)
            {
                SteerLeft();
            }
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                SteerRight();
            }
            if (Input.GetAxisRaw("Vertical") > 0)
            {
                Accelerate();
            }
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                Decelerate();
            }

            if (Input.GetButton("Fire1"))
            {
                if (Input.mousePosition.x < Screen.width / 2)
                {
                    SteerLeft();
                }
                else
                {
                    SteerRight();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!alive)
        {
            return;
        }
        sampleCount++;
        distanceTraveled += velocity * Time.fixedDeltaTime;
        averageVelocity = (averageVelocity * sampleCount + velocity) / (sampleCount + 1);
        transform.position += transform.right * velocity * Time.fixedDeltaTime;

        if (!PlayerSpawner.instance.play)
        {
            float[] inputs = new float[4];
            inputs[0] = Physics2D.Raycast(transform.position, Quaternion.Euler(0, 0, -30) * transform.right, 1000, 1 << 8).distance;
            inputs[1] = Physics2D.Raycast(transform.position, Quaternion.Euler(0, 0, 0) * transform.right, 1000, 1 << 8).distance;
            inputs[2] = Physics2D.Raycast(transform.position, Quaternion.Euler(0, 0, 30) * transform.right, 1000, 1 << 8).distance;
            inputs[3] = velocity;
            float[] outputs = brain.FeedForward(inputs);
            // find max output -> do decision

            int steerDecision = -1;
            if (outputs[0] > outputs[1])
            {
                steerDecision = 0;
            }
            else
            {
                steerDecision = 1;
            }
            int moveDecision = -1;
            float max = 0;
            for (int j = 2; j < outputs.Length; j++)
            {
                if (outputs[j] > max)
                {
                    moveDecision = j;
                    max = outputs[j];
                }
            }

            if (steerDecision == 0)
            {
                SteerLeft();
            }
            else if (steerDecision == 1)
            {
                SteerRight();
            }
            if (moveDecision == 2)
            {
                Accelerate();
            }
            else if (moveDecision == 3)
            {
                Decelerate();
            }
            else if (moveDecision == 4)
            {
                // neither accelerate or decelerate
            }
        }

        if (fitnessBoost)
        {
            Physics2D.OverlapCircleNonAlloc(transform.position, 2, nearbyColliders, 1 << 8);
            float minDistance = float.MaxValue;
            Collider2D closest = null;
            foreach (Collider2D col in nearbyColliders)
            {
                if (col == null)
                {
                    break;
                }
                float disp = (col.transform.position - transform.position).magnitude;
                if (disp < minDistance)
                {
                    minDistance = disp;
                    closest = col;
                }
            }
            //Debug.DrawRay(closest.transform.position, transform.position - closest.transform.position, Color.blue);

            Vector2 oppositeDirection = (closest.transform.position - transform.position).normalized * -1;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, oppositeDirection, 1000, 1 << 8);
            if (hit)
            {
                //Debug.DrawRay(hit.transform.position, transform.position - hit.transform.position, Color.red);
                Vector3 middle = (closest.transform.position + hit.transform.position) / 2;
                if ((transform.position - middle).magnitude < .25f)
                {
                    fitnessBoost = false;
                    lastFitnessBoostDistance = distanceTraveled;
                    score += 50;
                }
            }
        }
        else
        {
            if (lastFitnessBoostDistance + fitnessBoostMargin < distanceTraveled)
            {
                fitnessBoost = true;
            }
        }
    }

    void SteerLeft()
    {
        transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.fixedDeltaTime));
    }

    void SteerRight()
    {
        transform.Rotate(new Vector3(0, 0, -1 * rotateSpeed * Time.fixedDeltaTime));
    }

    void Accelerate()
    {
        velocity += acceleration * Time.fixedDeltaTime;
        if (velocity > maxVel)
        {
            velocity = maxVel;
        }
    }

    void Decelerate()
    {
        velocity -= acceleration * Time.fixedDeltaTime;
        if (velocity < minVel)
        {
            velocity = minVel;
        }
    }

    public void Die()
    {
        if (alive)
        {
            score += distanceTraveled * averageVelocity;

            if (PlayerSpawner.instance.play || PlayerSpawner.instance.testBrain)
            {
                alive = false;
                SceneManager.LoadScene(0);
            }
            else
            {
                PlayerSpawner.instance.populationAlive--;
                alive = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void Reset()
    {
        alive = true;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        score = 0;
        lap = 0;
        distanceTraveled = 0;
        averageVelocity = minVel;
        sampleCount = 0;
        velocity = minVel;
        fitnessBoost = true;
        lastFitnessBoostDistance = 0;
        gameObject.SetActive(true);
    }
}
