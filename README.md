# Freddie's Castle Defense

## Overview
"Freddie's Castle Defense" is a hybrid tower defense and action game where players control Freddie, 
a heroic swordsman defending his castle against waves of zombies. Unlike traditional tower defense games, 
players actively engage in combat using sword attacks while managing their position.

## Core Components

### Audio Management (AudioManager.cs)
- Implements Singleton pattern for global audio control
- Handles background music (not implemented in submission) and sound effects
- Features adjustable volume and pitch settings
- Supports randomized variations for effects like footsteps
- Provides methods for enabling/disabling music and SFX

#### Castle Health (CastleHealth.cs)
- Manages castle's health system and visual state
- Features different sprites based on damage levels (100, 50, 0)
- Implements damage flash effects and healing mechanics
- Handles game over state when castle is destroyed
- Includes UI health bar with color transitions

#### Health Bar (HealthBar.cs)
- Provides visual representation of castle health
- Implements smooth health bar animations
- Features color-coded health states
- Supports billboard mode to always face camera
- Shows numerical health values

#### Hero Knight (HeroKnight.cs)
- Controls player character movement and combat
- Implements 3-hit combo system
- Features upgradeable damage system
- Handles footstep sounds during movement
- Uses circle colliders for attack detection

### Wave Management (WaveManager.cs)
- Controls game progression through zombie waves
- Manages wave difficulty scaling
- Handles between-wave upgrades (sword damage/castle healing)
- Implements boss wave mechanics
- Features UI for wave status and kill counting
- Includes start game and game over states

#### Zombie Health (ZombieHealth.cs)
- Manages zombie health and damage states
- Handles death animations and effects
- Supports both normal and boss zombie types
- Implements hurt state cooldown
- Triggers appropriate death sounds

#### Zombie Spawner (ZombieSpawner.cs)
- Controls zombie spawning mechanics
- Manages multiple spawn lanes
- Handles boss zombie spawning
- Implements difficulty scaling per wave
- Controls zombie sorting layers and visibility
- Features special lane management for boss zombies

## Key Features

### Combat System
- Real-time sword combat with combo system
- Upgradeable sword damage
- Hit detection using physics2D
- Visual and audio feedback for attacks

### Wave System
- Progressive difficulty scaling
- Boss waves every third wave
- Between-wave upgrade choices
- Kill counting and wave completion

### Boss Mechanics
- Larger & stronger boss zombies every third wave
- Increased size and health
- Special lane management
- Distinct visual appearance

### UI Systems
- Health bar for castle
- Wave counters
- Kill counters
- Start game panel with controls display
- Wave complete panel with upgrade options
- Game over panel with restart functionality

## Controls
- **Movement**: WASD keys
- **Attack**: Left Mouse Button
