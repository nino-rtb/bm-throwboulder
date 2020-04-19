using System;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ThrowBoulder
{
    public class ThrowBoulderSubModule : MBSubModuleBase
    {
        private bool debugMode = false;
        private InputKey triggerKey = (InputKey) 47; // V
        protected override void OnSubModuleLoad()
        {
            logDebugMessage("ThrowBoulder module loaded");
        }

        private void logDebugMessage(string msg)
        {
            if (debugMode)
            {
                InformationManager.DisplayMessage(new InformationMessage(msg));
            }
        }

        private bool isInMission()
        {
            bool isInMission = false;
            bool gameIsValid = Game.Current != null;
            if (gameIsValid)
            {
                bool gameStateIsInvalid = Game.Current.CurrentState > 0;
                if (!gameStateIsInvalid)
                {
                    bool missionIsInvalid = Mission.Current == null || Mission.Current.Scene == null;
                    if (!missionIsInvalid)
                    {
                        isInMission = true;
                    }
                }
            }
            return isInMission;
        }

        private bool playerIsInMissionAndAlive()
        {
            bool isAlive = false;
            if (isInMission())
            {
                bool playerIsValid = Agent.Main != null;
                if (playerIsValid)
                {
                    isAlive = true;
                }
            }
            return isAlive;
        }

        private bool isTriggerKeyPressed()
        {
            bool keyIsPressed = false;
            if (Input.IsKeyPressed(triggerKey))
            {
                keyIsPressed = true;
            }
            return keyIsPressed;
        }

        private Vec3 getBallisticErrorAppliedDirection(Vec3 lookDirection)
        {
            Mat3 mat = new Mat3
            {
                f = lookDirection,
                u = Vec3.Up
            };
            mat.Orthonormalize();
            float a = MBRandom.RandomFloat * 6.28318548f;
            mat.RotateAboutForward(a);
            float f = 1f * MBRandom.RandomFloat;
            mat.RotateAboutSide(f.ToRadians());
            return mat.f;
        }

        private void throwBoulderAction()
        {
            Mission mission = Mission.Current;
            Agent player = Agent.Main;
            Mat3 identity = Mat3.Identity;
            identity.f = getBallisticErrorAppliedDirection(player.LookDirection);
            identity.Orthonormalize();
            ItemObject missileItem = Game.Current.ObjectManager.GetObject<ItemObject>("boulder");
            Vec3 originOfMissile = new Vec3(player.Position.x, player.Position.y, player.Position.z + 2, player.Position.w); // + on Z otherwise it's too close to the ground
            mission.AddCustomMissile(
                player,
                new MissionWeapon(
                    missileItem,
                    null,
                    1
                ),
                originOfMissile, // origin of projectile
                player.LookDirection, // direction projectile is going
                identity, // orientation of projectile? (doesn't matter for boulder but might for pointed missiles)
                50,
                50,
                false,
                null,
                -1
            );
        }

        protected override void OnApplicationTick(float dt)
        {
            if (playerIsInMissionAndAlive())
            {
                if (isTriggerKeyPressed())
                {
                    throwBoulderAction();
                }
            }
        }
    }
}
