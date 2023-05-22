using System.Collections;
using System.Collections.Generic;
using static EnemyStates;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateManager : MonoBehaviour
{
    public GameObject isDeaf;
    public GameObject isBlind;

    public Transform TransformR, TransformL;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
    public float visionRange;
    public float AudioRange;
    public Transform rayOrigin;
    public Transform target;
    public float visionAngle;

    public float HP;
    public float speed;

    public bool isBlinded;
    public bool isDeafened;

    public static EnemyStateManager instance;
    public EnemyStates currentState;

    public NavMeshAgent agent;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        currentState = new IdleEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();
    }

    public void DetectCharacterVision()
    {
        if (!isBlinded)
        {
            Debug.Log("NOT BLINDED");
            //Guardamos todos los objetos encontrados con el overlap
            Collider[] _targets = Physics.OverlapSphere(transform.position, visionRange, targetLayer);
            //Si ha encontrado algún objeto, la longitud del array es mayor que 0
            if (_targets.Length > 0)
            {
                //Calculamos la direccion hacia el objeto
                Vector3 _targetDir = _targets[0].transform.position - rayOrigin.position;
                //Si esta fuera del angulo de vision, lo ignoramos
                //Se calcula si esta dentro con el angulo que hay entre el forward y la direccion
                //del objetivo. Si este angulo es menor que la mitad del angulo de vision, esta dentro
                if (Vector3.Angle(transform.forward, _targetDir) > visionAngle / 2f)
                {
                    return;
                }
                //Lanzamos un rayo desde el enemigo hacia el jugador para comprobar si esta
                //escondido detras de alguna pared u obstaculo
                //Sumamos un offset al origen en el eje Y para que no lance el rayo desde los pies
                if (Physics.Raycast(rayOrigin.position, _targetDir.normalized,
                    _targetDir.magnitude, obstacleLayer) == false)
                {
                    target = _targets[0].transform;
                }
                //Dibujamos el rayo que comprueba si esta tras un obstaculo
                //Sumamos un offset al origen en el eje Y para que no lance el rayo desde los pies
                Debug.DrawRay(rayOrigin.position, _targetDir, Color.magenta);
            }
            //Si el array está vacío, no ha encontrado nada
            else
            {
                //Dejamos el target a null para que deje de perseguirlo
                target = null;
            }
        }


        if (isBlinded)
        {
            isBlind.SetActive(true);
            isBlind.transform.LookAt(nahinController.instance.transform);
            StartCoroutine(finishNeutralize());
        }
    }

    public void DetectCharacterHearing()
    {
        if (!isDeafened)
        {
            Debug.Log("NOT DEAFENED");
            //Guardamos todos los objetos encontrados con el overlap
            Collider[] _targets = Physics.OverlapSphere(transform.position, AudioRange, targetLayer);
            //Si ha encontrado algún objeto, la longitud del array es mayor que 0
            if (_targets.Length > 0)
            {
                //Calculamos la direccion hacia el objeto
                Vector3 _targetDir = _targets[0].transform.position - rayOrigin.position;
                if (nahinController.instance.isSneaking == true)
                {
                    return;
                }
                //Lanzamos un rayo desde el enemigo hacia el jugador para comprobar si esta
                //escondido detras de alguna pared u obstaculo
                //Sumamos un offset al origen en el eje Y para que no lance el rayo desde los pies
                if (Physics.Raycast(rayOrigin.position, _targetDir.normalized,
                    _targetDir.magnitude, obstacleLayer) == false)
                {
                    target = _targets[0].transform;
                }
                //Dibujamos el rayo que comprueba si esta tras un obstaculo
                //Sumamos un offset al origen en el eje Y para que no lance el rayo desde los pies
                Debug.DrawRay(rayOrigin.position, _targetDir, Color.magenta);
            }
            //Si el array está vacío, no ha encontrado nada
            else
            {
                //Dejamos el target a null para que deje de perseguirlo
                target = null;
            }
        }
        if (isDeafened)
        {
            isDeaf.SetActive(true);
            isDeaf.transform.LookAt(nahinController.instance.transform);
            StartCoroutine(finishNeutralize());
        }
        
    }

    public void Patrol()
    {

    }

    public void Chase()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
            agent.speed = speed;
            if (agent.remainingDistance <= 1f)
            {
                agent.velocity = Vector3.zero;
            }
        }
    }
    IEnumerator finishNeutralize()
    {
        //hay que hacer que el tiempo que dura la ceguera o la sordera sea igual a una variable del controlador de Nahin
        yield return new WaitForSeconds(15f);
        isDeaf.SetActive(false);
        isBlind.SetActive(false);
        isBlinded = false;
        isDeafened = false;
        transform.tag = "NeutralizeTarget";
    }
    private void OnDrawGizmos()
    {
        //Dibujamos el rango de vision
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        //Dibujamos el cono de vision
        Gizmos.color = Color.green;
        //Rotamos los helper para que tengan la rotacion igual a la mitad del angulo de vision
        //Para dibujar el cono de vision, rotamos dos objetos vacios para luego lanzar un rayo
        //en el forward de cada uno de ellos y dibuje el cono
        TransformL.localRotation = Quaternion.Euler(0f, visionAngle / -2f, 0f);
        TransformR.localRotation = Quaternion.Euler(0f, visionAngle / 2f, 0f);
        Gizmos.DrawRay(TransformL.position, TransformL.forward * visionRange);
        Gizmos.DrawRay(TransformR.position, TransformR.forward * visionRange);
    }
}
