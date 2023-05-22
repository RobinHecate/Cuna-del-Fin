using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStates
{
    public enum STATES
    {
        IDLE, PATROL, CHASE, ATTACK, BLINDED, DEAFENED
    }

    public enum EVENTS
    {
        START, UPDATE, EXIT
    }
    public virtual void Start() { stage = EVENTS.UPDATE; }
    public virtual void Update() { stage = EVENTS.UPDATE; }
    public virtual void Exit() { stage = EVENTS.EXIT; }

    public STATES name;
    protected EVENTS stage;
    protected EnemyStates nextState;

    public EnemyStates Process()
    {
        //Si el evento en el que estoy es el de entrada, hago el método correspondiente de entrada
        if (stage == EVENTS.START) Start();
        //Si el evento en el que estoy es el de update, hago el método correspondiente
        if (stage == EVENTS.UPDATE) Update();
        //Si el evento en el que estoy es el de salida, hago el método correspondiente
        if (stage == EVENTS.EXIT)
        {
            Exit();
            //Y devolvemos el siguiente estado al que ir
            return nextState;
        }
        //Devolvemos el resultado del método
        return this;
    }

    public class IdleEnemy : EnemyStates
    {
        public IdleEnemy() : base()
        {
            name = STATES.IDLE;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            EnemyStateManager.instance.DetectCharacterHearing();
            EnemyStateManager.instance.DetectCharacterVision();
            if (EnemyStateManager.instance.target != null)
            {
                nextState = new Chase();
                stage = EVENTS.EXIT;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Patrol: EnemyStates
    {
        public Patrol() : base()
        {
            name = STATES.PATROL;
        }
        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            EnemyStateManager.instance.DetectCharacterHearing();
            EnemyStateManager.instance.DetectCharacterVision();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Chase : EnemyStates
    {
        public Chase() : base()
        {
            name = STATES.CHASE;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            EnemyStateManager.instance.DetectCharacterHearing();
            EnemyStateManager.instance.DetectCharacterVision();
            EnemyStateManager.instance.Chase();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
