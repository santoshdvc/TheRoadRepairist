using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotholeManager : MonoBehaviour
{
    public Transform[] m_spawnPoints;
    public GameObject m_potholePrefab;

    public Vector2 m_spawnInterval = new Vector2(6.0f, 8.0f);

    public float m_timer;

    public List<Pothole> m_spawned;

    public static PotholeManager Instance { get; private set; }

    public Player m_player1;
    public Player m_player2;
    public float m_scoreLossPerSec = 0.5f;
    public Text m_panelty;

    private void Awake()
    {
        Instance = this;   
    }

    void Start()
    {
        for(int i=0; i<m_spawnPoints.Length; i++)
        {
            m_spawnPoints[i].gameObject.SetActive(false);
        }

        m_timer = Random.Range(m_spawnInterval.x, m_spawnInterval.y) * 0.1f;
    }

    void Update()
    {
        m_timer -= Time.deltaTime;

        if(m_player1 != null)
        {
            m_player1.m_score -= m_spawned.Count * m_scoreLossPerSec * Time.deltaTime;
        }

        if (m_player2 != null)
        {
            m_player2.m_score -= m_spawned.Count * m_scoreLossPerSec * Time.deltaTime;
        }

        m_panelty.text = "-" + m_spawned.Count * m_scoreLossPerSec + " Per Sec";

        if(m_timer <= 0.0f)
        {
            Transform spawnPoint = m_spawnPoints[0];

            int i = 0;
            while (i < 60)
            {
                i++;
                spawnPoint = m_spawnPoints[Random.Range(0, m_spawnPoints.Length)];

                if(IsOverAnyOtherPothole(spawnPoint.position))
                {
                    continue;
                }

                break;
            }

            GameObject addMe = GameObject.Instantiate(m_potholePrefab, spawnPoint.transform.position, Quaternion.identity);

            m_spawned.Add(addMe.GetComponent<Pothole>());

            m_timer = Random.Range(m_spawnInterval.x, m_spawnInterval.y);
        }
    }

    bool IsOverAnyOtherPothole(Vector3 pos)
    {
        for (int j = 0; j < m_spawned.Count; j++)
        {
            if ((pos - m_spawned[j].transform.position).magnitude < 1.0f)
            {
                return true;
            }
        }
        return false;
    }

    public void RemovePothole(Pothole ph)
    {
        Destroy(ph.gameObject, 2.0f);
        m_spawned.Remove(ph);
    }
}
