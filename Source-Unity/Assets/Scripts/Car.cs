using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;

public class Car : MonoBehaviour
{
    public Vector2 m_timeRange = new Vector2(5.0f, 15.0f);
    public float m_speed = 1.0f;
    public float m_endX;
    public GameObject m_smoke;
    public GameObject m_warningEffect;

    bool m_stop;
    Transform m_transform;
    Vector3 m_startPos;
    Vector3 m_endPos;
    float m_wait;
    bool m_playWarning = true;
    bool m_playHorn = true;

    void Start()
    {
        m_transform = transform;
        m_startPos = m_transform.position;
        m_endPos = m_startPos;
        m_endPos.x = m_endX;

        m_wait = Random.Range(m_timeRange.x, m_timeRange.y);
    }

    public void ForceStop()
    {
        m_stop = true;
        MasterAudio.PlaySound3DAtTransformAndForget("Accident", transform);
        MasterAudio.PlaySound3DAtTransformAndForget("Hurt", transform);
        MasterAudio.PlaySound3DAtTransformAndForget("CarWarning", transform);
    }

    void Update()
    {
        if(m_stop)
        {
            return;
        }

        m_wait -= Time.deltaTime;
        if(m_wait > 0.0f)
        {
            if(m_wait < 3.0f)
            {
                m_warningEffect.SetActive(true);

                if(m_playWarning)
                {
                    m_playWarning = false;
                    //MasterAudio.PlaySound3DAtTransformAndForget("CarStart", transform);
                }

                if (m_playHorn)
                {
                    m_playHorn = false;
                    MasterAudio.PlaySound3DAtTransformAndForget("CarStart", transform, 0.2f, null, 3.0f);
                }
            }
            else
            {
                m_warningEffect.SetActive(false);
            }
            return;
        }

        m_playHorn = true;
        m_playWarning = true;
        if (m_wait < -0.5f)
        {
            m_warningEffect.SetActive(false);
        }

        m_transform.position = Vector3.MoveTowards(m_transform.position, m_endPos, Time.deltaTime * m_speed);

        if((m_transform.position-m_endPos).magnitude < 2.0f)
        {
            m_wait = Random.Range(m_timeRange.x, m_timeRange.y);
            m_transform.position = m_startPos;
            m_smoke.SetActive(false);
        }
        else
        {
            m_smoke.SetActive(true);
        }
    }
}
