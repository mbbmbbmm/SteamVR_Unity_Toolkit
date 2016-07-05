namespace VRTK
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using System.Collections.Generic;

    public class VRTK_UIPointer : MonoBehaviour
    {
        public VRTK_ControllerEvents controller;

        [HideInInspector]
        public PointerEventData pointerEventData;

        private void Start()
        {
            ConfigureEventSystem();
            ConfigureWorldCanvas();
            if (controller == null)
            {
                controller = this.GetComponent<VRTK_ControllerEvents>();
            }
        }

        private void ConfigureEventSystem()
        {
            var eventSystem = GameObject.FindObjectOfType<EventSystem>();

            if (!eventSystem)
            {
                Debug.LogError("A VRTK_UIPointer requires an EventSystem");
            }

            //delete existing standalone input module
            var standaloneInputModule = eventSystem.gameObject.GetComponent<StandaloneInputModule>();
            if (standaloneInputModule)
            {
                Destroy(standaloneInputModule);
            }

            //if it doesn't already exist, add the custom event system
            var eventSystemInput = eventSystem.GetComponent<VRTK_EventSystemVRInput>();
            if (!eventSystemInput)
            {
                eventSystemInput = eventSystem.gameObject.AddComponent<VRTK_EventSystemVRInput>();
                eventSystemInput.Initialise();
            }

            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.pointerId = (int)this.GetComponent<SteamVR_TrackedObject>().index + 1000;
            eventSystemInput.pointers.Add(this);
        }

        private void ConfigureWorldCanvas()
        {
            foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
            {
                if (canvas.renderMode != RenderMode.WorldSpace)
                {
                    continue;
                }

                //copy public params then delete existing graphic raycaster
                var defaultRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
                var customRaycaster = canvas.gameObject.GetComponent<VRTK_UIGraphicRaycaster>();
                //if it doesn't already exist, add the custom raycaster
                if (!customRaycaster)
                {
                    customRaycaster = canvas.gameObject.AddComponent<VRTK_UIGraphicRaycaster>();
                }

                if (defaultRaycaster)
                {
                    customRaycaster.ignoreReversedGraphics = defaultRaycaster.ignoreReversedGraphics;
                    customRaycaster.blockingObjects = defaultRaycaster.blockingObjects;
                    Destroy(defaultRaycaster);
                }

                //add a box collider and background image to ensure the rays always hit
                var canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
                if (!canvas.gameObject.GetComponent<BoxCollider>())
                {
                    var canvasBoxCollider = canvas.gameObject.AddComponent<BoxCollider>();
                    canvasBoxCollider.size = new Vector3(canvasSize.x, canvasSize.y, 10f);
                    canvasBoxCollider.center = new Vector3(0f, 0f, 5f);
                }

                if (!canvas.gameObject.GetComponent<Image>())
                {
                    canvas.gameObject.AddComponent<Image>().color = Color.clear;
                }
            }
        }
    }
}