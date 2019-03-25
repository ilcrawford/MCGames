﻿using System;
using System.Composition;
using System.Collections.Generic;
using System.Text;

using ILCrawford.MCGame.MCCore;
using ILCrawford.MCGame.MissileCommand.Models;

using Raylib;
using static Raylib.Raylib;

namespace ILCrawford.MCGame.MissileCommand
{
    [Export(typeof(IGame))]
    public class MissleCommand : IGame
    {
        private const int MAX_MISSILES = 100;
        private const int MAX_INTERCEPTORS = 30;
        private const int MAX_EXPLOSIONS = 100;
        private const int LAUNCHERS_AMOUNT = 3;           // Not a variable, should not be changed
        private const int BUILDINGS_AMOUNT = 6;           // Not a variable, should not be changed

        private const int LAUNCHER_SIZE = 80;
        private const int BUILDING_SIZE = 60;
        private const int EXPLOSION_RADIUS = 40;

        private const int MISSILE_SPEED = 1;
        private const int MISSILE_LAUNCH_FRAMES = 80;
        private const int INTERCEPTOR_SPEED = 10;
        private const int EXPLOSION_INCREASE_TIME = 90;          // In frames
        private const int EXPLOSION_TOTAL_TIME = 210;         // In frames

        private static Color EXPLOSION_COLOR = new Color(125, 125, 125, 125);
        static int screenWidth = 1500;
        static int screenHeight = 1000;

        static int framesCounter = 0;
        static bool gameOver = false;
        static bool pause = false;
        static int score = 0;

        static Missile[] missile = new Missile[MAX_MISSILES];
        static Interceptor[] interceptor = new Interceptor[MAX_INTERCEPTORS];
        static Explosion[] explosion = new Explosion[MAX_EXPLOSIONS];
        static Launcher[] launcher = new Launcher[LAUNCHERS_AMOUNT];
        static Building[] building = new Building[BUILDINGS_AMOUNT];
        static Plane[] plane = new Plane[1];

        static int explosionIndex = 0;
        static double distance;
        static int interceptorNumber = 0;
        static int missileIndex = 0;
        public string Name => "Missile Command";

        public void Run()
        {
            // Initialization (Note windowTitle is unused on Android)
            //---------------------------------------------------------
            InitWindow(screenWidth, screenHeight, "sample game: missile commander");

            InitGame();

            SetTargetFPS(60);
            //--------------------------------------------------------------------------------------

            // Main game loop
            while (!WindowShouldClose())    // Detect window close button or ESC key
            {
                // Update and Draw
                //----------------------------------------------------------------------------------
                UpdateDrawFrame();
                //----------------------------------------------------------------------------------
            }

            // De-Initialization
            //--------------------------------------------------------------------------------------
            UnloadGame();         // Unload loaded data (textures, sounds, models...)

            CloseWindow();        // Close window and OpenGL context
                                  //--------------------------------------------------------------------------------------

        }

        //--------------------------------------------------------------------------------------
        // Game Module Functions Definition
        //--------------------------------------------------------------------------------------

        // Initialize game variables
        void InitGame()
        {
            // Initialize missiles
            for (int i = 0; i < MAX_MISSILES; i++)
            {
                missile[i].origin = new Vector2(0, 0);
                missile[i].speed = new Vector2(0, 0);
                missile[i].position = new Vector2(0, 0);

                missile[i].active = false;
            }

            // Initialize interceptors
            for (int i = 0; i < MAX_INTERCEPTORS; i++)
            {
                interceptor[i].origin = new Vector2(0, 0);
                interceptor[i].speed = new Vector2(0, 0);
                interceptor[i].position = new Vector2(0, 0);

                interceptor[i].active = false;
            }

            // Initialize explosions
            for (int i = 0; i < MAX_EXPLOSIONS; i++)
            {
                explosion[i].position = new Vector2(0, 0);
                explosion[i].frame = 0;
                explosion[i].active = false;
            }

            plane[0].position = new Vector2(0, 0);
            plane[0].speed = new Vector2(0, 0);
            plane[0].position = new Vector2(0, 0);
            plane[0].active = false;

            // Initialize buildings and launchers
            int sparcing = screenWidth / (LAUNCHERS_AMOUNT + BUILDINGS_AMOUNT + 1);

            // Buildings and launchers placing
            launcher[0].position = new Vector2(1 * sparcing, screenHeight - LAUNCHER_SIZE / 2);
            building[0].position = new Vector2(2 * sparcing, screenHeight - BUILDING_SIZE / 2);
            building[1].position = new Vector2(3 * sparcing, screenHeight - BUILDING_SIZE / 2);
            building[2].position = new Vector2(4 * sparcing, screenHeight - BUILDING_SIZE / 2);
            launcher[1].position = new Vector2(5 * sparcing, screenHeight - LAUNCHER_SIZE / 2);
            building[3].position = new Vector2(6 * sparcing, screenHeight - BUILDING_SIZE / 2);
            building[4].position = new Vector2(7 * sparcing, screenHeight - BUILDING_SIZE / 2);
            building[5].position = new Vector2(8 * sparcing, screenHeight - BUILDING_SIZE / 2);
            launcher[2].position = new Vector2(9 * sparcing, screenHeight - LAUNCHER_SIZE / 2);

            // Buildings and launchers activation
            for (int i = 0; i < LAUNCHERS_AMOUNT; i++) launcher[i].active = true;
            for (int i = 0; i < BUILDINGS_AMOUNT; i++) building[i].active = true;

            // Initialize game variables
            score = 0;
        }

        void UpdateGame()
        {
            if (!gameOver)
            {
                if (IsKeyPressed('P')) pause = !pause;

                if (!pause)
                {
                    framesCounter++;


                    // Interceptors update
                    for (int i = 0; i < MAX_INTERCEPTORS; i++)
                    {
                        if (interceptor[i].active)
                        {
                            // Update position
                            interceptor[i].position.x += interceptor[i].speed.x;
                            interceptor[i].position.y += interceptor[i].speed.y;

                            // Distance to objective
                            distance = Math.Sqrt(Math.Pow(interceptor[i].position.x - interceptor[i].objective.x, 2) +
                                             Math.Pow(interceptor[i].position.y - interceptor[i].objective.y, 2));

                            if (distance < INTERCEPTOR_SPEED)
                            {
                                // Interceptor dissapears
                                interceptor[i].active = false;

                                // Explosion
                                explosion[explosionIndex].position = interceptor[i].position;
                                explosion[explosionIndex].active = true;
                                explosion[explosionIndex].frame = 0;
                                explosionIndex++;
                                if (explosionIndex == MAX_EXPLOSIONS) explosionIndex = 0;

                                break;
                            }
                        }
                    }

                    // Missiles update
                    for (int i = 0; i < MAX_MISSILES; i++)
                    {
                        if (missile[i].active)
                        {
                            // Update position
                            missile[i].position.x += missile[i].speed.x;
                            missile[i].position.y += missile[i].speed.y;

                            // Collision and missile out of bounds
                            if (missile[i].position.y > screenHeight) missile[i].active = false;
                            else
                            {
                                // CHeck collision with launchers
                                for (int j = 0; j < LAUNCHERS_AMOUNT; j++)
                                {
                                    if (launcher[j].active)
                                    {
                                        if (CheckCollisionPointRec(missile[i].position, new Rectangle(
                                            launcher[j].position.x - LAUNCHER_SIZE / 2, launcher[j].position.y - LAUNCHER_SIZE / 2,
                                                                                            LAUNCHER_SIZE, LAUNCHER_SIZE)))
                                        {
                                            // Missile dissapears
                                            missile[i].active = false;

                                            // Explosion and destroy building
                                            launcher[j].active = false;

                                            explosion[explosionIndex].position = missile[i].position;
                                            explosion[explosionIndex].active = true;
                                            explosion[explosionIndex].frame = 0;
                                            explosionIndex++;
                                            if (explosionIndex == MAX_EXPLOSIONS) explosionIndex = 0;

                                            break;
                                        }
                                    }
                                }

                                // CHeck collision with buildings
                                for (int j = 0; j < BUILDINGS_AMOUNT; j++)
                                {
                                    if (building[j].active)
                                    {
                                        if (CheckCollisionPointRec(missile[i].position, new Rectangle(
                                            building[j].position.x - BUILDING_SIZE / 2, building[j].position.y - BUILDING_SIZE / 2,
                                                                                                                BUILDING_SIZE, BUILDING_SIZE)))
                                        {
                                            // Missile dissapears
                                            missile[i].active = false;

                                            // Explosion and destroy building
                                            building[j].active = false;

                                            explosion[explosionIndex].position = missile[i].position;
                                            explosion[explosionIndex].active = true;
                                            explosion[explosionIndex].frame = 0;
                                            explosionIndex++;
                                            if (explosionIndex == MAX_EXPLOSIONS) explosionIndex = 0;

                                            break;
                                        }
                                    }
                                }

                                // CHeck collision with explosions
                                for (int j = 0; j < MAX_EXPLOSIONS; j++)
                                {
                                    if (explosion[j].active)
                                    {
                                        if (CheckCollisionPointCircle(missile[i].position, explosion[j].position, EXPLOSION_RADIUS * explosion[j].radiusMultiplier))
                                        {
                                            // Missile dissapears and we earn 100 points
                                            missile[i].active = false;
                                            score += 100;

                                            explosion[explosionIndex].position = missile[i].position;
                                            explosion[explosionIndex].active = true;
                                            explosion[explosionIndex].frame = 0;
                                            explosionIndex++;
                                            if (explosionIndex == MAX_EXPLOSIONS) explosionIndex = 0;

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //plane update
                    if (plane[0].active)
                    {
                        plane[0].position.x += plane[0].speed.x;
                        plane[0].position.y += plane[0].speed.y;

                        for (int j = 0; j < MAX_EXPLOSIONS; j++)
                        {
                            if (explosion[j].active)
                            {
                                if (CheckCollisionCircleRec(explosion[j].position,
                                                            EXPLOSION_RADIUS * explosion[j].radiusMultiplier,
                                                             new Rectangle(plane[0].position.x + 50,
                                                                           plane[0].position.y + 50,
                                                                           100, 100)))
                                {
                                    // Missile dissapears and we earn 100 points
                                    plane[0].active = false;
                                    score += 100;

                                    explosion[explosionIndex].position = plane[0].position;
                                    explosion[explosionIndex].active = true;
                                    explosion[explosionIndex].frame = 0;
                                    explosionIndex++;
                                    if (explosionIndex == MAX_EXPLOSIONS) explosionIndex = 0;

                                    break;
                                }
                            }
                        }
                    }

                    // Explosions update
                    for (int i = 0; i < MAX_EXPLOSIONS; i++)
                    {
                        if (explosion[i].active)
                        {
                            explosion[i].frame++;

                            if (explosion[i].frame <= EXPLOSION_INCREASE_TIME) explosion[i].radiusMultiplier = explosion[i].frame / (float)EXPLOSION_INCREASE_TIME;
                            else if (explosion[i].frame <= EXPLOSION_TOTAL_TIME) explosion[i].radiusMultiplier = 1 - (explosion[i].frame - (float)EXPLOSION_INCREASE_TIME) / (float)EXPLOSION_TOTAL_TIME;
                            else
                            {
                                explosion[i].frame = 0;
                                explosion[i].active = false;
                            }
                        }
                    }

                    // Fire logic
                    UpdateOutgoingFire();
                    UpdateIncomingFire();
                    UpdatePlane();

                    // Game over logic
                    int checker = 0;

                    for (int i = 0; i < LAUNCHERS_AMOUNT; i++)
                    {
                        if (!launcher[i].active) checker++;
                        if (checker == LAUNCHERS_AMOUNT) gameOver = true;
                    }

                    checker = 0;
                    for (int i = 0; i < BUILDINGS_AMOUNT; i++)
                    {
                        if (!building[i].active) checker++;
                        if (checker == BUILDINGS_AMOUNT) gameOver = true;
                    }
                }
            }
            else
            {
                if (IsKeyPressed(KEY_ENTER))
                {
                    InitGame();
                    gameOver = false;
                }
            }
        }

        void DrawGame()
        {
            BeginDrawing();

            ClearBackground(RAYWHITE);

            if (!gameOver)
            {
                // Draw missiles
                for (int i = 0; i < MAX_MISSILES; i++)
                {
                    if (missile[i].active)
                    {
                        DrawLine((int)missile[i].origin.x, (int)missile[i].origin.y, (int)missile[i].position.x, (int)missile[i].position.y, RED);

                        if (framesCounter % 16 < 8) DrawCircle((int)missile[i].position.x, (int)missile[i].position.y, 3, YELLOW);
                    }
                }

                // Draw interceptors
                for (int i = 0; i < MAX_INTERCEPTORS; i++)
                {
                    if (interceptor[i].active)
                    {
                        DrawLine((int)interceptor[i].origin.x, (int)interceptor[i].origin.y, (int)interceptor[i].position.x, (int)interceptor[i].position.y, GREEN);

                        if (framesCounter % 16 < 8) DrawCircle((int)interceptor[i].position.x, (int)interceptor[i].position.y, 3, BLUE);
                    }
                }

                // Draw explosions
                for (int i = 0; i < MAX_EXPLOSIONS; i++)
                {
                    if (explosion[i].active) DrawCircle((int)explosion[i].position.x, (int)explosion[i].position.y, EXPLOSION_RADIUS * explosion[i].radiusMultiplier, EXPLOSION_COLOR);
                }

                // Draw buildings and launchers
                for (int i = 0; i < LAUNCHERS_AMOUNT; i++)
                {
                    if (launcher[i].active) DrawRectangle((int)launcher[i].position.x - LAUNCHER_SIZE / 2, (int)launcher[i].position.y - LAUNCHER_SIZE / 2, LAUNCHER_SIZE, LAUNCHER_SIZE, GRAY);
                }

                for (int i = 0; i < BUILDINGS_AMOUNT; i++)
                {
                    if (building[i].active) DrawRectangle((int)building[i].position.x - BUILDING_SIZE / 2, (int)building[i].position.y - BUILDING_SIZE / 2, BUILDING_SIZE, BUILDING_SIZE, LIGHTGRAY);
                }

                // Draw score
                DrawText(String.Format("SCORE {0}", score), 20, 20, 40, LIGHTGRAY);

                if (plane[0].active)
                {
                    DrawRectangle((int)plane[0].position.x, (int)plane[0].position.y, 100, 100, RED);

                }

                if (pause) DrawText("GAME PAUSED", screenWidth / 2 - MeasureText("GAME PAUSED", 40) / 2, screenHeight / 2 - 40, 40, GRAY);
            }
            else DrawText("PRESS [ENTER] TO PLAY AGAIN", GetScreenWidth() / 2 - MeasureText("PRESS [ENTER] TO PLAY AGAIN", 20) / 2, GetScreenHeight() / 2 - 50, 20, GRAY);

            EndDrawing();
        }

        // Unload game variables
        void UnloadGame()
        {
            // TODO: Unload all dynamic loaded data (textures, sounds, models...)
        }

        // Update and Draw (one frame)
        void UpdateDrawFrame()
        {
            UpdateGame();
            DrawGame();
        }

        //--------------------------------------------------------------------------------------
        // Additional module functions
        //--------------------------------------------------------------------------------------
        void UpdateOutgoingFire()
        {

            int launcherShooting = 0;

            if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON)) launcherShooting = 1;
            if (IsMouseButtonPressed(MOUSE_MIDDLE_BUTTON)) launcherShooting = 2;
            if (IsMouseButtonPressed(MOUSE_RIGHT_BUTTON)) launcherShooting = 3;

            if (launcherShooting > 0 && launcher[launcherShooting - 1].active)
            {
                float module;
                float sideX;
                float sideY;

                // Activate the interceptor
                interceptor[interceptorNumber].active = true;

                // Assign start position
                interceptor[interceptorNumber].origin = launcher[launcherShooting - 1].position;
                interceptor[interceptorNumber].position = interceptor[interceptorNumber].origin;
                interceptor[interceptorNumber].objective = GetMousePosition();

                // Calculate speed
                module = (float)Math.Sqrt(Math.Pow(interceptor[interceptorNumber].objective.x - interceptor[interceptorNumber].origin.x, 2) +
                               Math.Pow(interceptor[interceptorNumber].objective.y - interceptor[interceptorNumber].origin.y, 2));

                sideX = (interceptor[interceptorNumber].objective.x - interceptor[interceptorNumber].origin.x) * INTERCEPTOR_SPEED / module;
                sideY = (interceptor[interceptorNumber].objective.y - interceptor[interceptorNumber].origin.y) * INTERCEPTOR_SPEED / module;

                interceptor[interceptorNumber].speed = new Vector2(sideX, sideY);

                // Update
                interceptorNumber++;
                if (interceptorNumber == MAX_INTERCEPTORS) interceptorNumber = 0;
            }
        }

        void UpdatePlane()
        {
            if (score % 200 == 0 && plane[0].active == false)
            {
                plane[0].active = true;
                plane[0].origin = new Vector2(0, 0);
                plane[0].position = plane[0].origin;
                plane[0].objective = new Vector2(screenWidth, 0);
                plane[0].speed = new Vector2(5, 0);
            }
            else
            {
                if (plane[0].position.x > screenWidth)
                {
                    plane[0].active = false;
                }
            }
        }

        void UpdateIncomingFire()
        {


            // Launch missile
            if (framesCounter % MISSILE_LAUNCH_FRAMES == 0)
            {
                float module;
                float sideX;
                float sideY;

                // Activate the missile
                missile[missileIndex].active = true;

                // Assign start position
                missile[missileIndex].origin = new Vector2(GetRandomValue(20, screenWidth - 20), -10);
                missile[missileIndex].position = missile[missileIndex].origin;
                missile[missileIndex].objective = new Vector2(GetRandomValue(20, screenWidth - 20), screenHeight + 10);

                // Calculate speed
                module = (float)Math.Sqrt(Math.Pow(missile[missileIndex].objective.x - missile[missileIndex].origin.x, 2) +
                               Math.Pow(missile[missileIndex].objective.y - missile[missileIndex].origin.y, 2));

                sideX = (missile[missileIndex].objective.x - missile[missileIndex].origin.x) * MISSILE_SPEED / module;
                sideY = (missile[missileIndex].objective.y - missile[missileIndex].origin.y) * MISSILE_SPEED / module;

                missile[missileIndex].speed = new Vector2(sideX, sideY);

                // Update
                missileIndex++;
                if (missileIndex == MAX_MISSILES) missileIndex = 0;
            }
        }
    }
}

