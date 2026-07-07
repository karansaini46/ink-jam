using System;
using UnityEngine;

namespace InkJam.Art
{
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        public ThemeData currentTheme;

        public event Action OnThemeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Try to load a default theme if none assigned
            if (currentTheme == null)
            {
                currentTheme = Resources.Load<ThemeData>("DefaultTheme");
            }
        }

        public void SetTheme(ThemeData newTheme)
        {
            if (newTheme == null) return;
            
            currentTheme = newTheme;
            currentTheme.Initialize();
            
            OnThemeChanged?.Invoke();
            Debug.Log($"[ThemeManager] Theme changed to {newTheme.name}");
        }
    }
}
