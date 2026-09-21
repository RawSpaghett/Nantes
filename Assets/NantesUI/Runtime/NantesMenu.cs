using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace Nantes.UI
{
    [DefaultExecutionOrder(-100)]
    public sealed class NantesMenu : MonoBehaviour
    {
        public GameObject mainPage, settingsPage, extrasPage, quitPage;
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
            if (!initialized) return;
            if (page != "main" && page != "settings" && page != "extras" && page != "quit") return;
            if(selector!=null)selector.PageChanged();
            if (page == "settings") { returnControl = settingsButton; fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen); }
            if (page == "extras") returnControl = extrasButton;
            if (page == "quit") returnControl = quitButton;
            CurrentPage = page;
            mainPage.SetActive(page == "main");
            settingsPage.SetActive(page == "settings");
            extrasPage.SetActive(page == "extras");
            quitPage.SetActive(page == "quit");
            firstControl = page == "settings" ? volumeSlider.gameObject : page == "extras" ? extrasBack.gameObject : page == "quit" ? quitBack.gameObject : newGameButton.gameObject;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            if (KeyboardNavigation) Focus(firstControl);
        }

        public void Back()
        {
            SavePreferences();
            Show("main");
            if (KeyboardNavigation && returnControl != null) Focus(returnControl.gameObject);
        }

        void Focus(GameObject control)
        {
            if (EventSystem.current != null && control != null && control.activeInHierarchy) EventSystem.current.SetSelectedGameObject(control);
        }

        void Activate(Button button,UnityAction action)
        {
            if(!button.isActiveAndEnabled||!button.IsInteractable())return;
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
