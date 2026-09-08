# State Machine Implementation Plan

## Overview
Implement a finite state machine (FSM) system for the game character using Godot C#, following the existing patterns in `examples/` folder.

## Architecture

### Core Components
1. **StateMachine Node** - Central coordinator managing state transitions
2. **BaseState Class** - Abstract base class defining state lifecycle pattern
3. **Individual State Classes** - One per state from states.md

### State Categories (from states.md)
- Damaged (Ground): Ragdoll, Tech ground, Stand up
- Damaged (Air): Ragdoll, Aerial Recovery, Collide with surface, Tech Surface
- Ground: Idle, Walk, Run, Melee Combat, Aim, Grapple, Grab
- Climb: Hold Soft/Hard, Climb Soft/Hard, Climb Jump Forward/Up/Off, Surface Stab, Aim, Grapple, Brace Hold Soft/Hard
- Air: Double Jump, Wall Jump, Grapple, Air Dash, Spin Slash, Aim Ranged Weapon, Grab

## Implementation Steps

### Phase 1: Core Infrastructure (Already Partially Implemented)
1. ✅ BaseState.cs exists with Character property and Machine reference
2. ⏳ Create BaseCharacter.cs (parent class for character with position, velocity, gravity, input handling)
3. ⏳ Update StateMachine.cs to use ParentCode.GetParent<BaseCharacter>() in _Ready()
4. ⏳ Add StateMachine node to player.tscn scene

### Phase 2: Idle State Implementation
5. Implement IdleState.cs (idle animation, input handling, state completion logic)

### Phase 3: Integration
6. Connect PlayerPhysics to StateMachine (coordinate movement with states)
7. Handle state-to-animation mapping for idle state
8. Add input handling per state
9. Test state transitions and interrupts

## Key Design Decisions

### State Machine Node Placement
- StateMachine as child of CharacterBody3D (or parent class)
- All states added as children of StateMachine node
- PlayerPhysics remains separate but receives state callbacks

### Interrupt Priority System
- Force (0): Damage, ragdoll activation
- Input (1): Player input during certain states
- TakeHit (2): Regular damage while in state

### State Lifecycle
1. OnStateBegin() - Called when entering state
2. _Process(delta) - Called each frame
3. _PhysicsProcess(delta) - Physics updates per frame
4. IsDone() - Returns true when state should end
5. CanInterrupt(type) - Returns true if state can be interrupted

## Files to Create/Modify

### New Files (Core Infrastructure & Idle State)
- `examples/BaseCharacter.cs` (new - parent class for character)
- `scenes/states/IdleState.cs` (idle movement state)

### Modified Files
- `examples/StateMachine.cs` (update ParentCode.GetParent<BaseCharacter>() in _Ready())
- `scenes/player/player.tscn` (add StateMachine node as child)
- `scenes/player/PlayerPhysics.cs` (integrate with state machine for movement coordination)

## Verification Steps
1. Verify BaseCharacter.cs has required properties (position, velocity, gravity, input)
2. Verify StateMachine.GetParent<BaseCharacter>() works correctly
3. Test IdleState transitions between idle/walk/run animations
4. Verify interrupt priority system works for IdleState
5. Test state-to-animation mapping for idle/base states
6. Verify PlayerPhysics integrates with StateMachine movement
7. Confirm StateMachine node added to player.tscn scene tree
