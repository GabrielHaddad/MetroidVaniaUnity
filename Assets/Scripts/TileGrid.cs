using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    [SerializeField] Color baseColor, offSetColor;
    GameObject highLight;
    SpriteRenderer spriteRenderer;

    void Awake() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        highLight = transform.GetChild(0).gameObject;
    }

    void Start() 
    {
        highLight.SetActive(false);
    }

    public void Init(bool isOffset)
    {
        spriteRenderer.color = isOffset ? offSetColor : baseColor;
    }

    void OnMouseEnter() 
    {
        highLight.SetActive(true);
    }

    void OnMouseExit() 
    {
        highLight.SetActive(false);
    }
}
