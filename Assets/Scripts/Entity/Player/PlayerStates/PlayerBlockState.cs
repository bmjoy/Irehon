﻿using System;
using Irehon;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class PlayerBlockState : PlayerRotatableState
{
    public const int StaminaCost = 2000;
    public PlayerBlockState(Player player) : base(player)
    {
        animator = player.GetComponent<Animator>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private Animator animator;
    private PlayerMovement playerMovement;
    public override float MovementSpeed => 0.75f;

    public override bool CanInteract => false;

    public override PlayerStateType Type => PlayerStateType.Block;

    public override void Enter(bool isResimulating)
    {
        if (!isResimulating)
        {
            this.abilitySystem.animator.SetBool("isBlocking", true);
            player.takeDamageProcessQuerry.Add(PlayerCombatMath.BlockDamageProcess);
        }
    }

    public override void Exit(bool isResimulating)
    {
        if (!isResimulating)
        {
            this.abilitySystem.animator.SetBool("isBlocking", false);
            player.takeDamageProcessQuerry.Remove(PlayerCombatMath.BlockDamageProcess);
        }
    }
    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (player.staminaPoints < StaminaCost)
        {
            return PlayerStateType.Idle;
        }

        if (input.IsKeyPressed(KeyCode.Space) && player.staminaPoints > PlayerJumpingState.JumpCost)
        {
            return PlayerStateType.Jump;
        }

        if (!input.IsKeyPressed(KeyCode.Mouse1))
        {
            return PlayerStateType.Idle;
        }

        if (input.IsKeyPressed(KeyCode.LeftShift) && input.GetMoveVector().x == 0 && input.GetMoveVector().y > 0 && player.staminaPoints > PlayerRunState.MinimalStamina)
        {
            return PlayerStateType.Run;
        }

        if (input.GetMoveVector() != Vector2.zero)
        {
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }
        this.playerMovement.Move(input.GetMoveVector(), MovementSpeed);
        this.animator.SetFloat("xMove", input.GetMoveVector().x);
        this.animator.SetFloat("zMove", input.GetMoveVector().y);

        return this.Type;
    }
}
