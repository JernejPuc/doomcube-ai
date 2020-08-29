using UnityEngine;

public class DCAreaSolo : MonoBehaviour
{
    public GameObject[] spawnAreas;
    public DCDummy[] doomCubeAgents;

    public void ResetArea(DCAgentSolo doomCubeAgent)
    {
        foreach (DCDummy doomCubeDummy in doomCubeAgents)
        {
            PlaceDummy(doomCubeDummy, Random.Range(0,9));
        }

        PlaceAgent(doomCubeAgent, Random.Range(0,9));
    }

    public void PlaceAgent(DCAgentSolo doomCube, int zone)
    {
        // Physics
        HaltAgent(doomCube);
        
        // Spawn position
        doomCube.transform.position = ChooseRandomPosition(zone);

        // Spawn direction
        doomCube.transform.rotation = ChooseRandomRotation();
    }

    public void PlaceDummy(DCDummy doomCube, int zone)
    {
        if (zone > 8)
        {
            // Spawn position
            doomCube.transform.position = transform.position + new Vector3(20f + ((float)(zone-8))*4f, 1.5f, 55f);

            // Spawn direction
            doomCube.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            // Spawn position
            doomCube.transform.position = ChooseRandomPosition(zone);

            // Spawn direction
            doomCube.transform.rotation = ChooseRandomRotation();
        }
    }

    public void HaltAgent(DCAgentSolo doomCube)
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

    public void ResolveHit(DCAgentSolo winnerAgent, DCDummy loserAgent, int score)
    {
        winnerAgent.AddReward(score);
        PlaceDummy(loserAgent, 8 + score);

        if (score > 2) winnerAgent.EndEpisode();
    }
}
