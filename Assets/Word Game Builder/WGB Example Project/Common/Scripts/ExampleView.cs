// ExampleView.cs
// Copyright (c) 2011-2016 Thinksquirrel Inc.
#pragma warning disable 0649
using Thinksquirrel.WordGameBuilder;
using Thinksquirrel.WordGameBuilder.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace Thinksquirrel.WordGameBuilderExample
{
    /// <summary>
    /// This class is responsible for rendering the UI.
    /// </summary>
    [AddComponentMenu("WGB Example Project/UI/Example View")]
    public class ExampleView : MonoBehaviour
    {
        public ExampleViewModel m_ViewModel;
        public Text m_Text;
        public Selectable m_Button;
        public Text m_ButtonText;
        public GameObject m_WildcardTilePanel;
        public Transform m_WildcardTileRoot;
        public GameObject m_WildcardTilePrefab;

        bool m_ShowDebugInfo;

        void Update()
        {
            if (!m_ViewModel || !m_Text || !m_Button || !m_ButtonText) return;

            if (m_ViewModel.agent.automaticMode)
            {
                m_Text.text = m_ViewModel.agent.agentStatus;
                m_Button.interactable = false;
                m_ButtonText.color = Color.white * 0.5f;
                return;
            }
            var input = m_ViewModel.player.orderedResult.input;
            var isValid = m_ViewModel.player.orderedResult.isValid;
            if (string.IsNullOrEmpty(input))
            {
                input = "Click on a tile to select letters";
            }
            else
            {
                var points = isValid ? string.Format(" - {0} points", m_ViewModel.player.orderedResult.score) : string.Empty;
                input = string.Format("<color=#{0}FF>{1}</color>{2}", isValid ? "00FF00" : "FF0000", input, points);
            }
            m_Text.supportRichText = true;
            m_Text.text = input;
            m_Button.interactable = isValid;
            m_ButtonText.color = isValid ? Color.white : Color.white * 0.5f;

            if (m_ViewModel.showWildcardPanel)
            {
                var letterLength = m_ViewModel.languages.currentLetters.Length;
                if (m_ViewModel.languages.currentLetters != null && letterLength > 0)
                {
                    if (!m_WildcardTilePanel.activeSelf)
                    {
                        m_WildcardTilePanel.SetActive(true);

                        if (m_WildcardTileRoot.childCount > 0)
                        {
                            for (var i = 0; i < Mathf.Min(m_WildcardTileRoot.childCount, letterLength); ++i)
                            {
                                var tileGo = m_WildcardTileRoot.GetChild(i).gameObject;
                                var tile = tileGo.GetComponentFromInterface<ISelectableLetterTile>();
                                tile.ChangeDefaultLetter(WordGameLanguage.current.letters[i]);
                                tile.gameObject.SetActive(true);
                                tile.SpawnTile();
                            }
                        }
                        for (var i = m_WildcardTileRoot.childCount; i < letterLength; ++i)
                        {
                            // ReSharper disable once RedundantCast
                            var tileGo = Instantiate(m_WildcardTilePrefab) as GameObject;
                            tileGo.transform.SetParent(m_WildcardTileRoot);
                            var tile = tileGo.GetComponentFromInterface<ISelectableLetterTile>();
                            tile.ChangeDefaultLetter(WordGameLanguage.current.letters[i]);
                            tile.SpawnTile();
                            var button = tileGo.GetComponent<Button>();
                            var letterIndex = i;
                            button.onClick.AddListener(() => OnWildcardPanelTileSelect(letterIndex));
                        }

                        // NOTE: Workaround for a Unity UI issue
                        var scrollView = m_WildcardTilePanel.GetComponentInChildren<ScrollRect>();
                        var grid = m_WildcardTilePanel.GetComponentInChildren<GridLayoutGroup>();

                        if (scrollView && grid)
                        {
                            scrollView.enabled = false;
                            grid.enabled = false;
                            grid.enabled = true;
                            scrollView.enabled = true;
                        }
                    }
                    else if (m_ViewModel.wildcardPanelSelection >= 0)
                    {
                        m_ViewModel.game.SelectWildcardLetter(m_ViewModel.languages.currentLetters[m_ViewModel.wildcardPanelSelection]);
                        m_ViewModel.wildcardPanelSelection = -1;
                    }
                }
                else
                {
                    if (m_WildcardTilePanel.activeSelf)
                    {
                        foreach (var tile in m_WildcardTileRoot.GetComponentsInChildrenFromInterface<ISelectableLetterTile>())
                        {
                            tile.gameObject.SetActive(false);
                            tile.DespawnTile();
                        }
                        m_WildcardTilePanel.SetActive(false);
                    }
                    m_ViewModel.showWildcardPanel = false;
                }
            }
            else if (m_WildcardTilePanel.activeSelf)
            {
                foreach (var tile in m_WildcardTileRoot.GetComponentsInChildrenFromInterface<ISelectableLetterTile>())
                {
                    tile.gameObject.SetActive(false);
                    tile.DespawnTile();
                }
                m_WildcardTilePanel.SetActive(false);
            }
        }

        /// <summary>
        /// UI callback for "Submit Word" button.
        /// </summary>
        public void OnWordSubmission()
        {
            if (!m_ViewModel.agent.automaticMode && m_ViewModel.player.orderedResult.isValid)
            {
                m_ViewModel.player.SubmitWord();
            }
        }

        /// <summary>
        /// UI callback for "Reset" button.
        /// </summary>
        public void OnReset()
        {
            m_ViewModel.game.ResetTiles();
        }

        /// <summary>
        /// UI callback for "Enable AI" toggle.
        /// </summary>
        public void OnAutomaticToggle()
        {
            m_ViewModel.agent.ToggleAutomaticMode();
        }
        
        /// <summary>
        /// UI callback for when a tile is selected from the "Select a Letter" panel.
        /// </summary>
        /// <param name="letterIndex">The index of the selected letter.</param>
        public void OnWildcardPanelTileSelect(int letterIndex)
        {
            m_ViewModel.wildcardPanelSelection = letterIndex;
        }

        void OnGUI()
        {
            if (!m_ViewModel.game.isInitialized)
                return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginVertical();

            m_ShowDebugInfo = GUILayout.Toggle(m_ShowDebugInfo, "Show Debug Info");

            if (m_ShowDebugInfo)
            {
                GUILayout.Label(string.Format("Language: {0}", m_ViewModel.languages.currentLanguage));

                GUI.enabled = m_ViewModel.languages.currentLanguageIsLoaded;
                GUILayout.BeginHorizontal();
                for (int i = 0; i < m_ViewModel.languages.languageNames.Length; i++)
                {
                    var language = m_ViewModel.languages.languageNames[i];
                    if (GUILayout.Button(language))
                    {
                        m_ViewModel.languages.ChangeLanguage(language);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUI.enabled = true;

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Score: {0}", m_ViewModel.player.score));
                GUILayout.Label(string.Format(" | High Score: {0}", m_ViewModel.player.highScore));

                if (m_ViewModel.player.lastWordScore > 0)
                {
                    GUILayout.Label(string.Format(" | Last Word: {0} ({1})", m_ViewModel.player.lastWord,
                                                  m_ViewModel.player.lastWordScore));
                }
                if (m_ViewModel.player.bestWordScore > 0)
                {
                    GUILayout.Label(string.Format(" | Best Word: {0} ({1})", m_ViewModel.player.bestWord,
                                                  m_ViewModel.player.bestWordScore));
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (m_ViewModel.game.tilePoolExists)
                {
                    GUILayout.Label(string.Format("Tiles Remaining: {0}", m_ViewModel.game.tilesRemaining));
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUI.enabled = m_ViewModel.languages.currentLanguageIsLoaded && m_ViewModel.player.inputEnabled &&
                              m_ViewModel.player.hasSelection;
                if (GUILayout.Button("Clear Selection"))
                {
                    m_ViewModel.player.ClearSelection();
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }
}
