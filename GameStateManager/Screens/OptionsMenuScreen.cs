using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameStateManager
{
    // The options screen is brought up over the top of the main menu screen,
    // and gives the user a chance to configure the game in various (funny) ways.
    public class OptionsMenuScreen : MenuScreen
    {
        private enum Ungulate
        {
            BactrianCamel,
            Dromedary,
            Llama
        }

        private MenuEntry ungulateEntry;
        private MenuEntry languageEntry;
        private MenuEntry frobnicateEntry;
        private MenuEntry elfEntry;
        public MenuEntry backEntry;
        private Ungulate currentUngulate;
        private List<string> languages;
        private int currentLanguage;
        private bool frobnicate;
        private int elf;


        // Constructs an options menu screen.
        public OptionsMenuScreen(string screenName, string menuTitle)
            : base(menuTitle)
        {
            Name = screenName;
            languages = new List<string> { "C#", "French", "English" };

            // Create our menu entries.
            ungulateEntry = new MenuEntry("Preferred ungulate: " + currentUngulate.ToString());
            languageEntry = new MenuEntry("Language: " + languages[currentLanguage]);
            frobnicateEntry = new MenuEntry("Frobnicate: " + frobnicate.ToString());
            elfEntry = new MenuEntry("Elf: " + elf.ToString());
            backEntry = new MenuEntry("Back");

            // Add the menu event handlers.
            ungulateEntry.Selected += UngulateEntry_OnSelected;
            languageEntry.Selected += LanguageEntry_OnSelected;
            frobnicateEntry.Selected += FrobnicateEntry_OnSelected;
            elfEntry.Selected += ElfEntry_OnSelected;
            backEntry.Selected += BackEntry_OnSelected;

            // Add entries to the menu.
            Entries.Add(ungulateEntry);
            Entries.Add(languageEntry);
            Entries.Add(frobnicateEntry);
            Entries.Add(elfEntry);
            Entries.Add(backEntry);
        }


        // Event handler for when the "Ungulate" entry is selected.
        void UngulateEntry_OnSelected(User user)
        {
            currentUngulate++;

            if (currentUngulate > Ungulate.Llama)
                currentUngulate = 0;

            ungulateEntry.Text = "Preferred ungulate: " + currentUngulate.ToString();
        }


        // Event handler for when the "Language" entry is selected.
        private void LanguageEntry_OnSelected(User user)
        {
            currentLanguage++;

            if (currentLanguage > languages.Count - 1)
                currentLanguage = 0;

            languageEntry.Text = "Language: " + languages[currentLanguage];
        }


        // Event handler for when the "Frobnicate" entry is selected.
        private void FrobnicateEntry_OnSelected(User user)
        {
            frobnicate = !frobnicate;
            frobnicateEntry.Text = "Frobnicate: " + frobnicate.ToString();
        }


        // Event handler for when the "Elf" entry is selected.
        private void ElfEntry_OnSelected(User user)
        {
            elf++;
            elfEntry.Text = "Elf: " + elf.ToString();
        }


        // Event handler for when the "Back" entry is selected or the Options Menu is dismissed.
        private void BackEntry_OnSelected(User user)
        {
            OnHide();
        }
    }
}