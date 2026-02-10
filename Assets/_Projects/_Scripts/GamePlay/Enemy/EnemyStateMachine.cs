using System.Collections.Generic;
using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 敌人状态枚举
    /// </summary>
    public enum EnemyStateType
    {
        Idle,       // 待机
        Patrol,     // 巡逻
        Alert,      // 警戒
        Chase,      // 追击
        Attack,     // 攻击
        Stunned,    // 眩晕/被贴点
        Dead,       // 死亡
        Friendly    // 友方状态（茶杯变身后）
    }

    /// <summary>
    /// 敌人状态基类 - 所有状态继承此类
    /// </summary>
    public abstract class EnemyState
    {
        protected EnemyBase Enemy;
        protected EnemyStateMachine StateMachine;

        public EnemyState(EnemyBase enemy, EnemyStateMachine stateMachine)
        {
            Enemy = enemy;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }

    /// <summary>
    /// 敌人状态机 - 管理敌人的所有状态和状态切换
    /// </summary>
    public class EnemyStateMachine
    {
        private EnemyBase _enemy;
        private Dictionary<EnemyStateType, EnemyState> _states = new Dictionary<EnemyStateType, EnemyState>();
        
        public EnemyState CurrentState { get; private set; }
        public EnemyStateType CurrentStateType { get; private set; }
        public EnemyStateType PreviousStateType { get; private set; }

        public EnemyStateMachine(EnemyBase enemy)
        {
            _enemy = enemy;
        }

        public void RegisterState(EnemyStateType stateType, EnemyState state)
        {
            if (!_states.ContainsKey(stateType))
            {
                _states.Add(stateType, state);
            }
        }

        public void ChangeState(EnemyStateType newStateType)
        {
            if (!_states.ContainsKey(newStateType))
            {
                Debug.LogWarning($"[EnemyStateMachine] 状态 {newStateType} 未注册");
                return;
            }

            CurrentState?.Exit();
            PreviousStateType = CurrentStateType;
            CurrentStateType = newStateType;
            CurrentState = _states[newStateType];
            CurrentState.Enter();

            Debug.Log($"[EnemyStateMachine] {_enemy.gameObject.name}: {PreviousStateType} -> {CurrentStateType}");
        }

        public void Update() => CurrentState?.Update();
        public void FixedUpdate() => CurrentState?.FixedUpdate();
    }

    #region 具体状态实现

    /// <summary>
    /// 待机状态
    /// </summary>
    public class EnemyIdleState : EnemyState
    {
        private float _idleTimer;
        private float _idleDuration;

        public EnemyIdleState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            _idleTimer = 0f;
            _idleDuration = Random.Range(1f, 3f);
            Enemy.SetAnimationState("Idle");
        }

        public override void Update()
        {
            if (Enemy.CanSeePlayer())
            {
                StateMachine.ChangeState(EnemyStateType.Alert);
                return;
            }

            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _idleDuration)
            {
                StateMachine.ChangeState(EnemyStateType.Patrol);
            }
        }
    }

    /// <summary>
    /// 巡逻状态
    /// </summary>
    public class EnemyPatrolState : EnemyState
    {
        private Vector3 _patrolTarget;
        private float _patrolTimer;

        public EnemyPatrolState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            _patrolTarget = Enemy.GetRandomPatrolPoint();
            _patrolTimer = 0f;
            Enemy.SetAnimationState("Walk");
        }

        public override void Update()
        {
            if (Enemy.CanSeePlayer())
            {
                StateMachine.ChangeState(EnemyStateType.Alert);
                return;
            }

            Enemy.MoveTo(_patrolTarget, Enemy.PatrolSpeed);

            _patrolTimer += Time.deltaTime;
            if (Enemy.HasReachedDestination(_patrolTarget) || _patrolTimer > 10f)
            {
                StateMachine.ChangeState(EnemyStateType.Idle);
            }
        }
    }

    /// <summary>
    /// 警戒状态
    /// </summary>
    public class EnemyAlertState : EnemyState
    {
        private float _alertTimer;
        private const float AlertDuration = 0.5f;

        public EnemyAlertState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            _alertTimer = 0f;
            Enemy.SetAnimationState("Alert");
            Enemy.LookAtPlayer();
        }

        public override void Update()
        {
            _alertTimer += Time.deltaTime;

            if (_alertTimer >= AlertDuration)
            {
                if (Enemy.IsPlayerInChaseRange())
                {
                    StateMachine.ChangeState(EnemyStateType.Chase);
                }
                else
                {
                    StateMachine.ChangeState(EnemyStateType.Patrol);
                }
            }
        }
    }

    /// <summary>
    /// 追击状态
    /// </summary>
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            Enemy.SetAnimationState("Run");
        }

        public override void Update()
        {
            if (Enemy.IsPlayerInAttackRange())
            {
                StateMachine.ChangeState(EnemyStateType.Attack);
                return;
            }

            if (!Enemy.IsPlayerInChaseRange())
            {
                StateMachine.ChangeState(EnemyStateType.Patrol);
                return;
            }

            Enemy.ChasePlayer();
        }
    }

    /// <summary>
    /// 攻击状态
    /// </summary>
    public class EnemyAttackState : EnemyState
    {
        private bool _hasAttacked;

        public EnemyAttackState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            _hasAttacked = false;
            Enemy.LookAtPlayer();
        }

        public override void Update()
        {
            if (!Enemy.IsPlayerInAttackRange())
            {
                StateMachine.ChangeState(EnemyStateType.Chase);
                return;
            }

            // 如果还没有攻击且不在攻击动画中，执行攻击
            if (!_hasAttacked && !Enemy.IsAttacking)
            {
                Enemy.PerformAttack();
                _hasAttacked = true;
            }

            // 如果攻击动画结束，重置攻击标记以便下次攻击
            if (_hasAttacked && !Enemy.IsAttacking)
            {
                _hasAttacked = false;
            }

            Enemy.LookAtPlayer();
        }
    }

    /// <summary>
    /// 眩晕/被贴点状态
    /// </summary>
    public class EnemyStunnedState : EnemyState
    {
        public EnemyStunnedState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            Enemy.SetAnimationState("Stunned");
            Enemy.OnPointAttached();
        }

        public override void Exit()
        {
            Enemy.OnPointDetached();
        }
    }

    /// <summary>
    /// 死亡状态
    /// </summary>
    public class EnemyDeadState : EnemyState
    {
        public EnemyDeadState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            Enemy.SetAnimationState("Dead");
            Enemy.OnDeath();
        }
    }

    /// <summary>
    /// 友方状态
    /// </summary>
    public class EnemyFriendlyState : EnemyState
    {
        public EnemyFriendlyState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            Enemy.SetAnimationState("Friendly");
            Enemy.OnBecomeFriendly();
        }

        public override void Update()
        {
            Enemy.FriendlyBehavior();
        }

        public override void Exit()
        {
            Enemy.OnBecomeHostile();
        }
    }

    #endregion
}

