using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    public float damage;

    BoxCollider triggerBox;

    // Start is called before the first frame update
    void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "NeutralizeTarget" || other.tag == "Neutralized")
        {
            Debug.Log("Damaged");
            var enemy = other.gameObject.GetComponent<EnemyStateManager>();

            if (enemy != null)
            {
                enemy.transform.GetComponent<Animator>().Play("Hurt", 0, 0);
                enemy.HP -= damage;
                if (enemy.HP <= 0)
                {
                    Destroy(enemy.gameObject);
                }
            }
        }
        
    }

    public void EnableTriggerBox()
    {
        triggerBox.enabled = true;
    }

    public void DisableTriggerBox()
    {
        triggerBox.enabled = false;
    }
}
