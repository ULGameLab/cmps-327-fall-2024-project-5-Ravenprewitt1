using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior {EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    //pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    //properties
    public float speed = 1.0f;
    public float visionDistance = 5;
    public int maxCounter = 5;
    protected int playerCloseCounter;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1; 

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        pathFinder = new PathFinder();
        playerGameObject = GameObject.FindWithTag("Player");
        playerCloseCounter = maxCounter;
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            //Debug.Log("Enemy stopped since the player has reached the goal or the player is dead");
            return;
        }

        switch(behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }

    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Dumb Enemy: Keeps Walking in Random direction, Will not chase player
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                
                //Changed the color to white to differentiate from other enemies
                material.color = Color.white;
                
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Enemy chases the player when it is nearby
    private void HandleEnemyBehavior2()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 

                //Changed the color to black to differentiate from other enemies
                material.color = Color.black;

                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    playerCloseCounter--;

                    if(playerCloseCounter <= 0)
                    {
                        if (Vector3.Distance(playerGameObject.gameObject.transform.position, transform.position) < visionDistance)
                        {
                            
                            path.Clear();
                            //if an player is close reset counter
                            playerCloseCounter = maxCounter;
                            break;

                        }
                    }
                    //if counter is over 0, got to chase
                    if (playerCloseCounter > 0) state = EnemyState.CHASE;
                    else state = EnemyState.DEFAULT;

                }
                break;

            //IMPLEMENT
            case EnemyState.CHASE:
                material.color = Color.red;
                if (path.Count <= 0)
                {
                    targetTile = playerGameObject.GetComponent<Player>().currentTile;
                    path = pathFinder.FindPathAStar(currentTile,targetTile);
                    playerCloseCounter = 0;
                }
                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }

                break;


            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Enemy chases the player from three tiles away when it is nearby
    private void HandleEnemyBehavior3()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 

                //Changed the color to gray to differentiate from other enemies
                material.color = Color.gray;

                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;


                //if too close, clear path
                if(Vector3.Distance(playerGameObject.gameObject.transform.position, transform.position) < 3)
                {
                    path.Clear();
                }


                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    playerCloseCounter--;

                    if (playerCloseCounter <= 0)
                    {
                        if (Vector3.Distance(playerGameObject.gameObject.transform.position, transform.position) < visionDistance)
                        {

                            path.Clear();
                            //if an player is close reset counter
                            playerCloseCounter = maxCounter;
                            break;

                        }
                    }
                    //if counter is over 0, got to chase
                    if (playerCloseCounter > 0) state = EnemyState.CHASE;
                    else state = EnemyState.DEFAULT;
                }

                break;

            //IMPLEMENT
            case EnemyState.CHASE:

                material.color = Color.magenta;

                // Call the function to find the next tile 3 tiles behind the player
                //targetTile = FindChaseTargetTile(playerGameObject);
                
                    Vector3 playerPosition = playerGameObject.transform.position;
                    Vector3 playerVelocity = playerGameObject.GetComponent<Player>().velocity;

                    // Predict where the player will be after 'lookaheadTime'
                    float lookaheadTime = 1.0f;  // Time in seconds (adjust based on your game mechanics)
                    Vector3 targetPredictedPosition = playerPosition + playerVelocity * lookaheadTime;

                    // Get direction of the player's movement (normalized)
                    Vector3 directionToPlayer = playerVelocity.normalized;

                    // Calculate the position 3 tiles behind the predicted position
                    // Assuming 'mapTileSize' is the size of a tile in world space (adjust accordingly)
                    float tileDistance = 3;  // 3 tiles behind
                    Vector3 targetPositionBehindPlayer = targetPredictedPosition - directionToPlayer * tileDistance;

                    // Now we need to find the tile closest to 'targetPositionBehindPlayer'
                    targetTile = null;
                    float minDistance = Mathf.Infinity;

                    // Search for the closest tile to the target position behind the player
                    foreach (Tile adjacent in currentTile.Adjacents)
                    {
                        float distanceToTarget = Vector3.Distance(adjacent.transform.position, targetPositionBehindPlayer);
                        if (distanceToTarget < minDistance)
                        {
                            minDistance = distanceToTarget;
                            targetTile = adjacent;
                        }
                    }
               
               
                // If a valid target tile is found, calculate the path to it
                if (targetTile != null && path.Count <= 0)
                {
                    path = pathFinder.FindPathAStar(currentTile, targetTile);
                    playerCloseCounter = 0;
                }

                // If there's a valid path, move to the next tile in the path
                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();  // Move to the next tile in the path
                    state = EnemyState.MOVING;  // Transition to MOVING state
                }

                break;


               


            default:
                state = EnemyState.DEFAULT;
                break;
        }

    }


    
        



}

