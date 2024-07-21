//using UnityEngine.U2D;

using UnityEngine;

namespace HeartUnity.View
{
    public class ReusableSettingInput
    {
        public ReusableSettingMenu menu;
        public int selectedIndex = 0;
        public void ManualUpdate()
        {
            if (menu.unitUIs.Count == 0) return;
            if (menu.engineView.inputManager.ConsumeButtonDown(DefaultButtons.CANCEL))
            {
                AudioPlayer.PlaySFX("click");
                menu.RequestReturn();
            }
            for (int i = 0; i < menu.unitUIs.Count; i++)
            {
                ReusableSettingMenu.SettingUnitUI item = menu.unitUIs[i];
                var selected = i == selectedIndex && menu.engineView.inputManager.LatestInputDevice != InputManager.InputDevice.MOUSE;
                if (item.slider != null)
                {
                    item.slider.selectedImage.SetActive(selected);
                }
                if (item.toggle != null)
                {
                    item.toggle.selectedIndicator.SetActive(selected);
                }
                if (item.button != null)
                {
                    item.button.selectedIndicator.SetActive(selected);
                }

            }
            if (menu.engineView.inputManager.IsButtonDown(DefaultButtons.DOWN))
            {
                selectedIndex++;
            }
            if (menu.engineView.inputManager.IsButtonDown(DefaultButtons.UP))
            {
                selectedIndex--;
            }
            selectedIndex = Mathf.Clamp(selectedIndex, 0, menu.unitUIs.Count - 1);
            ReusableSettingMenu.SettingUnitUI selectedSU = menu.unitUIs[selectedIndex];
            if (selectedSU.slider != null)
            {
                var move = 0f;
                if (menu.engineView.inputManager.IsButtonDown(DefaultButtons.LEFT))
                {
                    move = -0.1f;
                }
                if (menu.engineView.inputManager.IsButtonDown(DefaultButtons.RIGHT))
                {
                    move = 0.1f;
                }
                selectedSU.slider.value = Mathf.Clamp(selectedSU.slider.value + move, 0, 1f);
            }

            CheckScrollSnap(selectedSU.button);
            CheckScrollSnap(selectedSU.slider);
            CheckScrollSnap(selectedSU.toggle);

            if (menu.engineView.inputManager.IsButtonDown(DefaultButtons.CONFIRM))
            {

                if (selectedSU.slider != null)
                {

                }
                if (selectedSU.toggle != null)
                {
                    selectedSU.toggle.IsOn = !selectedSU.toggle.IsOn;
                    menu.ToogleUpdated(selectedSU.toggle.IsOn, selectedSU.settingRT);
                }
                if (selectedSU.button != null)
                {
                    if(selectedSU.settingRT != null)
                        menu.ButtonPressed(selectedSU.settingRT);
                    if (selectedSU.language != null)
                        menu.LanguageButtonPressed(selectedSU.language);
                    if (selectedSU.dialogConfirm.HasValue) {
                        menu.PressDialogButton(selectedSU.dialogConfirm.Value);
                    }
                    if (selectedSU.leaveLanguage) {
                        menu.LeaveLanguageButton();
                    }
                }
            }

            void CheckScrollSnap(MonoBehaviour obj)
            {
                if (obj == null) return;
                if (menu.engineView.inputManager.LatestInputDevice == InputManager.InputDevice.MOUSE) return;
                var rt = obj.gameObject.GetComponent<RectTransform>();
                var maxDis = EngineView.GetDistanceFromScreenCorner(rt);
                if(maxDis != 0)
                    menu.scroll.SnapToY(rt, maxDis + 8000);

            }
        }

    }
}