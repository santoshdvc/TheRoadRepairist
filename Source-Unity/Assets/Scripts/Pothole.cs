using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DarkTonic.MasterAudio;

public class Pothole : MonoBehaviour
{

    [Serializable]
    public class TimedEvent
    {
        public enum EventType
        { GameObject, ParticleSystem, Function, Collider }
        public enum PlaySound
        { None, Dig, Crack, RabbitJump}
        [Range(0.0f, 1.0f)]
        public float m_time;
        public bool m_triggered;
        public EventType m_type;
        public GameObject m_GO;
        public ParticleSystem m_particleSystem;
        public string m_function;
        public Collider m_collider;
        public PlaySound m_sound = PlaySound.None;

        public void Trigger(Pothole potehole)
        {
            if (m_triggered)
                return;
            m_triggered = true;
            switch(m_type)
            {
                default:
                    break;
                case EventType.GameObject:
                    m_GO.SetActive(true);
                    break;
                case EventType.Function:
                    potehole.InvokeFunction(m_function);
                    break;
                case EventType.ParticleSystem:
                    ParticleSystem.EmissionModule em = m_particleSystem.emission;
                    em.enabled = true;
                    m_particleSystem.Play(true);
                    break;
                case EventType.Collider:
                    m_collider.enabled = true;
                    break;
            }

            switch(m_sound)
            {
                default:
                case PlaySound.None:
                    break;
                case PlaySound.Dig:
                    MasterAudio.PlaySound3DAtTransformAndForget("Dig", potehole.transform);
                    break;
                case PlaySound.Crack:
                    MasterAudio.PlaySound3DAtTransformAndForget("Break", potehole.transform);
                    break;
                case PlaySound.RabbitJump:
                    MasterAudio.PlaySound3DAtTransformAndForget("RabbitJump", potehole.transform);
                    break;
            }
        }

        public void UnTrigger(Pothole potehole)
        {
            if (m_triggered == false)
                return;
            m_triggered = false;
            switch(m_type)
            {
                default:
                    break;
                case EventType.GameObject:
                    m_GO.SetActive(false);
                    break;
                case EventType.Function:
                    break;
                case EventType.ParticleSystem:
                    ParticleSystem.EmissionModule em = m_particleSystem.emission;
                    em.enabled = false;
                    break;
                case EventType.Collider:
                    m_collider.enabled = false;
                    break;

            }
        }
    }

    [Header("-------Events--------")]
    public TimedEvent [] m_events;
    [Header("---------------------")]
    public Animator m_rabbitAnimator;
    [Range(0.0f, 1.0f)]
    public float m_potholeState;
    public float m_timeFactor = 1.0f;
    public float m_rabbitMoveSpeed = 1.0f;
    public ParticleSystem m_finishedEffected;
    public bool m_pop;

    Transform m_rabbitTransform;
    Vector3 m_rabbitOrgPosition;
    bool m_showRabbit;

    public void InvokeFunction(string func)
    {
        Invoke(func, 0.0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rabbitTransform = m_rabbitAnimator.GetComponent<Transform>();
        m_rabbitOrgPosition = m_rabbitTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_pop)
        {
            IncreaseTime(Time.deltaTime * m_timeFactor);
        }

        if(m_showRabbit)
        {
            m_rabbitTransform.localPosition = Vector3.Lerp(m_rabbitTransform.localPosition, Vector3.zero, Time.deltaTime*m_rabbitMoveSpeed);
        }
        else
        {
            m_rabbitTransform.localPosition = Vector3.Lerp(m_rabbitTransform.localPosition, m_rabbitOrgPosition, Time.deltaTime*m_rabbitMoveSpeed);
        }
    }

    public void IncreaseTime(float by)
    {
        m_potholeState += by;
        m_potholeState = Mathf.Clamp01(m_potholeState);

        for(int i=0; i<m_events.Length; i++)
        {
            if(m_events[i].m_time <= m_potholeState)
            {
                if(m_events[i].m_triggered == false)
                {
                    m_events[i].Trigger(this);
                }
            }
        }

        if(m_pop && m_potholeState >= 1.0f)
        {
            m_pop = false;
        }
    }

    public void DecreaseTime(float by)
    {
        m_pop = false;
        HideRabbit();
        m_potholeState -= by;
        m_potholeState = Mathf.Clamp01(m_potholeState);

        for(int i=0; i<m_events.Length; i++)
        {
            if(m_events[i].m_time > m_potholeState)
            {
                if(m_events[i].m_triggered == true)
                {
                    m_events[i].UnTrigger(this);
                }
            }
        }
    }

    public void ShowRabbit()
    {
        m_showRabbit = true;
    }

    public void HideRabbit()
    {
        m_showRabbit = false;
    }

    public void AnimEventTriggerRabbitJump()
    {
        m_rabbitAnimator.SetTrigger("Jump");
    }
}
