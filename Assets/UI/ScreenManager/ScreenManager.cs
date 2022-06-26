using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nash1m.UI.ScreenManager
{
    public class ScreenManager : MonoBehaviour
    {
        public List<Screen> screens;
        public Screen firstScreen;
        
        private Screen _currentScreen;
        
        private void Start()
        {
            if(firstScreen)
                Show(firstScreen);
        }

        

        public void Show(Screen screen)
        {
            screen.SetActive(true);
            _currentScreen = screen;
        }
        public void Show<T>() where T: Screen
        {
            _currentScreen = GetScreen<T>();
            _currentScreen.SetActive(true);
        }
        public void Hide(Screen screen)
        {
            screen.SetActive(false);
        }
        public void Hide<T>() where T: Screen
        {
            GetScreen<T>().SetActive(false);
        }
        
        public void ChangeScreen(Screen screen)
        {
            Hide(_currentScreen);

            if (_currentScreen is { })
                _currentScreen.animator.onAnimationEnd += OnAnimationEnd;
            else
                Show(screen);
         

            void OnAnimationEnd()
            {
                Show(screen);
                _currentScreen.animator.onAnimationEnd -= OnAnimationEnd;
            }
        }
        
        public T GetScreen<T>() where T : Screen
        {
            var screensCount = screens.Count;
            for (var i = 0; i < screensCount; i++)
            {
                if(screens[i] is T) return screens[i] as T;
            }
            return null;
        }
    }
}
