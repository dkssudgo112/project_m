using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class River : MonoBehaviour
{
    private const int speedPenaltyInRiver = 150;
    private const int speedPenaltyInRiverForAI = 2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerInfo>().AddEventSpeedValue(-speedPenaltyInRiver);
        }
        if (collision.gameObject.CompareTag("AI"))
        {
            collision.gameObject.GetComponent<AIAgent>()._basicSpeed -= speedPenaltyInRiverForAI;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerInfo>().AddEventSpeedValue(+speedPenaltyInRiver);
        }
        if (collision.gameObject.CompareTag("AI"))
        {
            collision.gameObject.GetComponent<AIAgent>()._basicSpeed += speedPenaltyInRiverForAI;
        }
    }
}
