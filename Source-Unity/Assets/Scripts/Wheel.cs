using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    Transform m_transform;
    public float m_rotation = 360.0f;
    public Vector3 m_axis;

    // Start is called before the first frame update
    void Start()
    {
        m_transform = transform;
        m_axis *= m_rotation;
    }

    // Update is called once per frame
    void Update()
    {
        m_transform.Rotate(m_axis * Time.deltaTime);
    }
}
