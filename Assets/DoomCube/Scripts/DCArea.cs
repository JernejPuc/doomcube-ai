using UnityEngine;

public class DCArea : MonoBehaviour
{
    public GameObject[] spawnAreas;
    public DCAgent[] doomCubeAgents;
    
    public void PlaceAgent(DCAgent doomCube)
    {
        // Physics
        HaltAgent(doomCube);

        // Spawn zone
        int zone = Random.Range(0,9);
        
        // Spawn position
        var newPosition = Vector3.zero;
        Collider[] hitColliders;
        bool overlaps = false;

        float colliderRadius = doomCube.GetComponent<SphereCollider>().radius;

        do
        {
            newPosition = ChooseRandomPosition(zone);
            hitColliders = Physics.OverlapSphere(newPosition, colliderRadius);
            overlaps = false;

            foreach (Collider hitCollider in hitColliders)
            {
                DCAgent otherAgent = hitCollider.transform.GetComponent<DCAgent>();

                if (otherAgent != null) overlaps = true;
            }
        } while (overlaps);

        doomCube.transform.position = newPosition;

        // Spawn direction
        doomCube.transform.rotation = ChooseRandomRotation();
    }

    public void HaltAgent(DCAgent doomCube)
    {
        Rigidbody rigidbody = doomCube.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    public Vector3 ChooseRandomPosition(int zone)
    {
        // Get zone ranges
        var spawnTransform = spawnAreas[zone].transform;
        var xRange = spawnTransform.localScale.x / 2.1f;
        var zRange = spawnTransform.localScale.z / 2.1f;

        return new Vector3(Random.Range(-xRange, xRange), 1.5f, Random.Range(-zRange, zRange)) + spawnTransform.position;
    }

    public static Quaternion ChooseRandomRotation()
    {
        return Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f);
    }

    public void ResolveHit(DCAgent winnerAgent, DCAgent loserAgent, float stepRatio)
    {
        // winnerAgent.AddReward(2f - stepRatio);
        // loserAgent.AddReward(-2f + stepRatio);
        winnerAgent.SetReward(1f);
        loserAgent.SetReward(-1f);
        winnerAgent.EndEpisode();
        loserAgent.EndEpisode();
    }
}
