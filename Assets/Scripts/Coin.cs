using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private Animator anim;
    private void OnTriggerEnter(Collider other)
    {
        anim = GetComponent<Animator>();
        if (other.tag == "Player")
        {
            GameManager.Instance.GetCoin();
            anim.SetTrigger("collected");
        }
    }
} 