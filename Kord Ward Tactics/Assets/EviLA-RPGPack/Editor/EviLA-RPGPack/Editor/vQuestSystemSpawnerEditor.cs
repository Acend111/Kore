using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Spawners;

namespace EviLA.AddOns.RPGPack.Spawners
{
    [CustomEditor(typeof(vQuestSystemSpawner), true)]
    public class vQuestSystemSpawnerEditor : Editor
    {
        protected virtual void OnSceneGUI()
        {

            vQuestSystemSpawner spawner = (vQuestSystemSpawner)target;

            if (!spawner.drawGizmos)
                return;

            foreach (var prefab in spawner.prefabs)
            {

                foreach (var spawnPoint in prefab.spawnPoints)
                {
                    Handles.color = new Color(0f, (prefab.spawnPoints.IndexOf(spawnPoint) + 1) % 10f, 0f, 0.25f);
                    Handles.SphereCap(prefab.spawnPoints.IndexOf(spawnPoint), spawnPoint.position, spawnPoint.rotation, prefab.spawnRadius);
                    Handles.color = new Color(Color.red.r, Color.red.g, Color.red.b);
                    Handles.CubeCap(prefab.spawnPoints.IndexOf(spawnPoint), spawnPoint.position, spawnPoint.rotation, 0.25f);
                }
            }

        }
    }
}