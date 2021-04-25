using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMPclient
{
    public class PosSender
    {
        private static Vector3 oldPos;
        private static float oldRot;

        public static void Update()
        {
            if(SceneManager.GetActiveScene().buildIndex >= 2)
            {
                if (Vector3.Distance(oldPos, PlayerMovement.Instance.transform.position) > 0.3f || Mathf.Abs(oldRot - Camera.main.transform.rotation.eulerAngles.y) >= 10f)
                {
                    ClientSend.ClientMove(PlayerMovement.Instance.transform.position, Camera.main.transform.eulerAngles.y);
                    oldPos = PlayerMovement.Instance.transform.position;
                    oldRot = PlayerMovement.Instance.transform.rotation.eulerAngles.y;
                }
            }
        }
    }
}