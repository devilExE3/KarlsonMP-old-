using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace KarlsonMP
{
    class PrefabInstancer
    {
        public static bool Initialized { get; private set; } = false;

        public static void LoadPrefabs()
        {
            if (Initialized)
                return;
            foreach (GameObject go in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (go.name == "Enemy")
                {
                    Animator animator = go.GetComponentInChildren<Animator>();
                    animator.SetBool("Running", false);
                    animator.SetBool("Aiming", false);
                    go.GetComponent<NavMeshAgent>().enabled = false;
                    go.transform.localScale = new Vector3(0.27f, 0.27f, 0.27f);
                    foreach (Collider coll in go.GetComponentsInChildren<BoxCollider>())
                        coll.enabled = false;
                    foreach (Collider coll in go.GetComponentsInChildren<CapsuleCollider>())
                        coll.enabled = false;
                    foreach (Collider coll in go.GetComponentsInChildren<Collider>())
                        coll.enabled = false;
                    enemy = UnityEngine.Object.Instantiate(go);
                    enemy.name = "KarlsonMP-Instance Enemy";
                    UnityEngine.Object.DontDestroyOnLoad(enemy);
                    enemy.SetActive(false);
                    enemy.transform.position = new Vector3(-10000f, -10000f, -10000f);
                    continue;
                }
                if (go.name == "Pistol")
                {
                    pistol = UnityEngine.Object.Instantiate(go);
                    pistol.name = "KarlsonMP-Instance Pistol";
                    UnityEngine.Object.DontDestroyOnLoad(pistol);
                    pistol.SetActive(false);
                    continue;
                }
                if (go.name == "Boomer")
                {
                    boomer = UnityEngine.Object.Instantiate(go);
                    boomer.name = "KarlsonMP-Instance Boomer";
                    UnityEngine.Object.DontDestroyOnLoad(boomer);
                    boomer.SetActive(false);
                    continue;
                }
                if (go.name == "Ak47")
                {
                    ak47 = UnityEngine.Object.Instantiate(go);
                    ak47.name = "KarlsonMP-Instance Ak47";
                    UnityEngine.Object.DontDestroyOnLoad(ak47);
                    ak47.SetActive(false);
                    continue;
                }
                if (go.name == "Shotgun")
                {
                    shotgun = UnityEngine.Object.Instantiate(go);
                    shotgun.name = "KarlsonMP-Instance Pistol";
                    UnityEngine.Object.DontDestroyOnLoad(shotgun);
                    shotgun.SetActive(false);
                    continue;
                }
                if (go.name == "grappler")
                {
                    grappler = UnityEngine.Object.Instantiate(go);
                    grappler.name = "KarlsonMP-Instance Pistol";
                    UnityEngine.Object.DontDestroyOnLoad(grappler);
                    grappler.SetActive(false);
                    continue;
                }
            }
            SceneManager.LoadScene("MainMenu");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Initialized = true;
        }

        private static GameObject enemy;
        private static GameObject pistol;
        private static GameObject boomer;
        private static GameObject ak47;
        private static GameObject shotgun;
        private static GameObject grappler;

        public static GameObject CreateEnemy()
        {
            GameObject go = UnityEngine.Object.Instantiate(enemy);
            go.name = "Player";
            go.SetActive(true);
            return go;
        }

        public static GameObject CreatePistol()
        {
            GameObject go = UnityEngine.Object.Instantiate(pistol);
            go.name = "Pistol";
            go.SetActive(true);
            return go;
        }
        public static GameObject CreateBoomer()
        {
            GameObject go = UnityEngine.Object.Instantiate(boomer);
            go.name = "Boomer";
            go.SetActive(true);
            return go;
        }
        public static GameObject CreateAk47()
        {
            GameObject go = UnityEngine.Object.Instantiate(ak47);
            go.name = "Ak47";
            go.SetActive(true);
            return go;
        }
        public static GameObject CreateShotgun()
        {
            GameObject go = UnityEngine.Object.Instantiate(shotgun);
            go.name = "Shotgun";
            go.SetActive(true);
            return go;
        }
        public static GameObject CreateGrappler()
        {
            GameObject go = UnityEngine.Object.Instantiate(grappler);
            go.name = "Grappler";
            go.SetActive(true);
            return go;
        }
    }
}
