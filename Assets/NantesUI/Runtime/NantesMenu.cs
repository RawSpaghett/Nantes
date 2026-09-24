using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace NantesGame.UI
{
    [DefaultExecutionOrder(-100)]
    [MovedFrom("Nantes.UI")]
    public sealed class NantesMenu : MonoBehaviour
    {
        public GameObject mainPage, settingsPage, extrasPage, quitPage;
        public GameObject controlsPage;
        public Button controlsButton, controlsBack;
        public Button newGameButton, continueButton, settingsButton, extrasButton, quitButton;
        public Button settingsBack, extrasBack, quitBack, quitConfirm;
        public Slider volumeSlider;
        public Toggle fullscreenToggle, reducedMotionToggle;
        public TMP_Text volumeValue, continueHint;
        public UnityEvent newGameRequested = new UnityEvent();
        public UnityEvent continueRequested = new UnityEvent();
        public UnityEvent quitRequested = new UnityEvent();
        public bool applyPreferences = true;
        public NantesTheme theme;
        public NantesTendrilSelector selector;
        public bool ReducedMotion { get; private set; }
        public bool KeyboardNavigation { get; private set; }
        public string CurrentPage { get; private set; } = "main";
        GameObject firstControl;
        Button returnControl;
        bool initialized;
        bool changingPage;
        const string VolumeKey = "Nantes.UI.MasterVolume";
        const string MotionKey = "Nantes.UI.ReducedMotion";

        void Awake()
        {
            if(selector!=null)selector.Initialize(this);
            newGameButton.onClick.AddListener(() => Activate(newGameButton,() => newGameRequested.Invoke()));
            continueButton.onClick.AddListener(() => Activate(continueButton,() => continueRequested.Invoke()));
            settingsButton.onClick.AddListener(() => Activate(settingsButton,() => Show("settings")));
            extrasButton.onClick.AddListener(() => Activate(extrasButton,() => Show("extras")));
            quitButton.onClick.AddListener(() => Activate(quitButton,() => Show("quit")));
            if(controlsButton)controlsButton.onClick.AddListener(() => Activate(controlsButton,() => Show("controls")));
            if(controlsBack)controlsBack.onClick.AddListener(Back);
            settingsBack.onClick.AddListener(() => Activate(settingsBack,Back));
            extrasBack.onClick.AddListener(() => Activate(extrasBack,Back));
            quitBack.onClick.AddListener(() => Activate(quitBack,Back));
            quitConfirm.onClick.AddListener(() => Activate(quitConfirm,() => quitRequested.Invoke()));
            volumeSlider.onValueChanged.AddListener(SetVolume);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            reducedMotionToggle.onValueChanged.AddListener(SetReducedMotion);
            initialized = true;
        }

        void Start()
        {
            float volume = applyPreferences ? PlayerPrefs.GetFloat(VolumeKey, 1) : 1;
            ReducedMotion = applyPreferences && PlayerPrefs.GetInt(MotionKey, 0) == 1;
            volumeSlider.SetValueWithoutNotify(volume);
            reducedMotionToggle.SetIsOnWithoutNotify(ReducedMotion);
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            volumeValue.text = Mathf.RoundToInt(volume * 100) + "%";
            if (applyPreferences) AudioListener.volume = volume;
            SetContinueAvailable(continueButton.interactable);
            Show("main");
        }

        void Update()
        {
            if (changingPage) return;
            Keyboard k = Keyboard.current;
            Gamepad g = Gamepad.current;
            if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > .01f) UsePointer();
            bool navigation = k != null && (k.upArrowKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame || k.tabKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
            navigation |= g != null && (g.dpad.ReadValue().sqrMagnitude > .1f || g.leftStick.ReadValue().sqrMagnitude > .3f || g.buttonSouth.wasPressedThisFrame);
            if (navigation)
            {
                KeyboardNavigation = true;
                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null) Focus(firstControl);
            }
            if (k != null && k.escapeKey.wasPressedThisFrame || g != null && g.buttonEast.wasPressedThisFrame)
            {
                if (CurrentPage == "main") Show("quit");
                else Back();
            }
        }

        public void UsePointer()
        {
            if (!KeyboardNavigation) return;
            KeyboardNavigation = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        public void SetContinueAvailable(bool available)
        {
            continueButton.interactable = available;
            continueHint.gameObject.SetActive(!available);
            if (!available && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == continueButton.gameObject) Focus(newGameButton.gameObject);
        }

        public void Show(string page)
        {
            if (!initialized || changingPage) return;
            if (page != "main" && page != "settings" && page != "extras" && page != "quit" && page != "controls") return;
            if (page == "controls" && !controlsPage) return;
            if(selector!=null)selector.PageChanged();
            if (page == "settings") { returnControl = settingsButton; fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen); }
            if (page == "extras") returnControl = extrasButton;
            if (page == "quit") returnControl = quitButton;
            if (page == "controls") returnControl = controlsButton;
            string previous = CurrentPage;
            CurrentPage = page;
            firstControl = page == "controls" ? controlsBack.gameObject : page == "settings" ? volumeSlider.gameObject : page == "extras" ? extrasBack.gameObject : page == "quit" ? quitBack.gameObject : newGameButton.gameObject;
            if (previous == "controls" && page == "main") firstControl = controlsButton.gameObject;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            if (!ReducedMotion && previous != page && (previous == "controls" || page == "controls"))
            {
                StartCoroutine(ChangeControlsPage(page));
                return;
            }
            SetPage(page);
            if (KeyboardNavigation) Focus(firstControl);
        }

        void SetPage(string page)
        {
            mainPage.SetActive(page == "main");
            settingsPage.SetActive(page == "settings");
            extrasPage.SetActive(page == "extras");
            quitPage.SetActive(page == "quit");
            if(controlsPage)controlsPage.SetActive(page == "controls");
        }

        IEnumerator ChangeControlsPage(string page)
        {
            changingPage = true;
            bool opening = page == "controls";
            var list = mainPage.GetComponent<CanvasGroup>() ?? mainPage.AddComponent<CanvasGroup>();
            var sheet = controlsPage.GetComponent<CanvasGroup>() ?? controlsPage.AddComponent<CanvasGroup>();
            var content = (RectTransform)controlsPage.transform.Find("Content");
            var listRect = (RectTransform)mainPage.transform;
            Vector2 listPosition = listRect.anchoredPosition;
            list.interactable = sheet.interactable = false;
            list.blocksRaycasts = sheet.blocksRaycasts = false;
            mainPage.SetActive(true); controlsPage.SetActive(true);
            settingsPage.SetActive(false); extrasPage.SetActive(false); quitPage.SetActive(false);
            float elapsed = 0;
            while (elapsed < .42f)
            {
                float t = Mathf.SmoothStep(0, 1, elapsed / .42f);
                float reveal = opening ? t : 1 - t;
                sheet.alpha = reveal;
                list.alpha = 1 - reveal;
                content.anchoredPosition = new Vector2(28 * (1 - reveal), 0);
                listRect.anchoredPosition = listPosition - Vector2.right * (18 * reveal);
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            listRect.anchoredPosition = listPosition;
            content.anchoredPosition = Vector2.zero;
            list.alpha = sheet.alpha = 1;
            list.interactable = sheet.interactable = true;
            list.blocksRaycasts = sheet.blocksRaycasts = true;
            SetPage(page);
            changingPage = false;
            if (KeyboardNavigation) Focus(firstControl);
        }

        public void Back()
        {
            if (changingPage) return;
            SavePreferences();
            Show("main");
            if (!changingPage && KeyboardNavigation && returnControl != null) Focus(returnControl.gameObject);
        }

        void Focus(GameObject control)
        {
            if (EventSystem.current != null && control != null && control.activeInHierarchy) EventSystem.current.SetSelectedGameObject(control);
        }

        void Activate(Button button,UnityAction action)
        {
            if(changingPage||!button.isActiveAndEnabled||!button.IsInteractable())return;
            if(selector!=null&&selector.isActiveAndEnabled)selector.Strike((RectTransform)button.transform,action);
            else action.Invoke();
        }

        void SetVolume(float value)
        {
            volumeValue.text = Mathf.RoundToInt(value * 100) + "%";
            if (!applyPreferences) return;
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumeKey, value);
        }

        void SetFullscreen(bool value)
        {
            if (applyPreferences) Screen.fullScreen = value;
        }

        void SetReducedMotion(bool value)
        {
            ReducedMotion = value;
            if (applyPreferences) PlayerPrefs.SetInt(MotionKey, value ? 1 : 0);
        }

        void SavePreferences() { if (applyPreferences) PlayerPrefs.Save(); }
        void OnApplicationPause(bool paused) { if (paused) SavePreferences(); }
        void OnApplicationQuit() { SavePreferences(); }
    }
}
