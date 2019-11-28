using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    PlayerController player;

    public TextMeshProUGUI numOfThermalSensors;

    public
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!player)
        {
            player = FindObjectOfType<PlayerController>();
        }
        else
        {
            numOfThermalSensors.SetText(player.thermalVisionUses.ToString());
        }

    }
}
