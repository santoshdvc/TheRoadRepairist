using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RootMotion.FinalIK;
using UnityEngine.SceneManagement;
using DarkTonic.MasterAudio;
using XInputDotNetPure;

public class Player : MonoBehaviour
{
    public int m_playerID = 1;
    public float m_rotateFactor = 1.0f;
    public float m_moveFactor = 1.0f;
    public float m_minimumInput = 0.5f;
    public VariableJoystick m_variableJoystick;
    public float m_moveAwayDot = 0.5f;
    public float m_fixRateOverTime = 0.5f;
    public ParticleSystem m_hammerEffect;
    public RagdollUtility m_ragdollUtility;
    public Rigidbody m_pelvis;
    public Collider m_mainCollider;
    public float m_hitForce = 100.0f;
    public GameObject m_deadPanel;
    public float m_score;
    public Text m_scoreCard;

    Animator m_animator;
    Rigidbody m_rb;

    float m_hitPlayedGap;
    bool m_dead;
    Pothole m_currentPothole;

    // Start is called before the first frame update
    void Start()
    {
        print(Input.GetJoystickNames().ToString());
        m_animator = GetComponent<Animator>();
        m_rb = GetComponent<Rigidbody>();
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }


    private void OnAnimatorMove()
    {
        Vector3 displacement = m_animator.deltaPosition * m_moveFactor;
        //print(displacement + "    " + displacement.magnitude);
        m_rb.MovePosition(transform.position + displacement);
    }

    float m_potholeTriggerStay;
    private void OnTriggerStay(Collider other)
    {
        if(m_currentPothole == null)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Potholes"))
            {
                m_potholeTriggerStay += Time.fixedDeltaTime;
                if(m_potholeTriggerStay > 0.3f)
                {
                    SetPothole(other);
                }
            }
        }
    }

    void SetPothole(Collider other)
    {
        m_potholeTriggerStay = 0.0f;
        m_currentPothole = other.transform.parent.GetComponent<Pothole>();
        m_animator.SetBool("Fixing", true);

        //ParticleSystem.EmissionModule em = m_hammerEffect.emission;
        //em.enabled = true;
        m_hammerEffect.Play(true);
    }

    public Player m_otherPlayer;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Potholes"))
        {
            SetPothole(other);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("CarHit") && m_dead == false)
        {
            m_dead = true;
            m_mainCollider.enabled = false;
            m_rb.isKinematic = true;
            m_hitForceVector = transform.position - other.transform.position;
            m_hitForceVector.y = 1.0f;
            m_hitForceVector *= m_hitForce;
            Invoke("DelayedHitForce", 0.1f);
            m_ragdollUtility.EnableRagdoll();

            if (m_otherPlayer == null)
            {
                other.GetComponentInParent<Car>().ForceStop();
                m_deadPanel.SetActive(true);
            }
            else
            {
                if (m_otherPlayer.m_dead)
                {
                    other.GetComponentInParent<Car>().ForceStop();
                    m_deadPanel.SetActive(true);
                }
            }
            if (m_playerID == 2)
            {
                GamePad.SetVibration(0, 0.5f, 0.5f);
            }
            if(m_playerID == 2)
                Invoke("StopViberation", 1.55f);
        }
    }

    Vector3 m_hitForceVector;

    void DelayedHitForce()
    {
        m_pelvis.AddForce(m_hitForceVector);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_dead)
            return;
        

        m_scoreCard.text = "Points: " + m_score.ToString("0.00");

        Vector3 axis;
        
        if(m_playerID == 1)
        {
            axis = new Vector3(Input.GetAxis("Horizontal1"), 0.0f, Input.GetAxis("Vertical1"));
        }
        else if(m_playerID == 2)
        {
            axis = new Vector3(Input.GetAxis("Horizontal2"), 0.0f, Input.GetAxis("Vertical2"));
        }
        else
        {
            axis = new Vector3(Input.GetAxis("Horizontal")+m_variableJoystick.Horizontal, 0.0f, Input.GetAxis("Vertical")+m_variableJoystick.Vertical);
        }

        if (m_currentPothole == null)
        {
            // movement //
            if (axis.magnitude > 0.01f)
            {
                if (axis.magnitude < m_minimumInput)
                {
                    axis = axis.normalized * m_minimumInput;
                }
                Quaternion targetRotation = Quaternion.LookRotation(axis, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_rotateFactor);
            }
            m_animator.SetFloat("Speed", Mathf.Min(1.0f, axis.magnitude));
        }
        else
        {
            m_hitPlayedGap += Time.deltaTime;
            if(m_hitPlayedGap > 0.5f)
            {
                m_hitPlayedGap = 0.0f;
                MasterAudio.PlaySound3DAtTransformAndForget("Ham", transform);
                MasterAudio.PlaySound3DAtTransformAndForget("Hammer", transform);
            }
            // fixing //
            m_animator.SetFloat("Speed", 0.0f);
            Vector3 lookAt = m_currentPothole.transform.position - transform.position;
            lookAt.y = 0.0f;
            lookAt.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(lookAt, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_rotateFactor);

            m_currentPothole.DecreaseTime(Time.deltaTime * m_fixRateOverTime);
            if(m_currentPothole.m_potholeState <= 0.0f)
            {
                PotholeFixed();
                return;
            }

            if (axis.magnitude > 0.5f)
            {
                float dot = Vector3.Dot(lookAt, axis.normalized);
                if(dot < m_moveAwayDot)
                {
                    UnsetPothole();
                    return;
                }
            }

            
        }
    }

    void UnsetPothole()
    {
        m_currentPothole = null;
        m_animator.SetBool("Fixing", false);
        //ParticleSystem.EmissionModule em = m_hammerEffect.emission;
        //em.enabled = false;
        m_hammerEffect.Stop(true);
    }

    void PotholeFixed()
    {
        if(m_currentPothole == null)
        {
            Debug.LogError("pothole error");
            return;
        }

        MasterAudio.PlaySound3DAtTransformAndForget("Warp", transform);

        if (m_playerID == 2)
        {
            GamePad.SetVibration(0, 0.5f, 0.5f);
        }
        Invoke("StopViberation", 0.25f);
        m_currentPothole.m_finishedEffected.Play(true);

        m_score += 5.0f;

        PotholeManager.Instance.RemovePothole(m_currentPothole);
        UnsetPothole();
    }

    void StopViberation()
    {
        if (m_playerID == 2)
        {
            GamePad.SetVibration(0, 0.0f, 0.0f);
        }
    }
}
