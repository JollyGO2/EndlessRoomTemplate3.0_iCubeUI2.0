using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PulsingUI : MonoBehaviour
{
    [SerializeField] bool isPulsing = false;
    [SerializeField] float pulsingSpeed = 5f;

    [Range(0f, 1f)]
    [SerializeField] float maxScale = 0.05f;
    [Range(0f, 1f)]
    [SerializeField] float minScale = 0.05f;
    private Vector3 originalScaling;

    // Start is called before the first frame update
    void Start()
    {
        //thisImage = GetComponent<Image>();
        originalScaling = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPulsing)
        {
            float alpha = (Mathf.Sin(Time.time * pulsingSpeed) + 1f) / 2f; // 01

            float max = 1 + maxScale;
            float min = 1 - minScale;
            Vector3 scale = new Vector3(Mathf.Lerp(min, max, alpha), Mathf.Lerp(min, max, alpha), 1);
            
            transform.localScale = scale; // Scale
        }
    }

    public void SetPulsing(bool pulsing)
    {
        if (isPulsing && !pulsing)
        {
            transform.localScale = originalScaling; //Reset Back to 1
        }
        isPulsing = pulsing;
    }
}
