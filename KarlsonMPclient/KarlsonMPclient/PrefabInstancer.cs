using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

namespace KarlsonMPclient
{
    class PrefabInstancer
    {
        public static bool initialized { get; private set; } = false;

        public static void LoadPrefabs()
        {
            foreach(GameObject go in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if(go.name == "Enemy")
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
                }
            }
            enemy.name = "KarlsonMP-Instance Enemy";
            UnityEngine.Object.DontDestroyOnLoad(enemy);
            enemy.SetActive(false);
            SceneManager.LoadScene("MainMenu");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            initialized = true;
        }

        private static GameObject enemy;

        public static GameObject CreateEnemy()
        {
            GameObject go = UnityEngine.Object.Instantiate(enemy);
            go.name = "Player";
            go.SetActive(true);
            return go;
        }
    }
}
