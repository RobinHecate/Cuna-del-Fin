using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class States
{
    public enum STATES
    {
        IDLE, RUN, FALL, SNEAK, ATTACK, POTENTIATION, POTENTIATIONATTACK, WALLPARKOUR
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
    protected States nextState;

    public States Process()
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

    public class IdleNahin : States
    {
        public IdleNahin() : base()
        {
            name = STATES.IDLE;
        }

        public override void Start()
        {
            nahinController.instance.speed = 6f;
            base.Start();
        }

        public override void Update()
        {
            ParkourManager.instance.Parkour();
            nahinController.instance.anim.SetLayerWeight(1, 0f);
            if (ParkourManager.instance.playerInaction == false)
            {
                nahinController.instance.Movement();
                nahinController.instance.IdleJump();
            }
            nahinController.instance.Grounded();            
            nahinController.instance.Neutralize();            
           
            if (nahinController.instance.horizontal != 0 || nahinController.instance.vertical != 0)
            {
                nextState = new Run();
                stage = EVENTS.EXIT;
            }
            if (nahinController.instance.moonEnergy >= .1f && Input.GetKeyDown(KeyCode.R) && nahinController.instance.PotentiationUnlocked)
            {
                nextState = new Potentiation();
                stage = EVENTS.EXIT;
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                nahinController.instance.isSneaking = true;
                nextState = new Sneaking();
                stage = EVENTS.EXIT;
            }
            if (Input.GetMouseButtonDown(0))
            {
                nahinController.instance.Attack();
                nextState = new Attacking();
                stage = EVENTS.EXIT;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Run : States
    {
        public Run() : base()
        {
            name = STATES.RUN;
        }

        public override void Start()
        {
            nahinController.instance.speed = 6f;
            base.Start();
        }
        public override void Update()
        {
            var hitInfo = new ObstacleInfo();
            ParkourManager.instance.inFrontOfObstacle = hitInfo.hitFound;
            ParkourManager.instance.Parkour();
            nahinController.instance.anim.SetLayerWeight(1, 0f);
            if (ParkourManager.instance.playerInaction == false && ParkourManager.instance.inFrontOfObstacle == false)
            {
                nahinController.instance.anim.ResetTrigger("Jump");
                nahinController.instance.Movement();
                nahinController.instance.RunJump();
            }
            nahinController.instance.Grounded();            
            nahinController.instance.Neutralize();
            
         
            if (nahinController.instance.horizontal == 0f && nahinController.instance.vertical == 0f)
            {
                nextState = new IdleNahin();
                stage = EVENTS.EXIT;
            }
            if (nahinController.instance.moonEnergy >= .1f && Input.GetKeyDown(KeyCode.R) && nahinController.instance.PotentiationUnlocked)
            {
                nextState = new Potentiation();
                stage = EVENTS.EXIT;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                nahinController.instance.isSneaking = true;
                nextState = new Sneaking();
                stage = EVENTS.EXIT;
            }

            if (Input.GetMouseButtonDown(0))
            {
                nahinController.instance.Attack();
                nextState = new Attacking();
                stage = EVENTS.EXIT;
            }
        }
        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Potentiation : States
    {
        public Potentiation() : base()
        {
            name = STATES.POTENTIATION;
        }

        public override void Start()
        {
            
            base.Start();
        }
        public override void Update()
        {
            nahinController.instance.anim.SetLayerWeight(1, 2f);
            WallRun.instance.CheckWall();
            if (nahinController.instance.moonEnergy >= .1f)
            {
                nahinController.instance.moonEnergy -= Time.deltaTime;
                nahinController.instance.speed = nahinController.instance.buffedSpeed;
               
            }
            if (Input.GetMouseButtonDown(0))
            {
                nahinController.instance.PoweredAttack();
                nextState = new PoweredAttacking();
                stage = EVENTS.EXIT;
            }
            nahinController.instance.BuffedMovement();
            nahinController.instance.Grounded();
            nahinController.instance.StrongJump();
            WallRun.instance.HandleWallRunInput();

            nahinController.instance.Neutralize();
            if (nahinController.instance.moonEnergy <= .1f || Input.GetKeyDown(KeyCode.R))
            {
                nextState = new IdleNahin();
                stage = EVENTS.EXIT;
            }


        }
        public override void Exit()
        {
            base.Exit();
        }

    }

    public class Sneaking : States
    {
        public Sneaking() : base()
        {
            name = STATES.SNEAK;
        }

        public override void Start()
        {
            base.Start();
        }
        public override void Update()
        {
            nahinController.instance.anim.SetLayerWeight(2, 2f);
            nahinController.instance.Sneak();
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                nahinController.instance.isSneaking = false;
                nextState = new IdleNahin();
                stage = EVENTS.EXIT;
                nahinController.instance.anim.SetLayerWeight(2, 0f);
            }
        }
        public override void Exit()
        {
            base.Exit();
        }
    }

    public class WallInteraction : States
    {
        public WallInteraction() : base()
        {
            name = STATES.WALLPARKOUR;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Attacking : States
    {
        public Attacking() : base()
        {
            name = STATES.ATTACK;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            nahinController.instance.transform.LookAt(nahinController.instance.enemy);
            if (Input.GetMouseButtonDown(0))
            {
                nahinController.instance.Attack();
            }
            nahinController.instance.ExitAttack();

            if (nahinController.instance.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > .9 && nahinController.instance.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                nextState = new IdleNahin();
                stage = EVENTS.EXIT;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class PoweredAttacking : States
    {
        public PoweredAttacking() : base()
        {
            name = STATES.ATTACK;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            nahinController.instance.anim.SetLayerWeight(1, 2f);
            if (Input.GetMouseButtonDown(0))
            {
                nahinController.instance.PoweredAttack();
            }
            nahinController.instance.pExitAttack();

            if (nahinController.instance.anim.GetCurrentAnimatorStateInfo(1).normalizedTime > .9 && nahinController.instance.anim.GetCurrentAnimatorStateInfo(1).IsTag("Attack"))
            {
                nextState = new Potentiation();
                stage = EVENTS.EXIT;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
