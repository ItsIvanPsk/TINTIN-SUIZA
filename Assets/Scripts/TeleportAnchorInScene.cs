using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAnchorInScene : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;

    public void TeleportToRoof()
    {
        if (Player != null)
        {
            Console.WriteLine("Pressed!");
            Player.transform.position = new Vector3 (6.761f, 15.879f, -14.561f);
        }
    }

    public void TeleportToRocketMuseum()
    {
        if (Player != null)
        {
            Console.WriteLine("Pressed!");
            Player.transform.position = new Vector3(6.4f, 8.929f, -11.259f);
        }
    }

}
