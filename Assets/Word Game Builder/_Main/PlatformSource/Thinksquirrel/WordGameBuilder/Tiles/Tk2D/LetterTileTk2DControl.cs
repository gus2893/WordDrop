// LetterTileTk2DControl.cs
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

//! This namespace contains letter tile helper classes for 2D Toolkit.
namespace Thinksquirrel.WordGameBuilder.Tiles.Tk2D
{
    /// <summary>
    /// Adds 2D Toolkit display to a letter tile.
    /// </summary>
    /// <remarks>
    /// This component must be on the same object as a letter tile. Use LetterTileVisibilityControl to modify control and label visibility.
    /// </remarks>
    [RequireComponent(typeof(ILetterTileDisplay))]
    [AddComponentMenu("Word Game Builder/Tiles/2D Toolkit/Letter Tile 2D Toolkit Control")]
    [WGBDocumentationName("Thinksquirrel.WordGameBuilder.Tiles.Tk2D.LetterTileTk2DControl")]
    public sealed class LetterTileTk2DControl : WGBBase
    {
        [SerializeField] MonoBehaviour[] m_Sprites;
        [SerializeField] MonoBehaviour m_LetterLabel;
        [SerializeField] MonoBehaviour m_ScoreLabel;

        /// <summary>
        /// Gets or sets the list of sprites.
        /// </summary>
        /// <remarks>
        /// All sprites must be of the type tk2dBaseSprite.
        /// </remarks>
        public MonoBehaviour[] sprites { get { return m_Sprites; } set { m_Sprites = value; } }
        /// <summary>
        /// Gets or sets the letter label.
        /// </summary>
        /// <remarks>
        /// The label must be of the type tk2dTextMesh.
        /// </remarks>
        public MonoBehaviour letterLabel { get { return m_LetterLabel; } set { m_LetterLabel = value; } }
        /// <summary>
        /// Gets or sets the score label.
        /// </summary>
        /// <remarks>
        /// The label must be of the type tk2dTextMesh.
        /// </remarks>
        public MonoBehaviour scoreLabel { get { return m_ScoreLabel; } set { m_ScoreLabel = value; } }

        ILetterTileDisplay m_LetterTile;

        PropertyInfo m_ColorProperty;
        PropertyInfo m_TextProperty;
        PropertyInfo m_TextColorProperty;

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

            bool foundSprite = false;
            bool foundLabel = false;

            for(int i = 0; i < assemblies.Length; ++i)
            {
                if (foundSprite && foundLabel)
                    break;

                var assembly = assemblies[i];

                if (!foundSprite)
                {
                    var controlType = assembly.GetType("tk2dBaseSprite");

                    if (controlType != null && controlType.Namespace == null)
                    {
                        m_ColorProperty = controlType.ExtGetProperty("color");
                        foundSprite = m_ColorProperty != null;
                    }
                }

                if (!foundLabel)
                {
                    var labelType = assembly.GetType("tk2dTextMesh");

                    if (labelType != null && labelType.Namespace == null)
                    {
                        m_TextProperty = labelType.ExtGetProperty("text");
                        m_TextColorProperty = labelType.ExtGetProperty("color");
                        foundLabel = m_TextProperty != null && m_TextColorProperty != null;
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
            if (m_Sprites == null)
                return;

            if (m_ColorProperty != null && m_LetterTile.shouldChangeColor)
            {
                for (int i = 0; i < m_Sprites.Length; ++i)
                {
                    var sprite = m_Sprites [i];
                    
                    if (sprite)
                    {
                        try
                        {
                            m_ColorProperty.SetValue(sprite, m_LetterTile.currentBackgroundColor, null);
                        }
                        catch
                        {
                            WGBBase.LogError(string.Format("Unable to assign color property to {0}", m_Sprites [i].name), "Word Game Builder", "LetterTileTk2DControl");
                        }
                    }
                }
            }

            if (m_TextColorProperty != null && m_LetterTile.shouldChangeColor)
            {
                if (letterLabel)
                {
                    try
                    {
                        m_TextColorProperty.SetValue(m_LetterLabel, m_LetterTile.currentTextColor, null);
                    }
                    catch
                    {
                        WGBBase.LogError(string.Format("Unable to assign color property to {0}", m_LetterLabel.name), "Word Game Builder", "LetterTileTk2DControl");
                    }
                }

                if (scoreLabel)
                {
                    try
                    {
                        m_TextColorProperty.SetValue(m_ScoreLabel, m_LetterTile.currentTextColor, null);
                    }
                    catch
                    {
                        WGBBase.LogError(string.Format("Unable to assign color property to {0}", m_ScoreLabel.name), "Word Game Builder", "LetterTileTk2DControl");
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
                        WGBBase.LogError(string.Format("Unable to assign text property to {0}", m_LetterLabel.name), "Word Game Builder", "LetterTileTk2DControl");
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
                        WGBBase.LogError(string.Format("Unable to assign text property to {0}", m_ScoreLabel.name), "Word Game Builder", "LetterTileTk2DControl");
                    }
                }
            }
        }
    }
}
