using UnityEngine;
using Unity.MLAgents;

public class DCAgentSolo : Agent
{
    // Set by user
    public GameObject area;
    public float fireRate;
    public float moveSpeed;
    public float turnSpeed;

    // Inferred
    DCAreaSolo m_AgentArea;
    DCDummy[] m_AgentTargets;
    Rigidbody m_AgentRb;
    Camera m_AgentCamera;

    // Effects
    ParticleSystem m_MuzzleFlash;
    AudioSource m_AudioSource;
    AudioClip m_ShootSFX;

    // Internal
    private float nextTimeToFire;
    private int score;

    public override void Initialize()
    {
        m_AgentArea = area.GetComponent<DCAreaSolo>();
        m_AgentTargets = m_AgentArea.doomCubeAgents;
        m_AgentRb = GetComponent<Rigidbody>();
        m_AgentCamera = transform.Find("AgentCamera").GetComponent<Camera>();

        m_MuzzleFlash = transform.Find("Weapon/Muzzle/MuzzleFlashYellow").GetComponent<ParticleSystem>();
        m_AudioSource = transform.Find("Weapon").GetComponent<AudioSource>();
        m_ShootSFX = m_AudioSource.clip;
        
        nextTimeToFire = 1f / fireRate;
        score = 0;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int triggerAction = Mathf.FloorToInt(vectorAction[0]);
        int actionMode = Mathf.FloorToInt(vectorAction[1]);
        int forwardAction = Mathf.FloorToInt(vectorAction[2]);
        int rightAction = Mathf.FloorToInt(vectorAction[3]);
        int turnAction = Mathf.FloorToInt(vectorAction[4]);

        // Existential penalty
        AddReward(-1f / MaxStep);

        // Encourage seeking / staying on targets
        if (HasTargetsInSight()) AddReward(1f / MaxStep);

        // Pull trigger
        if (triggerAction == 1)
        {
            // Discourage wastefulness
            AddReward(-1f / MaxStep);
            
            // Shoot
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                ShootWeapon();
            }
        }

        // Precision mode toggle
        float modeFactor = 3f;
        
        if (actionMode == 1) modeFactor = 1f;

        // Move
        float forwardAmount = 0f;
        float rightAmount = 0f;

        switch (forwardAction)
        {
            case 0:
                forwardAmount = modeFactor * moveSpeed;
                break;
            case 2:
                forwardAmount = -1f * modeFactor * moveSpeed;
                break;
        }

        switch (rightAction)
        {
            case 0:
                rightAmount = modeFactor * moveSpeed;
                break;
            case 2:
                rightAmount = -1f * modeFactor * moveSpeed;
                break;
        }

        m_AgentRb.velocity = transform.forward * forwardAmount + transform.right * rightAmount;

        // Rotate
        float turnAmount = 0f;

        switch (turnAction)
        {
            case 0:
                turnAmount = modeFactor * turnSpeed;
                break;
            case 2:
                turnAmount = -1f * modeFactor * turnSpeed;
                break;
        }

        transform.Rotate(transform.up * 1f, Time.fixedDeltaTime * turnAmount);
    }

    public override void Heuristic(float[] actionsOut)
    {
        // Defaults
        float triggerAction = 0f;
        float actionMode = 0f;
        float forwardAction = 1f;
        float rightAction = 1f;
        float turnAction = 1f;

        // Pull trigger
        if (Input.GetButton("Fire1")) triggerAction = 1f;

        // Precision mode toggle
        if (Input.GetButton("Fire2")) actionMode = 1f;

        // Move
        if (Input.GetKey(KeyCode.W))
        {
            forwardAction = 0f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            forwardAction = 2f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            rightAction = 0f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rightAction = 2f;
        }

        // Rotate
        float mouseSpeed = Input.GetAxis("Mouse X");

        if (mouseSpeed > 0.01)
        {
            turnAction = 0f;
        }
        else if (mouseSpeed < -0.01)
        {
            turnAction = 2f;
        }

        // Send out
        actionsOut[0] = triggerAction;
        actionsOut[1] = actionMode;
        actionsOut[2] = forwardAction;
        actionsOut[3] = rightAction;
        actionsOut[4] = turnAction;
    }

    public override void OnEpisodeBegin()
    {
        nextTimeToFire = Time.time + 1f / fireRate;
        score = 0;
        m_AgentArea.ResetArea(this);
    }

    bool HasTargetsInSight()
    {
        bool someTargetInSight = false;

        foreach (DCDummy doomCubeAgent in m_AgentTargets)
        {
            if (doomCubeAgent != this)
            {
                Vector3 tPos = m_AgentCamera.WorldToViewportPoint(doomCubeAgent.transform.position);

                if (tPos.x > 0f && tPos.x < 1f && tPos.y > 0f && tPos.y < 1f && tPos.z > 0f)
                {
                    RaycastHit hit;
                    Vector3 tDir = doomCubeAgent.transform.position - m_AgentCamera.transform.position;

                    if (Physics.Raycast(m_AgentCamera.transform.position, tDir, out hit, 150f))
                    {
                        DCDummy hitAgent = hit.transform.GetComponent<DCDummy>();

                        if (hitAgent != null && hitAgent == doomCubeAgent)
                        {
                            someTargetInSight = true;
                            break;
                        }
                    }
                }
            }
        }

        return someTargetInSight;
    }

    void ShootWeapon()
    {
        // VFX
        if (m_MuzzleFlash) m_MuzzleFlash.Play();

        // SFX
        if (m_ShootSFX) m_AudioSource.Play();

        // Hit detection
        RaycastHit hit;

        if (Physics.Raycast(m_AgentCamera.transform.position, m_AgentCamera.transform.forward, out hit, 150f))
        {
            DCDummy hitAgent = hit.transform.GetComponent<DCDummy>();

            if (hitAgent != null)
            {
                score += 1;
                m_AgentArea.ResolveHit(this, hitAgent, score);
            }
        }
    }
}
