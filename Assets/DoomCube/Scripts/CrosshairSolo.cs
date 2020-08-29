using UnityEngine;
using UnityEngine.UI;

public class CrosshairSolo : MonoBehaviour
{
    Camera m_Camera;
    Image m_Crosshair;

    void Start()
    {
        m_Camera = GetComponent<Camera>();
        m_Crosshair = transform.Find("Canvas/Image").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        // Hit detection
        RaycastHit hit;

        if (Physics.Raycast(m_Camera.transform.position, m_Camera.transform.forward, out hit, 150f))
        {

            DCDummy hitAgent = hit.transform.GetComponent<DCDummy>();

            if (hitAgent != null)
            {
                m_Crosshair.color = Color.red;
            }
            else
            {
                m_Crosshair.color = Color.white;
            }
        }
    }
}
