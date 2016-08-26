// LetterTileDaikonForgeControl.cs
// Copyright (c) 2011-2016 Thinksquirrel Inc.

using UnityEngine;
using Thinksquirrel.WordGameBuilder.ObjectModel;
using Thinksquirrel.WordGameBuilder.Internal;
using System.Reflection;

#if UNITY_WINRT && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using System.Threading.Tasks;
#endif

//! This namespace contains letter tile helper classes for Daikon Forge GUI.
namespace Thinksquirrel.WordGameBuilder.Tiles.DaikonForge
{
    /// <summary>
    /// Adds Daikon Forge GUI display to a letter tile.
    /// </summary>
    /// <remarks>
    /// This component must be on the same object as a letter tile. Use LetterTileVisibilityControl to modify control and label visibility.
    /// </remarks>
    [RequireComponent(typeof(ILetterTileDisplay))]
    [AddComponentMenu("Word Game Builder/Tiles/DF-GUI (Legacy)/Letter Tile Daikon Forge Control")]
    [WGBDocumentationName("Thinksquirrel.WordGameBuilder.Tiles.DaikonForge.LetterTileDaikonForgeControl")]
    public sealed class LetterTileDaikonForgeControl : WGBBase
    {
        [SerializeField] MonoBehaviour[] m_Controls;
        [SerializeField] MonoBehaviour m_LetterLabel;
        [SerializeField] MonoBehaviour m_ScoreLabel;

        /// <summary>
        /// Gets or sets the list of controls.
        /// </summary>
        /// <remarks>
        /// All controls must be of the type dfControl.
        /// </remarks>
        public MonoBehaviour[] controls { get { return m_Controls; } set { m_Controls = value; } }
        /// <summary>
        /// Gets or sets the letter label.
        /// </summary>
        /// <remarks>
        /// The label must be of the type dfLabel.
        /// </remarks>
        public MonoBehaviour letterLabel { get { return m_LetterLabel; } set { m_LetterLabel = value; } }
        /// <summary>
        /// Gets or sets the score label.
        /// </summary>
        /// <remarks>
        /// The label must be of the type dfLabel.
        /// </remarks>
        public MonoBehaviour scoreLabel { get { return m_ScoreLabel; } set { m_ScoreLabel = value; } }
        
        ILetterTileDisplay m_LetterTile;

        PropertyInfo m_ColorProperty;
        PropertyInfo m_TextProperty;


#if UNITY_WINRT && !UNITY_EDITOR
        async Task BindComponents()
#else
        void BindComponents()
#endif
        {
#if UNITY_WINRT && !UNITY_EDITOR
            var assemblyList = new List<Assembly>();
            var folder = Package.Current.InstalledLocation;
            foreach (var file in await folder.GetFilesAsync())
            {
                if (file.FileType == ".dll")
                {
                    var assemblyName = new AssemblyName(file.DisplayName);
                    var assembly = Assembly.Load(assemblyName);
                    assemblyList.Add(assembly);
                }
            }
            var assemblies = assemblyList.ToArray();
#else
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
#endif
            bool foundControl = false;
            bool foundLabel = false;

            for(int i = 0; i < assemblies.Length; ++i)
            {
                if (foundControl && foundLabel)
                    break;

                var assembly = assemblies[i];

                if (!foundControl)
                {
                    var controlType = assembly.GetType("dfControl");

                    if (controlType != null && controlType.Namespace == null)
                    {
                        m_ColorProperty = controlType.ExtGetProperty("Color");
                        foundControl = m_ColorProperty != null;
                    }
                }

                if (!foundLabel)
                {
                    var labelType = assembly.GetType("dfLabel");

                    if (labelType != null && labelType.Namespace == null)
                    {
                        m_TextProperty = labelType.ExtGetProperty("Text");
                        foundLabel = m_TextProperty != null;
                    }
                }
            }

        }

        void OnEnable()
        {
#if UNITY_WINRT && !UNITY_EDITOR
            var task = BindComponents();
            task.Wait();
#else
            BindComponents();
#endif
            m_LetterTile = GetComponentFromInterface<ILetterTileDisplay>();

            if (m_LetterTile != null)
            {
                m_LetterTile.onTileChange += UpdateTileSprite;
            }
            UpdateTileSprite();
        }

        void OnDisable()
        {
            if (m_LetterTile != null)
            {
                m_LetterTile.onTileChange -= UpdateTileSprite;
            }
        }

        void UpdateTileSprite()
        {
            if (m_Controls == null)
                return;

            if (m_ColorProperty != null && m_LetterTile.shouldChangeColor)
            {
                for (int i = 0; i < m_Controls.Length; ++i)
                {
                    var control = m_Controls[i];
                    
                    if (control)
                    {
                        try
                        {
                            m_ColorProperty.SetValue(control, (Color32)m_LetterTile.currentBackgroundColor, null);
                        }
                        catch
                        {
                            WGBBase.LogError(string.Format("Unable to assign color property to {0}", m_Controls[i].name), "Word Game Builder", "LetterTileDaikonForgeControl");
                        }
                    }
                }

                if (letterLabel)
                {
                    try
                    {
                        m_ColorProperty.SetValue(m_LetterLabel, (Color32)m_LetterTile.currentTextColor, null);
                    }
                    catch
                    {
                        WGBBase.LogError(string.Format("Unable to assign color property to {0}", m_LetterLabel.name), "Word Game Builder", "LetterTileDaikonForgeControl");
                    }
                }

                if (scoreLabel)
                {
                    try
                    {
                        m_ColorProperty.SetValue(m_ScoreLabel, (Color32)m_LetterTile.currentTextColor, null);
                    }
                    catch
                    {
                        WGBBase.LogError(string.Format("Unable to assign color property to {0}", m_ScoreLabel.name), "Word Game Builder", "LetterTileDaikonForgeControl");
                    }
                }

            }

            if (m_TextProperty != null && m_LetterTile.shouldChangeLabel)
            {
                if (letterLabel)
                {
                    try
                    {
                        m_TextProperty.SetValue(m_LetterLabel, m_LetterTile.currentLetterLabel, null);
                    }
                    catch
                    {
                        WGBBase.LogError(string.Format("Unable to assign text property to {0}", m_LetterLabel.name), "Word Game Builder", "LetterTileDaikonForgeControl");
                    }
                }

                if (scoreLabel)
                {
                    try
                    {
                        m_TextProperty.SetValue(m_ScoreLabel, m_LetterTile.currentScoreLabel, null);
                    }
                    catch
                    {
                        WGBBase.LogError(string.Format("Unable to assign text property to {0}", m_ScoreLabel.name), "Word Game Builder", "LetterTileDaikonForgeControl");
                    }
                }
            }
        }  
    }
}
