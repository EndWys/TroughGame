using UnityEngine;

namespace ProjectCore.Features.Proyotype
{
    public class CursorController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _lockOnStart = true;

        private void Start()
        {
            if (_lockOnStart)
            {
                SetGameplayMode();
            }
        }
        
        public void SetGameplayMode()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void SetUIMode()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
               if(Cursor.visible)
               {
                   SetGameplayMode();
               }
               else
               {
                   SetUIMode();
               }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && Cursor.lockState == CursorLockMode.Locked)
            {
                SetGameplayMode();
            }
        }
    }
}